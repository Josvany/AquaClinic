using System.ComponentModel.DataAnnotations;

namespace AquaClinic.App.Localization;

public sealed class RequiredUiAttribute(string texto) : RequiredAttribute
{
    public override string FormatErrorMessage(string name) => TextoUi.T(texto);
}

public sealed class EmailAddressUiAttribute(string texto) : ValidationAttribute
{
    private static readonly EmailAddressAttribute Validador = new();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return Validador.IsValid(value)
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }

    public override string FormatErrorMessage(string name) => TextoUi.T(texto);
}

public sealed class MinLengthUiAttribute(int length, string texto) : MinLengthAttribute(length)
{
    public override string FormatErrorMessage(string name) => TextoUi.T(texto);
}

public sealed class CompareUiAttribute(string otherProperty, string texto) : CompareAttribute(otherProperty)
{
    public override string FormatErrorMessage(string name) => TextoUi.T(texto);
}

public sealed class StringLengthUiAttribute(int maximumLength, string texto) : StringLengthAttribute(maximumLength)
{
    public override string FormatErrorMessage(string name) => TextoUi.T(texto);
}

public sealed class RangeUiAttribute(int minimum, int maximum, string texto) : RangeAttribute(minimum, maximum)
{
    public override string FormatErrorMessage(string name) => TextoUi.T(texto);
}
