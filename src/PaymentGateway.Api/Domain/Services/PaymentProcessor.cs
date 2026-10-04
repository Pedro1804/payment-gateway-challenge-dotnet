using System.Diagnostics;

using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;

namespace PaymentGateway.Api.Domain.Services;

public sealed class PaymentProcessor(IAcquiringBank bank, IPaymentsRepository payments, ILogger<PaymentProcessor> logger)
{
    public async Task<PaymentDecision> ProcessAsync(CardPayment payment, CancellationToken cancellationToken)
    {
        var authorization = await bank.AuthorizeAsync(payment, cancellationToken);
        var decision = Decide(payment, authorization);
        payments.Add(decision);
        logger.LogInformation("Payment {PaymentId} processed with status {Status}", decision.Id, decision.Status);
        return decision;
    }

    private static PaymentDecision Decide(CardPayment payment, BankAuthorization authorization) => authorization switch
    {
        BankAuthorization.Authorized => new(Guid.NewGuid(), PaymentStatus.Authorized, payment),
        BankAuthorization.Declined => new(Guid.NewGuid(), PaymentStatus.Declined, payment),
        _ => throw new UnreachableException()
    };
}
