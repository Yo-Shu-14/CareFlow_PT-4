using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareFlow.Application.DTOs;
using CareFlow.Application.Interfaces;
using AppBookingService = CareFlow.Application.Services.BookingService;
using CareFlow.Domain.Entities;
using CareFlow.Infrastructure.Data;
using CareFlow.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareFlow.Test;

public class AcceptanceScenariosTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDBContext> _options;

    public AcceptanceScenariosTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDBContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDBContext(_options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private (IBookingService Service, IBookingRepository Repository, AppDBContext Context) CreateContext()
    {
        var context = new AppDBContext(_options);
        var repository = new BookingRepository(context);
        var service = new AppBookingService(repository);
        return (service, repository, context);
    }

    [Fact]
    public async Task AC01_ViewDiagnosticServices_RemainingCapacityCorrectlyCalculatedFromPersistedBookings()
    {
        // Arrange
        var testDate = DateTime.Today.AddDays(2);
        var (service, repository, context) = CreateContext();
        await using (context)
        {
            // Initial check: CBC has DailyCapacity = 20
            var initialAvailability = await service.GetServiceAvailabilityAsync(testDate);
            var cbcInitial = initialAvailability.First(s => s.DiagnosticServiceId == 1);
            Assert.Equal(20, cbcInitial.RemainingCapacity);
            Assert.Equal(0, cbcInitial.BookedCount);

            // Persist a booking consuming 1 slot of CBC
            var bookingResult = await service.CreateBookingAsync(new BookingRequestDto
            {
                PatientId = 1,
                BookingDate = testDate,
                DiagnosticServiceIds = new List<int> { 1 }
            });
            Assert.True(bookingResult.Success);

            // Act: Re-query availability
            var updatedAvailability = await service.GetServiceAvailabilityAsync(testDate);
            var cbcUpdated = updatedAvailability.First(s => s.DiagnosticServiceId == 1);

            // Assert: Remaining capacity must now be 19, BookedCount 1
            Assert.Equal(1, cbcUpdated.BookedCount);
            Assert.Equal(19, cbcUpdated.RemainingCapacity);
            Assert.True(cbcUpdated.IsAvailable);
        }
    }

    [Fact]
    public async Task AC02_CreateBooking_MultipleAvailableServices_OneBookingRecordedWithAllServices()
    {
        // Arrange
        var testDate = DateTime.Today.AddDays(3);
        var (service, _, context) = CreateContext();
        await using (context)
        {
            var request = new BookingRequestDto
            {
                PatientId = 2,
                BookingDate = testDate,
                DiagnosticServiceIds = new List<int> { 1, 2, 4 } // CBC, Chest X-Ray, ECG
            };

            // Act
            var result = await service.CreateBookingAsync(request);

            // Assert: Operation successful
            Assert.True(result.Success);
            Assert.NotNull(result.BookingId);
            Assert.Equal(3, result.DiagnosticServiceNames.Count);

            // Query database directly to verify exactly 1 booking record with 3 associated services
            var persistedBooking = await context.Bookings
                .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.DiagnosticService)
                .FirstOrDefaultAsync(b => b.BookingId == result.BookingId.Value);

            Assert.NotNull(persistedBooking);
            Assert.Equal(2, persistedBooking.PatientId);
            Assert.Equal(testDate.Date, persistedBooking.BookingDate);
            Assert.Equal(3, persistedBooking.BookingServices.Count);

            var serviceIds = persistedBooking.BookingServices.Select(bs => bs.DiagnosticServiceId).ToList();
            Assert.Contains(1, serviceIds);
            Assert.Contains(2, serviceIds);
            Assert.Contains(4, serviceIds);
        }
    }

    [Fact]
    public async Task AC03_RequestFullyBookedService_RejectedWithoutIncompleteOrInvalidRecords()
    {
        // Arrange: Service 3 (Ultrasound) has capacity of 8
        var testDate = DateTime.Today.AddDays(4);
        var (service, _, context) = CreateContext();
        await using (context)
        {
            var ultrasound = await context.DiagnosticServices.FindAsync(3);
            Assert.NotNull(ultrasound);
            int capacity = ultrasound.DailyCapacity; // 8

            // Fill all capacity slots for Ultrasound
            for (int i = 0; i < capacity; i++)
            {
                var fillResult = await service.CreateBookingAsync(new BookingRequestDto
                {
                    PatientId = 1,
                    BookingDate = testDate,
                    DiagnosticServiceIds = new List<int> { 3 }
                });
                Assert.True(fillResult.Success);
            }

            int totalBookingsBefore = await context.Bookings.CountAsync();
            int totalBookingServicesBefore = await context.BookingServices.CountAsync();

            // Act: Request booking that includes the fully booked Ultrasound alongside an available service (CBC)
            var overbookingRequest = new BookingRequestDto
            {
                PatientId = 2,
                BookingDate = testDate,
                DiagnosticServiceIds = new List<int> { 1, 3 }
            };

            var failResult = await service.CreateBookingAsync(overbookingRequest);

            // Assert
            Assert.False(failResult.Success);
            Assert.Contains("capacity", failResult.Message, StringComparison.OrdinalIgnoreCase);

            // Verify no partial booking or orphan records were created
            int totalBookingsAfter = await context.Bookings.CountAsync();
            int totalBookingServicesAfter = await context.BookingServices.CountAsync();

            Assert.Equal(totalBookingsBefore, totalBookingsAfter);
            Assert.Equal(totalBookingServicesBefore, totalBookingServicesAfter);
        }
    }

    [Fact]
    public async Task AC04_ControlledStaleAvailabilityScenario_RevalidatesAndPreventsOverbooking()
    {
        // Arrange
        var testDate = DateTime.Today.AddDays(5);
        var (service, repository, context) = CreateContext();
        await using (context)
        {
            // Set service 3 (Ultrasound) capacity to 1 for precise test
            var ultrasound = await context.DiagnosticServices.FindAsync(3);
            Assert.NotNull(ultrasound);
            ultrasound.DailyCapacity = 1;
            await context.SaveChangesAsync();

            // Step 1: Staff sees capacity available
            var availabilityBefore = await repository.GetServiceAvailabilityAsync(testDate);
            var ultrasoundAvail = availabilityBefore.First(s => s.DiagnosticServiceId == 3);
            Assert.Equal(1, ultrasoundAvail.RemainingCapacity);

            // Step 2: Concurrently / in background, another booking takes that last spot
            var concurrentBookingResult = await service.CreateBookingAsync(new BookingRequestDto
            {
                PatientId = 1,
                BookingDate = testDate,
                DiagnosticServiceIds = new List<int> { 3 }
            });
            Assert.True(concurrentBookingResult.Success);

            // Step 3: Staff submits previously prepared booking that thought spot was free
            var staleBookingResult = await service.CreateBookingAsync(new BookingRequestDto
            {
                PatientId = 2,
                BookingDate = testDate,
                DiagnosticServiceIds = new List<int> { 3 }
            });

            // Assert: System re-checks availability at commit time and rejects the overbooking
            Assert.False(staleBookingResult.Success);

            // Confirm only 1 booking was committed for that service on that date
            var totalBookedCount = await context.BookingServices
                .CountAsync(bs => bs.DiagnosticServiceId == 3 && bs.Booking.BookingDate == testDate);
            Assert.Equal(1, totalBookedCount);
        }
    }

    [Fact]
    public async Task AC05_DatabaseFailureBeforeCommit_RollsBackTransactionWithoutPartialPersistence()
    {
        // Arrange
        var testDate = DateTime.Today.AddDays(6);
        var (_, repository, context) = CreateContext();
        await using (context)
        {
            int bookingsBefore = await context.Bookings.CountAsync();
            int bookingServicesBefore = await context.BookingServices.CountAsync();

            // Act & Assert: Attempt to create booking with non-existent service ID inside the transaction
            // Repository checks service existence and aborts/rolls back transaction
            var bookingId = await repository.CreateBookingAsync(1, testDate, new List<int> { 1, 9999 });

            Assert.Null(bookingId);

            // Verify transaction atomicity: No booking record or partial booking service was persisted
            int bookingsAfter = await context.Bookings.CountAsync();
            int bookingServicesAfter = await context.BookingServices.CountAsync();

            Assert.Equal(bookingsBefore, bookingsAfter);
            Assert.Equal(bookingServicesBefore, bookingServicesAfter);
        }
    }

    [Fact]
    public async Task AC06_ApplicationRestartSimulation_PreservesConfirmedBookingsAcrossContextExecutions()
    {
        // Arrange
        var testDate = DateTime.Today.AddDays(7);
        int confirmedBookingId;

        // Context 1: Represents initial application execution
        using (var context1 = new AppDBContext(_options))
        {
            var repo1 = new BookingRepository(context1);
            var service1 = new AppBookingService(repo1);

            var result = await service1.CreateBookingAsync(new BookingRequestDto
            {
                PatientId = 3,
                BookingDate = testDate,
                DiagnosticServiceIds = new List<int> { 2, 4 } // Chest X-Ray and ECG
            });

            Assert.True(result.Success);
            Assert.NotNull(result.BookingId);
            confirmedBookingId = result.BookingId.Value;
        }

        // Context 2: Represents reopening the application after closing
        using (var context2 = new AppDBContext(_options))
        {
            var repo2 = new BookingRepository(context2);
            var service2 = new AppBookingService(repo2);

            var retrievedBookings = await service2.GetBookingsByDateAsync(testDate);
            var retrievedBooking = retrievedBookings.FirstOrDefault(b => b.BookingId == confirmedBookingId);

            // Assert: Booking remains persisted and all services are available
            Assert.NotNull(retrievedBooking);
            Assert.Equal(3, retrievedBooking.PatientId);
            Assert.Equal("Pedro Reyes", retrievedBooking.PatientName);
            Assert.Equal(2, retrievedBooking.DiagnosticServiceNames.Count);
            Assert.Contains("Chest X-Ray", retrievedBooking.DiagnosticServiceNames);
            Assert.Contains("Electrocardiogram (ECG)", retrievedBooking.DiagnosticServiceNames);
        }
    }
}
