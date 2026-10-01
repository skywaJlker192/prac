using System.ComponentModel.DataAnnotations;

namespace TaskFlowApi.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class FutureDateAttribute : ValidationAttribute
{
    public FutureDateAttribute() : base("DueDate должен быть в будущем")
    {
    }

    public override bool IsValid(object? value)
    {
        return value is not DateTime date || date.ToUniversalTime() > DateTime.UtcNow;
    }
}
