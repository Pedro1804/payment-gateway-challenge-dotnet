# Implementation Roadmap — Payment Gateway

## Execution Strategy

**Strategy**: Core-then-Integrate + TDD at each phase.
**Approach**: Step 0: green CI (GET returns 404). Step 1 (POST): domain (validation + domain object) → bank logic behind a port → bank HTTP adapter → endpoint wiring → storage. Step 2 (GET): reading the stored payment. Then documentation. Each phase = one branch + one PR, green CI on each PR.

Rules common to all phases (see `context-document.md`): load `code-taste` and `comment-taste` before coding; `tdd-red-phase` → `tdd-green-refactor-phases` cycle; no card number or CVV in logs or responses; existing models modified only when necessary; ask the user to validate the proposed branch name.

---

## Phase 0: Green CI — GET returns 404 if the payment does not exist

**Description**: Fix the only red test of the skeleton so that every following PR has a green CI.

**Objectives**:
- **Implementation**: `PaymentsController.GetPaymentAsync`: return `NotFound()` when the repository returns `null`, otherwise `Ok(payment)`. No other change (models, repository, routes unchanged).

**Testing Criteria**:
- `Returns404IfPaymentNotFound` passes (existing test, already red: the RED phase is done).
- `RetrievesAPaymentSuccessfully` stays green.
- Green CI on the PR.

**Dependencies**: None

**Relevant Local Skills**: `code-taste`, `comment-taste`, `tdd-green-refactor-phases`

**Files to Modify**:
- `src/PaymentGateway.Api/Controllers/PaymentsController.cs` - `GetPaymentAsync` (lines 19-25)

**Proposed branch**: `fix/get-payment-not-found`

---

## Phase 1: Request validation and domain object

**Description**: Turn a payment request into a valid `CardPayment` domain object, or throw `InvalidPaymentException` on the first invalid field. No HTTP/bank dependency.

**Objectives**:
- **Implementation**: Value objects in `Domain/` validated in their constructor (they throw `InvalidPaymentException(Field, Message)`): `CardNumber` (14–19 digits, exposes `LastFour` as a `string`), `CardExpiry` (month 1–12, month+year ≥ current UTC month, receives the current date as a parameter), `Currency` (`EUR`/`USD`/`GBP`, case-sensitive), `Amount` (> 0), `Cvv` (3–4 digits). Immutable types (`sealed record`); `CardNumber`, `Cvv` masked in `ToString()`.
- **Implementation**: `CardPayment` (record grouping the value objects).
- **Implementation**: `PostPaymentRequest`: replace `CardNumberLastFour` with `CardNumber` (`string`), `Cvv` as `string`, all fields `required` non-nullable — change needed to receive the card and detect missing fields.
- **Implementation**: Request → domain mapping (`Services/PostPaymentRequestMapper.cs`, `ToCardPayment(DateOnly today)`): builds the value objects in field order; the first exception bubbles up.

**Testing Criteria**:
- Each rule has its edge cases tested (`[Theory]`): card with 13/14/19/20 digits, letters, empty, null; month 0/1/12/13; current month valid, previous month invalid, next year valid; `EUR`/`USD`/`GBP` valid, `eur`, `JPY`, `EURO` invalid; amount 0/-1 invalid, 1 valid; CVV with 2/3/4/5 digits, letters.
- A request with several invalid fields throws the exception of the first field (request order).
- `LastFour` keeps leading zeros (`...0123` → `"0123"`).
- Tests without the system clock (`FakeTimeProvider` or date passed as a parameter).

**Dependencies**: None

**Relevant Local Skills**: `code-taste`, `comment-taste`, `tdd-red-phase`, `tdd-green-refactor-phases`, `testing-first`

**Files to Modify**:
- `src/PaymentGateway.Api/Domain/{CardNumber,CardExpiry,Currency,Amount,Cvv,CardPayment}.cs` - new
- `src/PaymentGateway.Api/Models/Requests/PostPaymentRequest.cs` - fields fixed, `required`
- `src/PaymentGateway.Api/Services/PostPaymentRequestMapper.cs` - new
- `test/PaymentGateway.Api.Tests/Unit/Domain/*Tests.cs`, `Unit/Services/PostPaymentRequestMapperTests.cs`, `PaymentRequests.cs` builder at the root of the test project - new; reusable valid request builder
- `test/PaymentGateway.Api.Tests/PaymentGateway.Api.Tests.csproj` - `Microsoft.Extensions.TimeProvider.Testing` if `FakeTimeProvider` is used

**Proposed branch**: `feature/post-payment-validation`

---

## Phase 2: Payment processing through the bank port

**Description**: Orchestrate the submission of a `CardPayment` to the bank behind an interface, and produce an `Authorized` or `Declined` `Payment`.

**Objectives**:
- **Implementation**: `Application/IAcquiringBank` port: `Task<BankAuthorization> AuthorizeAsync(CardPayment payment, CancellationToken ct)`; `BankAuthorization` (record: authorized or not + optional authorization code).
- **Implementation**: `Application/AcquiringBankUnavailableException` (thrown by the adapters, propagated as is by the processor).
- **Implementation**: `Domain/Payment` (record: `Id` Guid, `Status` `PaymentStatus`, `CardNumberLastFour`, `ExpiryMonth`, `ExpiryYear`, `Amount` (amount + currency), internal `AuthorizationCode` not exposed). Reuse the existing `PaymentStatus` enum.
- **Implementation**: `Application/PaymentProcessor.ProcessAsync(CardPayment, CancellationToken)` → `Payment`; new `Id`; `Information` log with `PaymentId` and `Status` only.

**Testing Criteria**:
- Bank authorizes → `Authorized` `Payment` with correct last 4 digits, expiry, currency, amount.
- Bank declines → `Declined`.
- Bank throws `AcquiringBankUnavailableException` → exception propagated.
- The `CardPayment` sent to the bank is the one received (NSubstitute `Received()`).
- Logger: `NullLogger<T>` in tests (no assertion on logs).

**Dependencies**: Phase 1

**Relevant Local Skills**: `code-taste`, `comment-taste`, `tdd-red-phase`, `tdd-green-refactor-phases`

**Files to Modify**:
- `src/PaymentGateway.Api/Application/{IAcquiringBank,BankAuthorization,AcquiringBankUnavailableException,PaymentProcessor}.cs` - new
- `src/PaymentGateway.Api/Domain/Payment.cs` - new
- `test/PaymentGateway.Api.Tests/Unit/Application/PaymentProcessorTests.cs` - new
- `test/PaymentGateway.Api.Tests/PaymentGateway.Api.Tests.csproj` - add `NSubstitute`

**Proposed branch**: `feature/payment-processing`

---

## Phase 3: Acquiring bank HTTP adapter

**Description**: Implement `IAcquiringBank` by calling the simulator (`POST {Bank:BaseUrl}/payments`).

**Objectives**:
- **Implementation**: `Infrastructure/Bank/AcquiringBankClient : IAcquiringBank` (typed `HttpClient`); `BankPaymentRequest`/`BankPaymentResponse` DTOs with snake_case `[JsonPropertyName]`; `expiry_date` in `"MM/yyyy"` format.
- **Implementation**: Any non-`200` response (503, 400…), `HttpRequestException`, timeout (`TaskCanceledException` outside a requested cancellation) → `AcquiringBankUnavailableException`; `Warning` log with the HTTP code or the error type, without body.
- **Implementation**: Registration in `Program.cs`: `AddHttpClient<IAcquiringBank, AcquiringBankClient>` with `BaseAddress` = `Bank:BaseUrl`.

**Testing Criteria**:
- Fake `HttpMessageHandler`: exact JSON body sent (snake_case, `"04/2030"`, 2-digit month), `/payments` URL, POST method.
- `200 authorized:true` → authorized + code; `200 authorized:false` → declined.
- `503`, `400`, network exception → `AcquiringBankUnavailableException`.
- (Optional, manual) real call to the Docker simulator for a card ending in 1, 2 and 0.

**Dependencies**: Phase 2

**Relevant Local Skills**: `code-taste`, `comment-taste`, `tdd-red-phase`, `tdd-green-refactor-phases`

**Files to Modify**:
- `src/PaymentGateway.Api/Infrastructure/Bank/{AcquiringBankClient,BankPaymentRequest,BankPaymentResponse}.cs` - new
- `src/PaymentGateway.Api/Program.cs` - HTTP client registration
- `test/PaymentGateway.Api.Tests/Unit/Infrastructure/Bank/AcquiringBankClientTests.cs` - new (+ fake handler)

**Proposed branch**: `feature/acquiring-bank-client`

---

## Phase 4: POST endpoint wired

**Description**: Expose `POST /api/payments` by wiring mapping, processor and error handling.

**Objectives**:
- **Implementation**: `PaymentsController` `[HttpPost]`: mapping → on `InvalidPaymentException`, `400` `ValidationProblemDetails` with `paymentStatus` extension = `"Rejected"` + `Information` log with the **name** of the invalid field; otherwise `PaymentProcessor` → `201` through `CreatedAtAction` to the GET + `PostPaymentResponse`.
- **Implementation**: `AcquiringBankUnavailableException` → `502` `ProblemDetails` ("acquiring bank unavailable"), through `IExceptionHandler` (.NET 8) or `try/catch` in the action — pick the simplest.
- **Implementation**: Align the automatic `[ApiController]` 400 (deserialization errors) on the same format through `ApiBehaviorOptions.InvalidModelStateResponseFactory`.
- **Implementation**: `PostPaymentResponse.CardNumberLastFour` as `string` (needed: leading zeros). `PaymentDecision` → `PostPaymentResponse` mapping (through `CardPayment.CardNumber.LastFour`, `CardPayment.Expiry`, `CardPayment.Amount`).
- **Implementation**: `Program.cs`: `PaymentProcessor`, `TimeProvider.System`, `JsonStringEnumConverter`, `public partial class Program { }`.
- **Design**: Known pitfall: ASP.NET strips the `Async` suffix from action names → `CreatedAtAction(nameof(GetPaymentAsync))` fails ("No route matches"). Resolve it (rename the action or name the route).

**Testing Criteria**:
- Integration (`WebApplicationFactory<Program>`, `IAcquiringBank` substituted): authorized card → 201, `Location` = `/api/payments/{id}`, `status: "Authorized"`, `cardNumberLastFour` = 4 digits; declined → 201 `Declined`; invalid → 400 with the error of the first invalid field and bank `DidNotReceive()`; bank unavailable → 502.
- Missing or `null` field (e.g. without `cvv`) → 400 in the same format (`paymentStatus` = `"Rejected"`), bank not called.
- Non-integer JSON value (`"amount": 10.5`, hence not in minor units) or wrongly typed field → 400 in the same format (`paymentStatus` = `"Rejected"`), bank not called.
- The response body never contains the full number or the CVV.
- Swagger shows the endpoint; manual demo with `docker compose up --build`.

**Dependencies**: Phases 1, 2, 3

**Relevant Local Skills**: `code-taste`, `comment-taste`, `tdd-red-phase`, `tdd-green-refactor-phases`

**Files to Modify**:
- `src/PaymentGateway.Api/Api/PaymentsController.cs` - POST action
- `src/PaymentGateway.Api/Api/Responses/PostPaymentResponse.cs` - `CardNumberLastFour` string
- `src/PaymentGateway.Api/Program.cs` - registrations, JSON enum, `partial class Program`
- `test/PaymentGateway.Api.Tests/Integration/PostPaymentTests.cs` - new
- `test/PaymentGateway.Api.Tests/PaymentsControllerTests.cs` - adapt if compilation requires it (type of `CardNumberLastFour`)

**Proposed branch**: `feature/post-payment-endpoint`

---

## Phase 5: Payment storage

**Description**: Store each `Authorized`/`Declined` payment so that it can be retrieved by its `Id`. End of step 1.

**Objectives**:
- **Implementation**: `Domain/Ports/IPaymentsRepository` port (`Add(PaymentDecision)`, `Get(Guid)` → read model without full number or CVV, or `null`).
- **Implementation**: Evolve `PaymentsRepository` (`Infrastructure/Persistence/`): stores a dedicated entity (Id, Status, last 4 digits, expiry, amount, currency; neither full number nor CVV) in a `ConcurrentDictionary<Guid, …>`.
- **Implementation**: `PaymentProcessor` stores the payment after the bank response (never on Rejected or unavailability).
- **Implementation**: `Program.cs`: `AddSingleton<IPaymentsRepository, PaymentsRepository>`.

**Testing Criteria**:
- Unit: `PaymentProcessor` calls `Add` for Authorized and Declined, not on a bank exception.
- Unit: repository — `Get` after `Add` returns the payment, unknown `Get` returns `null`.
- Integration: invalid POST → nothing stored.
- Existing `PaymentsControllerTests` tests adapted to the new stored type (still compiling).

**Dependencies**: Phase 4

**Relevant Local Skills**: `code-taste`, `comment-taste`, `tdd-red-phase`, `tdd-green-refactor-phases`

**Files to Modify**:
- `src/PaymentGateway.Api/Domain/Ports/IPaymentsRepository.cs` - new
- `src/PaymentGateway.Api/Infrastructure/Persistence/PaymentsRepository.cs` - thread-safe domain storage
- `src/PaymentGateway.Api/Domain/Services/PaymentProcessor.cs` - storage
- `src/PaymentGateway.Api/Program.cs` - port registration
- `test/PaymentGateway.Api.Tests/Unit/...` - repository and processor tests; `PaymentsControllerTests.cs` adapted

**Proposed branch**: `feature/payment-storage`

---

## Phase 6: Retrieving a payment's details (GET)

**Description**: `GET /api/payments/{id}` returns the stored domain payment (the 404 has been in place since phase 0).

**Objectives**:
- **Implementation**: `GetPaymentAsync`: `IPaymentsRepository.Get` → `404` if absent (keep the phase 0 behavior), otherwise `200` + `GetPaymentResponse` (mapping from the repository read model, `CardNumberLastFour` as `string`). Remove `async` if useless.
- **Implementation**: Move `PaymentsControllerTests.cs` to `Integration/` and align it on `WebApplicationFactory<Program>`.

**Testing Criteria**:
- `Returns404IfPaymentNotFound` still green.
- Round trip: POST (substituted bank) then GET on the returned `Id` → same data, identical status, last 4 digits only.
- Fully green CI.

**Dependencies**: Phase 5

**Relevant Local Skills**: `code-taste`, `comment-taste`, `tdd-red-phase`, `tdd-green-refactor-phases`

**Files to Modify**:
- `src/PaymentGateway.Api/Api/PaymentsController.cs` - GET fixed
- `src/PaymentGateway.Api/Api/Responses/GetPaymentResponse.cs` - `CardNumberLastFour` string
- `test/PaymentGateway.Api.Tests/Integration/GetPaymentTests.cs` - formerly `PaymentsControllerTests.cs`

**Proposed branch**: `feature/get-payment-details`

---

## Deferred (Last Responsible Moment)

After the requirements: retry with backoff towards the bank (`Microsoft.Extensions.Http.Resilience`), then possibly idempotency. To plan separately if kept.
