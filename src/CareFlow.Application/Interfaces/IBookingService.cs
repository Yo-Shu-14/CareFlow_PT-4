using CareFlow.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using CareFlow.Application.DTOs;

namespace CareFlow.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResultDto> CreateBookingAsync(BookingRequestDto request);

    Task<List<BookingResultDto>> GetBookingsByDateAsync(DateTime bookingDate);
}