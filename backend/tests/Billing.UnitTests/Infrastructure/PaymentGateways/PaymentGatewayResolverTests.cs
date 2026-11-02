using Billing.Infrastructure.PaymentGateways;
using Billing.Infrastructure.PaymentGateways.Mocks;
using Moq;

namespace Billing.UnitTests.Infrastructure.PaymentGateways;

public class PaymentGatewayResolverTests
{
    private readonly PaymentGatewayResolver _resolver = new([new CardPaymentGateway(), new BankPaymentGateway()]);

    [Theory]
    [InlineData("card")]
    [InlineData("CARD")]
    [InlineData("Card")]
    public void Resolve_KnownIdInAnyCase_ReturnsGateway(string gatewayId)
    {
        // Act
        var gateway = _resolver.Resolve(gatewayId);

        // Assert
        Assert.IsType<CardPaymentGateway>(gateway);
    }

    [Fact]
    public void Resolve_UnknownId_ReturnsNull()
    {
        // Act
        var gateway = _resolver.Resolve("paypal");

        // Assert
        Assert.Null(gateway);
    }

    [Fact]
    public void GetAll_ReturnsEveryGateway()
    {
        // Act
        var gateways = _resolver.GetAll();

        // Assert
        Assert.Equal(2, gateways.Count);
    }

    [Fact]
    public void Constructor_DuplicateIdsIgnoringCase_Throws()
    {
        // Arrange
        var other = new Mock<IPaymentGateway>();
        other.Setup(gateway => gateway.Id).Returns("CARD");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PaymentGatewayResolver([new CardPaymentGateway(), other.Object]));
    }

    [Fact]
    public void MockGatewayIds_AreThePublicContract()
    {
        // Act & Assert
        Assert.Equal("card", new CardPaymentGateway().Id);
        Assert.Equal("bank", new BankPaymentGateway().Id);
    }
}
