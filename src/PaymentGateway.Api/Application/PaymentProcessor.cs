using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Application;

public sealed class PaymentProcessor(IAcquiringBank bank, ILogger<PaymentProcessor> logger)
{
    public async Task<PaymentDecision> ProcessAsync(CardPayment payment, CancellationToken cancellationToken)
    {
        var authorization = await bank.AuthorizeAsync(payment, cancellationToken);
        var status = authorization.IsAuthorized ? PaymentStatus.Authorized : PaymentStatus.Declined;
        var decision = new PaymentDecision(Guid.NewGuid(), status, payment, authorization.AuthorizationCode);
        logger.LogInformation("Payment {PaymentId} processed with status {Status}", decision.Id, decision.Status);
        return decision;
    }
}
