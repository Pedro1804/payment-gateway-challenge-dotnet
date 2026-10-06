# Code Context — Payment Gateway

## Relevant Classes and Functions

- **PaymentsController.GetPaymentAsync** (`src/PaymentGateway.Api/Api/PaymentsController.cs:19-25`) - GET `api/Payments/{id:guid}`; returns `OkObjectResult(null)` → 204 instead of 404 (known bug, red test, fixed in phase 0). `async` without `await`.
- **PaymentsController** (`src/PaymentGateway.Api/Api/PaymentsController.cs:8-17`) - `[Route("api/[controller]")]`, `[ApiController]`, injects the concrete `PaymentsRepository`.
- **PaymentsRepository** (`src/PaymentGateway.Api/Infrastructure/Persistence/PaymentsRepository.cs:5-18`) - public `List<PostPaymentResponse>`, `Add`, `Get` (`FirstOrDefault`); not thread-safe, stores a response DTO.
- **PostPaymentRequest** (`src/PaymentGateway.Api/Api/Requests/PostPaymentRequest.cs:3-11`) - holds `CardNumberLastFour` (int) instead of the full number, `Cvv` int: to change in phase 1.
- **PostPaymentResponse / GetPaymentResponse** (`src/PaymentGateway.Api/Api/Responses/*.cs`) - same fields; `CardNumberLastFour` as `int` (leading zeros lost). `GetPaymentResponse` unused.
- **PaymentStatus** (`src/PaymentGateway.Api/Domain/Model/PaymentStatus.cs`) - `Authorized`, `Declined`, `Rejected` .
- **Program.cs** (`src/PaymentGateway.Api/Program.cs:7-12`) - service registration (`AddControllers`, Swagger, `AddSingleton<PaymentsRepository>`). No `public partial class Program`.

## Configuration and Environment

- **Bank:BaseUrl** (`src/PaymentGateway.Api/appsettings.json:9-11`) - `http://localhost:8080`; overridden with `http://bank_simulator:8080` by `docker-compose.yml` (`Bank__BaseUrl`).
- **Dependencies**: `Swashbuckle.AspNetCore` 6.2.3 (API); tests: xUnit 2.4.1, `Microsoft.AspNetCore.Mvc.Testing` 6.0.24 (`test/PaymentGateway.Api.Tests/PaymentGateway.Api.Tests.csproj`). To add: `NSubstitute`, `Microsoft.Extensions.TimeProvider.Testing`.
- **Simulator**: `docker compose up bank_simulator` (port 8080); `imposters/bank_simulator.ejs` must not be modified.
- **CI**: `.github/workflows/ci.yml` (restore, build, test Release).

## Testing Patterns

- **Existing**: `test/PaymentGateway.Api.Tests/PaymentsControllerTests.cs:16-46` (GET OK through `WebApplicationFactory<PaymentsController>` + replaced repository) and `:48-60` (404 expected, red before phase 0).
- **Unit**: `test/PaymentGateway.Api.Tests/Unit/` mirrors `src` (`Domain/Model`, `Domain/Services`, `Api`, `Infrastructure/...`).
- **Integration**: `test/PaymentGateway.Api.Tests/Integration/`.
- **Utilities**: `global using Xunit;` (`Usings.cs`); a valid request builder (`ValidPaymentRequest`) to create in phase 1 and reuse.
