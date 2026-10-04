using System.Diagnostics;

using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Application;

public sealed class PaymentProcessor(IAcquiringBank bank, ILogger<PaymentProcessor> logger)
{
    public async Task<PaymentDecision> ProcessAsync(CardPayment payment, CancellationToken cancellationToken)
    {
        var authorization = await bank.AuthorizeAsync(payment, cancellationToken);
        var decision = Decide(payment, authorization);
        logger.LogInformation("Payment {PaymentId} processed with status {Status}", decision.Id, decision.Status);
        return decision;
    }

    private static PaymentDecision Decide(CardPayment payment, BankAuthorization authorization) => authorization switch
    {
        BankAuthorization.Authorized authorized =>
            new(Guid.NewGuid(), PaymentStatus.Authorized, payment, authorized.AuthorizationCode),
        BankAuthorization.Declined => new(Guid.NewGuid(), PaymentStatus.Declined, payment, null),
        _ => throw new UnreachableException()
    };
}
