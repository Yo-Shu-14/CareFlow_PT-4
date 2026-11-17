using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace CareFlow.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;

    public BookingService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<BookingResultDto> CreateBookingAsync(
        BookingRequestDto request)
    {
        if (request is null)
        {
            return Failed("Invalid booking request.");
        }

        if (request.PatientId <= 0)
        {
            return Failed("Please select a valid patient.");
        }

        if (request.BookingDate.Date < DateTime.Today)
        {
            return Failed("Booking date cannot be in the past.");
        }

        if (request.DiagnosticServiceIds is null ||
            request.DiagnosticServiceIds.Count < 1 ||
            request.DiagnosticServiceIds.Count > 3)
        {
            return Failed("Please select between 1 and 3 diagnostic services.");
        }

        if (request.DiagnosticServiceIds.Any(id => id <= 0))
        {
            return Failed("Invalid diagnostic service selection.");
        }

        if (request.DiagnosticServiceIds.Distinct().Count()
            != request.DiagnosticServiceIds.Count)
        {
            return Failed("Duplicate diagnostic services are not allowed.");
        }

        if (!await _bookingRepository.PatientExistsAsync(request.PatientId))
        {
            return Failed("The selected patient does not exist.");
        }

        var availability =
            await _bookingRepository.GetServiceAvailabilityAsync(
                request.BookingDate.Date);

        var selectedServices = availability
            .Where(service =>
                request.DiagnosticServiceIds.Contains(
                    service.DiagnosticServiceId))
            .ToList();

        if (selectedServices.Count != request.DiagnosticServiceIds.Count)
        {
            return Failed("One or more selected diagnostic services do not exist.");
        }

        if (selectedServices.Any(service =>
            service.RemainingCapacity <= 0))
        {
            return Failed("One or more selected services have reached daily capacity.");
        }

        try
        {
            var bookingId = await _bookingRepository.CreateBookingAsync(
                request.PatientId,
                request.BookingDate.Date,
                request.DiagnosticServiceIds);

            if (bookingId is null)
            {
                return Failed(
                    "Availability has changed. One or more services are now fully booked.");
            }

            return new BookingResultDto
            {
                Success = true,
                Message = "Booking successfully created.",
                BookingId = bookingId,
                PatientId = request.PatientId,
                BookingDate = request.BookingDate.Date,
                DiagnosticServiceNames = selectedServices
                    .Select(service => service.ServiceName)
                    .ToList()
            };
        }
        catch (Exception)
        {
            return Failed(
                "The booking could not be saved due to a database error.");
        }
    }

    public async Task<List<BookingResultDto>> GetBookingsByDateAsync(
        DateTime bookingDate)
    {
        var bookings = await _bookingRepository.GetBookingsByDateAsync(
            bookingDate.Date);

        return bookings.Select(booking => new BookingResultDto
        {
            Success = true,
            BookingId = booking.BookingId,
            PatientId = booking.PatientId,
            PatientName = $"{booking.Patient.FirstName} {booking.Patient.LastName}",
            BookingDate = booking.BookingDate,
            DiagnosticServiceNames = booking.BookingServices
                .Select(bs => bs.DiagnosticService.ServiceName)
                .ToList()
        }).ToList();
    }

    private static BookingResultDto Failed(string message)
    {
        return new BookingResultDto
        {
            Success = false,
            Message = message
        };
    }
}
