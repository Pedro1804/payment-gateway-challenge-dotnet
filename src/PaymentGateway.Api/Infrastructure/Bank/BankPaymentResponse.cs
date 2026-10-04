using System.Text.Json.Serialization;

using PaymentGateway.Api.Domain.Ports;

namespace PaymentGateway.Api.Infrastructure.Bank;

internal sealed record BankPaymentResponse(
    [property: JsonPropertyName("authorized")] bool Authorized,
    [property: JsonPropertyName("authorization_code")] string? AuthorizationCode)
{
    public BankAuthorization ToBankAuthorization() => Authorized
        ? new BankAuthorization.Authorized(AuthorizationCode!)
        : new BankAuthorization.Declined();
}
