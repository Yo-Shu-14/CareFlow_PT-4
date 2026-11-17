using System;
using System.Collections.Generic;
using System.Text;

namespace CareFlow.Application.DTOs;


public class BookingRequestDto
{
    public int PatientId { get; set; }

    public DateTime BookingDate { get; set; }

    public List<int> DiagnosticServiceIds { get; set; } = new();
}
