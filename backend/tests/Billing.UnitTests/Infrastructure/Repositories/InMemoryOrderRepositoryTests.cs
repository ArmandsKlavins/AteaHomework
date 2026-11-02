using Billing.Domain;
using Billing.Infrastructure.Repositories;

namespace Billing.UnitTests.Infrastructure.Repositories;

public class InMemoryOrderRepositoryTests
{
    private readonly InMemoryOrderRepository _repository = new();

    [Fact]
    public async Task TryAddAsync_SameOrderNumberTwice_OnlyFirstSucceeds()
    {
        // Act
        var first = await _repository.TryAddAsync(CreateOrder("ORD-1"), CancellationToken.None);
        var second = await _repository.TryAddAsync(CreateOrder("ORD-1"), CancellationToken.None);

        // Assert
        Assert.True(first);
        Assert.False(second);
    }

    [Fact]
    public async Task TryAddAsync_ConcurrentSubmitsOfOneOrderNumber_ExactlyOneSucceeds()
    {
        // Arrange
        var attempts = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => _repository.TryAddAsync(CreateOrder("ORD-1"), CancellationToken.None)));

        // Act
        var results = await Task.WhenAll(attempts);

        // Assert
        Assert.Single(results, added => added);
    }

    [Fact]
    public async Task RemoveAsync_FreesTheOrderNumber()
    {
        // Arrange
        var order = CreateOrder("ORD-1");
        await _repository.TryAddAsync(order, CancellationToken.None);

        // Act
        await _repository.RemoveAsync(order, CancellationToken.None);

        // Assert
        Assert.True(await _repository.TryAddAsync(CreateOrder("ORD-1"), CancellationToken.None));
    }

    private static Order CreateOrder(string orderNumber)
    {
        return new Order { OrderNumber = orderNumber, UserId = "user-1", PayableAmount = 10m, PaymentGatewayId = "card" };
    }
}
