namespace PaymentGateway.Api.Domain;

public sealed record Cvv
{
    public Cvv(string value)
    {
        if (value.Length is not (3 or 4) || !value.All(char.IsAsciiDigit))
        {
            throw new InvalidPaymentException("cvv", "CVV must contain 3 or 4 digits.");
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => "Cvv { *** }";
}
