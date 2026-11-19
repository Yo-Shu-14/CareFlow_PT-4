namespace CareFlow.Application.DTOs;

public class PatientDto
{
    public int PatientId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
