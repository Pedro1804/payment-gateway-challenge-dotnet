using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Infrastructure.Persistence;

public sealed record PaymentEntity(
    Guid Id,
    PaymentStatus Status,
    string CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount)
{
    public static PaymentEntity From(PaymentDecision decision)
    {
        var card = decision.CardPayment;
        return new PaymentEntity(
            decision.Id,
            decision.Status,
            card.CardNumber.LastFour,
            card.Expiry.Month,
            card.Expiry.Year,
            card.Amount.Currency.Code,
            card.Amount.MinorUnits);
    }

    public Payment ToPayment() =>
        new(Id, Status, CardNumberLastFour, CardExpiry.Restore(ExpiryMonth, ExpiryYear), new Amount(Amount, Currency));
}
