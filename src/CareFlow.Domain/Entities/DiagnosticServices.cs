using System;
using System.Collections.Generic;
using System.Text;

namespace CareFlow.Domain.Entities;

internal class DiagnosticServices
{
    public int DiagnosticServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public int DailyCapacity { get; set; }
}
