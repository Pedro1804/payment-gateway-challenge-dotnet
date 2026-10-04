namespace PaymentGateway.Api.Domain.Model;

public sealed record Payment(Guid Id, PaymentStatus Status, string CardNumberLastFour, CardExpiry Expiry, Amount Amount);
