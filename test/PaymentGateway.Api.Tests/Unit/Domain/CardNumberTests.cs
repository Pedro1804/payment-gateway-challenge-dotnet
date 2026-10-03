using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class CardNumberTests
{
    [Theory]
    [InlineData("12345678901234")]
    [InlineData("1234567890123456789")]
    public void AcceptsCardNumbersOf14To19Digits(string digits)
    {
        var cardNumber = new CardNumber(digits);

        Assert.Equal(digits, cardNumber.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567890123")]
    [InlineData("12345678901234567890")]
    [InlineData("1234abcd567890")]
    [InlineData("1234 5678 9012 34")]
    [InlineData("١٢٣٤٥٦٧٨٩٠١٢٣٤")]
    public void RejectsMissingOrMalformedCardNumbers(string? digits)
    {
        var exception = Assert.Throws<InvalidPaymentException>(() => new CardNumber(digits));

        Assert.Equal("cardNumber", exception.Field);
    }

    [Fact]
    public void LastFourKeepsLeadingZeros()
    {
        var cardNumber = new CardNumber("12345678900123");

        Assert.Equal("0123", cardNumber.LastFour);
    }

    [Fact]
    public void ToStringDoesNotExposeTheFullCardNumber()
    {
        var cardNumber = new CardNumber("12345678904242");

        Assert.DoesNotContain("12345678904242", cardNumber.ToString());
    }
}
