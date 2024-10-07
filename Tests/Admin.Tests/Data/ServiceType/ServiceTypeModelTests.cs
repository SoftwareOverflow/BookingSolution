using Admin.Data.ServiceTypes;
using Core.Dto;

namespace Admin.Tests.Data.ServiceType
{
    public class ServiceTypeModelTests
    {
        [Fact]
        public void ServiceTypeModel_NewService_HasDefaults()
        {
            var model = new ServiceTypeModel(new ServiceTypeDto());

            Assert.DoesNotContain(model.Repeater.MonthlyAbsoluteRepeat, x => x);
            Assert.Equal(0, model.Repeater.MonthlyRelativeRepeat.Count(x => x.Value.Any(y => y.Value)));
            Assert.Equal(RepeaterTypeDto.Weekly, model.Repeater.RepeatType);

            // Monday - Friday should be enabled by default
            for (int i = 1; i <= 5; i++)
            {
                Assert.True(model.Repeater.WeeklyRepeat[(DayOfWeek)i]);
            }
            Assert.False(model.Repeater.WeeklyRepeat[DayOfWeek.Saturday]);
            Assert.False(model.Repeater.WeeklyRepeat[DayOfWeek.Sunday]);
        }

        [Fact]
        public void ServiceTypeModel_NewService_MapsDefaults()
        {
            var model = new ServiceTypeModel(new ServiceTypeDto());

            var mappedDefault = model.MapToDto();

            Assert.Equal(RepeaterTypeDto.Weekly, mappedDefault.RepeatType);

            Assert.Equal(5, mappedDefault.Repeats.Count);

            // Should not contain weekends by default
            Assert.DoesNotContain(mappedDefault.Repeats, x => x.DayIdentifier == 0);
            Assert.DoesNotContain(mappedDefault.Repeats, x => x.DayIdentifier == 6);
        }

        [Fact]
        public void ServiceTypeModel_Existing_MapsWeekly()
        {
            List<RepeaterDto> repeats = [
                    new RepeaterDto(DayOfWeek.Monday),
                    new RepeaterDto(DayOfWeek.Wednesday),
                ];

            var model = new ServiceTypeModel(new ServiceTypeDto()
            {
                RepeatType = RepeaterTypeDto.Weekly,
                Repeats = new List<RepeaterDto>(repeats)
            });

            Assert.Equal(RepeaterTypeDto.Weekly, model.Service.RepeatType);

            Assert.Equal(2, model.Repeater.WeeklyRepeat.Count(x => x.Value));
            Assert.True(model.Repeater.WeeklyRepeat[DayOfWeek.Monday]);
            Assert.True(model.Repeater.WeeklyRepeat[DayOfWeek.Wednesday]);


            var mapped = model.MapToDto();

            Assert.Equal(RepeaterTypeDto.Weekly, mapped.RepeatType);

            Assert.Equal(repeats, mapped.Repeats);
        }

        [Fact]
        public void ServiceTypeModel_Existing_MapsMonthlyAbsolute()
        {
            List<RepeaterDto> repeats = [
                new RepeaterDto(1), // 1st
                new RepeaterDto(10), // 10th
                new RepeaterDto(27), //27th
                ];

            var model = new ServiceTypeModel(new ServiceTypeDto()
            {
                RepeatType = RepeaterTypeDto.MonthlyAbsolute,
                Repeats = repeats
            });

            Assert.Equal(3, model.Repeater.MonthlyAbsoluteRepeat.Count(x => x));
            Assert.True(model.Repeater.MonthlyAbsoluteRepeat[0]);
            Assert.True(model.Repeater.MonthlyAbsoluteRepeat[9]);
            Assert.True(model.Repeater.MonthlyAbsoluteRepeat[26]);

            model.Repeater.MonthlyAbsoluteRepeat[0] = false; // DayIndentifier 1
            model.Repeater.MonthlyAbsoluteRepeat[1] = true; // DayIdentifier 2
            model.Repeater.MonthlyAbsoluteRepeat[3] = true; // etc
            model.Repeater.MonthlyAbsoluteRepeat[30] = true;

            var mapped = model.MapToDto();
            Assert.Equal(5, mapped.Repeats.Count);
            Assert.Contains(mapped.Repeats, x => x.DayIdentifier == 2);
            Assert.Contains(mapped.Repeats, x => x.DayIdentifier == 4);
            Assert.Contains(mapped.Repeats, x => x.DayIdentifier == 10); // Original
            Assert.Contains(mapped.Repeats, x => x.DayIdentifier == 27); // Original
            Assert.Contains(mapped.Repeats, x => x.DayIdentifier == 31);
        }

        [Fact]
        public void ServiceTypeModel_Existing_MapsMonthlyRelative()
        {
            List<RepeaterDto> repeats = [
                new RepeaterDto(DayOfWeek.Monday, 1), // First Monday
                new RepeaterDto(DayOfWeek.Tuesday, 1), // First Tuesday
                new RepeaterDto(DayOfWeek.Friday, 1), // First Friday

                new RepeaterDto(DayOfWeek.Saturday, 2),
                new RepeaterDto(DayOfWeek.Sunday, 2),

                new RepeaterDto(DayOfWeek.Thursday, 3),

                new RepeaterDto(DayOfWeek.Friday, -1), // Last Friday
                ];

            var model = new ServiceTypeModel(new ServiceTypeDto()
            {
                RepeatType = RepeaterTypeDto.MonthlyRelative,
                Repeats = repeats
            });

            Assert.Equal(3, model.Repeater.MonthlyRelativeRepeat[1].Where(x => x.Value).Count());
            Assert.True(model.Repeater.MonthlyRelativeRepeat[1][DayOfWeek.Monday]);
            Assert.True(model.Repeater.MonthlyRelativeRepeat[1][DayOfWeek.Tuesday]);
            Assert.True(model.Repeater.MonthlyRelativeRepeat[1][DayOfWeek.Friday]);


            Assert.Equal(2, model.Repeater.MonthlyRelativeRepeat[2].Where(x => x.Value).Count());
            Assert.True(model.Repeater.MonthlyRelativeRepeat[2][DayOfWeek.Saturday]);
            Assert.True(model.Repeater.MonthlyRelativeRepeat[2][DayOfWeek.Sunday]);

            Assert.Single(model.Repeater.MonthlyRelativeRepeat[3].Where(x => x.Value));
            Assert.True(model.Repeater.MonthlyRelativeRepeat[3][DayOfWeek.Thursday]);

            Assert.Single(model.Repeater.MonthlyRelativeRepeat[-1].Where(x => x.Value));
            Assert.True(model.Repeater.MonthlyRelativeRepeat[-1][DayOfWeek.Friday]);

            model.Repeater.MonthlyRelativeRepeat[1][DayOfWeek.Monday] = false;
            model.Repeater.MonthlyRelativeRepeat[1][DayOfWeek.Tuesday] = false;
            model.Repeater.MonthlyRelativeRepeat[1][DayOfWeek.Friday] = false;
            model.Repeater.MonthlyRelativeRepeat[-1][DayOfWeek.Friday] = false;
            model.Repeater.MonthlyRelativeRepeat[-1][DayOfWeek.Monday] = true;

            var mapped = model.MapToDto();
            Assert.Equal(4, mapped.Repeats.Count);

            Assert.DoesNotContain(mapped.Repeats, x => x.Index == 1);

            Assert.Contains(mapped.Repeats, x => x.DayIdentifier == (int)DayOfWeek.Saturday && x.Index == 2);
            Assert.Contains(mapped.Repeats, x => x.DayIdentifier == (int)DayOfWeek.Sunday && x.Index == 2);
            
            Assert.Contains(mapped.Repeats, x => x.DayIdentifier == (int)DayOfWeek.Thursday && x.Index == 3);

            Assert.Contains(mapped.Repeats, x => x.DayIdentifier == (int)DayOfWeek.Monday && x.Index == -1);
        }


        [Fact]
        public void ServiceTypeModel_ChangeRepeatType_MapsCorrectly()
        {

        }
    }
}
