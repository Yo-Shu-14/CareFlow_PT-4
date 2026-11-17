using System;
using System.Collections.Generic;
using System.Text;

namespace CareFlow.Application.DTOs;

public class BookingResultDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? BookingId { get; set; }
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public DateTime BookingDate { get; set; }

    public List<string> DiagnosticServiceNames { get; set; } = new();
}
