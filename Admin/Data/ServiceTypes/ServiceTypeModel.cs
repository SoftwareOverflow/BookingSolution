using Core.Dto;
using System.ComponentModel.DataAnnotations;

namespace Admin.Data.ServiceTypes
{
    internal class ServiceTypeModel
    {
        [ValidateComplexType]
        public ServiceTypeDto Service { get; private set; }

        public RepeaterModel Repeater { get; set; }

        public ServiceTypeModel(ServiceTypeDto dto)
        {
            Service = dto;
            Repeater = new RepeaterModel(dto.RepeatType, dto.Repeats);
        }

        public ServiceTypeDto MapToDto()
        {
            Service.Repeats = Repeater.MapToDto();
            Service.RepeatType = Repeater.RepeatType!.Value; // TODO maybe logging if we hit a Service without a RepeatType while mapping - it should not be possible.

            return Service;
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
