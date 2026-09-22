# ZarinPal-SDK — Refactor & Improvement Report

**Date:** 2026-09-13
**Updated:** 2026-09-22 — Bugs §2.1–§2.5 fixed, verified (`dotnet build` 0 warnings, 130/130 tests pass, release v2.0.2)
**Scope:** `ZarinPal-SDK/` library, `tests/`, build/packaging, CI
**Sources:** `ZarinPal.cs`, `Config.cs`, `Resources/*`, `Validators/Validator.cs`, `Models/*`, `Interfaces/*`, `Extensions/ZarinPalServiceCollectionExtensions.cs`, `Constants/Endpoints.cs`, `Enums/RefundMethod.cs`, `Exceptions/*`, `ZarinPal-SDK.csproj`, `Directory.Build.props`, `global.json`, `.gitignore`, `docs/plans/improvement-plan.md`
> Status: §2.1–§2.5 implemented. Remaining P0 items: §2.6, §2.7.

---

## 1. Executive summary

The SDK is functional and well-structured (resource split, typed results, xUnit coverage for happy paths), and `docs/plans/improvement-plan.md` Phases 0-4 are largely done. Remaining work is correctness edge cases, architecture hardening, packaging fixes, and test realism — not a rewrite.

**Counts:** 2 correctness bugs remaining (P0), 8 architecture/design smells (P1), 5 packaging/build issues (P1), 4 testing/CI gaps (P1), 6 missing features (P2).

**Recommended order:**
1. P0 correctness (regex, dispose, enum serialization, null-guards)
2. P1 packaging + DI + error-parse dedup + docs versioning
3. P2 resilience/features (retry, transport seam, logging)

---

## 2. Findings — P0 Correctness / Bugs

### 2.1 Authority regex over-permissive — `Validators/Validator.cs:19,32` — ✅ FIXED
```csharp
// Before:
^[AS][0-9a-zA-Z]{35}$
// After:
^[AS][0-9A-Za-z]{35}$
```
`[0-9a-zA-Z]` spanned ASCII `0x39-0x61` and `0x7A-0x41`, admitting `: ; < = > ? @ [ \ ] ^ _ \``.
Tests in `ValidatorTests.cs:39` only use alphanumerics, so bug was latent.

**Impact:** Invalid authorities passed client validation, failed late at API.
**Fix applied:** `^[AS][0-9A-Za-z]{35}$` in both `#if NET8_0_OR_GREATER` and `#else` branches.
**Follow-up:** Add negative theory cases (`A...:...`, `A...[...]`, `A..._...`).

### 2.2 `JsonDocument` leak — `ZarinPal.cs:281-289` — ✅ FIXED
```csharp
JsonDocument doc;
try { doc = JsonDocument.Parse(responseContent); }
catch ... 
var root = doc.RootElement;
...
return root.Clone();
```
`doc` never disposed on success path.

**Fix applied:** Wrapped parsing block in `using (doc)`, `return root.Clone()` inside so disposal is safe. Trivial, no API change.

### 2.3 Silent null-masking — `Resources/Payments.cs:39,58`, `Verifications.cs:37`, `Inquiries.cs:36`, `Reversals.cs:36`, `Unverified.cs:31`, `Transactions.cs:72`, `Refunds.cs:81,111,156` — ✅ FIXED
```csharp
return result ?? new PaymentResult();
```
Masks deserialization returning null (empty `data`, wrong `dataPath`).

**Impact:** Callers got empty object instead of error; hard to debug.
**Fix applied:** All 10 sites now `return result ?? throw new ResponseException("API returned an empty ... response.")`. Breaking for anyone relying on empty object — see ADR note in §8.2. README `ResponseException` description updated accordingly.

### 2.4 `RefundMethod` serializes as int — `Models/RefundModels.cs:32-33`, `Resources/Refunds.cs:71-78`, `Enums/RefundMethod.cs:7-16` — ✅ FIXED
`System.Text.Json` defaults enums to numbers. GraphQL expects `InstantPayoutActionTypeEnum` (`PAYA`/`CARD` strings).

**Impact:** `AddRefund` with `Method` set sent `0/1` — server rejected or misrouted.
**Fix applied:** 
- Added `[JsonConverter(typeof(JsonStringEnumConverter))]` directly to `RefundMethod` enum and `RefundCreateRequest.Method`.
- Enforced mandatory `Validator.ValidateMethod(data.Method)` in `Refunds.CreateAsync`.
- Added unit tests for string serialization and deserialization.

### 2.5 Missing null/empty guards — ✅ FIXED
- `Payments.CreateAsync(PaymentRequest data)` derefs `data` without `ArgumentNullException`
- Same for `FeeCalculationAsync`, `VerifyAsync`, `InquireAsync`, `ReverseAsync`, `ListAsync`
- `Refunds.RetrieveAsync(string refundId)` had no empty check
- `Payments.GetRedirectUrl(string authority)` had no validation

**Fix applied:** 
- Added `if (data == null) throw new ArgumentNullException(nameof(data));` across all resource methods (compatible with `net8.0` and `netstandard2.0`).
- Added `Validator.ValidateAuthority(authority)` in `Payments.GetRedirectUrl`.
- Added `if (string.IsNullOrWhiteSpace(refundId)) throw new ValidationException("Refund ID is required.");` in `Refunds.RetrieveAsync`.
- Added unit test suite covering null guards and validations.

### 2.6 `RequestAsync` always sends body — `ZarinPal.cs:195-219`
Even `GET` builds `StringContent("{}"+merchant_id)` and attaches to `HttpRequestMessage`. All current callers use POST so latent, but `IZarinPalClient` is public — future GET misuse.

**Fix:** Only attach `Content` when `data != null || merchant_id injected`, or restrict to POST/PUT/PATCH. Alternatively split `PostAsync`/`GetAsync`.

### 2.7 Callback regex still naive — `Validators/Validator.cs:22,33`
```csharp
^https?://[a-zA-Z0-9.-]+(?::[0-9]+)?(?:/.*)?$
```
Rejects `https://example.com?x=1` (no `/`), IDN, `http://[::1]`; allows `http://a` single-label.

**Fix (P0-lite):** Keep regex for fast fail + add `Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme==http/https) && u.Host.Contains(".")` check, or switch fully to `Uri`. Update tests.

---

## 3. Findings — P1 Architecture / Design

### 3.1 Triplicated base-URL logic
`ZarinPal.cs:87-89,98-100,134-136,158-160,171-173` + `ZarinPalServiceCollectionExtensions.cs:48,56` — 6 copies of:
```
sandbox ? https://sandbox.zarinpal.com : https://payment.zarinpal.com
sandbox ? .../sandbox/.../graphql/ : .../next.../graphql/
```

**Fix:** Central `ZarinPalUrls.GetRestBaseUrl(sandbox)`, `GetGraphqlBaseUrl(sandbox)` in `Constants/`, or extend `Endpoints.cs`. Single source, unit-testable.

### 3.2 Duplicated error-envelope parsing — `ZarinPal.cs:274-423`
REST array/object branches (`335-375`), GraphQL branches (`299-332`), and `ExtractErrorMessage (396-423)` overlap ~60 lines.

**Fix:** One `ErrorEnvelopeParser` returning `(bool isError, Exception toThrow)`. Reduces drift (e.g. empty `errors: []` currently falls through to success in REST path — should it?).

### 3.3 Inconsistent headers
`ZarinPal.ConfigureHttpClientHeaders:186-190` sets `UserAgent + Accept: application/json`, but DI registration `ZarinPalServiceCollectionExtensions.cs:46-64` sets only `UserAgent`. Injected-`HttpClient` ctor `ZarinPal.cs:126` sets none.

**Fix:** Shared `ConfigureHttpClientHeaders` called from DI setup, or `ConfigureHttpClient` delegate. Assert `Accept` in integration test.

### 3.4 DI singleton freezes `HttpClient`
```csharp
services.AddSingleton<ZarinPal>(sp => new ZarinPal(cfg, factory, logger)); // Extensions:68-74
```
`CreateClient("ZarinPalRest"/"Graphql")` resolved once at first resolve — defeats `IHttpClientFactory` rotation/DNS refresh. `IZarinPalClient : IDisposable` + singleton invites disposing factory-managed clients (currently guarded by `_ownsHttpClients=false`, but interface invites misuse).

**Fix options:**
- (a) `AddTransient<ZarinPal>` + transient interfaces (simplest, factory-correct)
- (b) Typed clients (`AddHttpClient<ZarinPal>`) 
- (c) Split `IHttpTransport` (test seam, per improvement-plan Phase 2 deferred item)
Document choice in ADR — lifetime change is breaking for anyone resolving as singleton and holding state.

### 3.5 `IZarinPal` exposes concretes — `Interfaces/IZarinPal.cs:17-47`
```csharp
Payments Payments { get; } // concrete, not IPayments
```
Prevents mocking resources, couples interface to impl.

**Fix:** Introduce `IPayments`, `IRefunds`, etc., or expose only facade methods. Major version if done strictly; or add interfaces alongside concretes first.

### 3.6 Mutable `Config` + late validation — `Config.cs:9-33`, `ZarinPal.cs:60-62`
All setters mutable, no validation. Bad `MerchantId/Timeout/UserAgent` accepted until request time. `HttpClient { get; }` mutable property should be `readonly` field.

**Fix:** Validate in ctors (`ValidateMerchantId` if set, `Timeout > 0`, `UserAgent` non-empty) or add `Config.Validate()`. Make `HttpClient/GraphqlClient/BaseUrl` `readonly`.

### 3.7 Exception hierarchy confusion — `Exceptions/ResponseException.cs:30`, `ZarinPalApiException.cs:9`
`ZarinPalApiException : ResponseException` with base default `500` conflates business `code` (e.g. `-9`) with HTTP status. `GetStatusCode()` duplicates `StatusCode` property.

**Fix:** Keep hierarchy but remove parameterless `ResponseException(message)` default or make it explicit; remove `GetStatusCode()` (keep property); document `Code` vs `StatusCode` in XML docs + README table.

### 3.8 Fragile GraphQL `PostAsync("")` — `ZarinPal.cs:251`
Relies on trailing `/` in `BaseAddress`. Breaks if factory override drops slash.

**Fix:** `PostAsync(string.Empty)` → use `new HttpRequestMessage(HttpMethod.Post, "")` or ensure base ends with `/` + test.

---

## 4. Findings — P1 Packaging / Build

| # | File | Problem | Fix |
|---|------|---------|-----|
| 4.1 | `ZarinPal-SDK.csproj:23` | `<None Include=".github\workflows\publish.yml" />` wrong relative path (file is `../../.github/...` from csproj dir) — inert entry | Fix path or drop (workflows don't belong in nupkg) |
| 4.2 | `ZarinPal-SDK.csproj:8` vs `artifacts/` | `Version 2.0.0` vs stale on-disk `Ehsan.ZarinPal.SDK.1.1.0.nupkg` | Clean `artifacts/`, version via tag/minver or manual bump discipline |
| 4.3 | `ZarinPal-SDK.csproj:12-13` | `PublishRepositoryUrl + EmbedUntrackedSources` set but no `Microsoft.SourceLink.GitHub` → SourceLink inert | Add SourceLink package or remove flags |
| 4.4 | `LICENSE.txt` vs `Directory.Build.props:8` | `LICENSE.txt` placeholder `Copyright (c) [year] [fullname]` vs `Authors=Ehsan` | Fill MIT holder/year |
| 4.5 | `global.json:2-5` | `"allowPrerelease":true,"rollForward":"latestMajor"` contradicts pin-LTS intent, risks preview SDK (`NETSDK1057`) | `"allowPrerelease":false,"rollForward":"latestFeature"` + `version: 8.0.4xx` |
| 4.6 | `.gitignore:365-366` | `docs/` ignored → `docs/plans/improvement-plan.md` untracked, future ADRs/`CONTEXT.md` never versioned | Change to `docs/plans/local/` or un-ignore `docs/**/*.md`; keep `artifacts/` ignored |

---

## 5. Findings — P1 Tests / CI

- **Live E2E in CI:** `tests/ZarinPal.SDK.E2ETests/LiveSandboxE2ETests.cs:14-61` does real `sandbox.zarinpal.com` call; `ci.yml` runs E2E unconditionally; catch-all `Assert.True(true)` masks failures. **Fix:** `[Trait("Category","Live")]` + `SkipUnlessEnv("ZARINPAL_LIVE_TESTS")`, exclude Live from CI by filter.
- **Split-client never tested:** `RestResourceIntegrationTests.cs:21-30`, `GraphqlResourceIntegrationTests.cs:20-31`, `MockedSandboxE2ETests.cs:41` reuse same `HttpClient` for REST+GraphQL. **Fix:** Two `MockHttpMessageHandler`s with distinct `BaseAddress`, assert REST vs GraphQL host.
- **Missing negative paths:** resource-level validation failures, `RetrieveAsync("")`, `GetRedirectUrl(invalid)`, `merchant_id` precedence (only one positive assert `RestResourceIntegrationTests:64`), `data.code==101`, `data.code` missing, `errors` as string/object, non-2xx without `errors`, empty `errors[]`, `CancellationToken` propagation, `HttpRequestException`/timeout, `ILogger` output, DI null-args, named-client `BaseAddress/Timeout/Bearer`. **Fix:** Add theory cases; cheap high value.
- **No coverage gate:** Only `coverlet.collector` ref, no thresholds/report in CI. **Fix:** `--collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura` + threshold (e.g. 80% line) or `ReportGenerator` step. Stay on xUnit — no framework change needed.

---

## 6. Suggestions — P2 Features / Hardening (deferred, per improvement-plan Out-of-scope)

1. **Retry/backoff:** Polly `AddStandardResilienceHandler` or `AddTransientHttpErrorPolicy` on named clients for 408/429/5xx + GraphQL `errors` transient subset. Opt-in via `Config.RetryCount`.
2. **`IHttpTransport` seam:** Split `IZarinPalClient` transport (`SendRestAsync`, `SendGraphqlAsync`) from resources for unit tests without `HttpMessageHandler`.
3. **Typed `Metadata`:** `PaymentRequest.Metadata: object?` → `Dictionary<string,string>?` or `JsonObject?`. Current `object[]` default confusion noted in plan Phase 2.
4. **Wage input:** `Models/Wage.cs` is response-only; add `Wages` input on `PaymentRequest` if API supports split payout.
5. **Structured logging:** Beyond `LogDebug` in `ZarinPal.cs:221,250` — log `authority`, `code`, elapsed, redacted `merchant_id`. `ILogger` already injected.
6. **Runtime reconfig:** `Timeout/UserAgent` currently ctor-frozen; expose `IOptionsMonitor<Config>` or `ZarinPalOptions` for reload.

---

## 7. Proposed glossary (for `CONTEXT.md`)

| Term | Meaning | Identifier |
|------|---------|------------|
| Payment | Intent to pay (amount + callback + description) | — |
| Authority | 36-char pay token `A`/`S`+35 alphanumerics for redirect | `Authority` |
| Verification | Capture after callback (amount must match) | `Authority` + `Amount` → `RefId` |
| Inquiry | Status read | `Authority` |
| Reversal | Void before settlement (REST) | `Authority` |
| Refund | Post-settlement money-back (GraphQL) | `SessionId`, `RefundMethod PAYA/CARD`, `Reason` |
| Transaction | Ledger row (GraphQL list) | `TerminalId`, `Filter`, `Limit/Offset` |
| Wage | Fee split entry | `Iban`, `Amount` |
| Merchant / Terminal | REST credential (UUID) / GraphQL scope id | `MerchantId`, `TerminalId` |
| RefId | Success receipt after verify | `RefId`, `Code 100/101` |

Open questions carried from grill: confirm `Authority vs RefId` distinction, `Reverse vs Refund` boundary, `Wage` direction (in vs out).

## 8. Proposed ADRs (create sparingly, in `docs/adr/` after un-ignoring docs)

1. **Fix `RefundMethod` as string** — why breaking int→string, alternatives (custom converter vs options), migration.
2. **Throw on null payload vs empty object** — why fail-fast, affected resources.
3. **DI lifetime (singleton→transient/typed)** — factory rotation rationale, breaking note.
4. **`docs/` versioning** — why un-ignore markdown, what stays ignored (`artifacts/`, `bin/obj`).

---

## 9. Suggested incremental roadmap

**Step 1 — P0 (no design debate):** ~~authority regex, `using JsonDocument`,~~ ✅ DONE (§2.1–§2.3, verified); remaining: `ArgumentNull`+`Retrieve`/`Redirect` guards, `Uri` callback hardening, `RefundMethod` string converter + Method-required decision.
**Step 2 — P1 safe:** URL centralization, error-parser dedup, header consistency, `readonly` clients, `GetStatusCode` removal, csproj path/SourceLink/LICENSE/`global.json` pins, `docs/*.md` un-ignore.
**Step 3 — P1 debated (needs ADR):** null→throw, DI lifetime, `IZarinPal` interfaces, exception defaults.
**Step 4 — Tests:** Live quarantine, split-client tests, negative theories, coverage gate.
**Step 5 — P2:** Retry, transport seam, metadata typing, logging.

Each step: small PR, `dotnet build + dotnet test`, no facade signature change except where ADR notes break.

---

## Appendix — Key evidence paths

- `ZarinPal-SDK/ZarinPal.cs:60-65,71-184,195-272,274-394,521-529`
- `ZarinPal-SDK/Validators/Validator.cs:15-42,47-204`
- `ZarinPal-SDK/Resources/Payments.cs:29-70`, `Refunds.cs:30-157`
- `ZarinPal-SDK/Models/PaymentModels.cs:9-45`, `RefundModels.cs:9-40`
- `ZarinPal-SDK/Config.cs:9-33`, `Constants/Endpoints.cs:11-41`, `Enums/RefundMethod.cs:7-16`
- `ZarinPal-SDK/Interfaces/IZarinPal.cs:17-47`, `IZarinPalClient.cs`
- `ZarinPal-SDK/Extensions/ZarinPalServiceCollectionExtensions.cs:46-77`
- `ZarinPal-SDK/Exceptions/ResponseException.cs:30-42`, `ZarinPalApiException.cs:9-34`
- `ZarinPal-SDK/ZarinPal-SDK.csproj:4-26`, `Directory.Build.props:1-14`, `global.json:1-7`, `.gitignore:365-366`
- `tests/*/LiveSandboxE2ETests.cs:14-61`, `*IntegrationTests/*`, `ValidatorTests.cs:61 [Obsolete]`
