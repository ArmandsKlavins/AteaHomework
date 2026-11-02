using System.ComponentModel.DataAnnotations;

namespace Billing.Contracts.Validation;

/// <summary>
/// Validates that a decimal has at most the given number of decimal places (trailing zeros don't count).
/// Checks the number itself rather than its text, so it doesn't depend on the culture's decimal separator.
/// </summary>
public class MaxDecimalPlacesAttribute : ValidationAttribute
{
    private readonly int _decimalPlaces;

    /// <summary>
    /// Creates the attribute.
    /// </summary>
    /// <param name="decimalPlaces">The maximum number of decimal places allowed.</param>
    public MaxDecimalPlacesAttribute(int decimalPlaces)
        : base($"The field {{0}} must have at most {decimalPlaces} decimal places.")
    {
        _decimalPlaces = decimalPlaces;
    }

    /// <inheritdoc />
    public override bool IsValid(object? value)
    {
        if (value is not decimal amount)
        {
            return true;
        }

        return decimal.Round(amount, _decimalPlaces) == amount;
    }
}
