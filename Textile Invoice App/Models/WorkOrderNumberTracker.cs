using System.ComponentModel.DataAnnotations.Schema;

namespace Textile_Invoice_App.Models
{
    [Table("WORK_ORDER_NUMBER_TRACKER")]
    public partial class WorkOrderNumberTracker
    {
        [Column("COMPANY_PROFILE_ID")] public int CompanyProfileId { get; set; }
        [Column("CURRENT_WO_NO")] public int CurrentWoNo { get; set; }
    }
}