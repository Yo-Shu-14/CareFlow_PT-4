using System;
using System.Collections.Generic;
using System.Text;

namespace CareFlow.Domain.Entities;

internal class Booking
{
    public int BookingId { get; set; }

    public int PatientId { get; set; }

    public DateTime BookingDate { get; set; }

    public Patient Patient { get; set; } = null!;

    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
}
