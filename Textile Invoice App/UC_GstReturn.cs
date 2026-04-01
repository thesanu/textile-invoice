using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App
{
    // ════════════════════════════════════════════════════════════════════
    //  UC_GstReturn — GSTR-1 / GSTR-3B filing helper
    //  BUG FIX: The original toolbar used absolute X positions so controls
    //  overlapped on narrower windows.  The fixed version uses a two-row
    //  responsive layout:
    //    Row 1 (height 38): Return Type | Period | Month/Quarter | Year
    //    Row 2 (height 38): [spacer]  Generate | Export Excel | Export PDF
    //  Both rows are laid out via Resize handlers so they always fit.
    // ════════════════════════════════════════════════════════════════════
    public partial class UC_GstReturn : UserControl
    {
        // ── Palette ───────────────────────────────────────────────────
        static readonly Color Bg = Color.FromArgb(245, 247, 250);
        static readonly Color Surface = Color.White;
        static readonly Color Surface2 = Color.FromArgb(248, 249, 252);
        static readonly Color Blue = Color.FromArgb(37, 99, 235);
        static readonly Color BlueL = Color.FromArgb(59, 130, 246);
        static readonly Color BluePale = Color.FromArgb(239, 246, 255);
        static readonly Color Green = Color.FromArgb(22, 163, 74);
        static readonly Color GreenPale = Color.FromArgb(240, 253, 244);
        static readonly Color Amber = Color.FromArgb(180, 83, 9);
        static readonly Color AmberPale = Color.FromArgb(255, 251, 235);
        static readonly Color Purple = Color.FromArgb(109, 40, 217);
        static readonly Color PurplePale = Color.FromArgb(245, 243, 255);
        static readonly Color Red = Color.FromArgb(220, 38, 38);
        static readonly Color Border = Color.FromArgb(226, 232, 240);
        static readonly Color RowLine = Color.FromArgb(241, 245, 249);
        static readonly Color HeaderBg = Color.FromArgb(241, 245, 249);
        static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        static readonly Color TextMid = Color.FromArgb(71, 85, 105);
        static readonly Color TextLight = Color.FromArgb(148, 163, 184);

        // ── Controls ─────────────────────────────────────────────────
        string _lastPeriodLabel = "";

        ComboBox _cmbReturnType, _cmbPeriodType, _cmbMonth, _cmbQuarter, _cmbYear;
        TabControl _tabs;
        DataGridView _dgvB2B, _dgvB2C, _dgv3B, _dgvHsn;

        // Toolbar row-1 selector labels (need refs to toggle visibility)
        Label _lblMonth, _lblQuarter;

        // ── Summary labels ────────────────────────────────────────────
        Label _lblTotalTaxable, _lblTotalCgst, _lblTotalSgst,
              _lblTotalIgst, _lblTotalGrand, _lblInvoiceCount;

        public UC_GstReturn()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Bg;
            Build();
        }

        // ════════════════════════════════════════════════════════════
        //  BUILD
        // ════════════════════════════════════════════════════════════
        void Build()
        {
            // ── Page header ──────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            hdr.Controls.Add(new Label
            {
                Text = "GST Return Filing",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            });
            hdr.Controls.Add(new Label
            {
                Text = SessionManager.CompanyName + "  •  GSTIN: " + GetCompanyGstin(),
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });

            // ════════════════════════════════════════════════════════
            //  TOOLBAR — two rows, fully responsive, no overlap
            //  Row 1 height = 42  (selectors)
            //  Row 2 height = 42  (action buttons)
            //  Outer panel height = 84 + 1 (border)
            // ════════════════════════════════════════════════════════
            var bar = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Surface2 };
            bar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                // divider between row1 and row2
                g.DrawLine(new Pen(Border), 0, 44, bar.Width, 44);
                // bottom border
                g.DrawLine(new Pen(Border), 0, bar.Height - 1, bar.Width, bar.Height - 1);
            };

            // ── ROW 1: selectors ─────────────────────────────────────
            const int ROW1_Y = 7;   // vertical centre of row 1
            const int CMB_H = 28;
            const int LBL_Y = ROW1_Y + 6;

            // Return Type
            var lblRet = MkLbl("Return Type:", 0, LBL_Y);
            _cmbReturnType = MkCmb(0, ROW1_Y, 110);
            _cmbReturnType.Items.AddRange(new object[] { "GSTR-1", "GSTR-3B" });
            _cmbReturnType.SelectedIndex = 0;

            // Period Type
            var lblPeriod = MkLbl("Period:", 0, LBL_Y);
            _cmbPeriodType = MkCmb(0, ROW1_Y, 100);
            _cmbPeriodType.Items.AddRange(new object[] { "Monthly", "Quarterly", "Annual" });
            _cmbPeriodType.SelectedIndex = 0;

            // Month selector
            _lblMonth = MkLbl("Month:", 0, LBL_Y);
            _cmbMonth = MkCmb(0, ROW1_Y, 108);
            for (int m = 1; m <= 12; m++)
                _cmbMonth.Items.Add(new DateTime(2000, m, 1).ToString("MMMM"));
            _cmbMonth.SelectedIndex = DateTime.Today.Month - 1;

            // Quarter selector
            _lblQuarter = MkLbl("Quarter:", 0, LBL_Y);
            _cmbQuarter = MkCmb(0, ROW1_Y, 120);
            _cmbQuarter.Items.AddRange(new object[] { "Q1 (Apr-Jun)", "Q2 (Jul-Sep)", "Q3 (Oct-Dec)", "Q4 (Jan-Mar)" });
            int cm = DateTime.Today.Month;
            _cmbQuarter.SelectedIndex = cm >= 4 && cm <= 6 ? 0 : cm >= 7 && cm <= 9 ? 1 : cm >= 10 ? 2 : 3;
            _lblQuarter.Visible = false;
            _cmbQuarter.Visible = false;

            // Year
            var lblYear = MkLbl("Year:", 0, LBL_Y);
            _cmbYear = MkCmb(0, ROW1_Y, 84);
            for (int y = DateTime.Today.Year - 3; y <= DateTime.Today.Year + 1; y++)
                _cmbYear.Items.Add(y.ToString());
            _cmbYear.SelectedItem = DateTime.Today.Year.ToString();

            // Toggle month / quarter selectors
            _cmbPeriodType.SelectedIndexChanged += (s, e) =>
            {
                bool isMonthly = _cmbPeriodType.SelectedIndex == 0;
                bool isQuarterly = _cmbPeriodType.SelectedIndex == 1;
                _lblMonth.Visible = isMonthly;
                _cmbMonth.Visible = isMonthly;
                _lblQuarter.Visible = isQuarterly;
                _cmbQuarter.Visible = isQuarterly;
                LayoutRow1(bar);
            };

            bar.Controls.Add(lblRet);
            bar.Controls.Add(_cmbReturnType);
            bar.Controls.Add(lblPeriod);
            bar.Controls.Add(_cmbPeriodType);
            bar.Controls.Add(_lblMonth);
            bar.Controls.Add(_cmbMonth);
            bar.Controls.Add(_lblQuarter);
            bar.Controls.Add(_cmbQuarter);
            bar.Controls.Add(lblYear);
            bar.Controls.Add(_cmbYear);

            // ── ROW 2: action buttons ────────────────────────────────
            const int ROW2_Y = 52;
            const int BTN_H = 32;

            var btnRun = MkBtn("▶  Generate", Blue, Color.White, BlueL);
            btnRun.Location = new Point(0, ROW2_Y); btnRun.Height = BTN_H;
            btnRun.Click += GenerateReturn;

            var btnExcel = MkBtn("📊  Export Excel", Color.FromArgb(33, 115, 70), Color.White,
                                  Color.FromArgb(20, 83, 45));
            btnExcel.Location = new Point(0, ROW2_Y); btnExcel.Height = BTN_H;
            btnExcel.Click += ExportExcel;

            var btnPdf = MkBtn("📄  Export PDF Summary", Purple, Color.White,
                                Color.FromArgb(88, 28, 135));
            btnPdf.Location = new Point(0, ROW2_Y); btnPdf.Height = BTN_H;
            btnPdf.Click += ExportPdfSummary;

            bar.Controls.Add(btnRun);
            bar.Controls.Add(btnExcel);
            bar.Controls.Add(btnPdf);

            // ── Responsive layout via Resize ─────────────────────────
            // Row 1: pack selectors left-to-right starting at x=16
            // Row 2: pack action buttons right-to-left starting from right edge
            bar.Resize += (s, e) =>
            {
                LayoutRow1(bar);

                // Row 2 buttons — right-aligned
                int r = bar.Width - 12;
                btnPdf.Location = new Point(r - btnPdf.Width, ROW2_Y);
                btnExcel.Location = new Point(r - btnPdf.Width - btnExcel.Width - 8, ROW2_Y);
                btnRun.Location = new Point(r - btnPdf.Width - btnExcel.Width - btnRun.Width - 16, ROW2_Y);
            };

            // Trigger initial layout once shown
            this.Load += (s, e) => { bar.PerformLayout(); LayoutRow1(bar); };

            // ── Summary stat strip ────────────────────────────────────
            var stats = BuildStatStrip();

            // ── Tab pages ─────────────────────────────────────────────
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Padding = new Point(14, 4)
            };

            var tB2B = new TabPage("B2B Supplies (GSTR-1)") { BackColor = Surface };
            var tB2C = new TabPage("B2C Supplies (GSTR-1)") { BackColor = Surface };
            var t3B = new TabPage("Tax Liability (GSTR-3B)") { BackColor = Surface };
            var tHsn = new TabPage("HSN Summary") { BackColor = Surface };

            _dgvB2B = BuildDgv(); tB2B.Controls.Add(_dgvB2B);
            _dgvB2C = BuildDgv(); tB2C.Controls.Add(_dgvB2C);
            _dgv3B = BuildDgv(); t3B.Controls.Add(_dgv3B);
            _dgvHsn = BuildDgv(); tHsn.Controls.Add(_dgvHsn);

            _tabs.TabPages.Add(tB2B);
            _tabs.TabPages.Add(tB2C);
            _tabs.TabPages.Add(t3B);
            _tabs.TabPages.Add(tHsn);

            // ── Assemble ─────────────────────────────────────────────
            this.Controls.Add(_tabs);
            this.Controls.Add(stats);
            this.Controls.Add(bar);
            this.Controls.Add(hdr);
        }

        // ── Layout row 1 selectors left-to-right ─────────────────────
        // Called on bar.Resize and whenever period-type changes.
        void LayoutRow1(Panel bar)
        {
            const int START_X = 16;
            const int GAP = 6;
            const int LBL_Y = 13;   // vertical within row 1
            const int CMB_Y = 7;
            int x = START_X;

            void Place(Label lbl, ComboBox cmb)
            {
                if (!lbl.Visible) return;
                lbl.Location = new Point(x, LBL_Y);
                x += lbl.Width + 4;
                cmb.Location = new Point(x, CMB_Y);
                x += cmb.Width + GAP + 8;
            }

            // Find references from bar.Controls
            // (Controls were added in Build() — we reference the fields directly)
            var lblRet = bar.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Return Type:");
            var lblPeriod = bar.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Period:");
            var lblYear = bar.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "Year:");

            if (lblRet != null) { lblRet.Location = new Point(x, LBL_Y); x += lblRet.Width + 4; _cmbReturnType.Location = new Point(x, CMB_Y); x += _cmbReturnType.Width + GAP + 8; }
            if (lblPeriod != null) { lblPeriod.Location = new Point(x, LBL_Y); x += lblPeriod.Width + 4; _cmbPeriodType.Location = new Point(x, CMB_Y); x += _cmbPeriodType.Width + GAP + 8; }

            if (_lblMonth.Visible)
            {
                _lblMonth.Location = new Point(x, LBL_Y); x += _lblMonth.Width + 4;
                _cmbMonth.Location = new Point(x, CMB_Y); x += _cmbMonth.Width + GAP + 8;
            }
            if (_lblQuarter.Visible)
            {
                _lblQuarter.Location = new Point(x, LBL_Y); x += _lblQuarter.Width + 4;
                _cmbQuarter.Location = new Point(x, CMB_Y); x += _cmbQuarter.Width + GAP + 8;
            }
            if (lblYear != null) { lblYear.Location = new Point(x, LBL_Y); x += lblYear.Width + 4; _cmbYear.Location = new Point(x, CMB_Y); }
        }

        // ── Stat strip ────────────────────────────────────────────────
        Panel BuildStatStrip()
        {
            var strip = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Bg };

            _lblInvoiceCount = MkStatVal(Blue);
            _lblTotalTaxable = MkStatVal(TextDark);
            _lblTotalCgst = MkStatVal(Amber);
            _lblTotalSgst = MkStatVal(Amber);
            _lblTotalIgst = MkStatVal(Purple);
            _lblTotalGrand = MkStatVal(Green);

            var cards = new[]
            {
                MkStatCard("🧾", _lblInvoiceCount, "Invoices",      Blue,    BluePale),
                MkStatCard("💰", _lblTotalTaxable, "Taxable Value", TextDark, Surface2),
                MkStatCard("📘", _lblTotalCgst,    "CGST",          Amber,   AmberPale),
                MkStatCard("📗", _lblTotalSgst,    "SGST",          Amber,   AmberPale),
                MkStatCard("📙", _lblTotalIgst,    "IGST",          Purple,  PurplePale),
                MkStatCard("✅", _lblTotalGrand,   "Total Tax",     Green,   GreenPale),
            };

            strip.Resize += (s, e) =>
            {
                int x = 16, cardW = Math.Max(120, (strip.Width - 32) / cards.Length - 8);
                foreach (var c in cards)
                { c.Location = new Point(x, 10); c.Size = new Size(cardW, 58); x += cardW + 8; }
            };
            strip.Controls.AddRange(cards);
            return strip;
        }

        // ════════════════════════════════════════════════════════════
        //  GENERATE
        // ════════════════════════════════════════════════════════════
        void GenerateReturn(object sender, EventArgs e)
        {
            if (!int.TryParse(_cmbYear.SelectedItem?.ToString(), out int year)) return;
            int cid = SessionManager.CompanyProfileId;

            DateOnly dateFrom, dateTo;
            string periodLabel;

            switch (_cmbPeriodType.SelectedIndex)
            {
                case 1: // Quarterly
                    int q = _cmbQuarter.SelectedIndex;
                    if (q == 0) { dateFrom = new DateOnly(year, 4, 1); dateTo = new DateOnly(year, 6, 30); periodLabel = $"Q1 Apr-Jun {year}"; }
                    else if (q == 1) { dateFrom = new DateOnly(year, 7, 1); dateTo = new DateOnly(year, 9, 30); periodLabel = $"Q2 Jul-Sep {year}"; }
                    else if (q == 2) { dateFrom = new DateOnly(year, 10, 1); dateTo = new DateOnly(year, 12, 31); periodLabel = $"Q3 Oct-Dec {year}"; }
                    else { dateFrom = new DateOnly(year, 1, 1); dateTo = new DateOnly(year, 3, 31); periodLabel = $"Q4 Jan-Mar {year}"; }
                    break;

                case 2: // Annual — Indian FY
                    dateFrom = new DateOnly(year, 4, 1);
                    dateTo = new DateOnly(year + 1, 3, 31);
                    periodLabel = $"FY {year}-{(year + 1) % 100:D2} (Apr {year} – Mar {year + 1})";
                    break;

                default: // Monthly
                    int month = _cmbMonth.SelectedIndex + 1;
                    dateFrom = new DateOnly(year, month, 1);
                    dateTo = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
                    periodLabel = new DateTime(year, month, 1).ToString("MMMM yyyy");
                    break;
            }

            _lastPeriodLabel = periodLabel;

            foreach (var g in new[] { _dgvB2B, _dgvB2C, _dgv3B, _dgvHsn })
            { g.Columns.Clear(); g.Rows.Clear(); }

            try
            {
                using var db = new AppDbContext();

                var invoices = db.InvoiceHeaders
                    .Where(h => h.CompanyProfileId == cid
                             && h.InvoiceDate >= dateFrom
                             && h.InvoiceDate <= dateTo)
                    .Join(db.Accounts, h => h.ClientId, a => a.AccountId,
                        (h, a) => new
                        {
                            h.InvoiceId,
                            h.InvoiceNo,
                            h.InvoiceDate,
                            ClientName = a.AccNm ?? "",
                            ClientGstin = a.Gstin ?? "",
                            ClientState = a.BillState ?? "",
                            Taxable = h.TotalAmount ?? 0,
                            CgstPct = h.CgstPct ?? 0,
                            SgstPct = h.SgstPct ?? 0,
                            IgstPct = h.IgstPct ?? 0,
                            Cgst = h.Cgst ?? 0,
                            Sgst = h.Sgst ?? 0,
                            Igst = h.Igst ?? 0,
                            GrandTotal = h.GrandTotal ?? 0
                        })
                    .OrderBy(x => x.InvoiceDate).ToList();

                var b2b = invoices.Where(x => !string.IsNullOrWhiteSpace(x.ClientGstin)).ToList();
                var b2c = invoices.Where(x => string.IsNullOrWhiteSpace(x.ClientGstin)).ToList();

                // ── TAB 1: B2B ────────────────────────────────────────
                _dgvB2B.Columns.Add("InvNo", "INVOICE NO");
                _dgvB2B.Columns.Add("Date", "DATE");
                _dgvB2B.Columns.Add("Client", "RECEIVER NAME");
                _dgvB2B.Columns.Add("Gstin", "RECEIVER GSTIN");
                _dgvB2B.Columns.Add("State", "STATE");
                _dgvB2B.Columns.Add("Taxable", "TAXABLE VALUE");
                _dgvB2B.Columns.Add("CgstPct", "CGST RATE %");
                _dgvB2B.Columns.Add("CgstAmt", "CGST AMT");
                _dgvB2B.Columns.Add("SgstPct", "SGST RATE %");
                _dgvB2B.Columns.Add("SgstAmt", "SGST AMT");
                _dgvB2B.Columns.Add("IgstPct", "IGST RATE %");
                _dgvB2B.Columns.Add("IgstAmt", "IGST AMT");
                _dgvB2B.Columns.Add("Grand", "INVOICE VALUE");

                decimal b2bTax = 0, b2bCgst = 0, b2bSgst = 0, b2bIgst = 0, b2bGrand = 0;
                foreach (var r in b2b)
                {
                    b2bTax += r.Taxable; b2bCgst += r.Cgst; b2bSgst += r.Sgst; b2bIgst += r.Igst; b2bGrand += r.GrandTotal;
                    _dgvB2B.Rows.Add(r.InvoiceNo, r.InvoiceDate.ToString("dd/MM/yyyy"),
                        r.ClientName, r.ClientGstin, r.ClientState,
                        r.Taxable.ToString("N2"), r.CgstPct + "%", r.Cgst.ToString("N2"),
                        r.SgstPct + "%", r.Sgst.ToString("N2"),
                        r.IgstPct + "%", r.Igst.ToString("N2"), r.GrandTotal.ToString("N2"));
                }
                AddTotalRow(_dgvB2B, "TOTAL", b2bTax, b2bCgst, b2bSgst, b2bIgst, b2bGrand);

                // ── TAB 2: B2C ────────────────────────────────────────
                _dgvB2C.Columns.Add("InvNo", "INVOICE NO");
                _dgvB2C.Columns.Add("Date", "DATE");
                _dgvB2C.Columns.Add("Client", "CUSTOMER NAME");
                _dgvB2C.Columns.Add("State", "STATE");
                _dgvB2C.Columns.Add("Taxable", "TAXABLE VALUE");
                _dgvB2C.Columns.Add("CgstPct", "CGST RATE %");
                _dgvB2C.Columns.Add("CgstAmt", "CGST AMT");
                _dgvB2C.Columns.Add("SgstPct", "SGST RATE %");
                _dgvB2C.Columns.Add("SgstAmt", "SGST AMT");
                _dgvB2C.Columns.Add("IgstPct", "IGST RATE %");
                _dgvB2C.Columns.Add("IgstAmt", "IGST AMT");
                _dgvB2C.Columns.Add("Grand", "INVOICE VALUE");

                decimal b2cTax = 0, b2cCgst = 0, b2cSgst = 0, b2cIgst = 0, b2cGrand = 0;
                foreach (var r in b2c)
                {
                    b2cTax += r.Taxable; b2cCgst += r.Cgst; b2cSgst += r.Sgst; b2cIgst += r.Igst; b2cGrand += r.GrandTotal;
                    _dgvB2C.Rows.Add(r.InvoiceNo, r.InvoiceDate.ToString("dd/MM/yyyy"),
                        r.ClientName, r.ClientState,
                        r.Taxable.ToString("N2"), r.CgstPct + "%", r.Cgst.ToString("N2"),
                        r.SgstPct + "%", r.Sgst.ToString("N2"),
                        r.IgstPct + "%", r.Igst.ToString("N2"), r.GrandTotal.ToString("N2"));
                }
                AddTotalRow(_dgvB2C, "TOTAL", b2cTax, b2cCgst, b2cSgst, b2cIgst, b2cGrand);

                // ── TAB 3: GSTR-3B ────────────────────────────────────
                decimal totTax = b2bTax + b2cTax, totCgst = b2bCgst + b2cCgst,
                        totSgst = b2bSgst + b2cSgst, totIgst = b2bIgst + b2cIgst,
                        totGrand = b2bGrand + b2cGrand;

                _dgv3B.Columns.Add("Desc", "DESCRIPTION");
                _dgv3B.Columns.Add("Taxable", "TAXABLE VALUE (₹)");
                _dgv3B.Columns.Add("Cgst", "CGST (₹)");
                _dgv3B.Columns.Add("Sgst", "SGST (₹)");
                _dgv3B.Columns.Add("Igst", "IGST (₹)");
                _dgv3B.Columns.Add("Total", "TOTAL TAX (₹)");

                void Add3BRow(string desc, decimal tax, decimal cg, decimal sg, decimal ig,
                    bool bold = false, Color? bg = null)
                {
                    int ri = _dgv3B.Rows.Add(desc,
                        tax.ToString("N2"), cg.ToString("N2"),
                        sg.ToString("N2"), ig.ToString("N2"),
                        (cg + sg + ig).ToString("N2"));
                    if (bold || bg.HasValue)
                        foreach (DataGridViewCell cell in _dgv3B.Rows[ri].Cells)
                        {
                            if (bold) cell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                            if (bg.HasValue) cell.Style.BackColor = bg.Value;
                        }
                }

                Add3BRow("3.1(a) Outward Taxable Supplies — B2B", b2bTax, b2bCgst, b2bSgst, b2bIgst);
                Add3BRow("3.1(b) Outward Taxable Supplies — B2C", b2cTax, b2cCgst, b2cSgst, b2cIgst);
                Add3BRow("3.1  TOTAL Outward Supplies", totTax, totCgst, totSgst, totIgst, bold: true, bg: GreenPale);
                Add3BRow("3.2  Inter-state Supplies (IGST only)", 0, 0, 0, totIgst);
                Add3BRow("4    ITC Available (Input — enter manually)", 0, 0, 0, 0);
                Add3BRow("5.1  Tax Payable after ITC", totTax, totCgst, totSgst, totIgst, bold: true, bg: AmberPale);

                // ── TAB 4: HSN Summary ────────────────────────────────
                var hsnData = db.InvoiceItems
                    .Where(i => i.CompanyProfileId == cid)
                    .Join(db.InvoiceHeaders, i => i.InvoiceId, h => h.InvoiceId, (i, h) => new { i, h })
                    .Where(x => x.h.InvoiceDate >= dateFrom && x.h.InvoiceDate <= dateTo)
                    .AsEnumerable()
                    .GroupBy(x => x.i.HsnCode ?? "—")
                    .Select(g => new
                    {
                        Hsn = g.Key,
                        UQC = g.Select(x => x.i.Per).FirstOrDefault() ?? "NOS",
                        Qty = g.Sum(x => x.i.Qty ?? 0),
                        Taxable = g.Sum(x => x.i.Amount ?? 0),
                        CgstPct = g.Select(x => x.h.CgstPct ?? 0).FirstOrDefault(),
                        SgstPct = g.Select(x => x.h.SgstPct ?? 0).FirstOrDefault(),
                        IgstPct = g.Select(x => x.h.IgstPct ?? 0).FirstOrDefault(),
                    })
                    .OrderBy(x => x.Hsn).ToList();

                _dgvHsn.Columns.Add("Hsn", "HSN / SAC");
                _dgvHsn.Columns.Add("Desc", "DESCRIPTION");
                _dgvHsn.Columns.Add("Uqc", "UQC");
                _dgvHsn.Columns.Add("Qty", "TOTAL QTY");
                _dgvHsn.Columns.Add("Taxable", "TAXABLE VALUE");
                _dgvHsn.Columns.Add("CgstPct", "CGST RATE %");
                _dgvHsn.Columns.Add("CgstAmt", "CGST AMT");
                _dgvHsn.Columns.Add("SgstPct", "SGST RATE %");
                _dgvHsn.Columns.Add("SgstAmt", "SGST AMT");
                _dgvHsn.Columns.Add("IgstPct", "IGST RATE %");
                _dgvHsn.Columns.Add("IgstAmt", "IGST AMT");
                _dgvHsn.Columns.Add("Total", "TOTAL VALUE");

                foreach (var r in hsnData)
                {
                    decimal cg = Math.Round(r.Taxable * r.CgstPct / 100, 2);
                    decimal sg = Math.Round(r.Taxable * r.SgstPct / 100, 2);
                    decimal ig = Math.Round(r.Taxable * r.IgstPct / 100, 2);
                    decimal tot = r.Taxable + cg + sg + ig;
                    string desc = db.DesignMasters
                        .FirstOrDefault(d => d.HsnCode == r.Hsn)?.DesignName ?? "—";
                    _dgvHsn.Rows.Add(r.Hsn, desc, r.UQC, r.Qty.ToString("N3"),
                        r.Taxable.ToString("N2"),
                        r.CgstPct + "%", cg.ToString("N2"),
                        r.SgstPct + "%", sg.ToString("N2"),
                        r.IgstPct + "%", ig.ToString("N2"), tot.ToString("N2"));
                }

                // ── Update stat strip ─────────────────────────────────
                _lblInvoiceCount.Text = invoices.Count.ToString();
                _lblTotalTaxable.Text = "₹" + totTax.ToString("N2");
                _lblTotalCgst.Text = "₹" + totCgst.ToString("N2");
                _lblTotalSgst.Text = "₹" + totSgst.ToString("N2");
                _lblTotalIgst.Text = "₹" + totIgst.ToString("N2");
                _lblTotalGrand.Text = "₹" + (totCgst + totSgst + totIgst).ToString("N2");

                // Auto-switch to correct tab based on return type
                _tabs.SelectedIndex = _cmbReturnType.SelectedIndex == 1 ? 2 : 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating return:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Add highlighted total row ─────────────────────────────────
        void AddTotalRow(DataGridView g, string label,
            decimal tax, decimal cg, decimal sg, decimal ig, decimal grand)
        {
            if (g.Rows.Count == 0) return;
            var row = new DataGridViewRow(); row.CreateCells(g);
            row.Cells[0].Value = label;
            foreach (DataGridViewColumn col in g.Columns)
            {
                string n = col.Name;
                if (n == "Taxable") row.Cells[col.Index].Value = tax.ToString("N2");
                else if (n == "CgstAmt") row.Cells[col.Index].Value = cg.ToString("N2");
                else if (n == "SgstAmt") row.Cells[col.Index].Value = sg.ToString("N2");
                else if (n == "IgstAmt") row.Cells[col.Index].Value = ig.ToString("N2");
                else if (n == "Grand") row.Cells[col.Index].Value = grand.ToString("N2");
            }
            g.Rows.Add(row);
            int ti = g.Rows.Count - 1;
            foreach (DataGridViewCell cell in g.Rows[ti].Cells)
            {
                cell.Style.BackColor = GreenPale;
                cell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                cell.Style.ForeColor = Green;
            }
        }

        // ════════════════════════════════════════════════════════════
        //  EXPORT EXCEL
        // ════════════════════════════════════════════════════════════
        void ExportExcel(object sender, EventArgs e)
        {
            bool hasData = _dgvB2B.Columns.Count > 0 || _dgv3B.Columns.Count > 0;
            if (!hasData)
            { MessageBox.Show("Please generate the return first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            string period = _lastPeriodLabel.Replace(" ", "_").Replace("–", "-");
            string retType = _cmbReturnType.SelectedItem?.ToString()?.Replace("-", "") ?? "GST";

            using var sfd = new SaveFileDialog
            {
                Title = "Save GST Return Excel",
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"{retType}_{period}_{SessionManager.CompanyName.Replace(" ", "_")}.xlsx"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                var activeGrid = _tabs.SelectedIndex switch
                {
                    0 => _dgvB2B,
                    1 => _dgvB2C,
                    2 => _dgv3B,
                    3 => _dgvHsn,
                    _ => _dgvB2B
                };
                string sheetName = _tabs.SelectedTab?.Text ?? "GST";
                ExcelExportHelper.Export(activeGrid, sfd.FileName, sheetName);
                MessageBox.Show($"Exported successfully!\n\n{sfd.FileName}",
                    "Exported ✓", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            { MessageBox.Show("Export error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ════════════════════════════════════════════════════════════
        //  EXPORT PDF SUMMARY
        // ════════════════════════════════════════════════════════════
        void ExportPdfSummary(object sender, EventArgs e)
        {
            if (_dgv3B.Columns.Count == 0)
            { MessageBox.Show("Please generate the return first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            string period = _lastPeriodLabel;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"GST RETURN SUMMARY — {period}");
            sb.AppendLine($"Company  : {SessionManager.CompanyName}");
            sb.AppendLine($"GSTIN    : {GetCompanyGstin()}");
            sb.AppendLine($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}");
            sb.AppendLine(new string('─', 60));
            sb.AppendLine();
            sb.AppendLine($"Total Invoices        : {_lblInvoiceCount.Text}");
            sb.AppendLine($"Total Taxable Value   : {_lblTotalTaxable.Text}");
            sb.AppendLine($"CGST                  : {_lblTotalCgst.Text}");
            sb.AppendLine($"SGST                  : {_lblTotalSgst.Text}");
            sb.AppendLine($"IGST                  : {_lblTotalIgst.Text}");
            sb.AppendLine($"Total Tax Liability   : {_lblTotalGrand.Text}");
            sb.AppendLine(); sb.AppendLine(new string('─', 60));
            sb.AppendLine("GSTR-3B SECTION 3.1 DETAILS:"); sb.AppendLine();
            foreach (DataGridViewRow row in _dgv3B.Rows)
            {
                string desc = row.Cells["Desc"].Value?.ToString() ?? "";
                string total = row.Cells["Total"].Value?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(desc))
                    sb.AppendLine($"  {desc,-50} ₹{total,14}");
            }
            sb.AppendLine();
            sb.AppendLine("NOTE: ITC (Input Tax Credit) must be entered manually on GST portal.");
            sb.AppendLine("      This summary is for reference only. Always verify before filing.");

            using var dlg = new Form
            {
                Text = $"GST Summary — {period}",
                Size = new Size(700, 520),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Bg,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };
            var txt = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10F),
                BackColor = Surface,
                ForeColor = TextDark,
                Text = sb.ToString()
            };
            var btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Surface2 };
            var btnSave = MkBtn("💾  Save as TXT", Blue, Color.White, BlueL);
            btnSave.Location = new Point(14, 8);
            btnSave.Click += (s2, e2) =>
            {
                using var sfd = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt",
                    FileName = $"GST_Summary_{period.Replace(" ", "_")}.txt"
                };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                    MessageBox.Show("Saved!", "✓", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            var btnClose = MkBtn("Close", Color.FromArgb(100, 116, 139), Color.White,
                                  Color.FromArgb(71, 85, 105));
            btnClose.Location = new Point(btnSave.Right + 10, 8);
            btnClose.Click += (s2, e2) => dlg.Close();
            btnPanel.Controls.Add(btnSave); btnPanel.Controls.Add(btnClose);
            dlg.Controls.Add(txt); dlg.Controls.Add(btnPanel);
            dlg.ShowDialog(this.FindForm());
        }

        // ════════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════════
        string GetCompanyGstin()
        {
            try
            {
                using var db = new AppDbContext();
                return db.CompanyProfiles
                    .FirstOrDefault(c => c.CompanyProfileId == SessionManager.CompanyProfileId)
                    ?.Gstin ?? "Not set";
            }
            catch { return "—"; }
        }

        DataGridView BuildDgv()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Surface,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg;
            g.ColumnHeadersDefaultCellStyle.ForeColor = TextMid;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderBg;
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.ColumnHeadersHeight = 38;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.DefaultCellStyle.BackColor = Surface;
            g.DefaultCellStyle.ForeColor = TextMid;
            g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = BluePale;
            g.DefaultCellStyle.SelectionForeColor = TextDark;
            g.RowTemplate.Height = 36;

            g.RowsAdded += (s, e) => {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                    if (i >= 0 && i < g.Rows.Count)
                        g.Rows[i].DefaultCellStyle.BackColor =
                            i % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253);
            };
            g.RowPostPaint += (s, e) => {
                using var p = new Pen(RowLine, 1);
                e.Graphics.DrawLine(p, e.RowBounds.Left, e.RowBounds.Bottom - 1,
                    e.RowBounds.Right, e.RowBounds.Bottom - 1);
            };
            return g;
        }

        Label MkLbl(string text, int x, int y) => new Label
        {
            Text = text,
            ForeColor = TextMid,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(x, y)
        };

        ComboBox MkCmb(int x, int y, int w) => new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = TextDark,
            Font = new Font("Segoe UI", 9.5F),
            Location = new Point(x, y),
            Size = new Size(w, 28)
        };

        Button MkBtn(string text, Color bg, Color fg, Color hover)
        {
            var b = new Button
            {
                Text = text,
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 32),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = hover;
            return b;
        }

        Label MkStatVal(Color c) => new Label
        {
            Text = "—",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = c,
            AutoSize = true
        };

        Panel MkStatCard(string icon, Label val, string caption, Color accent, Color bg)
        {
            var p = new Panel { BackColor = bg, Size = new Size(140, 58) };
            p.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(4, 58), BackColor = accent });
            p.Controls.Add(new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 12F),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(12, 8)
            });
            val.Location = new Point(38, 4); p.Controls.Add(val);
            p.Controls.Add(new Label
            {
                Text = caption,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(38, 32)
            });
            return p;
        }
    }
}