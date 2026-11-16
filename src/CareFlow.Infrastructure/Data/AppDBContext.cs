using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using CareFlow.Domain.Entities;

namespace CareFlow.Infrastructure.Data;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<DiagnosticServices> DiagnosticServices => Set<DiagnosticServices>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<BookingService> BookingServices => Set<BookingService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDBContext).Assembly);
    }
}
