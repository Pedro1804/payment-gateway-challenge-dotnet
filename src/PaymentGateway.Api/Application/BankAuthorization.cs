namespace PaymentGateway.Api.Application;

public sealed record BankAuthorization(bool IsAuthorized, string? AuthorizationCode);
