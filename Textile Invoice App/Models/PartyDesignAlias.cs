using System.ComponentModel.DataAnnotations.Schema;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App.Models
{
    [Table("PARTY_DESIGN_ALIAS")]
    public partial class PartyDesignAlias
    {
        [Column("ALIAS_ID")] public int AliasId { get; set; }
        [Column("COMPANY_PROFILE_ID")] public int CompanyProfileId { get; set; }
        [Column("ACCOUNT_ID")] public int AccountId { get; set; }
        [Column("PARTY_DESIGN_NAME")] public string PartyDesignName { get; set; } = null!;
        [Column("DESIGN_ID")] public int DesignId { get; set; }
        [Column("NOTES")] public string? Notes { get; set; }
        public virtual Account? Account { get; set; }
        public virtual DesignMaster? Design { get; set; }
    }
}