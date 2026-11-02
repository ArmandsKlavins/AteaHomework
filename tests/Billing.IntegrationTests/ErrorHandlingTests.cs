using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Billing.IntegrationTests;

public class ErrorHandlingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ErrorHandlingTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UnknownRoute_Returns404ProblemDetails()
    {
        // Act
        var response = await _client.GetAsync("/api/unknown");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
