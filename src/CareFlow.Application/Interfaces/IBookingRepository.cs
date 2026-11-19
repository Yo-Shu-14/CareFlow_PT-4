using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareFlow.Application.DTOs;
using CareFlow.Domain.Entities;

namespace CareFlow.Application.Interfaces;

public interface IBookingRepository
{
    Task<bool> PatientExistsAsync(int patientId);

    Task<List<Patient>> GetPatientsAsync();

    Task<Patient?> GetPatientByIdAsync(int patientId);

    Task<List<DiagnosticServiceAvailabilityDto>> GetServiceAvailabilityAsync(DateTime bookingDate);

    Task<int?> CreateBookingAsync(
        int patientId,
        DateTime bookingDate,
        List<int> diagnosticServiceIds);

    Task<List<Booking>> GetBookingsByDateAsync(DateTime bookingDate);

    Task<List<Booking>> GetAllBookingsAsync();
}
