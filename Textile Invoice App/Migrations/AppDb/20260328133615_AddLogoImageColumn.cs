using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Textile_Invoice_App.Migrations.AppDb
{ 
    /// <inheritdoc />
    public partial class AddLogoImageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyProfileId = table.Column<int>(type: "int", nullable: false),
                    AccCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccNm = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupNm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gstin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillAdd1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillAdd2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillAdd3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillPincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillDistrict = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillPhone1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillPhone2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAdd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShipAdd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BrokerId = table.Column<int>(type: "int", nullable: true),
                    TransportId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ACCOUNTS__05B22F604961FA81", x => x.AccountId);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    CompanyProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__APP_USER__F3BEEBFF9D9D931A", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "CompanyProfiles",
                columns: table => new
                {
                    CompanyProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gstin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__COMPANY___1522A7AFA8E44C72", x => x.CompanyProfileId);
                });

            migrationBuilder.CreateTable(
                name: "DesignMasters",
                columns: table => new
                {
                    DesignId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyProfileId = table.Column<int>(type: "int", nullable: false),
                    DesignName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HsnCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DESIGN_M__C541F1369641403B", x => x.DesignId);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceHeaders",
                columns: table => new
                {
                    InvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyProfileId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    BrokerId = table.Column<int>(type: "int", nullable: true),
                    TransportId = table.Column<int>(type: "int", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Cgst = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Sgst = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Igst = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ChallanNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChallanDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CgstPct = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SgstPct = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IgstPct = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Roundup = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__INVOICE___0CE91F0882C04866", x => x.InvoiceId);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    DesignId = table.Column<int>(type: "int", nullable: true),
                    CoChNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PChNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HsnCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pcs = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Per = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CompanyProfileId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__INVOICE___ADFD89A01201DE24", x => x.ItemId);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceNumberTrackers",
                columns: table => new
                {
                    CompanyProfileId = table.Column<int>(type: "int", nullable: false),
                    CurrentInvoiceNo = table.Column<int>(type: "int", nullable: true, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__INVOICE___1522A7AF3AA41D6A", x => x.CompanyProfileId);
                });

            migrationBuilder.CreateTable(
                name: "ServiceMasters",
                columns: table => new
                {
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyProfileId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SERVICE___30358F5A6D97F45D", x => x.ServiceId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "CompanyProfiles");

            migrationBuilder.DropTable(
                name: "DesignMasters");

            migrationBuilder.DropTable(
                name: "InvoiceHeaders");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "InvoiceNumberTrackers");

            migrationBuilder.DropTable(
                name: "ServiceMasters");
        }
    }
}
