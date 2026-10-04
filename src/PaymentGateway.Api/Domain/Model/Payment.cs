namespace PaymentGateway.Api.Domain.Model;

public sealed record Payment(Guid Id, PaymentStatus Status, string CardNumberLastFour, CardExpiry Expiry, Amount Amount)
{
    public static Payment From(PaymentDecision decision)
    {
        var card = decision.CardPayment;
        return new Payment(decision.Id, decision.Status, card.CardNumber.LastFour, card.Expiry, card.Amount);
    }
}
