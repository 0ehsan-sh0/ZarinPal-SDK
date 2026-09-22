# 1. RefundMethod Serialization and Public API Null Guards

Date: 2026-09-22

## Status

Accepted

## Context

1. **RefundMethod Enum Serialization**: In ZarinPal's GraphQL API, the `AddRefund` mutation requires an `InstantPayoutActionTypeEnum` parameter which accepts string enum values (`"PAYA"` or `"CARD"`). In `System.Text.Json`, enums serialize as integers by default (`0` for `PAYA`, `1` for `CARD`). This caused the GraphQL mutation to fail at the server. Furthermore, `Refunds.CreateAsync` bypassed validation when `Method` was not set, despite `Validator.ValidateMethod` marking it as required.
2. **Public API Null and Empty Guards**: Resource methods such as `Payments.CreateAsync(PaymentRequest data)`, `Payments.FeeCalculationAsync`, `Verifications.VerifyAsync`, `Inquiries.InquireAsync`, `Reversals.ReverseAsync`, `Transactions.ListAsync`, `Refunds.CreateAsync`, and `Refunds.ListAsync` dereferenced request arguments without null checks, throwing raw `NullReferenceException`. Additionally, `Payments.GetRedirectUrl(string authority)` did not validate the authority format, and `Refunds.RetrieveAsync(string refundId)` did not guard against null or empty IDs.

## Decisions

1. **Enum String Serialization & Mandatory Method**:
   - Add `[JsonConverter(typeof(JsonStringEnumConverter))]` directly to the `RefundMethod` enum definition as well as `RefundCreateRequest.Method`.
   - Require `Method` on `RefundCreateRequest` and invoke `Validator.ValidateMethod(data.Method)` unconditionally in `Refunds.CreateAsync`.
2. **Fail-Fast Argument Guards**:
   - Add explicit null checks (`if (data == null) throw new ArgumentNullException(nameof(data));`) to all public resource methods across `Payments`, `Verifications`, `Inquiries`, `Reversals`, `Transactions`, and `Refunds` (using standard `ArgumentNullException` compatible with both `net8.0` and `netstandard2.0`).
   - Validate `authority` using `Validator.ValidateAuthority(authority)` in `Payments.GetRedirectUrl`.
   - Validate `refundId` using `Validator.ValidateSessionId` or empty-string check in `Refunds.RetrieveAsync`.

## Consequences

- **Breaking Changes**:
  - Requesting a refund without specifying `Method` (`PAYA` or `CARD`) will now fail validation immediately on the client side rather than failing on the server.
  - Passing `null` to public resource methods now throws `ArgumentNullException` immediately instead of deferred `NullReferenceException`.
  - Passing invalid authority to `GetRedirectUrl` now throws `ValidationException`.
- **Positive Impacts**:
  - GraphQL `AddRefund` payloads now correctly transmit `"PAYA"` / `"CARD"` as strings.
  - Runtime errors are caught early with descriptive exception types and messages.
