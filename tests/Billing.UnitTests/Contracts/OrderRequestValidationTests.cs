using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Billing.Contracts.Orders;

namespace Billing.UnitTests.Contracts;

public class OrderRequestValidationTests
{
    private readonly OrderRequest _valid = new()
    {
        OrderNumber = "ORD-1_a",
        UserId = "user-1",
        PayableAmount = 10.50m,
        PaymentGatewayId = "card"
    };

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        // Act
        var errors = Validate(_valid);

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("10.5")]
    [InlineData("10.550")]
    public void PayableAmount_AtMostTwoDecimals_IsValid(string amount)
    {
        // Arrange
        var request = _valid with { PayableAmount = decimal.Parse(amount, CultureInfo.InvariantCulture) };

        // Act
        var errors = Validate(request);

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("10.555")]
    public void PayableAmount_ZeroNegativeOrTooPrecise_IsInvalid(string amount)
    {
        // Arrange
        var request = _valid with { PayableAmount = decimal.Parse(amount, CultureInfo.InvariantCulture) };

        // Act
        var errors = Validate(request);

        // Assert
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(OrderRequest.PayableAmount)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("A/1")]
    [InlineData("A 1")]
    [InlineData("A?1")]
    public void OrderNumber_EmptyOrUnsafeForUrls_IsInvalid(string orderNumber)
    {
        // Arrange
        var request = _valid with { OrderNumber = orderNumber };

        // Act
        var errors = Validate(request);

        // Assert
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(OrderRequest.OrderNumber)));
    }

    [Fact]
    public void OrderNumber_Over50Characters_IsInvalid()
    {
        // Arrange
        var request = _valid with { OrderNumber = new string('a', 51) };

        // Act
        var errors = Validate(request);

        // Assert
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(OrderRequest.OrderNumber)));
    }

    [Fact]
    public void Description_Over500Characters_IsInvalid()
    {
        // Arrange
        var request = _valid with { Description = new string('a', 501) };

        // Act
        var errors = Validate(request);

        // Assert
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(OrderRequest.Description)));
    }

    private static List<ValidationResult> Validate(OrderRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }
}
