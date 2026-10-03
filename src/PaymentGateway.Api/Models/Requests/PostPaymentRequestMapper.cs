using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Models.Requests;

public static class PostPaymentRequestMapper
{
    public static CardPayment ToCardPayment(this PostPaymentRequest request, DateOnly today) =>
        throw new NotImplementedException();
}
