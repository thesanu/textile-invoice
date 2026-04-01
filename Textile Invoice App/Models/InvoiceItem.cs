using System;
using System.Collections.Generic;

namespace Textile_Invoice_App.Models;

public partial class InvoiceItem
{
    public int ItemId { get; set; }

    public int InvoiceId { get; set; }

    public int? DesignId { get; set; }

    public string? CoChNo { get; set; }

    public string? PChNo { get; set; }

    public string? HsnCode { get; set; }

    public int? Pcs { get; set; }

    public decimal? Qty { get; set; }

    public decimal? Rate { get; set; }

    public string? Per { get; set; }

    public decimal? Amount { get; set; }

    public int? CompanyProfileId { get; set; }
}
