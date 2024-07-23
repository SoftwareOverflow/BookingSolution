using Admin.Data.Events;
using Core.Dto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Admin.Tests.Data.Events
{
    public class PositionedEventBookingTests
    {

        [Fact]
        public void Event_NoPadding_ReturnsFalse()
        {
            var date = DateOnly.Parse("2024-12-21");

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = new DateTime(date, new TimeOnly(12, 30)).AddMinutes(-45),
                    EndTime = new DateTime(date, new TimeOnly(14, 30)),
                    EventPaddingStart = TimeSpan.Zero,
                    EventPaddingEnd = TimeSpan.Zero,
                });

            Assert.False(booking.HasPadding(date));
        }

        [Fact]
        public void Event_PaddingStart_ReturnsTrue()
        {
            var date = DateOnly.Parse("2024-12-21");
            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = new DateTime(date, new TimeOnly(17, 59)).AddMinutes(-45),
                    EndTime = DateTime.Now,
                    EventPaddingStart = TimeSpan.FromMinutes(7),
                    EventPaddingEnd = TimeSpan.Zero,
                });

            Assert.True(booking.HasPadding(date));
        }

        [Fact]
        public void Event_PaddingEnd_ReturnsTrue()
        {
            var date = DateOnly.Parse("2024-05-15");

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = new DateTime(date, new TimeOnly(15, 25)).AddMinutes(-45),
                    EndTime = new DateTime(date, new TimeOnly(15, 55)),
                    EventPaddingStart = TimeSpan.Zero,
                    EventPaddingEnd = TimeSpan.FromMinutes(35),
                });

            Assert.True(booking.HasPadding(date));
        }

        [Fact]
        public void Event_PaddingBoth_ReturnsTrue()
        {
            var date = new DateOnly(2023, 1, 29);

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = new DateTime(date, new TimeOnly(17, 30)).AddMinutes(-45),
                    EndTime = new DateTime(date, new TimeOnly(20, 00)),
                    EventPaddingStart = TimeSpan.FromMinutes(76),
                    EventPaddingEnd = TimeSpan.FromMinutes(35),
                });

            Assert.True(booking.HasPadding(date));
        }

        [Fact]
        public void Event_Padding_MultipleDays()
        {
            var startDate = DateOnly.Parse("2023-01-01");
            var endDate = new DateOnly(2023, 01, 03);

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = new DateTime(startDate, new TimeOnly(17, 30)).AddMinutes(-45),
                    EndTime = new DateTime(endDate, new TimeOnly(20, 00)),
                    EventPaddingStart = TimeSpan.FromMinutes(76),
                    EventPaddingEnd = TimeSpan.FromMinutes(35),
                });

            Assert.True(booking.HasPadding(startDate));
            Assert.True(booking.HasPadding(endDate));
            Assert.False(booking.HasPadding(startDate.AddDays(1)));
        }

        [Fact]
        public void Event_Top_NoPadding()
        {
            var dateTime = DateTime.Parse("2024-01-01 03:45");
            var date = DateOnly.FromDateTime(dateTime);

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = dateTime
                });


            // 3 hours and 45 mins
            var expectedTop = (int)((EventLayoutConsts.CellHeight * 2) * 3.75);

            Assert.Equal(expectedTop, booking.TopPx(date));
            Assert.Equal(expectedTop, booking.TopPx(date, true));
        }

        [Fact]
        public void Event_Top_WithPadding()
        {
            var dateTime = DateTime.Parse("2024-01-01 02:35");
            var date = DateOnly.FromDateTime(dateTime);

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = dateTime,
                    EventPaddingStart = TimeSpan.FromMinutes(50)
                });

            // 2h 35m (- 50 mins) => 2h 35m (1h 45m)
            var expectedUnpadded = (int)((EventLayoutConsts.CellHeight * 2) * (2 + (35f / 60f)));
            var expectedPaddedTop = (int)((EventLayoutConsts.CellHeight * 2) * 1.75f);

            Assert.Equal(expectedUnpadded, booking.TopPx(date, false));
            Assert.Equal(expectedPaddedTop, booking.TopPx(date, true));
        }

        [Fact]
        public void Event_Height_NoPadding()
        {
            var dateTime = DateTime.Parse("2024-01-01 03:45");
            var date = DateOnly.FromDateTime(dateTime);

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = dateTime,
                    EndTime = dateTime.AddHours(3).AddMinutes(45)
                });


            // 3h 45m.
            var expectedHeight = (int)((EventLayoutConsts.CellHeight * 2) * 3.75);

            Assert.Equal(expectedHeight, booking.HeightPx(date));
            Assert.Equal(expectedHeight, booking.TopPx(date, true));
        }

        [Fact]
        public void Event_Height_PaddingStart()
        {
            var dateTime = DateTime.Parse("2024-01-01 03:45");
            var date = DateOnly.FromDateTime(dateTime);

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = dateTime,
                    EndTime = dateTime.AddHours(3).AddMinutes(45),
                    EventPaddingStart = new TimeSpan(0, 15, 0)
                });


            var expectedHeightUnpadded = (int)((EventLayoutConsts.CellHeight * 2) * 3.75);
            var expectedHeightPadded = (int)((EventLayoutConsts.CellHeight * 2) * 4);

            Assert.Equal(expectedHeightUnpadded, booking.HeightPx(date));
            Assert.Equal(expectedHeightPadded, booking.HeightPx(date, true));
        }

        [Fact]
        public void Event_Height_PaddingEnd()
        {
            var dateTime = DateTime.Parse("2024-01-01 03:45");
            var date = DateOnly.FromDateTime(dateTime);

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = dateTime,
                    EndTime = dateTime.AddHours(7).AddMinutes(15),
                    EventPaddingEnd = new TimeSpan(0, 30, 0)
                });


            var expectedHeightUnpadded = (int)((EventLayoutConsts.CellHeight * 2) * 7.25);
            var expectedHeightPadded = (int)((EventLayoutConsts.CellHeight * 2) * 7.75);

            Assert.Equal(expectedHeightUnpadded, booking.HeightPx(date));
            Assert.Equal(expectedHeightPadded, booking.HeightPx(date, true));
        }

        [Fact]
        public void Event_LTR_NoClash()
        {
            var booking = new PositionedEventBooking(
                booking: new EventBooking());

            // Only fill EventLayoutConsts.EventsWidthPc of the width so there is place to still click.
            Assert.Equal(EventLayoutConsts.EventsWidthPc, booking.WidthPc(DateOnly.MaxValue));
            Assert.Equal(0, booking.LeftPc(DateOnly.MaxValue));
        }

        [Fact]
        public void Event_Top_MultipleDays()
        {
            var startDate = DateOnly.Parse("2023-01-01");
            var endDate = new DateOnly(2023, 01, 03);

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = new DateTime(startDate, new TimeOnly(1, 30)).AddMinutes(-45),
                    EndTime = new DateTime(endDate, new TimeOnly(20, 00)),
                    EventPaddingStart = TimeSpan.FromMinutes(120),
                    EventPaddingEnd = TimeSpan.FromMinutes(240),
                });

            Assert.Equal(0, booking.TopPx(startDate, true));
            Assert.NotEqual(0, booking.TopPx(startDate, false));

            Assert.Equal(0, booking.TopPx(startDate.AddDays(1), true));
            Assert.Equal(0, booking.TopPx(startDate.AddDays(1), false));

            Assert.Equal(0, booking.TopPx(endDate, true));
            Assert.Equal(0, booking.TopPx(endDate, false));
        }

        [Fact]
        public void Event_Height_MultipleDays()
        {
            var startDate = DateOnly.Parse("2023-01-01");
            var endDate = new DateOnly(2023, 01, 03);

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = new DateTime(startDate, new TimeOnly(1, 30)).AddMinutes(-45),
                    EndTime = new DateTime(endDate, new TimeOnly(20, 00)),
                    EventPaddingStart = TimeSpan.FromMinutes(120),
                    EventPaddingEnd = TimeSpan.FromMinutes(240),
                });

            // A full height day starts at 00:00 and ends at 23:59
            var fullHeight = (int)((EventLayoutConsts.CellHeight * 2) * new TimeOnly(23, 59).ToTimeSpan().TotalHours);

            Assert.Equal(fullHeight, booking.HeightPx(startDate, true));
            Assert.NotEqual(fullHeight, booking.HeightPx(startDate, false));

            Assert.Equal(fullHeight, booking.HeightPx(startDate.AddDays(1), true));
            Assert.Equal(fullHeight, booking.HeightPx(startDate.AddDays(1), false));

            Assert.Equal(fullHeight, booking.HeightPx(endDate, true));
            Assert.NotEqual(fullHeight, booking.HeightPx(endDate, false));
        }

        [Fact]
        public void Event_LTR_Pos_AddClash()
        {
            var date = DateOnly.Parse("2024-07-06");

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = new DateTime(date, new TimeOnly(10, 15)),
                    EndTime = new DateTime(date, new TimeOnly(10, 45)),
                }
                );

            booking.AddClash(date, 7, 7);
            booking.AddClash(date.AddDays(1), 0, 3);

            // 7 clashes = 8 conc events.
            // Only fill EventLayoutConsts.EventsWidthPc% of the width so there is place to still click.
            var expectedWidthPc = (int)(EventLayoutConsts.EventsWidthPc / 8f);
            Assert.Equal(expectedWidthPc, booking.WidthPc(date));
            Assert.Equal(expectedWidthPc * 7, booking.LeftPc(date));

            expectedWidthPc = (int)(EventLayoutConsts.EventsWidthPc / 4f);
            Assert.Equal(expectedWidthPc, booking.WidthPc(date.AddDays(1)));
            Assert.Equal(0, booking.LeftPc(date.AddDays(1)));
        }

        [Fact]
        public void Event_LTR_Pos_AddUnkownClash()
        {
            var date = DateOnly.Parse("2024-07-06");

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = new DateTime(date, new TimeOnly(10, 15)),
                    EndTime = new DateTime(date, new TimeOnly(10, 45)),
                }
                );

            Assert.Equal(EventLayoutConsts.EventsWidthPc, booking.WidthPc(date));
            
            booking.AddClash(date);

            Assert.True(EventLayoutConsts.EventsWidthPc > booking.WidthPc(date));
        }

        [Fact]
        public void Event_RequestWrongDay()
        {
            var date = new DateOnly(2024, 7, 6);

            var booking = new PositionedEventBooking(
                booking: new EventBooking
                {
                    StartTime = new DateTime(date, new TimeOnly(10, 15)),
                    EndTime = new DateTime(date, new TimeOnly(10, 45)),
                }
                );

            Assert.Equal(0, booking.TopPx(date.AddDays(1)));
            Assert.Equal(0, booking.TopPx(date.AddDays(-1)));

            Assert.Equal(0, booking.HeightPx(date.AddDays(1)));
            Assert.Equal(0, booking.HeightPx(date.AddDays(-1)));
            Assert.NotEqual(0, booking.HeightPx(date));
        }

        [Fact]
        public void Event_GetStart_Padding()
        {
            var date = new DateOnly(2024, 7, 6);
            var booking = new PositionedEventBooking(
            booking: new EventBooking
            {
                StartTime = new DateTime(date, new TimeOnly(0, 0)),
                EndTime = new DateTime(date, new TimeOnly(10, 45)),
                EventPaddingStart = new TimeSpan(0, 15, 0)
            });

            Assert.Equal(date, booking.GetStartDate(false));
            Assert.Equal(date.AddDays(-1), booking.GetStartDate(true));
        }

        [Fact]
        public void Event_GetEnd_Padding()
        {
            var date = new DateOnly(2024, 7, 6);
            var booking = new PositionedEventBooking(
            booking: new EventBooking
            {
                StartTime = new DateTime(date, new TimeOnly(0, 0)),
                EndTime = new DateTime(date, new TimeOnly(22, 45)),
                EventPaddingEnd = new TimeSpan(3, 15, 0)
            });

            Assert.Equal(date, booking.GetEndDate(false));
            Assert.Equal(date.AddDays(1), booking.GetEndDate(true));
        }
    }
}