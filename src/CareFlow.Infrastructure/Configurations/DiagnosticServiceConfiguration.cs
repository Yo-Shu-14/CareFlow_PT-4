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
    }
}
