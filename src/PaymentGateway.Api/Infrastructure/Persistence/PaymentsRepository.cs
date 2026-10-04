using System.Collections.Concurrent;

using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;

namespace PaymentGateway.Api.Infrastructure.Persistence;

public sealed class PaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, PaymentEntity> _payments = new();

    public void Add(PaymentDecision decision) => _payments[decision.Id] = PaymentEntity.From(decision);

    public Payment? Get(Guid id) => _payments.TryGetValue(id, out var entity) ? entity.ToPayment() : null;
}
