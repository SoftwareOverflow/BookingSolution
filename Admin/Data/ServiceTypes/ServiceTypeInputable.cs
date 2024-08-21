using Core.Dto;
using System.ComponentModel.DataAnnotations;

namespace Admin.Data.ServiceTypes
{
    public record ServiceTypeInputable
    {
        [ValidateComplexType]
        public ServiceTypeDto Service { get; private set; }

        public ServiceTypeInputable(ServiceTypeDto dto)
        {
            Service = dto;
        }

        public int DurationHours
        {
            get => Service.Duration.Hours;
            set
            {
                Service.Duration = new(value, Service.Duration.Minutes, 0);
            }
        }

        public int DurationMinutes
        {
            get => Service.Duration.Minutes;
            set
            {
                Service.Duration = new(Service.Duration.Hours, value, 0);
            }
        }
    }
}
