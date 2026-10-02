# Instructions for candidates

This is the .NET version of the Payment Gateway challenge. If you haven't already read this [README.md](https://github.com/cko-recruitment/) on the details of this exercise, please do so now. 

## Template structure
```
src/
    PaymentGateway.Api - a skeleton ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - an empty xUnit test project
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
docker-compose.yml - configures the bank simulator
PaymentGateway.sln
```

Feel free to change the structure of the solution, use a different test library etc.

## Running locally

Full stack (API + bank simulator), Swagger at http://localhost:5067/swagger:
```
docker compose up --build
docker compose logs -f payment_gateway
```

Development (debugging, hot reload): run only the bank in Docker and the API from your IDE or:
```
docker compose up bank_simulator
dotnet run --project src/PaymentGateway.Api
```
