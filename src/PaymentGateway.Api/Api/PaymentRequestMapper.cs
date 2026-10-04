using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Api.Requests;

namespace PaymentGateway.Api.Api;

public static class PaymentRequestMapper
{
    public static CardPayment ToCardPayment(this PostPaymentRequest request, DateOnly today) =>
        new(
            new CardNumber(request.CardNumber),
            new CardExpiry(request.ExpiryMonth, request.ExpiryYear, today),
            new Amount(request.Amount, request.Currency),
            new Cvv(request.Cvv));
}
