using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class CvvTests
{
    [Theory]
    [InlineData("123")]
    [InlineData("1234")]
    [InlineData("012")]
    public void AcceptsCvvsOf3Or4Digits(string digits)
    {
        var cvv = new Cvv(digits);

        Assert.Equal(digits, cvv.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("12a")]
    [InlineData("١٢٣")]
    public void RejectsMissingOrMalformedCvvs(string? digits)
    {
        var exception = Assert.Throws<InvalidPaymentException>(() => new Cvv(digits));

        Assert.Equal("cvv", exception.Field);
    }

    [Fact]
    public void ToStringDoesNotExposeTheCvv()
    {
        var cvv = new Cvv("987");

        Assert.DoesNotContain("987", cvv.ToString());
    }
}
