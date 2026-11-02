using Billing.Domain;
using Billing.Infrastructure.Repositories;
using Billing.Logic.Services;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Billing.UnitTests.Logic.Services;

public class ReceiptServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 2, 10, 15, 30, TimeSpan.Zero);

    private readonly Mock<IReceiptRepository> _receiptRepository = new();
    private readonly ReceiptService _service;

    public ReceiptServiceTests()
    {
        _service = new ReceiptService(_receiptRepository.Object, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task CreateReceiptAsync_StoresAndReturnsReceiptForTheOrder()
    {
        // Arrange
        var order = new Order { OrderNumber = "ORD-1", UserId = "user-1", PayableAmount = 10.50m, PaymentGatewayId = "card" };

        // Act
        var response = await _service.CreateReceiptAsync(order, "tx-1", CancellationToken.None);

        // Assert
        Assert.Equal("ORD-1", response.OrderNumber);
        Assert.Equal(10.50m, response.Amount);
        Assert.Equal(Now, response.PaidAt);
        Assert.Equal("tx-1", response.TransactionId);
        _receiptRepository.Verify(repository => repository.AddAsync(
            It.Is<Receipt>(receipt => receipt.OrderNumber == "ORD-1" && receipt.Amount == 10.50m && receipt.PaidAt == Now && receipt.TransactionId == "tx-1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReceiptAsync_PaidOrder_ReturnsReceipt()
    {
        // Arrange
        var receipt = new Receipt { OrderNumber = "ORD-1", Amount = 10.50m, PaidAt = Now, TransactionId = "tx-1" };
        _receiptRepository.Setup(repository => repository.GetAsync("ORD-1", It.IsAny<CancellationToken>())).ReturnsAsync(receipt);

        // Act
        var response = await _service.GetReceiptAsync("ORD-1", CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("tx-1", response.TransactionId);
    }

    [Fact]
    public async Task GetReceiptAsync_UnpaidOrder_ReturnsNull()
    {
        // Act
        var response = await _service.GetReceiptAsync("ORD-1", CancellationToken.None);

        // Assert
        Assert.Null(response);
    }
}
