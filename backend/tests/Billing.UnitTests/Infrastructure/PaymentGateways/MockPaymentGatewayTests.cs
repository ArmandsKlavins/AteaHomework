using Billing.Domain;
using Billing.Infrastructure.PaymentGateways.Mocks;

namespace Billing.UnitTests.Infrastructure.PaymentGateways;

public class MockPaymentGatewayTests
{
    [Fact]
    public async Task Card_ApprovesWithTransactionId()
    {
        // Arrange
        var order = CreateOrder(5000m);

        // Act
        var result = await new CardPaymentGateway().ChargeAsync(order, CancellationToken.None);

        // Assert
        Assert.True(result.IsApproved);
        Assert.StartsWith("card_", result.TransactionId);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1000)]
    public async Task Bank_UpToTheLimit_Approves(decimal amount)
    {
        // Arrange
        var order = CreateOrder(amount);

        // Act
        var result = await new BankPaymentGateway().ChargeAsync(order, CancellationToken.None);

        // Assert
        Assert.True(result.IsApproved);
        Assert.StartsWith("bank_", result.TransactionId);
    }

    [Fact]
    public async Task Bank_OverTheLimit_DeclinesWithReason()
    {
        // Arrange
        var order = CreateOrder(1000.01m);

        // Act
        var result = await new BankPaymentGateway().ChargeAsync(order, CancellationToken.None);

        // Assert
        Assert.False(result.IsApproved);
        Assert.Null(result.TransactionId);
        Assert.False(string.IsNullOrEmpty(result.DeclineReason));
    }

    private static Order CreateOrder(decimal amount)
    {
        return new Order { OrderNumber = "ORD-1", UserId = "user-1", PayableAmount = amount, PaymentGatewayId = "any" };
    }
}
