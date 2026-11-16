using System;
using System.Collections.Generic;
using System.Text;

namespace CareFlow.Domain.Entities;

internal class BookingService
{
    public int BookingServiceId { get; set; }

    public int BookingId { get; set; }

    public int DiagnosticServiceId { get; set; }

    public Booking Booking { get; set; } = null!;

    public DiagnosticServices DiagnosticService { get; set; } = null!;
}
