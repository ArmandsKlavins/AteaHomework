using System.Net;
using System.Net.Http.Json;
using Billing.Contracts.Receipts;
using Billing.Infrastructure.PaymentGateways;
using Billing.IntegrationTests.PaymentGateways;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Billing.IntegrationTests.Controllers;

public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OrdersControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProcessOrder_ApprovedPayment_Returns201WithReceiptThatCanBeFetchedAgain()
    {
        // Arrange
        var orderNumber = NewOrderNumber();

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", CreateOrder(orderNumber, 10.50m, "card"));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var receipt = await response.Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.Equal(orderNumber, receipt!.OrderNumber);
        Assert.Equal(10.50m, receipt.Amount);
        Assert.StartsWith("card_", receipt.TransactionId);
        Assert.Equal(receipt, await _client.GetFromJsonAsync<ReceiptResponse>(response.Headers.Location));
    }

    [Fact]
    public async Task ProcessOrder_SameOrderTwice_Returns409ProblemDetails()
    {
        // Arrange
        var order = CreateOrder(NewOrderNumber(), 10.50m, "card");
        await _client.PostAsJsonAsync("/api/orders", order);

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", order);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ProcessOrder_DeclinedPayment_Returns402AndTheOrderCanBeRetried()
    {
        // Arrange
        var orderNumber = NewOrderNumber();

        // Act
        var declined = await _client.PostAsJsonAsync("/api/orders", CreateOrder(orderNumber, 2000m, "bank"));
        var retried = await _client.PostAsJsonAsync("/api/orders", CreateOrder(orderNumber, 2000m, "card"));

        // Assert
        var problem = await AssertProblemAsync(declined, HttpStatusCode.PaymentRequired);
        Assert.Equal("Payment declined", problem.Title);
        Assert.Equal(HttpStatusCode.Created, retried.StatusCode);
    }

    [Fact]
    public async Task ProcessOrder_UnknownGateway_Returns400ProblemDetails()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", CreateOrder(NewOrderNumber(), 10.50m, "paypal"));

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProcessOrder_InvalidRequest_Returns400WithValidationErrors()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", CreateOrder(NewOrderNumber(), 0m, "card"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotEmpty(problem!.Errors);
    }

    [Fact]
    public async Task ProcessOrder_ChargeFails_Returns500ProblemDetailsAndBlocksTheOrderNumber()
    {
        // Arrange
        var client = _factory
            .WithWebHostBuilder(builder => builder
                .UseEnvironment("Production")
                .ConfigureTestServices(services => services.AddSingleton<IPaymentGateway, FailingPaymentGateway>()))
            .CreateClient();
        var order = CreateOrder(NewOrderNumber(), 10.50m, "failing");

        // Act
        var failed = await client.PostAsJsonAsync("/api/orders", order);
        var retried = await client.PostAsJsonAsync("/api/orders", order);

        // Assert
        var problem = await AssertProblemAsync(failed, HttpStatusCode.InternalServerError);
        Assert.Null(problem.Detail);
        await AssertProblemAsync(retried, HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetReceipt_UnknownOrder_Returns404ProblemDetails()
    {
        // Act
        var response = await _client.GetAsync($"/api/orders/{NewOrderNumber()}/receipt");

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound);
    }

    private static string NewOrderNumber()
    {
        // The app is shared by the tests in this class, so each test uses its own order number
        return $"ORD-{Guid.NewGuid():N}";
    }

    private static object CreateOrder(string orderNumber, decimal amount, string gatewayId)
    {
        return new { orderNumber, userId = "user-1", payableAmount = amount, paymentGatewayId = gatewayId };
    }

    private static async Task<ProblemDetails> AssertProblemAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal((int)expected, problem!.Status);
        return problem;
    }
}
