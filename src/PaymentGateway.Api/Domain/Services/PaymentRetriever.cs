using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;

namespace PaymentGateway.Api.Domain.Services;

public sealed class PaymentRetriever(IPaymentsRepository paymentsRepository)
{
    public Payment? Find(Guid id) => paymentsRepository.Get(id);
}
