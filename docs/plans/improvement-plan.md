# ZarinPal .NET SDK — Improvement Plan

## Context
- Single-project .NET library wrapping ZarinPal REST + GraphQL payment APIs.
- Builds clean today (0 warnings / 0 errors) but has correctness, architecture, packaging, and testing gaps.
- Current TFM: `net6.0` (EOL). Decided target: **multi-target `net8.0;netstandard2.0`**.
- Tests: to be implemented with **xUnit** (only mentioned here; no test scaffolding in this plan's scope).

## Phase 0 — Correctness / Bugs [Completed in v2.0.2]
- [x] **GraphQL `errors` handling** — `GraphqlAsync` parses `errors` array and throws `ResponseException`.
- [x] **REST business error codes** — `RequestAsync` maps ZarinPal `data.code` into typed `ZarinPalApiException`.
- [x] **Sandbox applies to GraphQL too** — `Config.Sandbox` routes GraphQL through environment-aware base URL.
- [x] **Tighten callback URL validation** — Validates host and scheme.
- [x] **Robust JSON handling & leak fixes** — Central `{data, errors}` parsing and `JsonDocument` disposal.
- [x] **`RefundMethod` serialization & validation** — `RefundMethod` string converter (`PAYA`/`CARD`) and mandatory method check in `Refunds.CreateAsync`.
- [x] **Public API null & empty guards** — Fail-fast `ArgumentNullException` and `ValidationException` guards across all resource methods and `GetRedirectUrl`.

## Phase 1 — Engineering baseline
- Add `global.json` pinning an LTS SDK (currently builds with a .NET 10 preview SDK → `NETSDK1057` warning).
- Retarget to **`net8.0;netstandard2.0`**, drop `net6.0` (EOL since Nov 2024).
- Remove stale `obj/Debug/net6.0|net8.0|netstandard2.0` artifacts; single restore.
- Add `Directory.Build.props` (shared properties) and Central Package Management (`Directory.Packages.props`).

## Phase 2 — API redesign
- **Typed response models** for the `data` envelope (e.g. `PaymentResult`, `VerifyResult`, `RefundResult`, `TransactionItem`); resources return them instead of raw `JsonElement`.
- Add **`CancellationToken`** to every async method.
- **`IHttpClientFactory`** + `AddZarinPal()`/`AddZarinPalClient(Config)` DI extension; allow injecting a configured `HttpClient`.
- Redesign `IZarinPalClient`: split transport from resources (e.g. an `IHttpTransport`) so it becomes a clean test seam.
- Single-pass serialization in `RequestAsync` (currently serialize → deserialize to `Dictionary` → re-serialize, `ZarinPal.cs:83-94`).
- Move `Wage` into `Models`; delete dead code (`Wage`, `ValidateWages`, `ValidateCardPan`).
- Switch regexes to cached `[GeneratedRegex]` (`Validator.cs`).
- Fix `PaymentRequest.Metadata` default (`new object[]{ }`, `PaymentModels.cs:23` → nullable dictionary/null).
- Strengthen model defaults/immutability; keep setters but validate at resource boundaries.

## Phase 3 — Packaging & observability
- NuGet metadata: `PackageId`, `Version`, `Authors`, `Description`, `PackageTags`, `RepositoryUrl`, `PackageLicenseExpression`, `GeneratePackageOnBuild`, `GenerateDocumentationFile`, SourceLink.
- Optional injected `ILogger` and resilient pipeline (retry/backoff) via `IHttpClientFactory`.
- Make user-agent and timeouts configurable (currently hardcoded `"ZarinPalSdk/v1 (.NET)"`).

## Phase 4 — Tests (xUnit) & docs [Completed]
- **xUnit** test projects added (`ZarinPal.SDK.UnitTests`, `ZarinPal.SDK.IntegrationTests`, `ZarinPal.SDK.E2ETests`) covering:
  - Validator unit tests (amount, authority, merchant_id, callback, mobile, email, etc.).
  - Model serialization and deserialization unit tests.
  - `RequestAsync`/`GraphqlAsync` tests with `MockHttpMessageHandler` (success, HTTP error, GraphQL `errors`, business `data.code`, empty body).
  - Dependency Injection container integration tests (`AddZarinPal`).
  - End-to-end sandbox payment, inquiry, reversal, fee calculation, and GraphQL refund flow tests.
- Centralized endpoint constants in `ZarinPal.Constants.Endpoints`.

## Out of scope / deferred
- Retry/Polly pipeline (Phase 3 optional).
- Multi-package split (REST vs GraphQL).
