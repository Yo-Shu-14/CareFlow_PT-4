using System;
using System.Collections.Generic;
using System.Text;
using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareFlow.Infrastructure.Configurations;

public class DiagnosticServiceConfiguration : IEntityTypeConfiguration<DiagnosticServices>
{
    public void Configure(EntityTypeBuilder<DiagnosticServices> builder)
    {
        builder.HasKey(s => s.DiagnosticServiceId);

        builder.Property(s => s.ServiceName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.DailyCapacity)
            .IsRequired();

        builder.ToTable(table =>
            table.HasCheckConstraint(
                "CK_DiagnosticService_DailyCapacity",
                "\"DailyCapacity\" > 0"));


        builder.HasData(
            new DiagnosticServices
            {
                DiagnosticServiceId = 1,
                ServiceName = "Complete Blood Count (CBC)",
                DailyCapacity = 20
            },
            new DiagnosticServices
            {
                DiagnosticServiceId = 2,
                ServiceName = "Chest X-Ray",
                DailyCapacity = 10
            },
            new DiagnosticServices
            {
                DiagnosticServiceId = 3,
                ServiceName = "Ultrasound",
                DailyCapacity = 8
            },
            new DiagnosticServices
            {
                DiagnosticServiceId = 4,
                ServiceName = "Electrocardiogram (ECG)",
                DailyCapacity = 12
            },
            new DiagnosticServices
            {
                DiagnosticServiceId = 5,
                ServiceName = "Blood Chemistry",
                DailyCapacity = 15
            }
        );

    }
}
