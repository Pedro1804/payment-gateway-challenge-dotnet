using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Infrastructure.Persistence;

namespace PaymentGateway.Api.Tests.Unit.Infrastructure.Persistence;

public class PaymentsRepositoryTests
{
    private readonly PaymentsRepository _paymentsRepository = new();

    [Fact]
    public void ReturnsTheRecordedPaymentWithoutSensitiveCardData()
    {
        // Arrange
        var cardPayment = PaymentRequests.ValidCardPayment();
        var decision = new PaymentDecision(Guid.NewGuid(), PaymentStatus.Declined, cardPayment);
        _paymentsRepository.Add(decision);

        // Act
        var payment = _paymentsRepository.Get(decision.Id);

        // Assert
        var expected = new Payment(decision.Id, PaymentStatus.Declined, "8877", cardPayment.Expiry, cardPayment.Amount);
        Assert.Equal(expected, payment);
    }

    [Fact]
    public void ReturnsNullForAnUnknownPayment()
    {
        // Act
        var payment = _paymentsRepository.Get(Guid.NewGuid());

        // Assert
        Assert.Null(payment);
    }
}
