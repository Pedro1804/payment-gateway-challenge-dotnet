using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Application;

public interface IAcquiringBank
{
    Task<BankAuthorization> AuthorizeAsync(CardPayment payment, CancellationToken cancellationToken);
}
