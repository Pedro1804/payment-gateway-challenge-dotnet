namespace PaymentGateway.Api.Domain.Model;

public sealed record CardExpiry
{
    public CardExpiry(int month, int year, DateOnly today)
    {
        if (IsNotAValidMonth(month))
        {
            throw new InvalidPaymentException("expiryMonth", "Expiry month must be between 1 and 12.");
        }

        if (IsBeforeCurrentMonth(month, year, today))
        {
            throw new InvalidPaymentException("expiryYear", "Card expiry date must not be in the past.");
        }

        Month = month;
        Year = year;
    }

    public int Month { get; }

    public int Year { get; }

    private static bool IsNotAValidMonth(int month) => month is < 1 or > 12;

    private static bool IsBeforeCurrentMonth(int month, int year, DateOnly today) =>
        year < today.Year || (year == today.Year && month < today.Month);
}
