using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

namespace PaymentGateway.Api.Tests.Integration;

public class HealthTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task ReportsHealthyWhileTheGatewayIsRunning()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
