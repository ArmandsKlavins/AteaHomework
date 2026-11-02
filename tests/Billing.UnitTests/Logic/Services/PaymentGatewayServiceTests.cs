using Billing.Infrastructure.PaymentGateways;
using Billing.Logic.Services;
using Moq;

namespace Billing.UnitTests.Logic.Services;

public class PaymentGatewayServiceTests
{
    [Fact]
    public void GetPaymentGateways_ReturnsTheIdOfEveryRegisteredGateway()
    {
        // Arrange
        var card = new Mock<IPaymentGateway>();
        card.Setup(gateway => gateway.Id).Returns("card");
        var bank = new Mock<IPaymentGateway>();
        bank.Setup(gateway => gateway.Id).Returns("bank");
        var resolver = new Mock<IPaymentGatewayResolver>();
        resolver.Setup(r => r.GetAll()).Returns([card.Object, bank.Object]);
        var service = new PaymentGatewayService(resolver.Object);

        // Act
        var gateways = service.GetPaymentGateways();

        // Assert
        Assert.Equal(["bank", "card"], gateways.Select(gateway => gateway.Id).Order());
    }
}
