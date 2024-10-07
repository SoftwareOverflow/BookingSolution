using Admin.Data.Appointments.Model;
using Core.Dto;
using Core.Dto.Appointment;

namespace Admin.Tests.Data.Appointments.Model
{
    public class AppointmentModelTests
    {

        [Fact]
        public void AppointmentModel_ValuesUpdate()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            var services = new List<ServiceTypeDto>()
            {
                new ()
                {
                    Guid = guid1,
                    Name = "Service 1",
                },

                new ()
                {
                    Guid = guid2,
                    Name = "Service 2"
                }
            };

            var person = new PersonDto() { FirstName = "Sally", LastName = "Smith", EmailAddress = "SmithSally@somewhere.com", PhoneNumber = "0123456789" };
            var appointment = new AppointmentDto("Some Name", person) { StartTime = new DateTime(2024, 8, 13, 14, 30, 0), EndTime = new DateTime(2024, 8, 13, 16, 0, 0), Service = services[1] };
            var model = new AppointmentModel(appointment, services);

            // Assert that backed values are reading correctly
            Assert.Equal("Some Name", model.Name);

            Assert.Equivalent(new DateTime(2024, 8, 13), model.StartDate!.Value.Date);
            Assert.Equivalent(new DateTime(2024, 8, 13), model.EndDate!.Value.Date);

            Assert.Equivalent(new TimeSpan(14, 30, 0), model.StartTime);
            Assert.Equivalent(new TimeSpan(16, 0, 0), model.EndTime);

            Assert.Equal(guid2, model.ServiceGuid);

            // Assert that backed values are updated correctly
            model.ServiceGuid = guid2;
            model.Name = "Another name";
            model.StartDate = new DateTime(2024, 9, 14);
            model.StartTime = new TimeSpan(9, 30, 0);
            model.EndDate = new DateTime(2024, 9, 14);
            model.EndTime = new TimeSpan(11, 45, 0);


            Assert.Equal("Another name", model.Appointment.Name);
            Assert.Equivalent(new DateTime(2024, 9, 14, 9, 30, 0), model.Appointment.StartTime);
            Assert.Equivalent(new DateTime(2024, 9, 14, 11, 45, 0), model.Appointment.EndTime);
            Assert.Equivalent(services[1], appointment.Service);

        }

        [Fact]
        public void AppointmentModel_NoServices()
        {
            var service = new ServiceTypeDto() { Guid = Guid.NewGuid() };
            var model = new AppointmentModel(new AppointmentDto("", new PersonDto()) { Service = service }, []);

            model.ServiceGuid = Guid.NewGuid(); // Won't match, shouldn't change our appointment service

            Assert.Equivalent(service, model.Appointment.Service);
        }

        [Fact]
        public void AppointmentModel_UnselectGuid()
        {
            var service = new ServiceTypeDto() { Guid = Guid.NewGuid() };
            var model = new AppointmentModel(new AppointmentDto("", new PersonDto()) { Service = service }, []);

            model.ServiceGuid = null;
            Assert.Null(model.Appointment.Service);
        }

        [Fact]
        public void AppointmentModel_DurationMismatchError()
        {
            var service = new ServiceTypeDto() { DurationMins = 30 };

            var services = new List<ServiceTypeDto>()
            {
                service,
                new () {Guid = Guid.NewGuid(), DurationMins = 60 },
            };

            var appointment = new AppointmentDto("", new PersonDto()) { Service = service };
            var model = new AppointmentModel(appointment, services);

            var date = DateTime.Now;
            model.StartDate = date;
            model.EndDate = date;
            model.StartTime = new TimeSpan(9, 0, 0);
            model.EndTime = new TimeSpan(10, 0, 0);

            Assert.Contains(model.Errors, x => x == AppointmentModelError.ServiceDurationMismatch);

            model.StartTime = new TimeSpan(9, 30, 0);

            Assert.DoesNotContain(model.Errors, x => x == AppointmentModelError.ServiceDurationMismatch);

            // Check that it removes when we change the service and it matches
            model.EndTime = new TimeSpan(10, 30, 0);
            Assert.Contains(model.Errors, x => x == AppointmentModelError.ServiceDurationMismatch);
            model.ServiceGuid = services[1].Guid;
            Assert.DoesNotContain(model.Errors, x => x == AppointmentModelError.ServiceDurationMismatch);

            // Check it removes when we remove the assosciated service
            model.StartTime = new TimeSpan(6, 15, 0);
            Assert.Contains(model.Errors, x => x == AppointmentModelError.ServiceDurationMismatch);
            model.ServiceGuid = null;
            Assert.DoesNotContain(model.Errors, x => x == AppointmentModelError.ServiceDurationMismatch);
        }

        [Fact]
        public void AppointmentModel_NameMismatchError()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            var services = new List<ServiceTypeDto>()
            {
                new ()
                {
                    Guid = guid1,
                    Name = "Service 1",
                },

                new ()
                {
                    Guid = guid2,
                    Name = "Service 2"
                }
            };

            var dto = new AppointmentDto("Service 1", new PersonDto())
            {
                Service = services[0]
            };
            var model = new AppointmentModel(dto, services);

            // Check matches start without
            model.Name = "Service 1";
            Assert.DoesNotContain(model.Errors, x => x == AppointmentModelError.ServiceNameMismatch);

            // Update Service - check name warning
            model.ServiceGuid = guid2;
            Assert.Contains(model.Errors, x => x == AppointmentModelError.ServiceNameMismatch);

            // Check error fix
            var error = model.Errors.Single(x => x == AppointmentModelError.ServiceNameMismatch);
            Assert.NotNull(error.Fix);
            error.Fix.Invoke(model);

            Assert.Equal("Service 2", model.Name);
            Assert.DoesNotContain(model.Errors, x => x == AppointmentModelError.ServiceNameMismatch);

            // Check manual name change triggers warning
            model.Name = "Anything but 'Service 2'";
            Assert.Contains(model.Errors, x => x == AppointmentModelError.ServiceNameMismatch);

            // Check manual name update removes warning
            model.Name = "Service 2";
            Assert.DoesNotContain(model.Errors, x => x == AppointmentModelError.ServiceNameMismatch);
        }
    }
}
