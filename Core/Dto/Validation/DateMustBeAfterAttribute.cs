using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DateMustBeAfterAttribute : ValidationAttribute
    {
        private readonly string TargetPropertyName;

        public DateMustBeAfterAttribute(string targetPropertyName)
            => TargetPropertyName = targetPropertyName;

        public string GetErrorMessage(string propertyName) =>
            $"{propertyName} must be after {TargetPropertyName}.";

        protected override ValidationResult? IsValid(
            object? value, ValidationContext validationContext)
        {
            var targetValue = validationContext.ObjectInstance
                .GetType()
                .GetProperty(TargetPropertyName)
                ?.GetValue(validationContext.ObjectInstance, null);

            if ((DateTime?)value < (DateTime?)targetValue)
            {
                var propertyName = validationContext.MemberName ?? string.Empty;
                return new ValidationResult(GetErrorMessage(propertyName), new[] { propertyName });
            }

            return ValidationResult.Success;
        }
    }
}
