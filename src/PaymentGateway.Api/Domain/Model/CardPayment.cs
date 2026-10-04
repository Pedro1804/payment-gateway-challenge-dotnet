namespace PaymentGateway.Api.Domain.Model;

public sealed record CardPayment(CardNumber CardNumber, CardExpiry Expiry, Amount Amount, Cvv Cvv);
