# Context Handoff - Phase 5: record payments

**Status**: Phase 5 complete on `feature/payment-storage` (from `main` at `1a4b8c0`), commits red `d98051c` → green `0fdd137` → refactor `1904983`, review fixes `623c1a6`, red `9334bf6` → green `9ebbb3d` → refactor `3b7acfd`, not pushed · **Next**: Phase 6 — GET payment details

## 🎯 Core Result
**Built**: `Domain/Ports/IPaymentsRepository`, domain read model `Domain/Model/Payment`, storage shape `Infrastructure/Persistence/PaymentEntity` (primitives, `From(PaymentDecision)` / `ToPayment()`), `PaymentsRepository` over `ConcurrentDictionary<Guid, PaymentEntity>`, `CardExpiry.Restore` (no validation), `PaymentProcessor` calls `Add` after the bank answers, DI registers the port as singleton. Repository dependencies are named `paymentsRepository`. 80 tests green, `-warnaserror` clean.
**Key insight**: Sensitive data exclusion is enforced by the entity's shape; rehydration needs `CardExpiry.Restore` since the validating constructor rejects past expiries.

## 🚦 Current State
**✅ Solid foundation**: Unit tests on repository (`Get` after `Add`, unknown → null) and processor (records Authorized/Declined, nothing when bank unavailable); integration `RejectedWithoutCallingTheBank.NorRecordingThePayment` substitutes `IPaymentsRepository`.
**⚠️ Needs attention**: `GetPayment` still returns `Ok(payment)` with the raw domain `Payment` (nested `Expiry`/`Amount` JSON) and declares `ActionResult<PostPaymentResponse>`.
**⏸️ Deferred**: Mapping to `GetPaymentResponse`, moving `PaymentsControllerTests` to `Integration/` (phase 6).

## 👥 Next Agent Guidance
**Phase 6**: Map `Payment` → `GetPaymentResponse` (`CardNumberLastFour` as string, `Amount.MinorUnits`, `Amount.Currency.Code`, `Expiry.Month/Year`). `PaymentRequests.ValidCardPayment()` is the shared test fixture; `PostPaymentTests` already registers a substitute `IPaymentsRepository` — a GET integration suite needs a real `PaymentsRepository` instead.

## 📋 Reference Links
- [decision-log.yaml](decision-log.yaml) - Technical choices made
