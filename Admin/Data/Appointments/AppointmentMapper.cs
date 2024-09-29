using Core.Dto;

namespace Admin.Data.Appointments
{
    internal static class AppointmentMapper
    {
        internal static List<PositionedAppointment> GetPositionedEventBookings(this List<AppointmentDto> events)
        {
            var positionedEvents = events.OrderBy(x => x.StartTimePadded).Select(x => new PositionedAppointment(x)).ToList();

            if (positionedEvents.Count == 0)
            {
                return positionedEvents;
            }

            var minDate = positionedEvents.Min(x => x.GetStartDate(true));
            var maxDate = positionedEvents.Max(x => x.GetEndDate(true));

            int index = 0;

            var currDate = minDate;
            while (currDate <= maxDate)
            {
                var rowEventMap = new Dictionary<int, List<PositionedAppointment>>();

                for (int i = index; i < positionedEvents.Count; i++)
                {
                    var booking = positionedEvents[i];

                    if (booking.HeightPx(currDate, true) == 0)
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

                        if (clashes.Count != 0)
                        {
                            for (int j = 0; j < clashes.Count; j++)
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

            return positionedEvents;
        }
    }
}