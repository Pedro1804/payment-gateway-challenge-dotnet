using System.Text.Json.Serialization;

using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Infrastructure.Bank;

internal sealed record BankPaymentResponse([property: JsonPropertyName("authorized")] bool Authorized)
{
    public BankAuthorization ToBankAuthorization() => Authorized
        ? new BankAuthorization.Authorized()
        : new BankAuthorization.Declined();
}
