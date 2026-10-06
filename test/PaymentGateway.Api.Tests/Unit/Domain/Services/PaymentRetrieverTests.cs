using NSubstitute;

using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;
using PaymentGateway.Api.Domain.Services;

namespace PaymentGateway.Api.Tests.Unit.Domain.Services;

public class PaymentRetrieverTests
{
    private readonly IPaymentsRepository _paymentsRepository = Substitute.For<IPaymentsRepository>();
    private readonly PaymentRetriever _retriever;

    public PaymentRetrieverTests()
    {
        _retriever = new PaymentRetriever(_paymentsRepository);
    }

    [Fact]
    public void FindsTheRecordedPayment()
    {
        // Arrange
        var cardPayment = PaymentRequests.ValidCardPayment();
        var recorded = new Payment(Guid.NewGuid(), PaymentStatus.Authorized, "8877", cardPayment.Expiry, cardPayment.Amount);
        _paymentsRepository.Get(recorded.Id).Returns(recorded);

        // Act
        var payment = _retriever.Find(recorded.Id);

        // Assert
        Assert.Equal(recorded, payment);
    }

    [Fact]
    public void FindsNothingForAnUnknownId()
    {
        // Act
        var payment = _retriever.Find(Guid.NewGuid());

        // Assert
        Assert.Null(payment);
    }
}
