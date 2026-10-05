using PaymentGateway.Api.Domain.Exceptions;
using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Tests.Unit.Domain.Model;

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
        // Act
        var expiry = new CardExpiry(month, year, Today);

        // Assert
        Assert.Equal(month, expiry.Month);
        Assert.Equal(year, expiry.Year);
    }

    [Theory]
    [InlineData(9, 2026)]
    [InlineData(12, 2025)]
    public void RejectsExpiriesInThePastOnTheYearField(int month, int year)
    {
        // Act
        var exception = Assert.Throws<InvalidPaymentException>(() => new CardExpiry(month, year, Today));

        // Assert
        Assert.Equal("expiryYear", exception.Field);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void RejectsOutOfRangeMonths(int month)
    {
        // Act
        var exception = Assert.Throws<InvalidPaymentException>(() => new CardExpiry(month, 2027, Today));

        // Assert
        Assert.Equal("expiryMonth", exception.Field);
    }

    [Fact]
    public void RestoresARecordedExpiryEvenOnceItHasPassed()
    {
        // Act
        var expiry = CardExpiry.Restore(9, 2026);

        // Assert
        Assert.Equal(9, expiry.Month);
        Assert.Equal(2026, expiry.Year);
    }
}
