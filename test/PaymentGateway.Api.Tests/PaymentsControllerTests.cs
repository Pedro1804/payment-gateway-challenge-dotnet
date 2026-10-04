using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Api;
using PaymentGateway.Api.Domain.Model;
using PaymentGateway.Api.Domain.Ports;
using PaymentGateway.Api.Infrastructure.Persistence;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var cardPayment = PaymentRequests.Valid().ToCardPayment(PaymentRequests.Today);
        var decision = new PaymentDecision(Guid.NewGuid(), PaymentStatus.Authorized, cardPayment);

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(decision);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => services
                .AddSingleton<IPaymentsRepository>(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/payments/{decision.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
