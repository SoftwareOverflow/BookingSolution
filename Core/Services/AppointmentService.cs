using AutoMapper;
using Core.Dto;
using Core.Interfaces;
using Core.Responses;
using Data.Entity.Appointments;
using Data.Interfaces;

namespace Core.Services
{
    internal class AppointmentService(IAppointmentContext appointmentContext, IMapper mapper) : IAppointmentService
    {
        private readonly IAppointmentContext AppointmentContext = appointmentContext;

        private readonly IMapper Mapper = mapper;

        /// <summary>
        /// Get all <see cref="AppointmentDto"/> objects in a given date range.
        /// </summary>
        /// <param name="start">The start date of the range (inclusive)</param>
        /// <param name="end">The end date of the range (inclusive)</param>
        /// <returns>Any events which occur in whole or part within the specified range</returns>
        public Task<ServiceResult<List<AppointmentDto>>> GetAppointmentsBetweenDates(DateOnly start, DateOnly end)
        {
            try
            {
                var result = AppointmentContext.GetAppointmentsBetweenDates(start, end);
                var dtos = Mapper.Map<ICollection<AppointmentDto>>(result);

                return Task.FromResult(new ServiceResult<List<AppointmentDto>>([.. dtos]));

            }
            catch (Exception)
            {
                // TODO logging
            }

            return Task.FromResult(ServiceResult<List<AppointmentDto>>.DefaultServerFailure());
        }


        public async Task<ServiceResult<AppointmentDto>> CreateOrUpdateAppointment(AppointmentDto appointment)
        {
            try
            {
                var entity = Mapper.Map<Appointment>(appointment);

                bool result = false;
                if (entity.Guid == Guid.Empty)
                {
                    result = await AppointmentContext.Create(entity);
                }
                else
                {
                    result = await AppointmentContext.Update(entity);
                }

                if (result)
                {
                    appointment = Mapper.Map<AppointmentDto>(entity);

                    return new ServiceResult<AppointmentDto>(appointment);
                }
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<AppointmentDto>.DefaultServerFailure();
        }

        public async Task<ServiceResult<bool>> DeleteAppointment(Guid id)
        {
            try
            {
                var result = await AppointmentContext.DeleteAppointment(id);

                if (result)
                {
                    return new ServiceResult<bool>(result);
                }
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<bool>.DefaultServerFailure();
        }
    }
}
