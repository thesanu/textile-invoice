using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<CompanyProfile> CompanyProfiles { get; set; }

    public virtual DbSet<DesignMaster> DesignMasters { get; set; }

    public virtual DbSet<InvoiceHeader> InvoiceHeaders { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<InvoiceNumberTracker> InvoiceNumberTrackers { get; set; }

    public virtual DbSet<ServiceMaster> ServiceMasters { get; set; }

    // ── Work Order Module ─────────────────────────────────────────────
    public virtual DbSet<PartyDesignAlias> PartyDesignAliases { get; set; }
    public virtual DbSet<WorkOrderHeader> WorkOrderHeaders { get; set; }
    public virtual DbSet<WorkOrderItem> WorkOrderItems { get; set; }
    public virtual DbSet<WorkOrderNumberTracker> WorkOrderNumberTrackers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
     => optionsBuilder.UseSqlServer(
         "Server=localhost;Database=ashutosh_DB;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__ACCOUNTS__05B22F604961FA81");

            entity.ToTable("ACCOUNTS");

            entity.Property(e => e.AccountId).HasColumnName("ACCOUNT_ID");
            entity.Property(e => e.AccCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ACC_CODE");
            entity.Property(e => e.AccNm)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ACC_NM");
            entity.Property(e => e.BillAdd1)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("BILL_ADD1");
            entity.Property(e => e.BillAdd2)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("BILL_ADD2");
            entity.Property(e => e.BillAdd3)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("BILL_ADD3");
            entity.Property(e => e.BillCity)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BILL_CITY");
            entity.Property(e => e.BillDistrict)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BILL_DISTRICT");
            entity.Property(e => e.BillPhone1)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BILL_PHONE1");
            entity.Property(e => e.BillPhone2)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BILL_PHONE2");
            entity.Property(e => e.BillPincode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("BILL_PINCODE");
            entity.Property(e => e.BillState)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BILL_STATE");
            entity.Property(e => e.BrokerId).HasColumnName("BROKER_ID");
            entity.Property(e => e.CompanyProfileId).HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.EmailAdd)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("EMAIL_ADD");
            entity.Property(e => e.GroupNm)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("GROUP_NM");
            entity.Property(e => e.Gstin)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("GSTIN");
            entity.Property(e => e.Pan)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PAN");
            entity.Property(e => e.ShipAdd)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("SHIP_ADD");
            entity.Property(e => e.TransportId).HasColumnName("TRANSPORT_ID");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TYPE");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__APP_USER__F3BEEBFF9D9D931A");

            entity.ToTable("APP_USERS");

            entity.HasIndex(e => e.Username, "UQ__APP_USER__B15BE12E3C4F8534").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("USER_ID");
            entity.Property(e => e.CompanyProfileId).HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("CREATED_DATE");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("FULL_NAME");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IS_ACTIVE");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("PASSWORD_HASH");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("USERNAME");
        });

        modelBuilder.Entity<CompanyProfile>(entity =>
        {
            entity.HasKey(e => e.CompanyProfileId).HasName("PK__COMPANY___1522A7AFA8E44C72");

            entity.ToTable("COMPANY_PROFILE");

            entity.Property(e => e.CompanyProfileId).HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.Address1)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ADDRESS1");
            entity.Property(e => e.Address2)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("ADDRESS2");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("CITY");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("COMPANY_NAME");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Gstin)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("GSTIN");
            entity.Property(e => e.LogoImage).HasColumnName("LOGO_IMAGE");
            entity.Property(e => e.Pan)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PAN");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("PHONE");
            entity.Property(e => e.Pincode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("PINCODE");
            entity.Property(e => e.State)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("STATE");
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<DesignMaster>(entity =>
        {
            entity.HasKey(e => e.DesignId).HasName("PK__DESIGN_M__C541F1369641403B");

            entity.ToTable("DESIGN_MASTER");

            entity.Property(e => e.DesignId).HasColumnName("DESIGN_ID");
            entity.Property(e => e.CompanyProfileId).HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.DefaultRate)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("DEFAULT_RATE");
            entity.Property(e => e.DesignName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("DESIGN_NAME");
            entity.Property(e => e.HsnCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("HSN_CODE");
            entity.Property(e => e.Unit)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("UNIT");
        });

        modelBuilder.Entity<InvoiceHeader>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__INVOICE___0CE91F0882C04866");

            entity.ToTable("INVOICE_HEADER");

            entity.Property(e => e.InvoiceId).HasColumnName("INVOICE_ID");
            entity.Property(e => e.BrokerId).HasColumnName("BROKER_ID");
            entity.Property(e => e.Cgst)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("CGST");
            entity.Property(e => e.CgstPct)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("CGST_PCT");
            entity.Property(e => e.ChallanDate).HasColumnName("CHALLAN_DATE");
            entity.Property(e => e.ChallanNo)
                .HasMaxLength(50)
                .HasColumnName("CHALLAN_NO");
            entity.Property(e => e.ClientId).HasColumnName("CLIENT_ID");
            entity.Property(e => e.CompanyProfileId).HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.GrandTotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("GRAND_TOTAL");
            entity.Property(e => e.Igst)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("IGST");
            entity.Property(e => e.IgstPct)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("IGST_PCT");
            entity.Property(e => e.InvoiceDate).HasColumnName("INVOICE_DATE");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("INVOICE_NO");
            entity.Property(e => e.Roundup)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("ROUNDUP");
            entity.Property(e => e.Sgst)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SGST");
            entity.Property(e => e.SgstPct)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("SGST_PCT");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("TOTAL_AMOUNT");
            entity.Property(e => e.TransportId).HasColumnName("TRANSPORT_ID");
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__INVOICE___ADFD89A01201DE24");

            entity.ToTable("INVOICE_ITEMS");

            entity.Property(e => e.ItemId).HasColumnName("ITEM_ID");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("AMOUNT");
            entity.Property(e => e.CoChNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CO_CH_NO");
            entity.Property(e => e.CompanyProfileId).HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.DesignId).HasColumnName("DESIGN_ID");
            entity.Property(e => e.HsnCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("HSN_CODE");
            entity.Property(e => e.InvoiceId).HasColumnName("INVOICE_ID");
            entity.Property(e => e.PChNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("P_CH_NO");
            entity.Property(e => e.Pcs).HasColumnName("PCS");
            entity.Property(e => e.Per)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("PER");
            entity.Property(e => e.Qty)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("QTY");
            entity.Property(e => e.Rate)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("RATE");
        });

        modelBuilder.Entity<InvoiceNumberTracker>(entity =>
        {
            entity.HasKey(e => e.CompanyProfileId).HasName("PK__INVOICE___1522A7AF3AA41D6A");

            entity.ToTable("INVOICE_NUMBER_TRACKER");

            entity.Property(e => e.CompanyProfileId)
                .ValueGeneratedNever()
                .HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.CurrentInvoiceNo)
                .HasDefaultValue(1)
                .HasColumnName("CURRENT_INVOICE_NO");
        });

        modelBuilder.Entity<ServiceMaster>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__SERVICE___30358F5A6D97F45D");

            entity.ToTable("SERVICE_MASTER");

            entity.Property(e => e.ServiceId).HasColumnName("SERVICE_ID");
            entity.Property(e => e.CompanyProfileId).HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("SERVICE_NAME");
        });

        // ── Work Order Module ─────────────────────────────────────────

        modelBuilder.Entity<PartyDesignAlias>(entity =>
        {
            entity.HasKey(e => e.AliasId).HasName("PK_PARTY_DESIGN_ALIAS");
            entity.ToTable("PARTY_DESIGN_ALIAS");
            entity.HasIndex(e => new { e.CompanyProfileId, e.AccountId, e.PartyDesignName })
                  .IsUnique().HasDatabaseName("UQ_PDA_PARTY_DESIGN");
            entity.Property(e => e.AliasId).HasColumnName("ALIAS_ID");
            entity.Property(e => e.CompanyProfileId).HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.AccountId).HasColumnName("ACCOUNT_ID");
            entity.Property(e => e.PartyDesignName)
                .HasMaxLength(100).IsUnicode(false).HasColumnName("PARTY_DESIGN_NAME");
            entity.Property(e => e.DesignId).HasColumnName("DESIGN_ID");
            entity.Property(e => e.Notes)
                .HasMaxLength(200).IsUnicode(false).HasColumnName("NOTES");
        });

        modelBuilder.Entity<WorkOrderHeader>(entity =>
        {
            entity.HasKey(e => e.WoId).HasName("PK_WORK_ORDER_HEADER");
            entity.ToTable("WORK_ORDER_HEADER");
            entity.Property(e => e.WoId).HasColumnName("WO_ID");
            entity.Property(e => e.CompanyProfileId).HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.WoDate).HasColumnName("WO_DATE");
            entity.Property(e => e.WoNo)
                .HasMaxLength(30).IsUnicode(false).HasColumnName("WO_NO");
            entity.Property(e => e.AccountId).HasColumnName("ACCOUNT_ID");
            entity.Property(e => e.ChallanNo)
                .HasMaxLength(50).IsUnicode(false).HasColumnName("CHALLAN_NO");
            entity.Property(e => e.ChallanDate).HasColumnName("CHALLAN_DATE");
            entity.Property(e => e.Remarks)
                .HasMaxLength(300).IsUnicode(false).HasColumnName("REMARKS");
            entity.Property(e => e.Status)
                .HasMaxLength(20).IsUnicode(false).HasColumnName("STATUS")
                .HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime").HasColumnName("CREATED_AT")
                .HasDefaultValueSql("GETDATE()");
            entity.HasMany(e => e.Items)
                .WithOne()
                .HasForeignKey(i => i.WoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkOrderItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK_WORK_ORDER_ITEMS");
            entity.ToTable("WORK_ORDER_ITEMS");
            entity.Property(e => e.ItemId).HasColumnName("ITEM_ID");
            entity.Property(e => e.WoId).HasColumnName("WO_ID");
            entity.Property(e => e.CompanyProfileId).HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.DesignId).HasColumnName("DESIGN_ID");
            entity.Property(e => e.PartyDesignName)
                .HasMaxLength(100).IsUnicode(false).HasColumnName("PARTY_DESIGN_NAME");
            entity.Property(e => e.Qty)
                .HasColumnType("decimal(10,2)").HasColumnName("QTY");
            entity.Property(e => e.Unit)
                .HasMaxLength(10).IsUnicode(false).HasColumnName("UNIT")
                .HasDefaultValue("MTR");
            entity.Property(e => e.Pcs).HasColumnName("PCS").HasDefaultValue(0);
            entity.Property(e => e.Remarks)
                .HasMaxLength(200).IsUnicode(false).HasColumnName("REMARKS");
        });

        modelBuilder.Entity<WorkOrderNumberTracker>(entity =>
        {
            entity.HasKey(e => e.CompanyProfileId).HasName("PK_WO_TRACKER");
            entity.ToTable("WORK_ORDER_NUMBER_TRACKER");
            entity.Property(e => e.CompanyProfileId)
                .ValueGeneratedNever().HasColumnName("COMPANY_PROFILE_ID");
            entity.Property(e => e.CurrentWoNo)
                .HasColumnName("CURRENT_WO_NO").HasDefaultValue(0);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}