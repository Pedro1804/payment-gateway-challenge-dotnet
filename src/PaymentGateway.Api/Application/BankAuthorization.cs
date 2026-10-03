namespace PaymentGateway.Api.Application;

public abstract record BankAuthorization
{
    private BankAuthorization()
    {
    }

    public sealed record Authorized(string AuthorizationCode) : BankAuthorization;

    public sealed record Declined : BankAuthorization;
}
