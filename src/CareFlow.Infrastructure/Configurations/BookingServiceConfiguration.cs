using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CareFlow.Domain.Entities;

namespace CareFlow.Infrastructure.Configurations;

public class BookingServiceConfiguration : IEntityTypeConfiguration<BookingService>
{
    public void Configure(EntityTypeBuilder<BookingService> builder)
    {
        builder.HasKey(bs => bs.BookingServiceId);

        builder.HasOne(bs => bs.Booking)
            .WithMany(b => b.BookingServices)
            .HasForeignKey(bs => bs.BookingId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bs => bs.DiagnosticService)
            .WithMany()
            .HasForeignKey(bs => bs.DiagnosticServiceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bs => new
        {
            bs.BookingId,
            bs.DiagnosticServiceId
        }).IsUnique();
    }
}
