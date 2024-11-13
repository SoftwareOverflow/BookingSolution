using Core.Dto.Appointment;
using Core.Responses;

namespace Core.Interfaces
{
    public interface ITimeBlockService
    {
        public Task<ServiceResult<ICollection<TimeBlockInstanceDto>>> GetTimeBlocksBetweenDates(DateOnly start, DateOnly end);

        public Task<ServiceResult<TimeBlockDto>> CreateOrUpdate(TimeBlockDto dto);

        public Task<ServiceResult<TimeBlockExceptionDto>> CreateOrUpdate(TimeBlockExceptionDto dto, Guid timeBlockGuid);

        public Task<ServiceResult<TimeBlockDto>> GetTimeBlock(Guid guid);

        public Task<ServiceResult<bool>> Delete(TimeBlockInstanceDto dto, bool deleteExceptions);
    }
}
