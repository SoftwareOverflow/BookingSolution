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

        public Task<ServiceResult<ICollection<TimeBlockInstanceDto>>> GetTimeBlocksBetweenDates(DateOnly start, DateOnly end)
        {
            try
            {
                var timeBlockResult = _appointmentContext.GetTimeBlocks();
                var timeBlocks = _mapper.Map<ICollection<TimeBlockDto>>(timeBlockResult);

                var timeBlockInstances = new List<TimeBlockInstanceDto>();

                foreach (var timeBlock in timeBlocks)
                {
                    if(end < DateOnly.FromDateTime(timeBlock.StartTime))
                    {
                        continue;
                    }

                    var days = timeBlock.EndTime.Subtract(timeBlock.StartTime).Days;
                    var startTime = TimeOnly.FromDateTime(timeBlock.StartTime);
                    var endTime = TimeOnly.FromDateTime(timeBlock.EndTime);

                    var date = start;
                    while (date <= end)
                    {
                        var result = RepeaterService.GetNextRepeaterDate(timeBlock, date);
                        if (result.IsSuccess)
                        {
                            date = result.Result;


                            var timeBlockInstance = new TimeBlockInstanceDto(timeBlock.Guid, timeBlock.Name, date)
                            {
                                StartTime = new DateTime(date, startTime),
                                EndTime = new DateTime(date, endTime).AddDays(days),
                            };

                            timeBlockInstances.Add(timeBlockInstance);
                        }

                        date = date.AddDays(1);
                    }
                }

                return Task.FromResult(new ServiceResult<ICollection<TimeBlockInstanceDto>>(timeBlockInstances));
            }
            catch (Exception)
            {
                // TODO logging
            }

            return Task.FromResult(ServiceResult<ICollection<TimeBlockInstanceDto>>.DefaultServerFailure());
        }
    }
}
