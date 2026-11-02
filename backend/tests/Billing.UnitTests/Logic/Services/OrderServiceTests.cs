using Billing.Contracts.Orders;
using Billing.Contracts.Receipts;
using Billing.Domain;
using Billing.Infrastructure.PaymentGateways;
using Billing.Infrastructure.PaymentGateways.Models;
using Billing.Infrastructure.Repositories;
using Billing.Logic.Models;
using Billing.Logic.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Billing.UnitTests.Logic.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IReceiptService> _receiptService = new();
    private readonly Mock<IPaymentGatewayResolver> _resolver = new();
    private readonly Mock<IPaymentGateway> _gateway = new();
    private readonly Mock<ILogger<OrderService>> _logger = new();
    private readonly OrderService _service;

    private readonly OrderRequest _request = new()
    {
        OrderNumber = "ORD-1",
        UserId = "user-1",
        PayableAmount = 10.50m,
        PaymentGatewayId = "card"
    };

    public OrderServiceTests()
    {
        _resolver.Setup(resolver => resolver.Resolve("card")).Returns(_gateway.Object);
        _orderRepository.Setup(repository => repository.TryAddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _service = new OrderService(_orderRepository.Object, _receiptService.Object, _resolver.Object, _logger.Object);
    }

    [Fact]
    public async Task ProcessOrderAsync_UnknownGateway_ReturnsUnknownGatewayWithoutClaimingTheOrderNumber()
    {
        // Arrange
        var request = _request with { PaymentGatewayId = "paypal" };

        // Act
        var result = await _service.ProcessOrderAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(OrderResultStatus.UnknownGateway, result.Status);
        _orderRepository.Verify(repository => repository.TryAddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOrderAsync_DuplicateOrderNumber_ReturnsDuplicateWithoutCharging()
    {
        // Arrange
        _orderRepository.Setup(repository => repository.TryAddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.ProcessOrderAsync(_request, CancellationToken.None);

        // Assert
        Assert.Equal(OrderResultStatus.Duplicate, result.Status);
        _gateway.Verify(gateway => gateway.ChargeAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOrderAsync_DeclinedPayment_ReturnsReasonAndFreesTheOrderNumber()
    {
        // Arrange
        SetupCharge(new PaymentResult { IsApproved = false, DeclineReason = "Limit exceeded." });

        // Act
        var result = await _service.ProcessOrderAsync(_request, CancellationToken.None);

        // Assert
        Assert.Equal(OrderResultStatus.Declined, result.Status);
        Assert.Equal("Limit exceeded.", result.DeclineReason);
        _orderRepository.Verify(repository => repository.RemoveAsync(It.Is<Order>(order => order.OrderNumber == "ORD-1"), It.IsAny<CancellationToken>()), Times.Once);
        _receiptService.Verify(service => service.CreateReceiptAsync(It.IsAny<Order>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOrderAsync_ChargeThrows_LogsAndRethrowsAndKeepsTheOrder()
    {
        // Arrange
        var exception = new TimeoutException();
        _gateway.Setup(gateway => gateway.ChargeAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<TimeoutException>(() => _service.ProcessOrderAsync(_request, CancellationToken.None));
        _logger.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _orderRepository.Verify(repository => repository.RemoveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOrderAsync_ApprovedPayment_ReturnsReceiptAndKeepsTheOrder()
    {
        // Arrange
        var receipt = new ReceiptResponse { OrderNumber = "ORD-1", Amount = 10.50m, PaidAt = DateTimeOffset.UnixEpoch, TransactionId = "tx-1" };
        SetupCharge(new PaymentResult { IsApproved = true, TransactionId = "tx-1" });
        _receiptService
            .Setup(service => service.CreateReceiptAsync(It.Is<Order>(order => order.OrderNumber == "ORD-1" && order.PayableAmount == 10.50m), "tx-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(receipt);

        // Act
        var result = await _service.ProcessOrderAsync(_request, CancellationToken.None);

        // Assert
        Assert.Equal(OrderResultStatus.Processed, result.Status);
        Assert.Same(receipt, result.Receipt);
        _orderRepository.Verify(repository => repository.RemoveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessOrderAsync_ApprovedPayment_ChargesAndCreatesReceiptWithoutTheRequestsCancellationToken()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        SetupCharge(new PaymentResult { IsApproved = true, TransactionId = "tx-1" });

        // Act
        await _service.ProcessOrderAsync(_request, cancellation.Token);

        // Assert
        _gateway.Verify(gateway => gateway.ChargeAsync(It.IsAny<Order>(), CancellationToken.None), Times.Once);
        _receiptService.Verify(service => service.CreateReceiptAsync(It.IsAny<Order>(), "tx-1", CancellationToken.None), Times.Once);
    }

    private void SetupCharge(PaymentResult result)
    {
        _gateway.Setup(gateway => gateway.ChargeAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
    }
}
