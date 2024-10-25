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

        public async Task<ServiceResult<TimeBlockDto>> GetTimeBlock(Guid guid)
        {
            try
            {
                var timeBlock = await _appointmentContext.GetTimeBlock(guid);

                if (timeBlock == null)
                {
                    return new ServiceResult<TimeBlockDto>(null, ResultType.ClientError, ["Unable to find Time Block with supplied Id"]);
                }

                var dto = _mapper.Map<TimeBlockDto>(timeBlock);
                return new ServiceResult<TimeBlockDto>(dto);
            }
            catch (Exception)
            {
                // TODO logging.
            }

            return ServiceResult<TimeBlockDto>.DefaultServerFailure();
        }

        public async Task<ServiceResult<TimeBlockDto>> CreateOrUpdateTimeBlock(TimeBlockDto dto)
        {
            try
            {
                if (dto.RepeatType != null && dto.Repeats.Count == 0)
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
                    result = await _appointmentContext.Update(entity);
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

        public async Task<ServiceResult<ICollection<TimeBlockInstanceDto>>> GetTimeBlocksBetweenDates(DateOnly start, DateOnly end)
        {
            try
            {
                var timeBlockResult = await _appointmentContext.GetTimeBlocks();
                var timeBlocks = _mapper.Map<ICollection<TimeBlockDto>>(timeBlockResult);

                var timeBlockInstances = new List<TimeBlockInstanceDto>();

                foreach (var timeBlock in timeBlocks)
                {
                    if (timeBlock.RepeatType == null)
                    {
                        if (timeBlock.StartTime <= end.ToDateTime(TimeOnly.MaxValue) && timeBlock.EndTime >= start.ToDateTime(TimeOnly.MinValue))
                        {
                            timeBlockInstances.Add(new(timeBlock.Guid, timeBlock.Name, DateOnly.FromDateTime(timeBlock.StartTime), false)
                            {
                                StartTime = timeBlock.StartTime,
                                EndTime = timeBlock.EndTime,
                            });
                        }

                        // No repeats to check, continue
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

                            // Next occurance is outside range
                            if (date > end)
                            {
                                break;
                            }

                            // Handle all repeats which are NOT exceptions
                            var exception = timeBlock.Exceptions.SingleOrDefault(e => e.DateToReplace == date);
                            if (exception == null)
                            {
                                var timeBlockInstance = new TimeBlockInstanceDto(timeBlock.Guid, timeBlock.Name, date)
                                {
                                    StartTime = new DateTime(date, startTime),
                                    EndTime = new DateTime(date, endTime).AddDays(days),
                                };

                                timeBlockInstances.Add(timeBlockInstance);
                            }
                        }

                        date = date.AddDays(1);
                    }

                    // Include any exceptions which occur in the range
                    var exceptionsInRange = timeBlock.Exceptions.Where(x => x.StartTime <= end.ToDateTime(TimeOnly.MaxValue) && x.EndTime >= start.ToDateTime(TimeOnly.MinValue));
                    foreach (var exception in exceptionsInRange)
                    {
                        var duration = exception.EndTime.Subtract(exception.StartTime);

                        if (duration > TimeSpan.Zero)
                        {

                            var timeBlockInstance = new TimeBlockInstanceDto(timeBlock.Guid, exception.Name, exception.DateToReplace, true)
                            {
                                StartTime = exception.StartTime,
                                EndTime = exception.EndTime,
                            };

                            timeBlockInstances.Add(timeBlockInstance);
                        }
                    }
                }

                return new ServiceResult<ICollection<TimeBlockInstanceDto>>(timeBlockInstances);
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<ICollection<TimeBlockInstanceDto>>.DefaultServerFailure();
        }
    }
}
