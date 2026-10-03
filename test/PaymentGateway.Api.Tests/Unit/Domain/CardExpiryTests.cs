using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class CardExpiryTests
{
    private static readonly DateOnly Today = new(2026, 10, 3);

    [Theory]
    [InlineData(10, 2026)]
    [InlineData(11, 2026)]
    [InlineData(1, 2027)]
    [InlineData(12, 2027)]
    public void AcceptsExpiriesFromTheCurrentMonthOnwards(int month, int year)
    {
        var expiry = new CardExpiry(month, year, Today);

        Assert.Equal(month, expiry.Month);
        Assert.Equal(year, expiry.Year);
    }

    [Theory]
    [InlineData(9, 2026)]
    [InlineData(12, 2025)]
    public void RejectsExpiriesInThePastOnTheYearField(int month, int year)
    {
        var exception = Assert.Throws<InvalidPaymentException>(() => new CardExpiry(month, year, Today));

        Assert.Equal("expiryYear", exception.Field);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(13)]
    public void RejectsMissingOrOutOfRangeMonths(int? month)
    {
        var exception = Assert.Throws<InvalidPaymentException>(() => new CardExpiry(month, 2027, Today));

        Assert.Equal("expiryMonth", exception.Field);
    }

    [Fact]
    public void RejectsMissingYear()
    {
        var exception = Assert.Throws<InvalidPaymentException>(() => new CardExpiry(12, null, Today));

        Assert.Equal("expiryYear", exception.Field);
    }

    [Fact]
    public void ReportsTheMonthFirstWhenMonthAndYearAreMissing()
    {
        var exception = Assert.Throws<InvalidPaymentException>(() => new CardExpiry(null, null, Today));

        Assert.Equal("expiryMonth", exception.Field);
    }
}
