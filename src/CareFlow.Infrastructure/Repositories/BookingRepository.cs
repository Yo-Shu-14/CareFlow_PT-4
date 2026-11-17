using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareFlow.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDBContext _context;

    public BookingRepository(AppDBContext context)
    {
        _context = context;
    }

    public async Task<bool> PatientExistsAsync(int patientId)
    {
        return await _context.Patients
            .AnyAsync(patient => patient.PatientId == patientId);
    }

    public async Task<List<DiagnosticServiceAvailabilityDto>>
        GetServiceAvailabilityAsync(DateTime bookingDate)
    {
        var services = await _context.DiagnosticServices
            .AsNoTracking()
            .OrderBy(service => service.ServiceName)
            .ToListAsync();

        var counts = await _context.BookingServices
            .Where(item => item.Booking.BookingDate == bookingDate.Date)
            .GroupBy(item => item.DiagnosticServiceId)
            .Select(group => new
            {
                DiagnosticServiceId = group.Key,
                BookedCount = group.Count()
            })
            .ToListAsync();

        var countLookup = counts.ToDictionary(
            item => item.DiagnosticServiceId,
            item => item.BookedCount);

        return services.Select(service =>
        {
            var bookedCount = countLookup.TryGetValue(
                service.DiagnosticServiceId, out var count)
                ? count
                : 0;

            var remaining = service.DailyCapacity - bookedCount;

            return new DiagnosticServiceAvailabilityDto
            {
                DiagnosticServiceId = service.DiagnosticServiceId,
                ServiceName = service.ServiceName,
                DailyCapacity = service.DailyCapacity,
                BookedCount = bookedCount,
                RemainingCapacity = remaining,
                IsAvailable = remaining > 0
            };
        }).ToList();
    }

    public async Task<int?> CreateBookingAsync(
        int patientId,
        DateTime bookingDate,
        List<int> diagnosticServiceIds)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Insert the booking inside the transaction first.
            // This obtains SQLite's write lock before checking capacity.
            var booking = new Booking
            {
                PatientId = patientId,
                BookingDate = bookingDate.Date
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var services = await _context.DiagnosticServices
                .Where(service =>
                    diagnosticServiceIds.Contains(
                        service.DiagnosticServiceId))
                .ToListAsync();

            if (services.Count != diagnosticServiceIds.Count)
            {
                await transaction.RollbackAsync();
                return null;
            }

            // Recheck capacity immediately before saving the services.
            foreach (var service in services)
            {
                var bookedCount = await _context.BookingServices
                    .CountAsync(item =>
                        item.DiagnosticServiceId ==
                            service.DiagnosticServiceId &&
                        item.Booking.BookingDate == bookingDate.Date);

                if (bookedCount >= service.DailyCapacity)
                {
                    await transaction.RollbackAsync();
                    return null;
                }
            }

            foreach (var serviceId in diagnosticServiceIds)
            {
                _context.BookingServices.Add(new BookingService
                {
                    BookingId = booking.BookingId,
                    DiagnosticServiceId = serviceId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return booking.BookingId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Booking>> GetBookingsByDateAsync(
        DateTime bookingDate)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Patient)
            .Include(booking => booking.BookingServices)
                .ThenInclude(item => item.DiagnosticService)
            .Where(booking => booking.BookingDate == bookingDate.Date)
            .OrderBy(booking => booking.BookingId)
            .ToListAsync();
    }
}
