using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Textile_Invoice_App.Models
{
    [Table("WORK_ORDER_HEADER")]
    public partial class WorkOrderHeader
    {
        [Column("WO_ID")] public int WoId { get; set; }
        [Column("COMPANY_PROFILE_ID")] public int CompanyProfileId { get; set; }
        [Column("WO_DATE")] public DateOnly WoDate { get; set; }
        [Column("WO_NO")] public string WoNo { get; set; } = null!;
        [Column("ACCOUNT_ID")] public int? AccountId { get; set; }
        [Column("CHALLAN_NO")] public string? ChallanNo { get; set; }
        [Column("CHALLAN_DATE")] public DateOnly? ChallanDate { get; set; }
        [Column("REMARKS")] public string? Remarks { get; set; }
        [Column("STATUS")] public string Status { get; set; } = "Pending";
        [Column("CREATED_AT")] public DateTime CreatedAt { get; set; }
        public virtual ICollection<WorkOrderItem> Items { get; set; } = new List<WorkOrderItem>();
    }
}