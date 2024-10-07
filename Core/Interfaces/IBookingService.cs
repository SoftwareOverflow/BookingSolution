using Core.Dto;
using Core.Dto.Appointment;
using Core.Dto.BookingRequest;
using Core.Responses;

namespace Core.Interfaces
{
    public interface IBookingService
    {
        /// <summary>
        /// Check if the service exists for the given business
        /// </summary>
        /// <param name="businessGuid">The GUID for the business</param>
        /// <param name="serviceGuid">The GUID for the service</param>
        /// <returns><see cref="ServiceResult{BookingRequestDto}"/> indicating if this business exists for the service</returns>
        public Task<ServiceResult<BookingRequestDto>> GetNewBookingRequest(Guid businessGuid, Guid serviceGuid);

        public Task<ServiceResult<AvailabilityDto>> GetAvailabilityBetweenDates(ServiceTypeDto dto, Guid businessGuid, DateOnly startDate, DateOnly endDate);

        public ServiceResult<DateOnly> GetNextServiceDate(ServiceTypeDto dto, DateOnly startDate);

        public Task<ServiceResult<AppointmentDto>> SendBookingRequest(BookingRequestDto dto, Guid businessGuid);
    }
}
