using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Validation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    internal class RequireMinDuration : ValidationAttribute
    {
        private double _minMinutes;

        public RequireMinDuration(double minMinutes, string errorMessage)
        {
            _minMinutes = minMinutes;
            ErrorMessage = errorMessage;
        }

        public override bool IsValid(object? value)
        {
            if (value == null || !(value is TimeSpan))
            {
                return false;
            }

            var span = (TimeSpan) value!;

            return span.TotalMinutes >= _minMinutes;
        }
    }
}
