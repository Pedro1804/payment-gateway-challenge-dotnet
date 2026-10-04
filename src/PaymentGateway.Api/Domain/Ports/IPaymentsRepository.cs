using PaymentGateway.Api.Domain.Model;

namespace PaymentGateway.Api.Domain.Ports;

public interface IPaymentsRepository
{
    void Add(PaymentDecision decision);

    Payment? Get(Guid id);
}
