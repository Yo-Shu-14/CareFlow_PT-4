using System;
using System.Collections.Generic;
using System.Text;
using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareFlow.Infrastructure.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.BookingId);

        builder.Property(b => b.BookingDate)
            .IsRequired();

        builder.HasOne(b => b.Patient)
            .WithMany()
            .HasForeignKey(b => b.PatientId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.PatientId, b.BookingDate });
    }
}
