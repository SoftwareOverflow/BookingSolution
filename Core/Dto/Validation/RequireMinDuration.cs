using System.ComponentModel.DataAnnotations;

namespace Core.Dto.Validation
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    internal class RequireMinDuration : ValidationAttribute
    {
        private double MinMinutes;

        public RequireMinDuration(double minMinutes, string errorMessage)
        {
            MinMinutes = minMinutes;
            ErrorMessage = errorMessage;
        }

        public override bool IsValid(object? value)
        {
            if (value == null || !(value is TimeSpan))
            {
                return false;
            }

            var span = (TimeSpan) value!;

            return span.TotalMinutes >= MinMinutes;
        }
    }
}
