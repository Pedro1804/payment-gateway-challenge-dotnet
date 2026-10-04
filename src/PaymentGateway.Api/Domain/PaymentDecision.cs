using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Domain;

public sealed record PaymentDecision(Guid Id, PaymentStatus Status, CardPayment CardPayment, string? AuthorizationCode);
