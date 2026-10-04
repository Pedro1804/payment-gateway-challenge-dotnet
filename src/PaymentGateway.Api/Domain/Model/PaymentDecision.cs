namespace PaymentGateway.Api.Domain.Model;

public sealed record PaymentDecision(Guid Id, PaymentStatus Status, CardPayment CardPayment);
