namespace PaymentGateway.Api.Domain;

public sealed record Currency
{
    private static readonly HashSet<string> SupportedCodes = ["EUR", "USD", "GBP"];

    public Currency(string? code)
    {
        if (code is null || !SupportedCodes.Contains(code))
        {
            throw new InvalidPaymentException("currency", "Currency must be one of EUR, USD or GBP.");
        }

        Code = code;
    }

    public string Code { get; }
}
