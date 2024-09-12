using AutoMapper;
using Core.Dto;
using Core.Dto.BookingRequest;
using Core.Dto.Services;
using Core.Interfaces;
using Core.Responses;
using Data.Entity.Appointments;
using Data.Interfaces;

namespace Core.Services
{
    internal class BookingService(IBookingContext bookingContext, IMapper mapper) : IBookingService
    {
        private readonly IBookingContext BookingContext = bookingContext;

        private readonly IMapper Mapper = mapper;

        // TODO maybe cache some so we don't need to repeat the calls when the user moves back and forth through their date selection

        public async Task<ServiceResult<AvailabilityDto>> GetAvailabilityBetweenDates(ServiceTypeDto dto, Guid businessGuid, DateOnly startDate, DateOnly endDate)
        {
            try
            {
                if (endDate < startDate)
                {
                    return new ServiceResult<AvailabilityDto>(null, ResultType.ClientError, ["end date must be after start date"]);
                }
                else if (endDate < DateOnly.FromDateTime(dto.StartDate ?? DateTime.MaxValue))
                {
                    return new ServiceResult<AvailabilityDto>(new AvailabilityDto());
                }

                var availableDays = new List<DateOnly>();
                var dateToMatch = startDate;
                while (dateToMatch <= endDate)
                {
                    var nextDayResult = GetNextServiceDate(dto, dateToMatch);

                    if (nextDayResult.IsSuccess)
                    {
                        var nextDay = nextDayResult.Result;

                        if (nextDay <= endDate)
                        {
                            availableDays.Add(nextDay);
                        }

                        // Skip all dates which weren't matched
                        dateToMatch = nextDay.AddDays(1);
                    }
                    else
                    {
                        // TODO logging

                        // Continue with the loop 
                        dateToMatch = dateToMatch.AddDays(1);
                    }
                }

                List<TimeAvailability> times = [];
                var time = dto.EarliestTime!.Value;
                while (time.Add(dto.Duration) <= dto.LatestTime!.Value)
                {
                    times.Add(new TimeAvailability(TimeOnly.FromTimeSpan(time)));

                    time = time.Add(TimeSpan.FromMinutes(dto.BookingFrequencyMins));
                }

                // TODO filter these times down so that they avoid clashes
                // TODO create an AppointmentContext &/ AppointmentService which will get all appointments between certain dates
                // (probably Service so we only have to write error handling code once)

                var availabilityDto = new AvailabilityDto()
                {
                    Availability = availableDays.Select(x =>
                    new DateAvailability(x)
                    {
                        Times = new List<TimeAvailability>(times) // Make a copy so we can modify it independently of other dates later
                    }).ToList()
                };

                var appointments = await BookingContext.GetBookingsBetweenDates(businessGuid, startDate, endDate);
                var existingAppointments = Mapper.Map<ICollection<Appointment>, ICollection<AppointmentDto>>(appointments);

                foreach (var dateAvailability in availabilityDto.Availability)
                {
                    var dateToCheck = dateAvailability.Date;

                    var appointmentsOnDate = existingAppointments.Where(a =>
                        dateToCheck.ToDateTime(new TimeOnly()) <= a.EndTimePadded && dateToCheck.ToDateTime(new TimeOnly(23, 59, 59)) >= a.StartTimePadded
                    );

                    HashSet<TimeOnly> clashedTimes = [];
                    foreach (var appointment in appointmentsOnDate)
                    {
                        var appointmentStart = appointment.GetStartTime(dateToCheck, true);
                        var appointmentEnd = appointment.GetEndTime(dateToCheck, true);

                        dateAvailability.Times.Where(t =>
                                appointmentStart < t.Time.Add(TimeSpan.FromMinutes(dto.DurationMins)) &&
                                appointmentEnd > t.Time
                        ).ToList().ForEach(x => clashedTimes.Add(x.Time));
                    }

                    for (int i = 0; i < dateAvailability.Times.Count; i++)
                    {
                        var t = dateAvailability.Times[i];

                        var isClash = clashedTimes.Contains(t.Time);

                        if (isClash)
                        {
                            dateAvailability.Times[i] = new TimeAvailability(t.Time)
                            {
                                State = AvailabilityState.AlreadyBooked,
                            };
                        }
                    }
                }


                var currentDate = startDate;
                while (currentDate <= endDate)
                {
                    if (availabilityDto.Availability.SingleOrDefault(x => x.Date == currentDate) == null)
                    {
                        availabilityDto.Availability.Add(new DateAvailability(currentDate));
                    }

                    currentDate = currentDate.AddDays(1);
                }

                availabilityDto.Availability = availabilityDto.Availability.OrderBy(x => x.Date).ToList();

                return new ServiceResult<AvailabilityDto>(availabilityDto);
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<AvailabilityDto>.DefaultServerFailure();
        }

        public ServiceResult<DateOnly> GetNextServiceDate(ServiceTypeDto dto, DateOnly startDate)
        {
            try
            {
                if (dto.Repeats.Count == 0)
                {
                    return new ServiceResult<DateOnly>(default, ResultType.ServerError, ["Unable to find any dates for this service"]);
                }
                if (startDate.ToDateTime(new TimeOnly()) < dto.StartDate)
                {
                    return new ServiceResult<DateOnly>(default, ResultType.ClientError, ["Service has not started yet!"]);
                }

                var currentDate = startDate;
                if (dto.RepeatType == ServiceRepeaterTypeDto.Weekly)
                {
                    return GetNextWeeklyDate(dto, currentDate);
                }
                else if (dto.RepeatType == ServiceRepeaterTypeDto.MonthlyAbsolute)
                {
                    return GetNextMonthlyAbsoluteDate(dto, currentDate);
                }
                else if (dto.RepeatType == ServiceRepeaterTypeDto.MonthlyRelative)
                {
                    return GetNextMonthlyRelativeDate(dto, currentDate);
                }
            }
            catch (Exception)
            {
                // TODO Logging.
            }

            // TODO logging - shouldn't get here - must not have a match for RepeatType
            return ServiceResult<DateOnly>.DefaultServerFailure();
        }

        private static ServiceResult<DateOnly> GetNextWeeklyDate(ServiceTypeDto dto, DateOnly startDate)
        {
            HashSet<DayOfWeek> daysOfWeek = [];

            dto.Repeats.ToList().ForEach(x => daysOfWeek.Add((DayOfWeek)x.DayIdentifier));

            while (!daysOfWeek.Contains(startDate.DayOfWeek))
            {
                startDate = startDate.AddDays(1);
            }

            return new ServiceResult<DateOnly>(startDate);
        }

        private static ServiceResult<DateOnly> GetNextMonthlyAbsoluteDate(ServiceTypeDto dto, DateOnly startDate)
        {
            HashSet<int> daysOfMonth = [];

            dto.Repeats.ToList().ForEach(x => daysOfMonth.Add(x.DayIdentifier));

            var currentDay = startDate.Day;

            // If we're later in the month than all of the options, set to first (lowest) value for next month.
            if (daysOfMonth.All(x => x < currentDay))
            {
                var nextDate = new DateOnly(startDate.Year, startDate.Month, daysOfMonth.Min()).AddMonths(1);
                return new ServiceResult<DateOnly>(nextDate);
            }
            else
            {
                foreach (var day in daysOfMonth.Order())
                {
                    if (day >= currentDay)
                    {
                        var nextDate = new DateOnly(startDate.Year, startDate.Month, day);
                        return new ServiceResult<DateOnly>(nextDate);
                    }
                };
            }

            return ServiceResult<DateOnly>.DefaultServerFailure();
        }

        private static ServiceResult<DateOnly> GetNextMonthlyRelativeDate(ServiceTypeDto dto, DateOnly startDate)
        {
            Dictionary<int, HashSet<DayOfWeek>> daysByWeek = new() {
                        { 1, []},
                        { 2, []},
                        { 3, []},
                        { -1, []},
                };

            dto.Repeats.Where(x => x.Index.HasValue).ToList().ForEach(x =>
            {
                if (daysByWeek.TryGetValue(x.Index!.Value, out var set))
                {
                    set.Add((DayOfWeek)x.DayIdentifier);
                }
                else
                {
                    // TODO logging, invalid Index value set
                }
            });

            // We should hit within a month in most cases, but limit the loop in case something is wrong.
            var currentDate = startDate;
            while (currentDate <= startDate.AddMonths(2))
            {
                var startOfMonth = new DateOnly(currentDate.Year, currentDate.Month, 1);
                var dateToCheck = startOfMonth;

                int index = 1;
                var daysOfWeek = daysByWeek[index];
                while (index <= 3)
                {
                    daysOfWeek = daysByWeek[index];
                    // Loop through the week to check
                    for (int i = 0; i < 7; i++)
                    {
                        if (dateToCheck >= startDate)
                        {
                            if (daysOfWeek.Contains(dateToCheck.DayOfWeek))
                            {
                                return new ServiceResult<DateOnly>(dateToCheck);
                            }
                        }
                        dateToCheck = dateToCheck.AddDays(1);
                    }

                    index++;
                }

                // No match in first 3 weeks, check final week.
                daysOfWeek = daysByWeek[-1];
                if (daysOfWeek.Count != 0)
                {
                    dateToCheck = startOfMonth.AddMonths(1).AddDays(-8); // Final week of the month
                    for (int i = 0; i < 7; i++)
                    {
                        if (dateToCheck >= startDate)
                        {
                            if (daysOfWeek.Contains(dateToCheck.DayOfWeek))
                            {
                                return new ServiceResult<DateOnly>(dateToCheck);
                            }
                        }

                        dateToCheck = dateToCheck.AddDays(1);
                    }
                }

                currentDate = currentDate.AddMonths(1);
            }

            // TODO logging - shouldn't get here
            return ServiceResult<DateOnly>.DefaultServerFailure();
        }

        public async Task<ServiceResult<BookingRequestDto>> GetNewBookingRequest(Guid businessGuid, Guid serviceGuid)
        {
            try
            {
                var service = await BookingContext.GetService(businessGuid, serviceGuid);

                if (service == null)
                {
                    return new ServiceResult<BookingRequestDto>(null, ResultType.ClientError, ["Failed to find service for business"]);
                }

                var serviceDto = Mapper.Map<ServiceTypeDto>(service);

                var today = DateOnly.FromDateTime(DateTime.Now);
                var nextAvailableDate = GetNextServiceDate(serviceDto, today);
                var bookingRequestDto = new BookingRequestDto(serviceDto, businessGuid, new PersonDto(), nextAvailableDate.IsSuccess ? nextAvailableDate.Result : today);

                return new ServiceResult<BookingRequestDto>(bookingRequestDto);
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<BookingRequestDto>.DefaultServerFailure();
        }

        public async Task<ServiceResult<AppointmentDto>> SendBookingRequest(BookingRequestDto dto, Guid businessGuid)
        {
            try
            {
                if(dto.SelectedTime == null)
                {
                    return new ServiceResult<AppointmentDto>(null, ResultType.ClientError, ["Booking time is required"]);
                }

                var requestStart = new DateTime(dto.SelectedDate, dto.SelectedTime.Value);

                // Map the BookingRequest to an appointment
                var appointment = new AppointmentDto(dto.Service.Name, dto.Person)
                {
                    Service = dto.Service,
                    StartTime = requestStart,
                    EndTime = requestStart.Add(dto.Service.Duration),
                };

                // TODO check for clashes
                var clashResult = await GetAvailabilityBetweenDates(dto.Service, businessGuid, dto.SelectedDate, dto.SelectedDate);

                if (clashResult.IsSuccess)
                {
                    var day = clashResult.Result!.Availability.Single();
                    if(day.Times.Count == 0)
                    {
                        return new ServiceResult<AppointmentDto>(null, ResultType.ClientError, ["Unable to book this service on this day"]);
                    }

                    if(day.Times.Single(x => x.Time == dto.SelectedTime.Value).State == AvailabilityState.Available)
                    {
                        var entity = Mapper.Map<Appointment>(appointment);

                        bool result = await BookingContext.CreateBookingRequest(entity, dto.BusinessGuid);

                        if (result)
                        {
                            appointment = Mapper.Map<AppointmentDto>(entity);

                            return new ServiceResult<AppointmentDto>(appointment);
                        }
                    } else
                    {
                        return new ServiceResult<AppointmentDto>(null, ResultType.ClientError, ["Requested time is not available"]);
                    }
                }
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<AppointmentDto>.DefaultServerFailure();
        }
    }
}
