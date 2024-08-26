using Admin.Data.ServiceTypes;
using Core.Dto;
using Core.Dto.Services;

namespace Admin.Tests.Data.ServiceType
{
    public class ServiceTypeModelTests
    {

        [Fact]
        public void ServiceTypeModel_MapsToModel()
        {

        }

        [Fact]
        public void ServiceTypeModel_NewService_HasDefaults()
        {
            var model = new ServiceTypeModel(new ServiceTypeDto());

            Assert.DoesNotContain(model.MonthlyAbsoluteRepeat, x => x);
            Assert.Equal(0, model.MonthlyRelativeRepeat.Count(x => x.Value.Any(y => y.Value)));
            Assert.Equal(ServiceRepeaterTypeDto.Weekly, model.Service.RepeatType);

            // Monday - Friday should be enabled by default
            for (int i = 1; i <= 5; i++)
            {
                Assert.True(model.WeeklyRepeat[(DayOfWeek)i]);
            }
            Assert.False(model.WeeklyRepeat[DayOfWeek.Saturday]);
            Assert.False(model.WeeklyRepeat[DayOfWeek.Sunday]);
        }

        [Fact]
        public void ServiceTypeModel_NewService_MapsBack()
        {
            var model = new ServiceTypeModel(new ServiceTypeDto());

            var mappedDefault = model.MapToDto();

            Assert.Equal(ServiceRepeaterTypeDto.Weekly, mappedDefault.RepeatType);

            Assert.Equal(5, mappedDefault.Repeats.Count);

            // Should not contain weekends by default
            Assert.DoesNotContain(mappedDefault.Repeats, x => x.DayIdentifier == 0);
            Assert.DoesNotContain(mappedDefault.Repeats, x => x.DayIdentifier == 6);
        }

        [Fact]
        public void ServiceTypeModel_Existing_MapsWeekly()
        {
            List<ServiceRepeaterDto> repeats = [
                    new ServiceRepeaterDto(DayOfWeek.Monday),
                    new ServiceRepeaterDto(DayOfWeek.Wednesday),
                ];

            var model = new ServiceTypeModel(new ServiceTypeDto()
            {
                RepeatType = ServiceRepeaterTypeDto.Weekly,
                Repeats = new List<ServiceRepeaterDto>(repeats)
            });

            Assert.Equal(ServiceRepeaterTypeDto.Weekly, model.Service.RepeatType);

            Assert.Equal(2, model.WeeklyRepeat.Count(x => x.Value));
            Assert.True(model.WeeklyRepeat[DayOfWeek.Monday]);
            Assert.True(model.WeeklyRepeat[DayOfWeek.Wednesday]);


            var mapped = model.MapToDto();

            Assert.Equal(ServiceRepeaterTypeDto.Weekly, mapped.RepeatType);

            Assert.Equal(repeats, mapped.Repeats);
        }

        [Fact]
        public void ServiceTypeModel_Existing_MapsMonthlyAbsolute()
        {
            List<ServiceRepeaterDto> repeats = [
                new ServiceRepeaterDto(1), // 1st
                new ServiceRepeaterDto(10), // 10th
                new ServiceRepeaterDto(27), //27th
                ];

            var model = new ServiceTypeModel(new ServiceTypeDto()
            {
                RepeatType = ServiceRepeaterTypeDto.MonthlyAbsolute,
                Repeats = repeats
            });

            Assert.Equal(3, model.MonthlyAbsoluteRepeat.Count(x => x));
            Assert.True(model.MonthlyAbsoluteRepeat[0]);
            Assert.True(model.MonthlyAbsoluteRepeat[9]);
            Assert.True(model.MonthlyAbsoluteRepeat[26]);

            model.MonthlyAbsoluteRepeat[0] = false; // DayIndentifier 1
            model.MonthlyAbsoluteRepeat[1] = true; // DayIdentifier 2
            model.MonthlyAbsoluteRepeat[3] = true; // etc
            model.MonthlyAbsoluteRepeat[30] = true;

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
            List<ServiceRepeaterDto> repeats = [
                new ServiceRepeaterDto(DayOfWeek.Monday, 1), // First Monday
                new ServiceRepeaterDto(DayOfWeek.Tuesday, 1), // First Tuesday
                new ServiceRepeaterDto(DayOfWeek.Friday, 1), // First Friday

                new ServiceRepeaterDto(DayOfWeek.Saturday, 2),
                new ServiceRepeaterDto(DayOfWeek.Sunday, 2),

                new ServiceRepeaterDto(DayOfWeek.Thursday, 3),

                new ServiceRepeaterDto(DayOfWeek.Friday, -1), // Last Friday
                ];

            var model = new ServiceTypeModel(new ServiceTypeDto()
            {
                RepeatType = ServiceRepeaterTypeDto.MonthlyRelative,
                Repeats = repeats
            });

            Assert.Equal(3, model.MonthlyRelativeRepeat[1].Where(x => x.Value).Count());
            Assert.True(model.MonthlyRelativeRepeat[1][DayOfWeek.Monday]);
            Assert.True(model.MonthlyRelativeRepeat[1][DayOfWeek.Tuesday]);
            Assert.True(model.MonthlyRelativeRepeat[1][DayOfWeek.Friday]);


            Assert.Equal(2, model.MonthlyRelativeRepeat[2].Where(x => x.Value).Count());
            Assert.True(model.MonthlyRelativeRepeat[2][DayOfWeek.Saturday]);
            Assert.True(model.MonthlyRelativeRepeat[2][DayOfWeek.Sunday]);

            Assert.Single(model.MonthlyRelativeRepeat[3].Where(x => x.Value));
            Assert.True(model.MonthlyRelativeRepeat[3][DayOfWeek.Thursday]);

            Assert.Single(model.MonthlyRelativeRepeat[-1].Where(x => x.Value));
            Assert.True(model.MonthlyRelativeRepeat[-1][DayOfWeek.Friday]);

            model.MonthlyRelativeRepeat[1][DayOfWeek.Monday] = false;
            model.MonthlyRelativeRepeat[1][DayOfWeek.Tuesday] = false;
            model.MonthlyRelativeRepeat[1][DayOfWeek.Friday] = false;
            model.MonthlyRelativeRepeat[-1][DayOfWeek.Friday] = false;
            model.MonthlyRelativeRepeat[-1][DayOfWeek.Monday] = true;

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
