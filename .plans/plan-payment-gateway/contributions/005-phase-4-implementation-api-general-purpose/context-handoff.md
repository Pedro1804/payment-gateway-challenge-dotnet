# Context Handoff - Phase 4 POST Endpoint

**Status**: Phase 4 complete on `feature/post-payment-endpoint` (branched from `main`) — PR #9 · **Next**: Phase 5 — record payments

## 🎯 Core Result
**Built**: `POST /api/payments` in `PaymentsController`: 201 `CreatedAtAction(GetPayment)` with `PostPaymentResponse` (`PaymentDecisionMapper`), 400 `ValidationProblemDetails` + `paymentStatus: "Rejected"` (`RejectedPaymentResponse`, shared with `InvalidModelStateResponseFactory`), 502 `ProblemDetails` "Acquiring bank unavailable". `Program.cs` registers `PaymentProcessor` (scoped), `TimeProvider.System`, `JsonStringEnumConverter`, `public partial class Program`. 73 tests green, build clean with `-warnaserror`. TDD commits: red `dcdeeb3` + `3f2b645` (nested regrouping, null amount), green `0ad6188`, refactor `a2514ec` (includes the lowercase `api/payments` route).
**Key insight**: `ControllerBase.ValidationProblem()` bypasses `InvalidModelStateResponseFactory` in .NET 8; the controller calls `RejectedPaymentResponse.From(ControllerContext)` directly.

## 🚦 Current State
**✅ Solid foundation**: `test/.../Integration/PostPaymentTests.cs` — abstract base (factory, substituted `IAcquiringBank`, `FixedTimeProvider` on `PaymentRequests.Today`) with nested `Processed`, `RejectedWithoutCallingTheBank`, `BankUnavailable`. Null `cvv`/`cardNumber`/`amount` verified rejected.
**⚠️ Needs attention**: The domain rejection log (`Information`, field name only) is not tested. `PaymentsControllerTests` (GET) still uses `WebApplicationFactory<PaymentsController>` and reads `Status` with a `JsonStringEnumConverter`.
**⏸️ Deferred**: Swagger check and manual `docker compose up --build` demo not done. Nothing is recorded yet (phase 5).

## 👥 Next Agent Guidance
**Phase 5**: Record the payment in `PostPaymentAsync` after `ProcessAsync` (not on 400/502). `PaymentsRepository.Get` now returns `PostPaymentResponse?`. Add the assertion that a POSTed payment is retrievable to the nested test classes.
**Phase 6**: GET action is `GetPayment` (sync); keep the name, the 201 `Location` depends on it.

---
## 🔗 Integration Points
**Expects**: `IAcquiringBank`, `Bank:BaseUrl` (phase 3).
**Provides**: `POST /api/payments`; `RejectedPaymentResponse.From` for any future 400.

## 📋 Reference Links
- [decision-log.yaml](decision-log.yaml) - Technical choices made

## 🗣️ User decisions after the phase
- Deserialization 400s (missing/null/wrong type) keep ASP.NET's raw errors (`$`, `request`, `Cvv`, `$.amount`, .NET type names) next to `paymentStatus: "Rejected"`: the user keeps the request DTO `required` and non-nullable and declined harmonizing them.
- Demo doc `docs/post-payment-examples.md` is intentionally left uncommitted.
