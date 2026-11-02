using System.Net.Http.Json;
using Billing.Contracts.PaymentGateways;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Billing.IntegrationTests.Controllers;

public class PaymentGatewaysControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PaymentGatewaysControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPaymentGateways_ReturnsTheRegisteredGateways()
    {
        // Act
        var gateways = await _client.GetFromJsonAsync<List<PaymentGatewayResponse>>("/api/payment-gateways");

        // Assert
        Assert.Equal(["bank", "card"], gateways!.Select(gateway => gateway.Id).Order());
    }
}
