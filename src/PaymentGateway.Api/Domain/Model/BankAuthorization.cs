namespace PaymentGateway.Api.Domain.Model;

public abstract record BankAuthorization
{
    private BankAuthorization()
    {
    }

    public sealed record Authorized : BankAuthorization;

    public sealed record Declined : BankAuthorization;
}
