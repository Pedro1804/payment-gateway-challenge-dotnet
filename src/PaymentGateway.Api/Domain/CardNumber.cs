namespace PaymentGateway.Api.Domain;

public sealed record CardNumber
{
    public CardNumber(string value)
    {
        if (value.Length is not (>= 14 and <= 19) || !value.All(char.IsAsciiDigit))
        {
            throw new InvalidPaymentException("cardNumber", "Card number must contain between 14 and 19 digits.");
        }

        Value = value;
    }

    public string Value { get; }

    public string LastFour => Value[^4..];

    public override string ToString() => $"CardNumber {{ LastFour = {LastFour} }}";
}
