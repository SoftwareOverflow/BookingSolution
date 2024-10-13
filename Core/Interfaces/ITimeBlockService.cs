using Core.Dto.Appointment;
using Core.Responses;

namespace Core.Interfaces
{
    public interface ITimeBlockService
    {
        public Task<ServiceResult<TimeBlockDto>> CreateOrUpdateTimeBlock(TimeBlockDto dto);
    }
}
