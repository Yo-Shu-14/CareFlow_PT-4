using System;
using System.Collections.Generic;
using System.Text;
using CareFlow.Domain.Entities;
using CareFlow.Application.DTOs;

namespace CareFlow.Application.Interfaces;


public interface IBookingRepository
{
    Task<bool> PatientExistsAsync(int patientId);

    Task<List<DiagnosticServiceAvailabilityDto>>
        GetServiceAvailabilityAsync(DateTime bookingDate);

    Task<int?> CreateBookingAsync(
        int patientId,
        DateTime bookingDate,
        List<int> diagnosticServiceIds);

    Task<List<Booking>> GetBookingsByDateAsync(DateTime bookingDate);
}
