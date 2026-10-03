using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Models.Requests;

public static class PostPaymentRequestMapper
{
    public static CardPayment ToCardPayment(this PostPaymentRequest request, DateOnly today) =>
        new(
            new CardNumber(request.CardNumber),
            new CardExpiry(request.ExpiryMonth, request.ExpiryYear, today),
            new Currency(request.Currency),
            new Amount(request.Amount),
            new Cvv(request.Cvv));
}
