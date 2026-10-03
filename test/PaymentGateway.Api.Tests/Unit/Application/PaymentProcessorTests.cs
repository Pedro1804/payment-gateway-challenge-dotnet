using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PaymentGateway.Api.Application;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Unit.Application;

public class PaymentProcessorTests
{
    private readonly IAcquiringBank _bank = Substitute.For<IAcquiringBank>();
    private readonly CardPayment _cardPayment = PaymentRequests.Valid().ToCardPayment(PaymentRequests.Today);
    private readonly PaymentProcessor _processor;

    public PaymentProcessorTests()
    {
        _processor = new PaymentProcessor(_bank, NullLogger<PaymentProcessor>.Instance);
    }

    [Fact]
    public async Task AuthorizesThePaymentWhenTheBankAuthorizesIt()
    {
        // Arrange
        BankAnswers(new BankAuthorization(true, "auth-code"));

        // Act
        var decision = await _processor.ProcessAsync(_cardPayment, CancellationToken.None);

        // Assert
        Assert.Equal(PaymentStatus.Authorized, decision.Status);
        Assert.Equal("auth-code", decision.AuthorizationCode);
        Assert.Equal(_cardPayment, decision.CardPayment);
        Assert.NotEqual(Guid.Empty, decision.Id);
    }

    [Fact]
    public async Task DeclinesThePaymentWhenTheBankDeclinesIt()
    {
        // Arrange
        BankAnswers(new BankAuthorization(false, null));

        // Act
        var decision = await _processor.ProcessAsync(_cardPayment, CancellationToken.None);

        // Assert
        Assert.Equal(PaymentStatus.Declined, decision.Status);
        Assert.Null(decision.AuthorizationCode);
        Assert.Equal(_cardPayment, decision.CardPayment);
    }

    [Fact]
    public async Task SubmitsTheReceivedCardPaymentToTheBank()
    {
        // Arrange
        BankAnswers(new BankAuthorization(true, "auth-code"));

        // Act
        await _processor.ProcessAsync(_cardPayment, CancellationToken.None);

        // Assert
        await _bank.Received(1).AuthorizeAsync(_cardPayment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GivesEachPaymentItsOwnId()
    {
        // Arrange
        BankAnswers(new BankAuthorization(true, "auth-code"));

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

    private void BankAnswers(BankAuthorization authorization) =>
        _bank.AuthorizeAsync(Arg.Any<CardPayment>(), Arg.Any<CancellationToken>()).Returns(authorization);
}
