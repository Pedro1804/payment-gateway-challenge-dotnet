using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;

namespace PaymentGateway.Api.Tests.Integration;

public abstract class PostPaymentTests : IDisposable
{
    private const string PaymentsPath = "/api/payments";

    private readonly IAcquiringBank _bank = Substitute.For<IAcquiringBank>();
    private readonly IPaymentsRepository _payments = Substitute.For<IPaymentsRepository>();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    protected PostPaymentTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services
                .AddSingleton(_bank)
                .AddSingleton(_payments)
                .AddSingleton<TimeProvider>(new FixedTimeProvider(PaymentRequests.Today))));
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    public sealed class Processed : PostPaymentTests
    {
        private const string FullCardNumber = "2222405343248877";

        [Fact]
        public async Task AsAuthorizedWhenTheBankAuthorizesIt()
        {
            // Arrange
            BankAnswers(new BankAuthorization.Authorized());

            // Act
            var response = await PostAsync(ValidBody());

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var payment = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Authorized", payment.GetProperty("status").GetString());
            Assert.Equal("8877", payment.GetProperty("cardNumberLastFour").GetString());
            Assert.Equal(4, payment.GetProperty("expiryMonth").GetInt32());
            Assert.Equal(2027, payment.GetProperty("expiryYear").GetInt32());
            Assert.Equal("GBP", payment.GetProperty("currency").GetString());
            Assert.Equal(1050, payment.GetProperty("amount").GetInt32());
            Assert.Equal($"{PaymentsPath}/{payment.GetProperty("id").GetGuid()}", response.Headers.Location?.AbsolutePath);
        }

        [Fact]
        public async Task AsDeclinedWhenTheBankDeclinesIt()
        {
            // Arrange
            BankAnswers(new BankAuthorization.Declined());

            // Act
            var response = await PostAsync(ValidBody());

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var payment = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Declined", payment.GetProperty("status").GetString());
        }

        [Fact]
        public async Task WithoutExposingTheFullCardNumberNorTheCvv()
        {
            // Arrange
            BankAnswers(new BankAuthorization.Authorized());

            // Act
            var response = await PostAsync(ValidBody());

            // Assert
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("8877", body);
            Assert.DoesNotContain(FullCardNumber, body);
            Assert.DoesNotContain("cvv", body, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class RejectedWithoutCallingTheBank : PostPaymentTests
    {
        [Fact]
        public async Task ReportingOnlyTheFirstInvalidField()
        {
            // Arrange
            var body = ValidBody();
            body["cardNumber"] = "1234";
            body["currency"] = "JPY";

            // Act
            var response = await PostAsync(body);

            // Assert
            var problem = await AssertRejectedAsync(response);
            var invalidFields = problem.GetProperty("errors").EnumerateObject().Select(error => error.Name);
            Assert.Equal(["cardNumber"], invalidFields);
            await AssertBankNotCalledAsync();
        }

        [Theory]
        [InlineData("cvv")]
        [InlineData("amount")]
        public async Task WhenAFieldIsMissing(string field)
        {
            // Arrange
            var body = ValidBody();
            body.Remove(field);

            // Act
            var response = await PostAsync(body);

            // Assert
            await AssertRejectedAsync(response);
            await AssertBankNotCalledAsync();
        }

        [Theory]
        [InlineData("cvv")]
        [InlineData("cardNumber")]
        [InlineData("amount")]
        public async Task WhenAFieldIsNull(string field)
        {
            // Arrange
            var body = ValidBody();
            body[field] = null;

            // Act
            var response = await PostAsync(body);

            // Assert
            await AssertRejectedAsync(response);
            await AssertBankNotCalledAsync();
        }

        [Theory]
        [InlineData("amount", "10.5")]
        [InlineData("expiryMonth", "\"april\"")]
        [InlineData("cvv", "123")]
        public async Task WhenAFieldHasTheWrongType(string field, string json)
        {
            // Arrange
            var body = ValidBody();
            body[field] = JsonNode.Parse(json);

            // Act
            var response = await PostAsync(body);

            // Assert
            await AssertRejectedAsync(response);
            await AssertBankNotCalledAsync();
        }

        [Fact]
        public async Task NorRecordingThePayment()
        {
            // Arrange
            var body = ValidBody();
            body["cvv"] = "12";

            // Act
            await PostAsync(body);

            // Assert
            _payments.DidNotReceiveWithAnyArgs().Add(default!);
        }

        private static async Task<JsonElement> AssertRejectedAsync(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Rejected", problem.GetProperty("paymentStatus").GetString());
            return problem;
        }

        private Task AssertBankNotCalledAsync() =>
            _bank.DidNotReceiveWithAnyArgs().AuthorizeAsync(default!, default);
    }

    public sealed class BankUnavailable : PostPaymentTests
    {
        [Fact]
        public async Task AnswersBadGateway()
        {
            // Arrange
            _bank.AuthorizeAsync(Arg.Any<CardPayment>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new AcquiringBankUnavailableException("Bank answered 503"));

            // Act
            var response = await PostAsync(ValidBody());

            // Assert
            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("Acquiring bank unavailable", problem.GetProperty("title").GetString());
        }
    }

    private static JsonObject ValidBody() =>
        JsonSerializer.SerializeToNode(PaymentRequests.Valid(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!
            .AsObject();

    private void BankAnswers(BankAuthorization authorization) =>
        _bank.AuthorizeAsync(Arg.Any<CardPayment>(), Arg.Any<CancellationToken>()).Returns(authorization);

    private Task<HttpResponseMessage> PostAsync(JsonObject body) => _client.PostAsJsonAsync(PaymentsPath, body);
}
