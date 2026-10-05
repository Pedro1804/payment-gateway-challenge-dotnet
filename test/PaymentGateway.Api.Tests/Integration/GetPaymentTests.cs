using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;

namespace PaymentGateway.Api.Tests.Integration;

public class GetPaymentTests : IDisposable
{
    private const string PaymentsPath = "/api/payments";

    private readonly IAcquiringBank _bank = Substitute.For<IAcquiringBank>();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GetPaymentTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services
                .AddSingleton(_bank)
                .AddSingleton<TimeProvider>(new FixedTimeProvider(PaymentRequests.Today))));
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    public static TheoryData<BankAuthorization> BankAuthorizations =>
        new() { new BankAuthorization.Authorized(), new BankAuthorization.Declined() };

    [Theory]
    [MemberData(nameof(BankAuthorizations))]
    public async Task ReturnsThePaymentAsItWasProcessed(BankAuthorization authorization)
    {
        // Arrange
        BankAnswers(authorization);
        var processed = await PostAsync(PaymentRequests.Valid().CardNumber);

        // Act
        var response = await _client.GetAsync($"{PaymentsPath}/{processed["id"]}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var retrieved = await response.Content.ReadFromJsonAsync<JsonNode>();
        Assert.True(JsonNode.DeepEquals(processed, retrieved), $"Expected {processed} but retrieved {retrieved}");
    }

    [Fact]
    public async Task ExposesOnlyTheLastFourCardDigitsKeepingTheirLeadingZeros()
    {
        // Arrange
        const string cardNumber = "2222405343240123";
        BankAnswers(new BankAuthorization.Authorized());
        var processed = await PostAsync(cardNumber);

        // Act
        var response = await _client.GetAsync($"{PaymentsPath}/{processed["id"]}");

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        var retrieved = JsonNode.Parse(body)!;
        Assert.Equal("0123", retrieved["cardNumberLastFour"]!.GetValue<string>());
        Assert.DoesNotContain(cardNumber, body);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{PaymentsPath}/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private void BankAnswers(BankAuthorization authorization) =>
        _bank.AuthorizeAsync(Arg.Any<CardPayment>(), Arg.Any<CancellationToken>()).Returns(authorization);

    private async Task<JsonNode> PostAsync(string cardNumber)
    {
        var request = PaymentRequests.Valid();
        request.CardNumber = cardNumber;
        var response = await _client.PostAsJsonAsync(PaymentsPath, request);
        return (await response.Content.ReadFromJsonAsync<JsonNode>())!;
    }
}