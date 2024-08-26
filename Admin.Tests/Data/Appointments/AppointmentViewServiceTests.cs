using Admin.Data.Events;
using Core.Dto;
using Core.Interfaces;
using Core.Responses;
using Moq;

namespace Admin.Tests.Data.Events
{
    public class AppointmentViewServiceTests
    {
        private readonly Mock<IAppointmentDataService> DataMock = new();
        private readonly Mock<IMessageService> MessageMock = new();
        private AppointmentViewService ViewService;

        public AppointmentViewServiceTests()
        {

            ViewService = new AppointmentViewService(DataMock.Object, MessageMock.Object);
        }


        /// <summary>
        /// <----- Time ---->
        /// <-- Event #1 -->
        ///                 <!-- Event #2 -->
        ///                                         <-- Event #3 -->
        /// </summary>
        [Fact]
        public async Task EventViewService_NoPadding_NoClashes()
        {
            var date = new DateOnly(2024, 6, 5);

            var mockList = new List<Core.Dto.Appointment>
                    {

                        new() {
                            Name = "Event #2 - Example",
                            StartTime = new DateTime(date, new TimeOnly(2, 30)),
                            EndTime = new DateTime(date, new TimeOnly(3, 30)),
                            PaddingEnd = TimeSpan.FromMinutes(0),
                            PaddingStart = TimeSpan.FromMinutes(0),
                        },
                        new() {
                            Name = "Event #3 - Breakfast",
                            StartTime = new DateTime(date, new TimeOnly(5, 30)),
                            EndTime = new DateTime(date, new TimeOnly(9, 0)),
                            PaddingEnd = TimeSpan.FromMinutes(0),
                            PaddingStart = TimeSpan.FromMinutes(0),
                        },
                        new() {
                            Name = "Event #1",
                            StartTime = new DateTime(date, new TimeOnly(0, 0)),
                            EndTime = new DateTime(date, new TimeOnly(2, 30)),
                            PaddingEnd = TimeSpan.FromMinutes(0),
                            PaddingStart = TimeSpan.FromMinutes(0),
                        },
                    };

            DataMock.Setup(x => x.GetBookingsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .Returns(
                    Task.FromResult(new ServiceResult<List<Appointment>>(mockList))
                );

            var result = await ViewService.GetEvents(date, date);

            Assert.Equal(mockList[2], result[0].Event);
            Assert.Equal(mockList[0], result[1].Event);
            Assert.Equal(mockList[1], result[2].Event);

            // Ensure we have no clashes by checking all items set to EventLayoutConsts.EventsWidthPc% width
            foreach (var item in result)
            {
                Assert.Equal(AppointmentLayoutConsts.EventsWidthPc, item.WidthPc(date));
            }
        }


        [Fact]
        public async Task EventViewService_NoPadding_Clashes()
        {
            /* <----- Time ---->
            * <-- Event #1 (1 clash) -->     <-- Event #3 --> (1 clash) -->
            *            <!--     Event #2   (2 clash)-->
            */

            var date = new DateOnly(2024, 6, 5);

            var mockList = new List<Core.Dto.Appointment>
                    {
                        new() {
                            Name = "Event #1",
                            StartTime = new DateTime(date, new TimeOnly(1, 30)),
                            EndTime = new DateTime(date, new TimeOnly(3, 30)),
                            PaddingEnd = TimeSpan.FromMinutes(0),
                            PaddingStart = TimeSpan.FromMinutes(0),
                        },
                        new() {
                            Name = "Event #2 - Example",
                            StartTime = new DateTime(date, new TimeOnly(2, 30)),
                            EndTime = new DateTime(date, new TimeOnly(5, 30)),
                            PaddingEnd = TimeSpan.FromMinutes(0),
                            PaddingStart = TimeSpan.FromMinutes(0),
                        },
                        new() {
                            Name = "Event #3 - Breakfast",
                            StartTime = new DateTime(date, new TimeOnly(4, 0)),
                            EndTime = new DateTime(date, new TimeOnly(7, 0)),
                            PaddingEnd = TimeSpan.FromMinutes(0),
                            PaddingStart = TimeSpan.FromMinutes(0),
                        }
                    };

            DataMock.Setup(x => x.GetBookingsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .Returns(
                    Task.FromResult(new ServiceResult<List<Appointment>>(mockList)));

            var result = await ViewService.GetEvents(date, date);

            Assert.Equal(mockList[0], result[0].Event);
            Assert.Equal(mockList[1], result[1].Event);
            Assert.Equal(mockList[2], result[2].Event);

            Assert.Equal(0, result[0].LeftPc(date));

            var width1 = result[0].WidthPc(date);
            Assert.NotEqual(0, width1);

            var event2Left = result[1].LeftPc(date);
            var event3Left = result[2].LeftPc(date);

            Assert.NotEqual(0, event2Left);
            Assert.Equal(0, event3Left);

            Assert.Equal(width1, result[1].WidthPc(date));
            Assert.Equal(width1, result[2].WidthPc(date));
        }

        [Fact]
        public async Task EventViewService_Padding_NoClashes()
        {
            /** <----- Time ---->
             * <-- <-- Event 1 --> -->  <-<-Event 2->->       <----<Event 3>-->
             */

            var date = new DateOnly(2024, 6, 5);

            var mockList = new List<Core.Dto.Appointment>
                    {
                        new() {
                            Name = "Event #1",
                            StartTime = new DateTime(date, new TimeOnly(1, 00)),
                            EndTime = new DateTime(date, new TimeOnly(2, 30)),
                            PaddingEnd = TimeSpan.FromMinutes(45),
                            PaddingStart = TimeSpan.FromMinutes(45),
                        },
                        new() {
                            Name = "Event #2 - Example",
                            StartTime = new DateTime(date, new TimeOnly(8, 0)),
                            EndTime = new DateTime(date, new TimeOnly(10, 30)),
                            PaddingEnd = TimeSpan.FromMinutes(15),
                            PaddingStart = TimeSpan.FromMinutes(15),
                        },
                        new() {
                            Name = "Event #3 - Breakfast",
                            StartTime = new DateTime(date, new TimeOnly(17, 0)),
                            EndTime = new DateTime(date, new TimeOnly(18, 0)),
                            PaddingEnd = TimeSpan.FromMinutes(180),
                            PaddingStart = TimeSpan.FromMinutes(60),
                        }
                    };

            DataMock.Setup(x => x.GetBookingsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .Returns(
                    Task.FromResult(new ServiceResult<List<Appointment>>(mockList))
                );

            var result = await ViewService.GetEvents(date, date);

            Assert.Equal(mockList[0], result[0].Event);
            Assert.Equal(mockList[1], result[1].Event);
            Assert.Equal(mockList[2], result[2].Event);

            // Ensure we have no clashes by checking all items set to EventLayoutConsts.EventsWidthPc% width
            foreach (var item in result)
            {
                Assert.Equal(AppointmentLayoutConsts.EventsWidthPc, item.WidthPc(date));
                Assert.Equal(0, item.LeftPc(date));
            }
        }

        [Fact]
        public async Task EventViewService_Padding_Clashes()
        {
            /** <----- Time ---->
             * <-- <-- Event 1 --> -->       <----<Event 3>-->
             *                  <-<-Event 2->->
             */

            var date = new DateOnly(2024, 6, 5);

            var mockList = new List<Core.Dto.Appointment>
                    {
                        new() {
                            Name = "Event #1",
                            StartTime = new DateTime(date, new TimeOnly(1, 00)),
                            EndTime = new DateTime(date, new TimeOnly(2, 30)),
                            PaddingEnd = TimeSpan.FromMinutes(45),
                            PaddingStart = TimeSpan.FromMinutes(45),
                        },
                        new() {
                            Name = "Event #2 - Example",
                            StartTime = new DateTime(date, new TimeOnly(2, 0)),
                            EndTime = new DateTime(date, new TimeOnly(5, 30)),
                            PaddingEnd = TimeSpan.FromMinutes(15),
                            PaddingStart = TimeSpan.FromMinutes(30),
                        },
                        new() {
                            Name = "Event #3 - Breakfast",
                            StartTime = new DateTime(date, new TimeOnly(6, 30)),
                            EndTime = new DateTime(date, new TimeOnly(8, 0)),
                            PaddingEnd = TimeSpan.FromMinutes(60),
                            PaddingStart = TimeSpan.FromMinutes(60),
                        }
                    };

            DataMock.Setup(x => x.GetBookingsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .Returns(
                    Task.FromResult(new ServiceResult<List<Appointment>>(mockList))
                );

            var result = await ViewService.GetEvents(date, date);

            Assert.Equal(mockList[0], result[0].Event);
            Assert.Equal(mockList[1], result[1].Event);
            Assert.Equal(mockList[2], result[2].Event);

            var width = result[0].WidthPc(date);
            Assert.Equal(0, result[0].LeftPc(date));
            Assert.NotEqual(AppointmentLayoutConsts.EventsWidthPc, width);

            Assert.NotEqual(0, result[1].LeftPc(date));
            Assert.Equal(width, result[1].WidthPc(date));

            Assert.Equal(0, result[2].LeftPc(date));
            Assert.Equal(width, result[2].WidthPc(date));
        }

        [Fact]
        public async Task EventViewService_MultipleDays_MultipleNumClashes()
        {
            /**
             *              |                           |           |
             * <1> <4> <5>      <6><7>            <------<9>---------->
             * <2>                 <8>              <10>     <11>
             *   <3>
             */

            var day1 = new DateOnly(2020, 1, 1);

            var mockList = new List<Appointment>
            {
                //region day1
                new Appointment
                {
                    Name = "1",
                    StartTime = new DateTime(day1, new TimeOnly(1, 0)),
                    EndTime = new DateTime(day1, new TimeOnly(2, 0)),
                },
                new Appointment
                {
                    Name = "2",
                    StartTime = new DateTime(day1, new TimeOnly(1, 0)),
                    EndTime = new DateTime(day1, new TimeOnly(2, 0)),
                },
                new Appointment
                {
                    Name = "3",
                    StartTime = new DateTime(day1, new TimeOnly(1, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(3, 0)),
                },
                new Appointment
                {
                    Name = "4",
                    StartTime = new DateTime(day1, new TimeOnly(2, 30)),
                    EndTime = new DateTime(day1, new TimeOnly(3, 30)),
                },
                new Appointment
                {
                    Name = "5",
                    StartTime = new DateTime(day1, new TimeOnly(4, 0)),
                    EndTime = new DateTime(day1, new TimeOnly(5, 30)),
                },
                //endregion day1

                //day2
                new Appointment
                {
                    Name = "6",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(9, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(12, 0)),
                },
                new Appointment
                {
                    Name = "7",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(13, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(14, 30)),
                },
                new Appointment
                {
                    Name = "8",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(13, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(14, 30)),
                },
                new Appointment
                {
                    Name = "9",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(22, 0)),
                    EndTime = new DateTime(day1.AddDays(3), new TimeOnly(1, 30)),
                    PaddingStart = TimeSpan.FromMinutes(180),
                    PaddingEnd = TimeSpan.FromMinutes(100)
                },
                new Appointment
                {
                    Name = "10",
                    StartTime = new DateTime(day1.AddDays(1), new TimeOnly(23, 0)),
                    EndTime = new DateTime(day1.AddDays(1), new TimeOnly(23, 59, 59)),
                },

                //day 3
                new Appointment
                {
                    Name = "11",
                    StartTime = new DateTime(day1.AddDays(2), new TimeOnly(2, 45)),
                    EndTime = new DateTime(day1.AddDays(2), new TimeOnly(4, 50)),
                },
            };


            DataMock.Setup(x => x.GetBookingsBetweenDates(It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .Returns(
                    Task.FromResult(new ServiceResult<List<Appointment>>(mockList))
                );

            var result = await ViewService.GetEvents(day1, day1.AddDays(2));

            Assert.True(result.Count == mockList.Count);

            //------- Check Widths -----------
            var widthThird = result[0].WidthPc(day1);
            var widthHalf = (int)(AppointmentLayoutConsts.EventsWidthPc / 2f);
            Assert.Equal(widthThird, result[1].WidthPc(day1));
            Assert.Equal(widthThird, result[2].WidthPc(day1));

            // 1 clash for "4", so it should fill half the space
            Assert.Equal(widthHalf, result[3].WidthPc(day1));

            // Nothing stopping "5" filling max size.
            Assert.Equal(AppointmentLayoutConsts.EventsWidthPc, result[4].WidthPc(day1));

            // Day 2
            Assert.Equal(AppointmentLayoutConsts.EventsWidthPc, result[5].WidthPc(day1.AddDays(1)));

            Assert.Equal(widthHalf, result[6].WidthPc(day1.AddDays(1)));
            Assert.Equal(widthHalf, result[7].WidthPc(day1.AddDays(1)));
            Assert.Equal(widthHalf, result[8].WidthPc(day1.AddDays(1)));
            Assert.Equal(widthHalf, result[9].WidthPc(day1.AddDays(1)));

            // Day 3
            Assert.Equal(widthHalf, result[8].WidthPc(day1.AddDays(2)));
            Assert.Equal(widthHalf, result[10].WidthPc(day1.AddDays(2)));

            // Day 4 - nothing in the way, should fill max
            Assert.Equal(AppointmentLayoutConsts.EventsWidthPc, result[10].WidthPc(day1.AddDays(3)));


            // ----- Check Lefts -----
            // day 1
            Assert.Equal(0, result[0].LeftPc(day1));
            Assert.True(result[0].LeftPc(day1) < result[1].LeftPc(day1));
            Assert.True(result[1].LeftPc(day1) < result[2].LeftPc(day1));
            Assert.Equal(0, result[3].LeftPc(day1));
            Assert.Equal(0, result[4].LeftPc(day1));

            // day 2
            Assert.Equal(0, result[5].LeftPc(day1.AddDays(1)));
            Assert.Equal(0, result[6].LeftPc(day1.AddDays(1)));
            Assert.True(result[6].LeftPc(day1.AddDays(1)) < result[7].LeftPc(day1.AddDays(1)));

            Assert.Equal(0, result[8].LeftPc(day1.AddDays(1)));
            Assert.True(result[8].LeftPc(day1.AddDays(1)) < result[9].LeftPc(day1.AddDays(1)));

            // day 3
            Assert.Equal(0, result[8].LeftPc(day1.AddDays(2)));
            Assert.True(result[8].LeftPc(day1.AddDays(2)) < result[10].LeftPc(day1.AddDays(2)));

            // day 4
            Assert.Equal(0, result[8].LeftPc(day1.AddDays(3)));
        }
    }
}
