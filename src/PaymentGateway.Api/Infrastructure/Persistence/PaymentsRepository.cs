using System.Collections.Concurrent;

using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;

namespace PaymentGateway.Api.Infrastructure.Persistence;

public sealed class PaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, Payment> _payments = new();

    public void Add(PaymentDecision decision)
    {
        var card = decision.CardPayment;
        _payments[decision.Id] = new Payment(decision.Id, decision.Status, card.CardNumber.LastFour, card.Expiry, card.Amount);
    }

    public Payment? Get(Guid id) => _payments.GetValueOrDefault(id);
}
