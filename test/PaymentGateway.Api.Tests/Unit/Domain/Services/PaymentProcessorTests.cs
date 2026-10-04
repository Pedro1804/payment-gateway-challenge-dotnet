using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PaymentGateway.Api.Api;
using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;
using PaymentGateway.Api.Domain.Services;

namespace PaymentGateway.Api.Tests.Unit.Domain.Services;

public class PaymentProcessorTests
{
    private static readonly BankAuthorization.Authorized AuthorizedByBank = new();
    private static readonly BankAuthorization.Declined DeclinedByBank = new();

    private readonly IAcquiringBank _bank = Substitute.For<IAcquiringBank>();
    private readonly IPaymentsRepository _payments = Substitute.For<IPaymentsRepository>();
    private readonly CardPayment _cardPayment = PaymentRequests.Valid().ToCardPayment(PaymentRequests.Today);
    private readonly PaymentProcessor _processor;

    public PaymentProcessorTests()
    {
        _processor = new PaymentProcessor(_bank, _payments, NullLogger<PaymentProcessor>.Instance);
    }

    [Fact]
    public async Task AuthorizesThePaymentWhenTheBankAuthorizesIt()
    {
        // Arrange
        BankAnswers(AuthorizedByBank);

        // Act
        var decision = await _processor.ProcessAsync(_cardPayment, CancellationToken.None);

        // Assert
        Assert.Equal(PaymentStatus.Authorized, decision.Status);
        Assert.Equal(_cardPayment, decision.CardPayment);
        Assert.NotEqual(Guid.Empty, decision.Id);
    }

    [Fact]
    public async Task DeclinesThePaymentWhenTheBankDeclinesIt()
    {
        // Arrange
        BankAnswers(DeclinedByBank);

        // Act
        var decision = await _processor.ProcessAsync(_cardPayment, CancellationToken.None);

        // Assert
        Assert.Equal(PaymentStatus.Declined, decision.Status);
        Assert.Equal(_cardPayment, decision.CardPayment);
    }

    [Fact]
    public async Task SubmitsTheReceivedCardPaymentToTheBank()
    {
        // Arrange
        BankAnswers(AuthorizedByBank);

        // Act
        await _processor.ProcessAsync(_cardPayment, CancellationToken.None);

        // Assert
        await _bank.Received(1).AuthorizeAsync(_cardPayment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivesEachPaymentItsOwnId()
    {
        // Arrange
        BankAnswers(AuthorizedByBank);

        // Act
        var first = await _processor.ProcessAsync(_cardPayment, CancellationToken.None);
        var second = await _processor.ProcessAsync(_cardPayment, CancellationToken.None);

        // Assert
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task PropagatesTheBankUnavailability()
    {
        // Arrange
        _bank.AuthorizeAsync(Arg.Any<CardPayment>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AcquiringBankUnavailableException("bank down"));

        // Act & Assert
        await Assert.ThrowsAsync<AcquiringBankUnavailableException>(
            () => _processor.ProcessAsync(_cardPayment, CancellationToken.None));
    }

    [Theory]
    [MemberData(nameof(BankAuthorizations))]
    public async Task RecordsThePaymentOnceTheBankHasAnswered(BankAuthorization authorization)
    {
        // Arrange
        BankAnswers(authorization);

        // Act
        var decision = await _processor.ProcessAsync(_cardPayment, CancellationToken.None);

        // Assert
        _payments.Received(1).Add(decision);
    }

    [Fact]
    public async Task RecordsNothingWhenTheBankIsUnavailable()
    {
        // Arrange
        _bank.AuthorizeAsync(Arg.Any<CardPayment>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AcquiringBankUnavailableException("bank down"));

        // Act
        await Assert.ThrowsAsync<AcquiringBankUnavailableException>(
            () => _processor.ProcessAsync(_cardPayment, CancellationToken.None));

        // Assert
        _payments.DidNotReceiveWithAnyArgs().Add(default!);
    }

    public static TheoryData<BankAuthorization> BankAuthorizations => new() { AuthorizedByBank, DeclinedByBank };

    private void BankAnswers(BankAuthorization authorization) =>
        _bank.AuthorizeAsync(Arg.Any<CardPayment>(), Arg.Any<CancellationToken>()).Returns(authorization);
}
