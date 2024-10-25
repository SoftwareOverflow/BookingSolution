using Core.Dto.Appointment;
using Core.Responses;

namespace Core.Interfaces
{
    public interface ITimeBlockService
    {
        public Task<ServiceResult<ICollection<TimeBlockInstanceDto>>> GetTimeBlocksBetweenDates(DateOnly start, DateOnly end);

        public Task<ServiceResult<TimeBlockDto>> CreateOrUpdateTimeBlock(TimeBlockDto dto);

        public Task<ServiceResult<TimeBlockDto>> GetTimeBlock(Guid guid);
    }
}
