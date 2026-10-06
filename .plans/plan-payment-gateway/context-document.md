# Context Document — Payment Gateway

## Behavioral Specification

A merchant submits a card payment (`POST`): the gateway validates the request, rejects it without calling the bank if it is invalid, otherwise forwards it to the acquiring bank and answers with an `Authorized` or `Declined` payment, identified by an `Id`. The merchant can then retrieve that payment by its `Id` (`GET`), without ever seeing the full card number.

### HTTP contract (REST)

| Case | Response |
|---|---|
| Valid POST, bank authorizes / declines | `201 Created`, `Location: /api/payments/{id}` header, body = payment (`Status` = `Authorized` / `Declined`) |
| Invalid POST (Rejected) | `400` `ValidationProblemDetails`: `paymentStatus` extension = `"Rejected"`, error of the first invalid field only, bank **not** called, nothing stored |
| Bank unavailable / unexpected response (503, 400, timeout, network) | `502 Bad Gateway` `ProblemDetails` stating that the bank is unavailable, nothing stored |
| GET known id | `200` + payment |
| GET unknown id | `404` |

Response fields (POST and GET): `Id` (Guid), `Status`, `CardNumberLastFour`, `ExpiryMonth`, `ExpiryYear`, `Currency`, `Amount`. `Status` serialized as text (`"Authorized"`), not as a number.

### Validation rules

| Field | Rule |
|---|---|
| CardNumber | required, 14–19 characters, digits only |
| ExpiryMonth | required, 1–12 |
| ExpiryYear | required; month+year ≥ current month (UTC) — a card expiring this month is valid |
| Currency | required, exactly `EUR`, `USD` or `GBP` (case-sensitive) |
| Amount | required, integer > 0, minor unit |
| Cvv | required, 3–4 characters, digits only |

Only the first error is returned, following the order of the request fields (cardNumber, expiry, currency, amount, cvv). `Amount` is already in minor units: the merchant sends `1050` for 10.50 and the gateway converts nothing. A non-integer JSON value (`10.5`) or a wrongly typed one fails deserialization: `[ApiController]` then returns a `400` by itself, to be aligned on the same format (`paymentStatus` extension).

### Assumptions (interpretations of the brief, to document in the README)

- "Expiry month + year in the future": a card is valid until the last day of its expiry month, so the current month is accepted.
- `Amount` > 0: not required by the brief, added because a zero or negative payment makes no sense.
- "Storing card information": only the last 4 digits are kept; the full number and the CVV are never stored (PCI compliance).
- Case-sensitive currencies: ISO 4217 codes are uppercase.
- A single error returned (the first one): code simplicity preferred over merchant convenience; to revisit if needed.

### Acquiring bank (simulator)

`POST {Bank:BaseUrl}/payments`, snake_case body: `card_number`, `expiry_date` (`"MM/yyyy"`), `currency`, `amount`, `cvv`. `200` response `{ "authorized": bool, "authorization_code": string }`. Last card digit: odd → authorized, even → declined, `0` → `503`.

## Codebase Patterns to Follow

- **Architecture**: a single `src/PaymentGateway.Api` project, ports & adapters by folder:
  - `Domain/Model/`: value objects, `CardPayment`, `PaymentDecision`, `PaymentStatus`, `InvalidPaymentException`; no dependency (no ASP.NET, no HTTP, no logging).
  - `Domain/Ports/`: interfaces to the outside world (`IAcquiringBank`, `IPaymentsRepository`) and their exchange types.
  - `Domain/Services/`: orchestration (`PaymentProcessor`).
  - `Api/`: inbound HTTP adapter (`PaymentsController`, `Requests/`, `Responses/`, request → domain mapper).
  - `Infrastructure/`: outbound adapters (`Bank/` bank HTTP client, `Persistence/` in-memory repository).
  - Namespaces follow folders; unit tests mirror the same tree under `Unit/`.
- **Domain**: `Amount` carries the amount in minor units and its `Currency`; the request → domain mapper lives in `Api/`. Properties declared below the constructor, read-only (`{ get; }`, not `init`, so that `with` cannot bypass validation).
- **Validation**: hand-written in the domain. Each value object validates itself in its constructor and throws `InvalidPaymentException(Field, Message)` (JSON field name): a value object cannot exist in an invalid state. The request → domain mapping lets the first exception bubble up; the HTTP layer translates it into a 400.
- **Request DTO**: all fields `required` and non-nullable (honored by System.Text.Json in .NET 8): a missing or `null` field is rejected by ASP.NET (automatic 400) before the domain, which receives non-nullable types and only validates the format. This automatic 400 must carry `paymentStatus` (phase 4).
- **Domain readability**: each checked rule is a named private method (`HasInvalidLength`, `ContainsNonDigits`, `IsNotSupported`, `IsBeforeCurrentMonth`…), phrased negatively so that `if`s read without `!`, placed after the properties.
- **Time**: the domain and the mapper receive `DateOnly today` as a parameter; the HTTP layer computes it through `TimeProvider` (.NET 8, injected). Domain tests: fixed date, no `FakeTimeProvider`.
- **Bank failure**: the adapter throws `AcquiringBankUnavailableException`; translated into a 502 at the HTTP level.
- **Logging**: injected `ILogger<T>`, only at useful points (payment outcome, rejection with the invalid field name, bank failure). Never: card number, CVV, request body.
- **Tests**: xUnit + `Assert`. NSubstitute for ports in unit tests (and check `DidNotReceive()` on the bank). Fake `HttpMessageHandler` to test the HTTP adapter. `WebApplicationFactory<Program>` for integration, with `IAcquiringBank` substituted (no Docker required). Tree `test/PaymentGateway.Api.Tests/Unit/...` and `Integration/...`. Naming: readable expected behavior; sections delimited by `// Arrange`, `// Act`, `// Assert` (user request, empty sections omitted).
- **TDD** at each step: red test → minimal code → refactor.

## Technical Constraints

- .NET 8, idiomatic C#, simple code, no over-engineering (no MediatR, CQRS, multi-project).
- The code must compile; `dotnet test` must pass (GitHub Actions CI on each PR).
- Only modify existing models when necessary.
- Do not modify `imposters/` or `.editorconfig`.
- In-memory storage only (no database).
- Never return or log the full card number or the CVV.
- One branch + one PR per phase; PRs without description; commits prefixed with `feature : `, `fix : `, `config : `…
- Retry/backoff towards the bank: out of scope until the requirements are done.

## Local Repository Skills

- `code-taste`, `comment-taste`: to load before writing or reviewing code.
- `tdd-red-phase`, `tdd-green-refactor-phases`: TDD cycle of each phase.
- `testing-first`: FIRST principles (written for Java/Kotlin, principles applicable).
- `dev-contribute`: execute a phase of this plan.
- `design-principles`, `execution-strategies`, `contribution-system`: plan references.
