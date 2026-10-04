using System.Net;
using System.Text.Json.Nodes;

using Microsoft.Extensions.Logging.Abstractions;

using PaymentGateway.Api.Api;
using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;
using PaymentGateway.Api.Infrastructure.Bank;

namespace PaymentGateway.Api.Tests.Unit.Infrastructure.Bank;

public class AcquiringBankClientTests
{
    private const string AuthorizationCode = "0bb07405-6d44-4b50-a14f-7ae0beff13ad";

    private const string AuthorizedAnswer =
        $$"""{ "authorized": true, "authorization_code": "{{AuthorizationCode}}" }""";

    private const string DeclinedAnswer = """{ "authorized": false, "authorization_code": "" }""";

    private static readonly Uri BankBaseAddress = new("http://bank.test");

    private readonly CardPayment _cardPayment = PaymentRequests.Valid().ToCardPayment(PaymentRequests.Today);

    [Fact]
    public async Task PostsThePaymentToTheBankPaymentsEndpoint()
    {
        // Arrange
        var bank = FakeBankHandler.Answering(HttpStatusCode.OK, AuthorizedAnswer);

        // Act
        await ClientFor(bank).AuthorizeAsync(_cardPayment, CancellationToken.None);

        // Assert
        Assert.Equal(HttpMethod.Post, bank.ReceivedMethod);
        Assert.Equal(new Uri("http://bank.test/payments"), bank.ReceivedUri);
    }

    [Fact]
    public async Task SendsTheCardDetailsInTheBankFormat()
    {
        // Arrange
        var bank = FakeBankHandler.Answering(HttpStatusCode.OK, AuthorizedAnswer);
        var expectedBody = JsonNode.Parse("""
            {
              "card_number": "2222405343248877",
              "expiry_date": "04/2027",
              "currency": "GBP",
              "amount": 1050,
              "cvv": "123"
            }
            """);

        // Act
        await ClientFor(bank).AuthorizeAsync(_cardPayment, CancellationToken.None);

        // Assert
        Assert.True(
            JsonNode.DeepEquals(expectedBody, JsonNode.Parse(bank.ReceivedBody ?? "null")),
            $"Unexpected body sent to the bank: {bank.ReceivedBody}");
    }

    [Fact]
    public async Task ReturnsTheAuthorizationCodeWhenTheBankAuthorizes()
    {
        // Arrange
        var bank = FakeBankHandler.Answering(HttpStatusCode.OK, AuthorizedAnswer);

        // Act
        var authorization = await ClientFor(bank).AuthorizeAsync(_cardPayment, CancellationToken.None);

        // Assert
        Assert.Equal(new BankAuthorization.Authorized(AuthorizationCode), authorization);
    }

    [Fact]
    public async Task ReturnsDeclinedWhenTheBankDeclines()
    {
        // Arrange
        var bank = FakeBankHandler.Answering(HttpStatusCode.OK, DeclinedAnswer);

        // Act
        var authorization = await ClientFor(bank).AuthorizeAsync(_cardPayment, CancellationToken.None);

        // Assert
        Assert.IsType<BankAuthorization.Declined>(authorization);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task ReportsTheBankUnavailableWhenItAnswersWithAnError(HttpStatusCode status)
    {
        // Arrange
        var bank = FakeBankHandler.Answering(status);

        // Act & Assert
        await Assert.ThrowsAsync<AcquiringBankUnavailableException>(
            () => ClientFor(bank).AuthorizeAsync(_cardPayment, CancellationToken.None));
    }

    [Fact]
    public async Task ReportsTheBankUnavailableWhenTheNetworkFails()
    {
        // Arrange
        var networkFailure = new HttpRequestException("connection refused");
        var bank = FakeBankHandler.Failing(networkFailure);

        // Act
        var exception = await Assert.ThrowsAsync<AcquiringBankUnavailableException>(
            () => ClientFor(bank).AuthorizeAsync(_cardPayment, CancellationToken.None));

        // Assert
        Assert.Same(networkFailure, exception.InnerException);
    }

    [Fact]
    public async Task ReportsTheBankUnavailableWhenTheCallTimesOut()
    {
        // Arrange
        var timeout = new TaskCanceledException("timed out", new TimeoutException());
        var bank = FakeBankHandler.Failing(timeout);

        // Act
        var exception = await Assert.ThrowsAsync<AcquiringBankUnavailableException>(
            () => ClientFor(bank).AuthorizeAsync(_cardPayment, CancellationToken.None));

        // Assert
        Assert.Same(timeout, exception.InnerException);
    }

    [Fact]
    public async Task LetsTheCallerCancellationThrough()
    {
        // Arrange
        var bank = FakeBankHandler.Answering(HttpStatusCode.OK, AuthorizedAnswer);
        using var cancelledByCaller = new CancellationTokenSource();
        await cancelledByCaller.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => ClientFor(bank).AuthorizeAsync(_cardPayment, cancelledByCaller.Token));
    }

    private static AcquiringBankClient ClientFor(FakeBankHandler bank) =>
        new(new HttpClient(bank) { BaseAddress = BankBaseAddress }, NullLogger<AcquiringBankClient>.Instance);
}
