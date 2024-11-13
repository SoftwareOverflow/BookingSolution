using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DateMustBeAfterAttribute : ValidationAttribute
    {
        private readonly string _targetPropertyName;
        private readonly string _targetPropFullName;
        private readonly string _fullName;

        public DateMustBeAfterAttribute(string targetPropertyName, string targetPropFullName, string fullName)
        {
            _targetPropertyName = targetPropertyName;
            _targetPropFullName = targetPropFullName;
            _fullName = fullName;
        }

        public string GetErrorMessage(string propertyName) =>
            $"{_fullName} must be after {_targetPropFullName}.";

        protected override ValidationResult? IsValid(
            object? value, ValidationContext validationContext)
        {
            var targetValue = validationContext.ObjectInstance
                .GetType()
                .GetProperty(_targetPropertyName)
                ?.GetValue(validationContext.ObjectInstance, null);

            if ((DateTime?)value <= (DateTime?)targetValue)
            {
                var propertyName = validationContext.MemberName ?? string.Empty;
                return new ValidationResult(GetErrorMessage(propertyName), new[] { propertyName });
            }

            return ValidationResult.Success;
        }
    }
}
