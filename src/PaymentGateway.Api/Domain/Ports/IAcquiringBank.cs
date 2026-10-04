using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Domain.Ports;

public interface IAcquiringBank
{
    Task<BankAuthorization> AuthorizeAsync(CardPayment payment, CancellationToken cancellationToken);
}
