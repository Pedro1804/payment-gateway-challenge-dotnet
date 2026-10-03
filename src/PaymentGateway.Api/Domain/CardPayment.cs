namespace PaymentGateway.Api.Domain;

public sealed record CardPayment(CardNumber CardNumber, CardExpiry Expiry, Currency Currency, Amount Amount, Cvv Cvv);
