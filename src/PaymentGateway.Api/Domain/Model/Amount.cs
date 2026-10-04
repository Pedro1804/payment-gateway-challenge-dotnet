namespace PaymentGateway.Api.Domain.Model;

public sealed record Amount
{
    public Amount(int minorUnits, string currency)
    {
        Currency = new Currency(currency);

        if (IsNotPositive(minorUnits))
        {
            throw new InvalidPaymentException("amount", "Amount must be a positive integer in minor currency units.");
        }

        MinorUnits = minorUnits;
    }

    public int MinorUnits { get; }

    public Currency Currency { get; }

    private static bool IsNotPositive(int minorUnits) => minorUnits <= 0;
}
