namespace PaymentGateway.Api.Domain.Model;

public sealed record CardNumber
{
    public CardNumber(string value)
    {
        if (HasInvalidLength(value) || ContainsNonDigits(value))
        {
            throw new InvalidPaymentException("cardNumber", "Card number must contain between 14 and 19 digits.");
        }

        Value = value;
    }

    public string Value { get; }

    public string LastFour => Value[^4..];

    public override string ToString() => $"CardNumber {{ LastFour = {LastFour} }}";

    private static bool HasInvalidLength(string value) => value.Length is < 14 or > 19;

    private static bool ContainsNonDigits(string value) => !value.All(char.IsAsciiDigit);
}
