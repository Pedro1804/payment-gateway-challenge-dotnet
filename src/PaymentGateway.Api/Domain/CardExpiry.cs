namespace PaymentGateway.Api.Domain;

public sealed record CardExpiry
{
    public CardExpiry(int? month, int? year, DateOnly today) => throw new NotImplementedException();

    public int Month => throw new NotImplementedException();

    public int Year => throw new NotImplementedException();
}
