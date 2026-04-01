    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DesignMaster
    {
        public int DesignId { get; set; }

        public int CompanyProfileId { get; set; }

        public string DesignName { get; set; } = null!;

        public string? HsnCode { get; set; }

        public decimal? DefaultRate { get; set; }

        public string? Unit { get; set; }

        // ✅ ADD THIS
        [Column("DESIGN_IMAGE")]
        public byte[]? DesignImage { get; set; }
    }   