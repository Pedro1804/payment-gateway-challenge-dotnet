using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;

namespace PaymentGateway.Api.Infrastructure.Bank;

public sealed class AcquiringBankClient(HttpClient httpClient, ILogger<AcquiringBankClient> logger) : IAcquiringBank
{
    public Task<BankAuthorization> AuthorizeAsync(CardPayment payment, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
