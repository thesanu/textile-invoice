using System;
using System.Collections.Generic;

namespace Textile_Invoice_App.Models;

public partial class InvoiceNumberTracker
{
    public int CompanyProfileId { get; set; }

    public int? CurrentInvoiceNo { get; set; }
}
