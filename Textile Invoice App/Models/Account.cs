using System;
using System.Collections.Generic;

namespace Textile_Invoice_App.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public int CompanyProfileId { get; set; }

    public string? AccCode { get; set; }

    public string AccNm { get; set; } = null!;

    public string? GroupNm { get; set; }

    public string? Gstin { get; set; }

    public string? Pan { get; set; }

    public string? BillAdd1 { get; set; }

    public string? BillAdd2 { get; set; }

    public string? BillAdd3 { get; set; }

    public string? BillCity { get; set; }

    public string? BillPincode { get; set; }

    public string? BillDistrict { get; set; }

    public string? BillState { get; set; }

    public string? BillPhone1 { get; set; }

    public string? BillPhone2 { get; set; }

    public string? EmailAdd { get; set; }

    public string? ShipAdd { get; set; }

    public int? BrokerId { get; set; }

    public int? TransportId { get; set; }

    public string? Type { get; set; }
}
