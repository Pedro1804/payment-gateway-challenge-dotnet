# Context Handoff - Phase 3 Acquiring Bank HTTP Adapter

**Status**: Phase 3 complete — PR #7 (`feature/acquiring-bank-client`; `e936e15` and `3601101` not pushed yet) · **Next**: Phase 4 — POST endpoint (`feature/post-payment-endpoint`)

## 🎯 Core Result
**Built**: `Infrastructure/Bank/AcquiringBankClient : IAcquiringBank` (typed HttpClient, `POST payments`), internal DTOs `BankPaymentRequest`/`BankPaymentResponse` (snake_case, `expiry_date` `MM/yyyy`), registration in `Program.cs`. 60 tests green. TDD commits: red `046a563`, green `2bb6b5d`, refactor `afcc640`, then `e936e15` (card payment inlined in tests) and `3601101` (bank failures logged as Error).
**Key insight**: Caller cancellation propagates as `OperationCanceledException`; only timeouts, network errors and non-200 statuses become `AcquiringBankUnavailableException`.

## 🚦 Current State
**✅ Solid foundation**: Client unit-tested with `FakeBankHandler` (configurable via `Answers`/`Fails`, records method, URI and body).
**⚠️ Needs attention**: A 200 with a null/invalid body, or `authorized:true` without code, is not handled (decision 4). Build shows pre-existing nullable warnings CS8603/CS8618 in `Api/Responses/*` and `PaymentsRepository.cs`.
**⏸️ Deferred**: Manual run against the Docker simulator (optional in the roadmap) not done. `PaymentProcessor` registration remains for phase 4.

## 👥 Next Agent Guidance
**Phase 4**: Map only `AcquiringBankUnavailableException` to 502; let `OperationCanceledException` pass. Pass `HttpContext.RequestAborted` (or the action's `CancellationToken`) to `PaymentProcessor`. In integration tests substitute `IAcquiringBank` so no HTTP call is made.
**Phase 5**: Unaffected by this phase.

---
## 🔗 Integration Points
**Expects**: `Bank:BaseUrl` configured (`appsettings.json`: `http://localhost:8080`; Docker: `http://bank_simulator:8080`).
**Provides**: `IAcquiringBank` resolvable from DI.

## 📋 Reference Links
- [decision-log.yaml](decision-log.yaml) - Technical choices made
