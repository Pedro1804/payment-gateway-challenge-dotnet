using System.Text.Json.Serialization;

using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Infrastructure.Bank;

internal sealed record BankPaymentRequest(
    [property: JsonPropertyName("card_number")] string CardNumber,
    [property: JsonPropertyName("expiry_date")] string ExpiryDate,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("amount")] int Amount,
    [property: JsonPropertyName("cvv")] string Cvv)
{
    public static BankPaymentRequest From(CardPayment payment) => new(
        payment.CardNumber.Value,
        $"{payment.Expiry.Month:D2}/{payment.Expiry.Year}",
        payment.Amount.Currency.Code,
        payment.Amount.MinorUnits,
        payment.Cvv.Value);

    public override string ToString() => "BankPaymentRequest { *** }";
}
