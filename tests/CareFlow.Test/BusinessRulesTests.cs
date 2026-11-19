using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CareFlow.Application.DTOs;
using CareFlow.Application.Services;
using CareFlow.Infrastructure.Data;
using CareFlow.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareFlow.Test;

public class BusinessRulesTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDBContext> _options;

    public BusinessRulesTests()
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

    private (BookingService Service, AppDBContext Context) CreateService()
    {
        var context = new AppDBContext(_options);
        var repository = new BookingRepository(context);
        var service = new BookingService(repository);
        return (service, context);
    }

    [Fact]
    public async Task BR01_CreateBooking_NonExistentPatient_FailsWithFeedback()
    {
        // Arrange
        var (service, context) = CreateService();
        await using (context)
        {
            var request = new BookingRequestDto
            {
                PatientId = 9999, // Does not exist
                BookingDate = DateTime.Today.AddDays(1),
                DiagnosticServiceIds = new List<int> { 1 }
            };

            // Act
            var result = await service.CreateBookingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("patient does not exist", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task BR02_CreateBooking_EmptyServices_FailsWithFeedback()
    {
        // Arrange
        var (service, context) = CreateService();
        await using (context)
        {
            var request = new BookingRequestDto
            {
                PatientId = 1,
                BookingDate = DateTime.Today.AddDays(1),
                DiagnosticServiceIds = new List<int>()
            };

            // Act
            var result = await service.CreateBookingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("between 1 and 3", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task BR02_CreateBooking_MoreThanThreeServices_FailsWithFeedback()
    {
        // Arrange
        var (service, context) = CreateService();
        await using (context)
        {
            var request = new BookingRequestDto
            {
                PatientId = 1,
                BookingDate = DateTime.Today.AddDays(1),
                DiagnosticServiceIds = new List<int> { 1, 2, 3, 4 }
            };

            // Act
            var result = await service.CreateBookingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("between 1 and 3", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task BR03_CreateBooking_PastDate_FailsWithFeedback()
    {
        // Arrange
        var (service, context) = CreateService();
        await using (context)
        {
            var request = new BookingRequestDto
            {
                PatientId = 1,
                BookingDate = DateTime.Today.AddDays(-1), // Past date
                DiagnosticServiceIds = new List<int> { 1 }
            };

            // Act
            var result = await service.CreateBookingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("past", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task BR06_CreateBooking_DuplicateServicesInRequest_FailsWithFeedback()
    {
        // Arrange
        var (service, context) = CreateService();
        await using (context)
        {
            var request = new BookingRequestDto
            {
                PatientId = 1,
                BookingDate = DateTime.Today.AddDays(1),
                DiagnosticServiceIds = new List<int> { 1, 1 } // Duplicate
            };

            // Act
            var result = await service.CreateBookingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("duplicate", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
