using System;
using System.Collections.Generic;
using System.Text;

namespace CareFlow.Domain.Entities;

internal class Patient
{
    public int PatientId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}
