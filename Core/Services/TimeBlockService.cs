using AutoMapper;
using Core.Dto.Appointment;
using Core.Interfaces;
using Core.Responses;
using Data.Entity.Appointments;
using Data.Interfaces;

namespace Core.Services
{
    internal class TimeBlockService(IAppointmentRepo appointmentContext, IMapper mapper) : ITimeBlockService
    {
        private readonly IAppointmentRepo _appointmentContext = appointmentContext;

        private readonly IMapper _mapper = mapper;

        public async Task<ServiceResult<TimeBlockDto>> CreateOrUpdateTimeBlock(TimeBlockDto dto)
        {
            try
            {
                if(dto.RepeatType != null && dto.Repeats.Count == 0)
                {
                    return new ServiceResult<TimeBlockDto>(null, ResultType.ClientError, ["Cannot create a repeating time block without any repeats!"]);
                }

                var entity = _mapper.Map<TimeBlock>(dto);

                var result = false;
                if (dto.Guid == Guid.Empty)
                {
                    result = await _appointmentContext.Create(entity);
                }
                else
                {
                    // UPDATE
                }

                if (result)
                {
                    dto = _mapper.Map<TimeBlockDto>(entity);
                    return new ServiceResult<TimeBlockDto>(dto);
                }
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<TimeBlockDto>.DefaultServerFailure();
        }
    }
}
