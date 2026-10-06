# Context Handoff - Phase 1 Domain Logic

**Status**: Phase 1 complete · **Next**: Phase 2 — payment processing through the bank port (`feature/payment-processing`)

## 🎯 Core Result
**Built**: Value objects `CardNumber`, `CardExpiry`, `Currency`, `Amount` (amount + currency), `Cvv`, `CardPayment` aggregate, and `PostPaymentRequest.ToCardPayment(today)` in `Services/`. 45 green tests; request fields `required`, domain rules as named private methods.
**Key insight**: Validation through exceptions (`InvalidPaymentException.Field` = JSON field); first error only, in request order.

## 🚦 Current State
**✅ Solid foundation**: A constructed `CardPayment` is always valid; `CardNumber.LastFour` (string, leading zeros kept) ready for `Payment`; `ToString()` masks card and CVV.
**⚠️ Needs attention**: Pre-existing warnings CS8603 (`PaymentsRepository.Get`) and CS8618 (`Currency` of the response models) — will disappear in phases 4/5.
**⏸️ Deferred**: Translation of `InvalidPaymentException` into 400 + `paymentStatus` and computation of `today` through `TimeProvider`: phase 4.

## 👥 Next Agent Guidance
**Phase 2**: Consume `CardPayment` as is; `Payment.CardNumberLastFour` = `CardNumber.LastFour`, amount/currency through `Amount`. Do not re-validate in the application. Tests: `// Arrange`, `// Act`, `// Assert` sections.
**Phase 4**: a missing/`null` field is rejected by ASP.NET before the mapper: add `paymentStatus` to that automatic 400 and test it. `PaymentRequests.Valid()` (`test/PaymentGateway.Api.Tests/PaymentRequests.cs`) is the valid request builder to reuse; `PaymentRequests.Today` = 2026-10-03.

## 🔗 Integration Points
**Expects**: Caller providing `DateOnly today` in UTC.
**Provides**: `CardPayment` (pure domain, no dependency) and `InvalidPaymentException`.
