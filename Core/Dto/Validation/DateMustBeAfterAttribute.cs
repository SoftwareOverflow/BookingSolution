using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DateMustBeAfterAttribute : ValidationAttribute
    {
        private readonly string _targetPropertyName;

        public DateMustBeAfterAttribute(string targetPropertyName)
            => _targetPropertyName = targetPropertyName;

        public string GetErrorMessage(string propertyName) =>
            $"{propertyName} must be after {_targetPropertyName}.";

        protected override ValidationResult? IsValid(
            object? value, ValidationContext validationContext)
        {
            var targetValue = validationContext.ObjectInstance
                .GetType()
                .GetProperty(_targetPropertyName)
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
