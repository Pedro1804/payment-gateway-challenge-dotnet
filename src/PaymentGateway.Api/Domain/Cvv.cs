namespace PaymentGateway.Api.Domain;

public sealed record Cvv
{
    public Cvv(string value)
    {
        if (HasInvalidLength(value) || ContainsNonDigits(value))
        {
            throw new InvalidPaymentException("cvv", "CVV must contain 3 or 4 digits.");
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => "Cvv { *** }";

    private static bool HasInvalidLength(string value) => value.Length is not (3 or 4);

    private static bool ContainsNonDigits(string value) => !value.All(char.IsAsciiDigit);
}
