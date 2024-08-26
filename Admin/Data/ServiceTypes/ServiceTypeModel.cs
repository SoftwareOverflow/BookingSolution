using Core.Dto;
using Core.Dto.Services;
using System.ComponentModel.DataAnnotations;

namespace Admin.Data.ServiceTypes
{
    public record ServiceTypeModel
    {
        [ValidateComplexType]
        public ServiceTypeDto Service { get; private set; }

        public Dictionary<DayOfWeek, bool> WeeklyRepeat { get; private set; } = new()
        {
            {DayOfWeek.Monday, true},
            {DayOfWeek.Tuesday, true},
            {DayOfWeek.Wednesday, true},
            {DayOfWeek.Thursday, true},
            {DayOfWeek.Friday, true},
            {DayOfWeek.Saturday, false},
            {DayOfWeek.Sunday, false}
        };

        public bool[] MonthlyAbsoluteRepeat { get; private set; } = new bool[31];

        /// <summary>
        /// Maps the occurance number (index) to the weekday on which it is repeated
        /// </summary>
        public Dictionary<int, Dictionary<DayOfWeek, bool>> MonthlyRelativeRepeat { get; private set; } = new() {
            {1, new() { {DayOfWeek.Monday, false}, {DayOfWeek.Tuesday, false}, {DayOfWeek.Wednesday, false}, {DayOfWeek.Thursday, false}, {DayOfWeek.Friday, false}, {DayOfWeek.Saturday, false}, {DayOfWeek.Sunday, false} } },
            {2, new() { {DayOfWeek.Monday, false}, {DayOfWeek.Tuesday, false}, {DayOfWeek.Wednesday, false}, {DayOfWeek.Thursday, false}, {DayOfWeek.Friday, false}, {DayOfWeek.Saturday, false}, {DayOfWeek.Sunday, false} } },
            {3, new() { {DayOfWeek.Monday, false}, {DayOfWeek.Tuesday, false}, {DayOfWeek.Wednesday, false}, {DayOfWeek.Thursday, false}, {DayOfWeek.Friday, false}, {DayOfWeek.Saturday, false}, {DayOfWeek.Sunday, false} } },
            {-1, new() { {DayOfWeek.Monday, false}, {DayOfWeek.Tuesday, false}, {DayOfWeek.Wednesday, false}, {DayOfWeek.Thursday, false}, {DayOfWeek.Friday, false}, {DayOfWeek.Saturday, false}, {DayOfWeek.Sunday, false} } },
        };


        public ServiceTypeModel(ServiceTypeDto dto)
        {
            Service = dto;

            MapRepeats();
        }

        private void MapRepeats()
        {
            if (Service.Repeats.Count == 0)
            {
                return;
            } else
            {
                // Reset the defaults to empty
                foreach (var item in WeeklyRepeat.Keys)
                {
                    WeeklyRepeat[item] = false;
                }
            }

            foreach (var repeat in Service.Repeats)
            {
                try
                {
                    if (Service.RepeatType == ServiceRepeaterTypeDto.Weekly)
                    {
                        WeeklyRepeat[(DayOfWeek)repeat.DayIdentifier] = true;
                    }
                    else if (Service.RepeatType == ServiceRepeaterTypeDto.MonthlyAbsolute)
                    {
                        MonthlyAbsoluteRepeat[repeat.DayIdentifier - 1] = true;
                    }
                    else if (Service.RepeatType == ServiceRepeaterTypeDto.MonthlyRelative)
                    {
                        if (repeat.Index == null)
                        {
                            // TODO logging, continue as this is invalid state.

                            continue;
                        }

                        if (MonthlyRelativeRepeat.TryGetValue((int)repeat.Index, out var days))
                        {
                            days[(DayOfWeek)repeat.DayIdentifier] = true;
                        }
                        else
                        {
                            // TODO logging, shouldn't get into this state where we can't map the dictionary;
                        }

                    }

                }
                catch (InvalidCastException)
                {
                    // TODO logging.
                    // Catch any failed casts (to DayOfWeek). Show user a message, log it, carry on.
                }
                catch (Exception)
                {
                    // TODO logging. Continue
                }
            }
        }

        public int DurationHours
        {
            get => Service.Duration.Hours;
            set
            {
                Service.Duration = new(value, Service.Duration.Minutes, 0);
            }
        }

        public int DurationMinutes
        {
            get => Service.Duration.Minutes;
            set
            {
                Service.Duration = new(Service.Duration.Hours, value, 0);
            }
        }

        public ServiceTypeDto MapToDto()
        {
            var repeats = new List<ServiceRepeaterDto>();

            if (Service.RepeatType == ServiceRepeaterTypeDto.Weekly)
            {
                WeeklyRepeat.Where(x => x.Value).Select(x => x.Key).ToList().ForEach(r =>
                {
                    repeats.Add(new ServiceRepeaterDto(r));
                });
            }
            else if (Service.RepeatType == ServiceRepeaterTypeDto.MonthlyAbsolute)
            {
                for (int i = 0; i < MonthlyAbsoluteRepeat.Length; i++)
                {
                    if (MonthlyAbsoluteRepeat[i])
                    {
                        repeats.Add(new ServiceRepeaterDto(i+1));
                    }
                }
            }
            else if (Service.RepeatType == ServiceRepeaterTypeDto.MonthlyRelative)
            {
                foreach (var week in MonthlyRelativeRepeat) {
                    foreach(var dayOfWeek in week.Value.Where(x => x.Value).Select(x => x.Key))
                    {
                        repeats.Add(new ServiceRepeaterDto(dayOfWeek, week.Key));   
                    }
                }
            }

            Service.Repeats = repeats;
            return Service;
        }
    }
}
