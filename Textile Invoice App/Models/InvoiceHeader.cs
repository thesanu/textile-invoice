using System;
using System.Collections.Generic;

namespace Textile_Invoice_App.Models;

public partial class InvoiceHeader
{
    public int InvoiceId { get; set; }

    public int CompanyProfileId { get; set; }

    public string? InvoiceNo { get; set; }

    public DateOnly InvoiceDate { get; set; }

    public int ClientId { get; set; }

    public int? BrokerId { get; set; }

    public int? TransportId { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? Cgst { get; set; }

    public decimal? Sgst { get; set; }

    public decimal? Igst { get; set; }

    public decimal? GrandTotal { get; set; }

    public string? ChallanNo { get; set; }

    public DateOnly? ChallanDate { get; set; }

    public decimal? CgstPct { get; set; }

    public decimal? SgstPct { get; set; }

    public decimal? IgstPct { get; set; }

    public decimal? Roundup { get; set; }
}
