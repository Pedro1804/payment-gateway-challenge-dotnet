namespace PaymentGateway.Api.Domain;

public sealed record Amount
{
    public Amount(int minorUnits, string currency)
    {
        Currency = new Currency(currency);

        if (minorUnits <= 0)
        {
            throw new InvalidPaymentException("amount", "Amount must be a positive integer in minor currency units.");
        }

        MinorUnits = minorUnits;
    }

    public int MinorUnits { get; }

    public Currency Currency { get; }
}
