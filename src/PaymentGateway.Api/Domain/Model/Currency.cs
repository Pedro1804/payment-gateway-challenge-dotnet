namespace PaymentGateway.Api.Domain.Model;

public sealed record Currency
{
    private static readonly HashSet<string> SupportedCodes = ["EUR", "USD", "GBP"];

    public Currency(string code)
    {
        if (IsNotSupported(code))
        {
            throw new InvalidPaymentException("currency", "Currency must be one of EUR, USD or GBP.");
        }

        Code = code;
    }

    public string Code { get; }

    private static bool IsNotSupported(string code) => !SupportedCodes.Contains(code);
}
