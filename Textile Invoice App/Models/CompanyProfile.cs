using System;
using System.Collections.Generic;

namespace Textile_Invoice_App.Models;

public partial class CompanyProfile
{
    public int CompanyProfileId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Gstin { get; set; }

    public string? Pan { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Pincode { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Status { get; set; }

    public byte[]? LogoImage { get; set; }
}
