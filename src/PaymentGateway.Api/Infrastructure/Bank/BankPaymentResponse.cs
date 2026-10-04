using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Infrastructure.Bank;

internal sealed record BankPaymentResponse(
    [property: JsonPropertyName("authorized")] bool Authorized,
    [property: JsonPropertyName("authorization_code")] string? AuthorizationCode);
