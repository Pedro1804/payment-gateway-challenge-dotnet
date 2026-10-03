namespace PaymentGateway.Api.Domain;

public sealed record CardPayment(CardNumber CardNumber, CardExpiry Expiry, Amount Amount, Cvv Cvv);
