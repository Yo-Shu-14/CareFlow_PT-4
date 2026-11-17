using System;
using System.Collections.Generic;
using System.Text;

namespace CareFlow.Application.DTOs;

public class DiagnosticServiceAvailabilityDto
{
    public int DiagnosticServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public int DailyCapacity { get; set; }

    public int BookedCount { get; set; }

    public int RemainingCapacity { get; set; }

    public bool IsAvailable { get; set; }
}
