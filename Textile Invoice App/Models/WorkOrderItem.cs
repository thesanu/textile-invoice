using System.ComponentModel.DataAnnotations.Schema;

namespace Textile_Invoice_App.Models
{
    [Table("WORK_ORDER_ITEMS")]
    public partial class WorkOrderItem
    {
        [Column("ITEM_ID")] public int ItemId { get; set; }
        [Column("WO_ID")] public int WoId { get; set; }
        [Column("COMPANY_PROFILE_ID")] public int CompanyProfileId { get; set; }
        [Column("DESIGN_ID")] public int? DesignId { get; set; }
        [Column("PARTY_DESIGN_NAME")] public string? PartyDesignName { get; set; }
        [Column("QTY")] public decimal? Qty { get; set; }
        [Column("UNIT")] public string? Unit { get; set; }
        [Column("PCS")] public int? Pcs { get; set; }
        [Column("REMARKS")] public string? Remarks { get; set; }
    }
}