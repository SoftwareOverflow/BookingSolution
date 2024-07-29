using Core.Dto;

namespace Admin.Data.Events
{
    internal static class EventBookingMapper
    {
        internal static List<PositionedEventBooking> GetPositionedEventBookings(this List<EventBooking> events)
        {
            var result = new List<PositionedEventBooking>();

            var positionedEvents = events.OrderBy(x => x.StartTime.Subtract(x.PaddingStart)).Select(x => new PositionedEventBooking(x)).ToList();

            var minDate = positionedEvents.Min(x => x.GetStartDate(true));
            var maxDate = positionedEvents.Max(x => x.GetEndDate(true));

            int index = 0;

            var currDate = minDate;
            while (currDate <= maxDate)
            {
                var rowEventMap = new Dictionary<int, List<PositionedEventBooking>>();

                for (int i = index; i < positionedEvents.Count; i++)
                {
                    var booking = positionedEvents[i];

                    if(booking.HeightPx(currDate, true) == 0)
                    {
                        continue;
                    }
                    

                    var totalClashes = rowEventMap.Values.ToList()
                        .Sum(l =>
                            l.Where(x =>
                                booking.GetStartTime(currDate, true) < x.GetEndTime(currDate, true))
                        .Count());

                    bool located = false;
                    int rowTest = 0;
                    while (!located)
                    {
                        rowEventMap.TryGetValue(rowTest, out var eventsToCheck);
                        eventsToCheck ??= [];

                        var clashes = eventsToCheck.Where(x => booking.GetStartTime(currDate, true) < x.GetEndTime(currDate, true)).ToList();
                        //totalClashes += clashes.Count;
                        if (clashes.Count != 0)
                        {
                            for (int j = 0; j < clashes.Count(); j++)
                            {
                                clashes[j].AddClash(currDate);
                            }
                        }
                        else
                        {
                            booking.AddClash(currDate, rowTest, totalClashes);
                            eventsToCheck.Add(booking);
                            rowEventMap[rowTest] = eventsToCheck;

                            located = true;
                        }

                        rowTest++;
                    }
                }

                currDate = currDate.AddDays(1);
            }

            /*for (int i = 0; i < positionedEvents.Count; i++)
            {
                var pe = positionedEvents[i];

                var startDate = DateOnly.FromDateTime(pe.Event.StartTime);
                var endDate = DateOnly.FromDateTime(pe.Event.EndTime);

                var currDate = startDate;
                while (currDate <= endDate)
                {
                    var clashes = 0;

                    var eventStart = pe.GetStartTime(currDate, true);
                    var eventEnd = pe.GetEndTime(currDate, true);

                    int j = i + 1;
                    for (j = i + 1; j < positionedEvents.Count; j++)
                    {
                        var eventToCheck = positionedEvents[j];
                        var nextEventStart = eventToCheck.GetStartTime(currDate, true);

                        if (nextEventStart > eventEnd)
                        {
                            clashes++;
                        } else
                        {
                            break;
                        }
                    }

                    // TODO This might not work at extremes (k = i, or j = max)
                    int k = i;
                    for (k = i; k < j && k < positionedEvents.Count; k++)
                    {
                        int position = k - i;

                        positionedEvents[k].AddClash(currDate, position, clashes);
                    }

                    currDate = currDate.AddDays(1);
                }
            }*/

            return positionedEvents;
        }

        private static Dictionary<DateOnly, EventBooking> SplitEventsByDate(PositionedEventBooking booking)
        {
            var result = new Dictionary<DateOnly, EventBooking>();

            var paddedStart = booking.Event.StartTime.Subtract(booking.Event.PaddingStart);
            var paddedEnd = booking.Event.EndTime.Add(booking.Event.PaddingEnd);

            var startDate = DateOnly.FromDateTime(paddedStart);
            var endDate = DateOnly.FromDateTime(paddedEnd);

            var currDate = startDate;
            while (currDate <= endDate)
            {
                result[currDate] = booking.Event with
                {
                    StartTime = new DateTime(currDate, booking.GetStartTime(currDate, true)),
                    EndTime = new DateTime(currDate, booking.GetEndTime(currDate, true)),
                    PaddingEnd = TimeSpan.Zero,
                    PaddingStart = TimeSpan.Zero,
                };

                currDate = currDate.AddDays(1);
            }

            return result;
        }
    }
}
