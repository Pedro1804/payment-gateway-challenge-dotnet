namespace PaymentGateway.Api.Domain;

public sealed record Amount
{
    public Amount(int? minorUnits) => throw new NotImplementedException();

    public int MinorUnits => throw new NotImplementedException();
}
