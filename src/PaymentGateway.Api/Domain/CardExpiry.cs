namespace PaymentGateway.Api.Domain;

public sealed record CardExpiry
{
    public CardExpiry(int month, int year, DateOnly today)
    {
        if (month is not (>= 1 and <= 12))
        {
            throw new InvalidPaymentException("expiryMonth", "Expiry month must be between 1 and 12.");
        }

        if (year < today.Year || (year == today.Year && month < today.Month))
        {
            throw new InvalidPaymentException("expiryYear", "Card expiry date must not be in the past.");
        }

        Month = month;
        Year = year;
    }

    public int Month { get; }

    public int Year { get; }
}
