using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class AmountTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(1050)]
    [InlineData(int.MaxValue)]
    public void AcceptsPositiveAmountsInMinorUnits(int minorUnits)
    {
        var amount = new Amount(minorUnits);

        Assert.Equal(minorUnits, amount.MinorUnits);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsMissingZeroOrNegativeAmounts(int? minorUnits)
    {
        var exception = Assert.Throws<InvalidPaymentException>(() => new Amount(minorUnits));

        Assert.Equal("amount", exception.Field);
    }
}
