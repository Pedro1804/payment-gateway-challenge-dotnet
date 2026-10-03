using PaymentGateway.Api.Domain;

namespace PaymentGateway.Api.Application;

public sealed class PaymentProcessor(IAcquiringBank bank, ILogger<PaymentProcessor> logger)
{
    public Task<PaymentDecision> ProcessAsync(CardPayment payment, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
