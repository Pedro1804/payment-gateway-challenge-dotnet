namespace PaymentGateway.Api.Domain;

public sealed record Amount
{
    public Amount(int? minorUnits, string? currency)
    {
        Currency = new Currency(currency);

        if (minorUnits is not > 0)
        {
            throw new InvalidPaymentException("amount", "Amount must be a positive integer in minor currency units.");
        }

        MinorUnits = minorUnits.Value;
    }

    public int MinorUnits { get; }

    public Currency Currency { get; }
}
