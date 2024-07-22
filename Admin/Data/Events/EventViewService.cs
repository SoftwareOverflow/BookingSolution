using Core.Interfaces;

namespace Admin.Data.Events
{
    public class EventViewService(IEventBookingDataService dataService)
    {
        public async Task<List<PositionedEventBooking>> GetEvents()
        {
            var events = await dataService.GetAllEntries();

            var positionedEvents = events.Select(x => new PositionedEventBooking(x, [])).ToList();

            events = events.OrderBy(x => x.StartTime.Subtract(x.EventPaddingStart)).ToList();

            /*for(int i=0; i < events.Count; i++)
            {
                var endTime = events[i].EndTime.Add(events[i].EventPaddingEnd);

                int j = i + 1;
                int overlapCount = 0;
                while (j < events.Count)
                {
                    var nextEvent = events[j];

                    if(nextEvent.StartTime.Subtract(nextEvent.EventPaddingStart) < endTime)
                    {
                        overlapCount++;
                    } else
                    {
                        break;
                    }


                    j++;
                }

                if (overlapCount > 0)
                {
                    var position = 0;
                    for(int x = i; x < j; x++)
                    {
                        positionedEvents[x].Position = position;
                        positionedEvents[x].Clashes = overlapCount;

                        position++;
                    }
                }
            }*/

            return positionedEvents;
        }
    }
}
