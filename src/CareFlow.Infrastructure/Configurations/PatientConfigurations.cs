using CareFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace CareFlow.Infrastructure.Configurations;

public class PatientConfigurations : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(p => p.PatientId);

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(100);


        builder.HasData(
            new Patient
            {
                PatientId = 1,
                FirstName = "Juan",
                LastName = "Dela Cruz"
            },
            new Patient
            {
                PatientId = 2,
                FirstName = "Maria",
                LastName = "Santos"
            },
            new Patient
            {
                PatientId = 3,
                FirstName = "Pedro",
                LastName = "Reyes"
            },
            new Patient
            {
                PatientId = 4,
                FirstName = "Ana",
                LastName = "Garcia"
            },
            new Patient
            {
                PatientId = 5,
                FirstName = "Carlos",
                LastName = "Mendoza"
            }
        );

    }
}
