using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public static class PostPaymentRequestMapper
{
    public static CardPayment ToCardPayment(this PostPaymentRequest request, DateOnly today) =>
        new(
            new CardNumber(request.CardNumber),
            new CardExpiry(request.ExpiryMonth, request.ExpiryYear, today),
            new Amount(request.Amount, request.Currency),
            new Cvv(request.Cvv));
}
