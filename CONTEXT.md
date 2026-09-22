# Domain Context & Ubiquitous Language

This document defines the canonical domain terms and ubiquitous language for the **ZarinPal .NET SDK**. It serves as the single source of truth for domain concepts across the SDK and documentation.

---

## Ubiquitous Language

| Term | Domain Concept | Primary Identifier / Shape | Protocol |
| :--- | :--- | :--- | :--- |
| **Merchant** | A registered business or individual accepting payments via ZarinPal. Identified by a UUID. | `MerchantId` (UUID string) | REST / GraphQL |
| **Terminal** | A specific gateway terminal or checkout point under a merchant account. | `TerminalId` (string) | GraphQL |
| **Payment Request** | An intent to initiate a payment transaction with an amount, callback URL, and description. | `Authority` (generated on success) | REST |
| **Authority** | A 36-character unique transaction identifier starting with `A` or `S` followed by 35 alphanumeric characters. Used to redirect the customer to the payment gateway. | `Authority` (`^[AS][0-9A-Za-z]{35}$`) | REST |
| **Verification** | The confirmation step executed after the customer is returned to the `CallbackUrl`. Validates that the payment succeeded and amount matches. | `RefId` (reference ID upon success), `Code` (100 = initial success, 101 = already verified) | REST |
| **Inquiry** | Read-only check of a transaction status by its `Authority`. | `Authority` | REST |
| **Reversal** | Cancellation/voiding of a transaction before end-of-day bank settlement. | `Authority` | REST |
| **Refund** | Return of funds to a customer post-settlement. Supports payout methods (`PAYA`, `CARD`). | `SessionId` (string), `RefundMethod` (`PAYA`, `CARD`) | GraphQL |
| **Transaction** | A historical ledger entry representing a payment or payout. | `Id` / `TerminalId` | GraphQL |
| **Wage** | Fee-splitting or revenue-sharing entry designating an amount and target IBAN. | `Iban`, `Amount` | REST / GraphQL |
| **RefId** | Unique bank reference / receipt number returned after a successful transaction verification. | Numeric string / integer | REST |
