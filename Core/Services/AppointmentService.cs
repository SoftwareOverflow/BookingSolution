﻿using AutoMapper;
using Core.Dto.Appointment;
using Core.Interfaces;
using Core.Responses;
using Data.Entity.Appointments;
using Data.Interfaces;

namespace Core.Services
{
    internal class AppointmentService(IAppointmentRepo appointmentContext, IMapper mapper) : IAppointmentService
    {
        private readonly IAppointmentRepo _appointmentContext = appointmentContext;

        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// Get all <see cref="AppointmentDto"/> objects in a given date range.
        /// </summary>
        /// <param name="start">The start date of the range (inclusive)</param>
        /// <param name="end">The end date of the range (inclusive)</param>
        /// <returns>Any events which occur in whole or part within the specified range</returns>
        public Task<ServiceResult<List<AppointmentDtoBase>>> GetAppointmentsBetweenDates(DateOnly start, DateOnly end)
        {
            try
            {
                var result = _appointmentContext.GetAppointmentsBetweenDates(start, end);
                var dtos = _mapper.Map<ICollection<AppointmentDtoBase>>(result);

                return Task.FromResult(new ServiceResult<List<AppointmentDtoBase>>([.. dtos]));
            }
            catch (Exception)
            {
                // TODO logging
            }

            return Task.FromResult(ServiceResult<List<AppointmentDtoBase>>.DefaultServerFailure());
        }


        public async Task<ServiceResult<AppointmentDto>> CreateOrUpdateAppointment(AppointmentDto appointment)
        {
            try
            {
                var entity = _mapper.Map<Appointment>(appointment);

                bool result = false;
                if (entity.Guid == Guid.Empty)
                {
                    result = await _appointmentContext.Create(entity);
                }
                else
                {
                    result = await _appointmentContext.Update(entity);
                }

                if (result)
                {
                    appointment = _mapper.Map<AppointmentDto>(entity);

                    return new ServiceResult<AppointmentDto>(appointment);
                }
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<AppointmentDto>.DefaultServerFailure();
        }

        public async Task<ServiceResult<bool>> DeleteAppointment(Guid id)
        {
            try
            {
                var result = await _appointmentContext.DeleteAppointment(id);

                if (result)
                {
                    return new ServiceResult<bool>(result);
                }
            }
            catch (Exception)
            {
                // TODO logging
            }

            return ServiceResult<bool>.DefaultServerFailure();
        }
    }
}
