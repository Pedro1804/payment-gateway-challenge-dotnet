using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests;

public static class PaymentRequests
{
    public static readonly DateOnly Today = new(2026, 10, 3);

    public static PostPaymentRequest Valid() => new()
    {
        CardNumber = "2222405343248877",
        ExpiryMonth = 4,
        ExpiryYear = 2027,
        Currency = "GBP",
        Amount = 1050,
        Cvv = "123"
    };
}
