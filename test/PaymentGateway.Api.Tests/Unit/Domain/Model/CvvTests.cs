using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Tests.Unit.Domain.Model;

public class CvvTests
{
    [Theory]
    [InlineData("123")]
    [InlineData("1234")]
    [InlineData("012")]
    public void AcceptsCvvsOf3Or4Digits(string digits)
    {
        // Act
        var cvv = new Cvv(digits);

        // Assert
        Assert.Equal(digits, cvv.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("12a")]
    [InlineData("١٢٣")]
    public void RejectsMalformedCvvs(string digits)
    {
        // Act
        var exception = Assert.Throws<InvalidPaymentException>(() => new Cvv(digits));

        // Assert
        Assert.Equal("cvv", exception.Field);
    }

    [Fact]
    public void ToStringDoesNotExposeTheCvv()
    {
        // Arrange
        var cvv = new Cvv("987");

        // Act
        var text = cvv.ToString();

        // Assert
        Assert.DoesNotContain("987", text);
    }
}
