using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;

namespace PaymentGateway.Api.Infrastructure.Persistence;

public sealed class PaymentsRepository : IPaymentsRepository
{
    public void Add(PaymentDecision decision) => throw new NotImplementedException();

    public Payment? Get(Guid id) => throw new NotImplementedException();
}
