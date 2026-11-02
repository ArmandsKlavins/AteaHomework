using Billing.Domain;
using Billing.Infrastructure.Repositories;

namespace Billing.UnitTests.Infrastructure.Repositories;

public class InMemoryReceiptRepositoryTests
{
    private readonly InMemoryReceiptRepository _repository = new();

    [Fact]
    public async Task GetAsync_StoredReceipt_ReturnsItByOrderNumber()
    {
        // Arrange
        var receipt = new Receipt { OrderNumber = "ORD-1", Amount = 10m, PaidAt = DateTimeOffset.UnixEpoch, TransactionId = "tx-1" };
        await _repository.AddAsync(receipt, CancellationToken.None);

        // Act
        var found = await _repository.GetAsync("ORD-1", CancellationToken.None);

        // Assert
        Assert.Same(receipt, found);
    }

    [Fact]
    public async Task GetAsync_UnknownOrderNumber_ReturnsNull()
    {
        // Act
        var found = await _repository.GetAsync("ORD-1", CancellationToken.None);

        // Assert
        Assert.Null(found);
    }
}
