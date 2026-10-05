using PaymentGateway.Api.Api.Responses;
using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Api;

public static class PaymentDecisionMapper
{
    public static PostPaymentResponse ToPostPaymentResponse(this PaymentDecision decision) => new()
    {
        Id = decision.Id,
        Status = decision.Status.ToResponseStatus(),
        CardNumberLastFour = decision.CardPayment.CardNumber.LastFour,
        ExpiryMonth = decision.CardPayment.Expiry.Month,
        ExpiryYear = decision.CardPayment.Expiry.Year,
        Currency = decision.CardPayment.Amount.Currency.Code,
        Amount = decision.CardPayment.Amount.MinorUnits
    };
}
