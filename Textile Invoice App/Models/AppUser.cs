using System;
using System.Collections.Generic;

namespace Textile_Invoice_App.Models;

public partial class AppUser
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FullName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int CompanyProfileId { get; set; }
}
