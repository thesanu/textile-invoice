using System;
using System.Collections.Generic;

namespace Textile_Invoice_App.Models;

public partial class ServiceMaster
{
    public int ServiceId { get; set; }

    public string? ServiceName { get; set; }

    public string? Description { get; set; }

    public int? CompanyProfileId { get; set; }
}
