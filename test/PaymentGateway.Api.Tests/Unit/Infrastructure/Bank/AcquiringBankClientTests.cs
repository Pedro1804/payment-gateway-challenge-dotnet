using System.Net;
using System.Text.Json.Nodes;

using Microsoft.Extensions.Logging.Abstractions;

using PaymentGateway.Api.Domain.Exceptions;
using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Infrastructure.Bank;

namespace PaymentGateway.Api.Tests.Unit.Infrastructure.Bank;

public class AcquiringBankClientTests
{
    private const string AuthorizedAnswer =
        """{ "authorized": true, "authorization_code": "0bb07405-6d44-4b50-a14f-7ae0beff13ad" }""";

    private const string DeclinedAnswer = """{ "authorized": false, "authorization_code": "" }""";

    private static readonly Uri BankBaseAddress = new("http://bank.test");

    private readonly FakeBankHandler _bank = new();
    private readonly AcquiringBankClient _client;

    public AcquiringBankClientTests()
    {
        _client = new AcquiringBankClient(
            new HttpClient(_bank) { BaseAddress = BankBaseAddress }, NullLogger<AcquiringBankClient>.Instance);
    }

    [Fact]
    public async Task PostsThePaymentToTheBankPaymentsEndpoint()
    {
        // Arrange
        _bank.Answers(HttpStatusCode.OK, AuthorizedAnswer);

        // Act
        await _client.AuthorizeAsync(
            PaymentRequests.ValidCardPayment(), CancellationToken.None);

        // Assert
        Assert.Equal(HttpMethod.Post, _bank.ReceivedMethod);
        Assert.Equal(new Uri("http://bank.test/payments"), _bank.ReceivedUri);
    }

    [Fact]
    public async Task SendsTheCardDetailsInTheBankFormat()
    {
        // Arrange
        _bank.Answers(HttpStatusCode.OK, AuthorizedAnswer);
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
        await _client.AuthorizeAsync(
            PaymentRequests.ValidCardPayment(), CancellationToken.None);

        // Assert
        Assert.True(
            JsonNode.DeepEquals(expectedBody, JsonNode.Parse(_bank.ReceivedBody ?? "null")),
            $"Unexpected body sent to the bank: {_bank.ReceivedBody}");
    }

    [Fact]
    public async Task ReturnsAuthorizedWhenTheBankAuthorizes()
    {
        // Arrange
        _bank.Answers(HttpStatusCode.OK, AuthorizedAnswer);

        // Act
        var authorization = await _client.AuthorizeAsync(
            PaymentRequests.ValidCardPayment(), CancellationToken.None);

        // Assert
        Assert.IsType<BankAuthorization.Authorized>(authorization);
    }

    [Fact]
    public async Task ReturnsDeclinedWhenTheBankDeclines()
    {
        // Arrange
        _bank.Answers(HttpStatusCode.OK, DeclinedAnswer);

        // Act
        var authorization = await _client.AuthorizeAsync(
            PaymentRequests.ValidCardPayment(), CancellationToken.None);

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
        _bank.Answers(status);

        // Act & Assert
        await Assert.ThrowsAsync<AcquiringBankUnavailableException>(
            () => _client.AuthorizeAsync(
                PaymentRequests.ValidCardPayment(), CancellationToken.None));
    }

    [Fact]
    public async Task ReportsTheBankUnavailableWhenTheNetworkFails()
    {
        // Arrange
        var networkFailure = new HttpRequestException("connection refused");
        _bank.Fails(networkFailure);

        // Act
        var exception = await Assert.ThrowsAsync<AcquiringBankUnavailableException>(
            () => _client.AuthorizeAsync(
                PaymentRequests.ValidCardPayment(), CancellationToken.None));

        // Assert
        Assert.Same(networkFailure, exception.InnerException);
    }

    [Fact]
    public async Task ReportsTheBankUnavailableWhenTheCallTimesOut()
    {
        // Arrange
        var timeout = new TaskCanceledException("timed out", new TimeoutException());
        _bank.Fails(timeout);

        // Act
        var exception = await Assert.ThrowsAsync<AcquiringBankUnavailableException>(
            () => _client.AuthorizeAsync(
                PaymentRequests.ValidCardPayment(), CancellationToken.None));

        // Assert
        Assert.Same(timeout, exception.InnerException);
    }

    [Fact]
    public async Task LetsTheCallerCancellationThrough()
    {
        // Arrange
        _bank.Answers(HttpStatusCode.OK, AuthorizedAnswer);
        using var cancelledByCaller = new CancellationTokenSource();
        await cancelledByCaller.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _client.AuthorizeAsync(
                PaymentRequests.ValidCardPayment(), cancelledByCaller.Token));
    }
}
