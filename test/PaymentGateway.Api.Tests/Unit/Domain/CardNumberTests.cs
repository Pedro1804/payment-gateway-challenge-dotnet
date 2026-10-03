using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class CardNumberTests
{
    [Theory]
    [InlineData("12345678901234")]
    [InlineData("1234567890123456789")]
    public void AcceptsCardNumbersOf14To19Digits(string digits)
    {
        // Act
        var cardNumber = new CardNumber(digits);

        // Assert
        Assert.Equal(digits, cardNumber.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567890123")]
    [InlineData("12345678901234567890")]
    [InlineData("1234abcd567890")]
    [InlineData("1234 5678 9012 34")]
    [InlineData("١٢٣٤٥٦٧٨٩٠١٢٣٤")]
    public void RejectsMalformedCardNumbers(string digits)
    {
        // Act
        var exception = Assert.Throws<InvalidPaymentException>(() => new CardNumber(digits));

        // Assert
        Assert.Equal("cardNumber", exception.Field);
    }

    [Fact]
    public void LastFourKeepsLeadingZeros()
    {
        // Arrange
        var cardNumber = new CardNumber("12345678900123");

        // Act
        var lastFour = cardNumber.LastFour;

        // Assert
        Assert.Equal("0123", lastFour);
    }

    [Fact]
    public void ToStringDoesNotExposeTheFullCardNumber()
    {
        // Arrange
        var cardNumber = new CardNumber("12345678904242");

        // Act
        var text = cardNumber.ToString();

        // Assert
        Assert.DoesNotContain("12345678904242", text);
    }
}
