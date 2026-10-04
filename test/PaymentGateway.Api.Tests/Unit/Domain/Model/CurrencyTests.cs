using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Tests.Unit.Domain.Model;

public class CurrencyTests
{
    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    public void AcceptsSupportedCurrencies(string code)
    {
        // Act
        var currency = new Currency(code);

        // Assert
        Assert.Equal(code, currency.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("eur")]
    [InlineData("JPY")]
    [InlineData("EURO")]
    public void RejectsUnsupportedOrMiscasedCurrencies(string code)
    {
        // Act
        var exception = Assert.Throws<InvalidPaymentException>(() => new Currency(code));

        // Assert
        Assert.Equal("currency", exception.Field);
    }
}
