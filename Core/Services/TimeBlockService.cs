using AutoMapper;
using Core.Dto.Appointment;
using Core.Helpers;
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

        public async Task<ServiceResult<TimeBlockDto>> CreateOrUpdate(TimeBlockDto dto)
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

                var exceptionsDict = _appointmentContext.GetTimeBlockExceptionsBetweenDates(start, end) ?? [];

                var instances = TimeBlockHelper.GetTimeBlockInstancesBetweenDates(timeBlocks, start, end);

                // Include any exceptions which occur in the range. This is separate as we might have exceptions where the assosciated TimeBlock has been deleted.
                foreach (var entry in exceptionsDict)
                {
                    var exceptionsInRange = _mapper.Map<ICollection<TimeBlockExceptionDto>>(entry.Value);
                    foreach (var exception in exceptionsInRange)
                    {
                        var duration = exception.EndTime.Subtract(exception.StartTime);

                        if (duration > TimeSpan.Zero)
                        {
                            var timeBlockInstance = new TimeBlockInstanceDto(entry.Key, exception.Name, exception.DateToReplace, IsException: true)
                            {
                                StartTime = exception.StartTime,
                                EndTime = exception.EndTime,
                                Guid = exception.Guid, // Explicitly set the guid for exceptions as these are actual entities,
                            };

                            instances.Add(timeBlockInstance);
                        }
                    }
                }

                return new ServiceResult<ICollection<TimeBlockInstanceDto>>(instances);
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<ICollection<TimeBlockInstanceDto>>.DefaultServerFailure();
        }

        public async Task<ServiceResult<TimeBlockExceptionDto>> CreateOrUpdate(TimeBlockExceptionDto dto, Guid timeBlockGuid)
        {
            try
            {
                var getTimeBlockTask = GetTimeBlock(timeBlockGuid);
                var getInstancesTask = GetTimeBlocksBetweenDates(dto.DateToReplace, dto.DateToReplace);

                var timeBlockResult = await getTimeBlockTask;
                var instancesResult = await getInstancesTask;

                if (timeBlockResult.IsSuccess && instancesResult.IsSuccess)
                {
                    var timeBlock = timeBlockResult.Result!;
                    var instanceToReplace = instancesResult.Result!.SingleOrDefault(i => i.TimeBlockGuid == timeBlock.Guid && i.InstanceDate == dto.DateToReplace);

                    if (instanceToReplace == null)
                    {
                        return new ServiceResult<TimeBlockExceptionDto>(null, ResultType.ClientError, ["Cannot find instance to replace. Ensure date is valid"]);
                    }

                    var entity = _mapper.Map<TimeBlockException>(dto);
                    var result = false;
                    if (dto.Guid == Guid.Empty)
                    {
                        result = await _appointmentContext.Create(entity, timeBlockGuid);
                    }
                    else
                    {
                        result = await _appointmentContext.Update(entity, timeBlockGuid);
                    }

                    if (result)
                    {
                        dto = _mapper.Map<TimeBlockExceptionDto>(entity);
                        return new ServiceResult<TimeBlockExceptionDto>(dto);
                    }
                }
                else
                {
                    if (timeBlockResult.ResultType == ResultType.ClientError || instancesResult.ResultType == ResultType.ClientError)
                    {
                        return new ServiceResult<TimeBlockExceptionDto>(null, ResultType.ClientError, ["Cannot find assosciated time block details"]);
                    }

                    // TODO Logging
                }
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<TimeBlockExceptionDto>.DefaultServerFailure();
        }

        public async Task<ServiceResult<bool>> Delete(TimeBlockInstanceDto dto, bool deleteExceptions)
        {
            try
            {
                var result = false;
                if (dto.IsException)
                {
                    var entity = new TimeBlockException()
                    {
                        Guid = dto.Guid,
                        DateToReplace = dto.InstanceDate,
                        Name = dto.Name
                    };

                    result = await _appointmentContext.DeleteException(entity, dto.TimeBlockGuid);
                }
                else
                {
                    result = await _appointmentContext.DeleteTimeBlock(dto.TimeBlockGuid, deleteExceptions);
                }

                if (result)
                {
                    return new ServiceResult<bool>(true);
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
