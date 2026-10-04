using PaymentGateway.Api.Api.Responses;
using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Api;

public static class PaymentMapper
{
    public static GetPaymentResponse ToGetPaymentResponse(this Payment payment) => new()
    {
        Id = payment.Id,
        Status = payment.Status,
        CardNumberLastFour = payment.CardNumberLastFour,
        ExpiryMonth = payment.Expiry.Month,
        ExpiryYear = payment.Expiry.Year,
        Currency = payment.Amount.Currency.Code,
        Amount = payment.Amount.MinorUnits
    };
}