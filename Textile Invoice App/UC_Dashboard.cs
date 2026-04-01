using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App
{
    public partial class UC_Dashboard : UserControl
    {
        // ── Palette ───────────────────────────────────────────────────
        static readonly Color Bg = Color.FromArgb(245, 247, 250);
        static readonly Color Surface = Color.White;
        static readonly Color Surface2 = Color.FromArgb(248, 249, 252);
        static readonly Color Blue = Color.FromArgb(37, 99, 235);
        static readonly Color BlueL = Color.FromArgb(59, 130, 246);
        static readonly Color BluePale = Color.FromArgb(239, 246, 255);
        static readonly Color Green = Color.FromArgb(22, 163, 74);
        static readonly Color Amber = Color.FromArgb(217, 119, 6);
        static readonly Color Purple = Color.FromArgb(124, 58, 237);
        static readonly Color Teal = Color.FromArgb(13, 148, 136);
        static readonly Color Border = Color.FromArgb(226, 232, 240);
        static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        static readonly Color TextMid = Color.FromArgb(71, 85, 105);
        static readonly Color TextLight = Color.FromArgb(148, 163, 184);

        Panel _pnlCards;
        DataGridView _dgv;
        Label _lblCompany, _lblDate, _lblUser;

        // ── Connection string from EF — single source of truth ────────
        static string GetConnStr()
        {
            using var db = new AppDbContext();
            return db.Database.GetConnectionString()!;
        }

        public UC_Dashboard()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Bg;
            Build();
            LoadData();
        }

        // ════════════════════════════════════════════════════════════
        //  BUILD
        // ════════════════════════════════════════════════════════════
        void Build()
        {
            // ── Top header strip ──────────────────────────────────────
            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Bg };

            _lblCompany = new Label
            {
                Text = SessionManager.CompanyName,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 10)
            };
            _lblUser = new Label
            {
                Text = "Welcome, " + SessionManager.FullName,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMid,
                AutoSize = true,
                Location = new Point(26, 40)
            };
            _lblDate = new Label
            {
                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy"),
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMid,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // New Invoice button — anchored right, never overlaps date label
            var btnNewInv = new Button
            {
                Text = "➕  New Invoice",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Size = new Size(148, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                UseVisualStyleBackColor = false
            };
            btnNewInv.FlatAppearance.BorderSize = 0;
            btnNewInv.FlatAppearance.MouseOverBackColor = BlueL;
            btnNewInv.Click += (s, e) => NavigateTo<UC_CreateInvoice>(d => d.BtnCreateInvoice);

            pnlTop.Controls.Add(_lblCompany);
            pnlTop.Controls.Add(_lblUser);
            pnlTop.Controls.Add(_lblDate);
            pnlTop.Controls.Add(btnNewInv);

            // Reposition right-anchored controls on resize — no overlap
            pnlTop.Resize += (s, e) =>
            {
                int right = pnlTop.Width - 16;
                btnNewInv.Location = new Point(right - btnNewInv.Width, 18);
                _lblDate.Location = new Point(right - btnNewInv.Width - _lblDate.Width - 16, 26);
            };

            // ── Scrollable body ───────────────────────────────────────
            var scroll = new Panel { Dock = DockStyle.Fill, BackColor = Bg, AutoScroll = true };

            // Stat cards panel — height auto-adjusts via LayoutCards
            _pnlCards = new Panel { Dock = DockStyle.Top, Height = 140, BackColor = Bg };
            _pnlCards.Resize += (s, e) => LayoutCards();

            // ── Recent invoices table ─────────────────────────────────
            var pnlTable = new Panel { Dock = DockStyle.Top, Height = 420, BackColor = Surface };
            var tStripe = new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue };

            var tHead = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Surface2 };
            tHead.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border), 0, tHead.Height - 1, tHead.Width, tHead.Height - 1);

            tHead.Controls.Add(new Label
            {
                Text = "🧾  Recent Invoices",
                Font = new Font("Segoe UI Emoji", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(14, 14)
            });

            // ── Action buttons in table header — positioned right-to-left
            // so they never overlap each other regardless of window width
            var btnViewAll = new Button
            {
                Text = "View All →",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BackColor = BluePale,
                ForeColor = Blue,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Size = new Size(94, 28),
                UseVisualStyleBackColor = false
            };
            btnViewAll.FlatAppearance.BorderSize = 1;
            btnViewAll.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
            btnViewAll.FlatAppearance.MouseOverBackColor = Color.FromArgb(219, 234, 254);
            btnViewAll.Click += (s, e) => NavigateTo<UC_InvoiceList>(d => d.BtnInvoiceList);

            var btnNewInv2 = new Button
            {
                Text = "➕  Create Invoice",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BackColor = Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Size = new Size(136, 28),
                UseVisualStyleBackColor = false
            };
            btnNewInv2.FlatAppearance.BorderSize = 0;
            btnNewInv2.FlatAppearance.MouseOverBackColor = BlueL;
            btnNewInv2.Click += (s, e) => NavigateTo<UC_CreateInvoice>(d => d.BtnCreateInvoice);

            tHead.Controls.Add(btnViewAll);
            tHead.Controls.Add(btnNewInv2);

            // Both buttons positioned from right, never overlapping
            tHead.Resize += (s, e) =>
            {
                int r = tHead.Width - 12;
                btnViewAll.Location = new Point(r - btnViewAll.Width, 9);
                btnNewInv2.Location = new Point(r - btnViewAll.Width - btnNewInv2.Width - 8, 9);
            };

            _dgv = BuildDgv();
            pnlTable.Controls.Add(_dgv);     // Fill first
            pnlTable.Controls.Add(tHead);    // Top after
            pnlTable.Controls.Add(tStripe);  // Top last

            scroll.Controls.Add(pnlTable);
            scroll.Controls.Add(_pnlCards);
            this.Controls.Add(scroll);
            this.Controls.Add(pnlTop);

            this.VisibleChanged += (s, e) => { if (Visible) LoadData(); };
        }

        // ════════════════════════════════════════════════════════════
        //  LOAD DATA
        // ════════════════════════════════════════════════════════════
        void LoadData()
        {
            int cid = SessionManager.CompanyProfileId;
            int totalClients = 0, totalDesigns = 0, totalInvoices = 0,
                todayInv = 0, totalServices = 0, nextInvNo = 0;
            decimal totalRevenue = 0, todayRev = 0;

            string connStr = GetConnStr();
            try
            {
                using var conn = new SqlConnection(connStr);
                conn.Open();
                string todayStr = DateTime.Today.ToString("yyyy-MM-dd");

                var queries = new Dictionary<string, (string sql, bool hasDate)>
                {
                    ["clients"] = ("SELECT COUNT(*) FROM ACCOUNTS WHERE COMPANY_PROFILE_ID = @cid", false),
                    ["designs"] = ("SELECT COUNT(*) FROM DESIGN_MASTER WHERE COMPANY_PROFILE_ID = @cid", false),
                    ["invoices"] = ("SELECT COUNT(*) FROM INVOICE_HEADER WHERE COMPANY_PROFILE_ID = @cid", false),
                    ["revenue"] = ("SELECT ISNULL(SUM(GRAND_TOTAL),0) FROM INVOICE_HEADER WHERE COMPANY_PROFILE_ID = @cid", false),
                    ["todayinv"] = ("SELECT COUNT(*) FROM INVOICE_HEADER WHERE COMPANY_PROFILE_ID = @cid AND CAST(INVOICE_DATE AS DATE) = @today", true),
                    ["todayrev"] = ("SELECT ISNULL(SUM(GRAND_TOTAL),0) FROM INVOICE_HEADER WHERE COMPANY_PROFILE_ID = @cid AND CAST(INVOICE_DATE AS DATE) = @today", true),
                    ["services"] = ("SELECT COUNT(*) FROM SERVICE_MASTER WHERE COMPANY_PROFILE_ID = @cid", false),
                    ["nextinv"] = ("SELECT ISNULL(CURRENT_INVOICE_NO,0) FROM INVOICE_NUMBER_TRACKER WHERE COMPANY_PROFILE_ID = @cid", false),
                };

                foreach (var kv in queries)
                {
                    try
                    {
                        using var cmd = new SqlCommand(kv.Value.sql, conn);
                        cmd.Parameters.AddWithValue("@cid", cid);
                        if (kv.Value.hasDate)
                            cmd.Parameters.AddWithValue("@today", todayStr);
                        var result = cmd.ExecuteScalar();
                        switch (kv.Key)
                        {
                            case "clients": totalClients = Convert.ToInt32(result); break;
                            case "designs": totalDesigns = Convert.ToInt32(result); break;
                            case "invoices": totalInvoices = Convert.ToInt32(result); break;
                            case "revenue": totalRevenue = Convert.ToDecimal(result); break;
                            case "todayinv": todayInv = Convert.ToInt32(result); break;
                            case "todayrev": todayRev = Convert.ToDecimal(result); break;
                            case "services": totalServices = Convert.ToInt32(result); break;
                            case "nextinv": nextInvNo = Convert.ToInt32(result); break;
                        }
                    }
                    catch { }
                }
            }
            catch { }

            // ── Rebuild stat cards ─────────────────────────────────────
            _pnlCards.Controls.Clear();
            var cardDefs = new (string Icon, string Val, string Name, string Sub, Color Accent)[]
            {
                ("👤", totalClients.ToString(),       "Total Clients",   "All accounts",                  Blue  ),
                ("🎨", totalDesigns.ToString(),       "Total Designs",   "Design master",                 Purple),
                ("🧾", todayInv.ToString(),           "Invoices Today",  DateTime.Today.ToString("dd MMM"),Amber ),
                ("💰", "₹" + FormatNum(todayRev),    "Revenue Today",   "Grand total",                   Green ),
                ("📅", totalInvoices.ToString(),      "Total Invoices",  "All time",                      BlueL ),
                ("⚙️",  totalServices.ToString(),      "Services",        "Service master",                Teal  ),
                ("📊", "₹" + FormatNum(totalRevenue),"Total Revenue",   "All invoices",                  Green ),
                ("🔢", (nextInvNo + 1).ToString(),   "Next Invoice No", "Auto tracked",                  Amber ),
            };
            foreach (var c in cardDefs)
                _pnlCards.Controls.Add(BuildCard(c.Icon, c.Val, c.Name, c.Sub, c.Accent));
            LayoutCards();

            // ── Recent invoices ────────────────────────────────────────
            _dgv.Rows.Clear();
            try
            {
                using var conn = new SqlConnection(connStr);
                conn.Open();
                string sql = @"
                    SELECT TOP 10
                        CAST(h.INVOICE_NO AS NVARCHAR(20)) AS InvNo,
                        ISNULL(a.ACC_NM,'—')               AS Client,
                        CONVERT(VARCHAR(11), h.INVOICE_DATE, 106) AS InvDate,
                        ISNULL(h.TOTAL_AMOUNT,0)           AS TotalAmt,
                        ISNULL(h.GRAND_TOTAL,0)            AS GrandTotal
                    FROM  INVOICE_HEADER h
                    LEFT JOIN ACCOUNTS a
                           ON TRY_CAST(a.ACCOUNT_ID AS INT) = TRY_CAST(h.CLIENT_ID AS INT)
                    WHERE h.COMPANY_PROFILE_ID = @cid
                    ORDER BY h.INVOICE_DATE DESC, h.INVOICE_ID DESC";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@cid", cid);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                    _dgv.Rows.Add(
                        rdr["InvNo"].ToString(),
                        rdr["Client"].ToString(),
                        rdr["InvDate"].ToString(),
                        "₹" + Convert.ToDecimal(rdr["TotalAmt"]).ToString("N2"),
                        "₹" + Convert.ToDecimal(rdr["GrandTotal"]).ToString("N2")
                    );
            }
            catch (Exception ex)
            {
                _dgv.Rows.Clear();
                _dgv.Rows.Add("ERR", ex.Message.Length > 70 ? ex.Message[..70] : ex.Message, "", "", "");
            }
        }

        // ════════════════════════════════════════════════════════════
        //  NAVIGATION HELPER — generic, works for any page
        // ════════════════════════════════════════════════════════════
        void NavigateTo<T>(Func<Dashboard, Button> getNavBtn) where T : UserControl, new()
        {
            Control p = this.Parent;
            while (p != null && !(p is Dashboard)) p = p.Parent;
            if (p is Dashboard dash)
            {
                dash.LoadPage(new T());
                dash.HighlightNavBtn(getNavBtn(dash));
            }
        }

        // ════════════════════════════════════════════════════════════
        //  STAT CARDS — fully responsive
        //  Card size is computed from available width so it scales on
        //  any monitor / DPI setting. Fonts scale proportionally too.
        // ════════════════════════════════════════════════════════════
        Panel BuildCard(string icon, string val, string name, string sub, Color accent)
        {
            // Card panel — size set by LayoutCards, not hardcoded here
            var p = new Panel { BackColor = Surface };

            // Left accent bar — 4px wide, full height
            var bar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = accent
            };
            p.Controls.Add(bar);

            // Icon
            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 16F),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(14, 10)
            };
            p.Controls.Add(lblIcon);

            // Value — large bold number
            var lblVal = new Label
            {
                Text = val,
                Font = new Font("Consolas", 18F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(14, 40)
            };
            p.Controls.Add(lblVal);

            // Name label
            var lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = TextMid,
                AutoSize = true,
                Location = new Point(14, 76)
            };
            p.Controls.Add(lblName);

            // Sub label
            var lblSub = new Label
            {
                Text = sub,
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(14, 94)
            };
            p.Controls.Add(lblSub);

            // Subtle border via Paint
            p.Paint += (s, e) =>
            {
                var rc = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                e.Graphics.DrawRectangle(new Pen(Border, 1), rc);
            };

            return p;
        }

        // ── Responsive card layout ────────────────────────────────────
        // All sizes derived from available width — no hardcoded pixels.
        void LayoutCards()
        {
            if (_pnlCards == null || _pnlCards.Controls.Count == 0) return;

            const int PAD = 20;   // outer padding
            const int GAP = 10;   // gap between cards
            const int H = 116;  // card height

            int avail = Math.Max(200, _pnlCards.Width - PAD * 2);
            int count = _pnlCards.Controls.Count;

            // Responsive column count based on available width
            int cols = avail >= 1000 ? 4
                     : avail >= 680 ? 3
                     : avail >= 420 ? 2
                     : 1;

            int rows = (int)Math.Ceiling((double)count / cols);
            int w = Math.Max(140, (avail - GAP * (cols - 1)) / cols);

            for (int i = 0; i < count; i++)
            {
                if (_pnlCards.Controls[i] is not Panel card) continue;
                int row = i / cols, col = i % cols;
                card.Size = new Size(w, H);
                card.Location = new Point(PAD + col * (w + GAP), PAD / 2 + row * (H + GAP));
            }

            _pnlCards.Height = PAD / 2 + rows * (H + GAP) + PAD / 2;
        }

        // ════════════════════════════════════════════════════════════
        //  DATA GRID
        // ════════════════════════════════════════════════════════════
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
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Surface2;
            g.ColumnHeadersDefaultCellStyle.ForeColor = TextMid;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Surface2;
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.ColumnHeadersHeight = 38;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.BackColor = Surface;
            g.DefaultCellStyle.ForeColor = TextMid;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.DefaultCellStyle.SelectionBackColor = BluePale;
            g.DefaultCellStyle.SelectionForeColor = TextDark;
            g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.RowTemplate.Height = 38;

            // Alternate row shading
            g.RowsAdded += (s, e) =>
            {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                    if (i >= 0 && i < g.Rows.Count)
                        g.Rows[i].DefaultCellStyle.BackColor =
                            i % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253);
            };

            // Hairline row separator
            g.RowPostPaint += (s, e) =>
            {
                using var pen = new Pen(Border, 1);
                e.Graphics.DrawLine(pen, e.RowBounds.Left, e.RowBounds.Bottom - 1,
                    e.RowBounds.Right, e.RowBounds.Bottom - 1);
            };

            // FIX: CellFormatting applies to ALL rows including row 0
            // Previously row 0 appeared black because SelectionForeColor
            // overrode the formatting on the first selected row.
            g.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                // INV NO column — always blue bold regardless of selection
                if (e.ColumnIndex == g.Columns["InvNo"]?.Index)
                {
                    e.CellStyle.ForeColor = Blue;
                    e.CellStyle.Font = new Font("Consolas", 9F, FontStyle.Bold);
                    e.FormattingApplied = true;
                }
                // GRAND TOTAL column — always green bold
                if (e.ColumnIndex == g.Columns["Grand"]?.Index)
                {
                    e.CellStyle.ForeColor = Green;
                    e.CellStyle.Font = new Font("Consolas", 9.5F, FontStyle.Bold);
                    e.FormattingApplied = true;
                }
            };

            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "INV NO", Name = "InvNo", MinimumWidth = 90 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CLIENT", Name = "Client", MinimumWidth = 180 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DATE", Name = "Date", MinimumWidth = 110 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "AMOUNT", Name = "Amount", MinimumWidth = 110 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "GRAND TOTAL", Name = "Grand", MinimumWidth = 120 });

            return g;
        }

        // ── Format large numbers compactly ────────────────────────────
        string FormatNum(decimal v)
        {
            if (v >= 10000000) return (v / 10000000M).ToString("0.0") + "Cr";
            if (v >= 100000) return (v / 100000M).ToString("0.0") + "L";
            if (v >= 1000) return (v / 1000M).ToString("0.0") + "K";
            return v.ToString("0");
        }
    }
}