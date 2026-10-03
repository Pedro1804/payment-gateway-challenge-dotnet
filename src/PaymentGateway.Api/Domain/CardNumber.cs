namespace PaymentGateway.Api.Domain;

public sealed record CardNumber
{
    public CardNumber(string? value) => throw new NotImplementedException();

    public string Value => throw new NotImplementedException();

    public string LastFour => throw new NotImplementedException();
}
