using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareFlow.Application.DTOs;

namespace CareFlow.Application.Interfaces;

public interface IBookingService
{
    Task<List<PatientDto>> GetPatientsAsync();

    Task<List<DiagnosticServiceAvailabilityDto>> GetServiceAvailabilityAsync(DateTime bookingDate);

    Task<BookingResultDto> CreateBookingAsync(BookingRequestDto request);

    Task<List<BookingResultDto>> GetBookingsByDateAsync(DateTime bookingDate);

    Task<List<BookingResultDto>> GetAllBookingsAsync();
}
