using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class CurrencyTests
{
    [Theory]
    [InlineData("EUR")]
    [InlineData("USD")]
    [InlineData("GBP")]
    public void AcceptsSupportedCurrencies(string code)
    {
        var currency = new Currency(code);

        Assert.Equal(code, currency.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("eur")]
    [InlineData("JPY")]
    [InlineData("EURO")]
    public void RejectsMissingUnsupportedOrMiscasedCurrencies(string? code)
    {
        var exception = Assert.Throws<InvalidPaymentException>(() => new Currency(code));

        Assert.Equal("currency", exception.Field);
    }
}
