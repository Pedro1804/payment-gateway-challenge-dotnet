# Payment Gateway

.NET implementation of the Checkout.com payment gateway challenge: an API that lets a merchant process a card payment through an acquiring bank and retrieve it afterwards.

## Documentation

- [Design considerations & assumptions](docs/design-considerations.md): architecture, API contract, validation rules, storage and what is out of scope.
- [Request examples](docs/post-payment-examples.md): `curl` requests and the responses to expect for every scenario.

## Structure
```
src/
    PaymentGateway.Api - the payment gateway ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - xUnit unit and integration tests
docs/ - design considerations and request examples
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
docker-compose.yml - runs the gateway and the bank simulator
PaymentGateway.sln
```

## Running locally

Full stack (API + bank simulator), Swagger at http://localhost:5067/swagger and liveness at http://localhost:5067/health:
```
docker compose up --build
docker compose logs -f payment_gateway
```

Development (debugging, hot reload): run only the bank in Docker and the API from your IDE or:
```
docker compose up bank_simulator
dotnet run --project src/PaymentGateway.Api
```

## Running the tests
```
dotnet test
```
