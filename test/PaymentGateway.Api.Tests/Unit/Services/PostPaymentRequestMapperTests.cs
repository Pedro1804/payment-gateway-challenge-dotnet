using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Unit.Services;

public class PostPaymentRequestMapperTests
{
    [Fact]
    public void MapsAValidRequestToACardPayment()
    {
        // Arrange
        var request = PaymentRequests.Valid();

        // Act
        var payment = request.ToCardPayment(PaymentRequests.Today);

        // Assert
        Assert.Equal("2222405343248877", payment.CardNumber.Value);
        Assert.Equal(4, payment.Expiry.Month);
        Assert.Equal(2027, payment.Expiry.Year);
        Assert.Equal(1050, payment.Amount.MinorUnits);
        Assert.Equal("GBP", payment.Amount.Currency.Code);
        Assert.Equal("123", payment.Cvv.Value);
    }

    [Fact]
    public void RejectsTheFirstInvalidFieldInRequestOrder()
    {
        // Arrange
        var request = PaymentRequests.Valid();
        request.Currency = "JPY";
        request.Cvv = "12";

        // Act
        var exception = Assert.Throws<InvalidPaymentException>(() => request.ToCardPayment(PaymentRequests.Today));

        // Assert
        Assert.Equal("currency", exception.Field);
    }

    [Fact]
    public void CardPaymentToStringDoesNotExposeCardNumberOrCvv()
    {
        // Arrange
        var request = PaymentRequests.Valid();
        var payment = request.ToCardPayment(PaymentRequests.Today);

        // Act
        var text = payment.ToString();

        // Assert
        Assert.DoesNotContain(request.CardNumber, text);
        Assert.DoesNotContain(request.Cvv, text);
    }
}
