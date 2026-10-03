using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests.Unit.Models;

public class PostPaymentRequestMapperTests
{
    [Fact]
    public void MapsAValidRequestToACardPayment()
    {
        var payment = PaymentRequests.Valid().ToCardPayment(PaymentRequests.Today);

        Assert.Equal("2222405343248877", payment.CardNumber.Value);
        Assert.Equal(4, payment.Expiry.Month);
        Assert.Equal(2027, payment.Expiry.Year);
        Assert.Equal("GBP", payment.Currency.Code);
        Assert.Equal(1050, payment.Amount.MinorUnits);
        Assert.Equal("123", payment.Cvv.Value);
    }

    [Fact]
    public void RejectsTheFirstInvalidFieldInRequestOrder()
    {
        var request = PaymentRequests.Valid();
        request.Currency = "JPY";
        request.Cvv = "12";

        var exception = Assert.Throws<InvalidPaymentException>(() => request.ToCardPayment(PaymentRequests.Today));

        Assert.Equal("currency", exception.Field);
    }

    [Fact]
    public void RejectsAnEmptyRequestOnTheCardNumber()
    {
        var exception = Assert.Throws<InvalidPaymentException>(
            () => new PostPaymentRequest().ToCardPayment(PaymentRequests.Today));

        Assert.Equal("cardNumber", exception.Field);
    }

    [Fact]
    public void CardPaymentToStringDoesNotExposeCardNumberOrCvv()
    {
        var request = PaymentRequests.Valid();

        var payment = request.ToCardPayment(PaymentRequests.Today);

        Assert.DoesNotContain(request.CardNumber!, payment.ToString());
        Assert.DoesNotContain(request.Cvv!, payment.ToString());
    }
}
