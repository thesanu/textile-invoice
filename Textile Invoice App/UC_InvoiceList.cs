using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Textile_Invoice_App
{
    public partial class UC_InvoiceList : UserControl
    {
        static readonly Color Bg = Color.FromArgb(245, 247, 250);
        static readonly Color Surface = Color.White;
        static readonly Color Surface2 = Color.FromArgb(248, 249, 252);
        static readonly Color HeaderBg = Color.FromArgb(241, 245, 249);  // LIGHT header
        static readonly Color Blue = Color.FromArgb(37, 99, 235);
        static readonly Color BlueHov = Color.FromArgb(29, 78, 216);
        static readonly Color BluePale = Color.FromArgb(239, 246, 255);
        static readonly Color Green = Color.FromArgb(22, 163, 74);
        static readonly Color GreenPale = Color.FromArgb(240, 253, 244);
        static readonly Color Purple = Color.FromArgb(109, 40, 217);
        static readonly Color PurplePale = Color.FromArgb(245, 243, 255);
        static readonly Color Amber = Color.FromArgb(180, 83, 9);
        static readonly Color AmberPale = Color.FromArgb(255, 251, 235);
        static readonly Color AmberBdr = Color.FromArgb(253, 230, 138);
        static readonly Color BlueBdr = Color.FromArgb(191, 219, 254);
        static readonly Color PurpleBdr = Color.FromArgb(221, 214, 254);
        static readonly Color Red = Color.FromArgb(220, 38, 38);
        static readonly Color RedPale = Color.FromArgb(254, 242, 242);
        static readonly Color RedBdr = Color.FromArgb(254, 202, 202);
        static readonly Color Border = Color.FromArgb(226, 232, 240);
        static readonly Color RowLine = Color.FromArgb(241, 245, 249);
        static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        static readonly Color TextMid = Color.FromArgb(71, 85, 105);
        static readonly Color TextLight = Color.FromArgb(148, 163, 184);

        DataGridView dgv;
        TextBox txtSearch;
        DateTimePicker dtpFrom, dtpTo;
        Label lblCount, lblTotalAmt, lblTotalGrand;
        int _lastHover = -1;

        // ── Pagination state ──────────────────────────────────────
        private System.Collections.Generic.List<object[]> _allRows = new();
        private int _pageSize = 50;
        private int _pageIndex = 0;
        private Label _lblPageInfo;
        private Button _btnPrev, _btnNext;
        private ComboBox _cmbPageSize;

        public UC_InvoiceList()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Bg;
            Build();
            LoadGrid();
        }

        void Build()
        {
            // ── Page header ──────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            hdr.Controls.Add(new Label
            {
                Text = "Invoice List",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            });
            hdr.Controls.Add(new Label
            {
                Text = SessionManager.CompanyName,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });

            // ── Stat strip ───────────────────────────────────────────
            lblCount = StatVal(Blue);
            lblTotalAmt = StatVal(Amber);
            lblTotalGrand = StatVal(Green);
            var stats = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Bg };
            stats.Controls.AddRange(new Control[] {
                StatCard("🧾", lblCount,      "Invoices",    Blue,   BluePale),
                StatCard("💵", lblTotalAmt,   "Sub Total",   Amber,  AmberPale),
                StatCard("💰", lblTotalGrand, "Grand Total", Green,  GreenPale),
            });
            stats.Resize += (s, e) => {
                int x = 24;
                foreach (Control c in stats.Controls)
                { c.Location = new Point(x, 10); c.Size = new Size(192, 56); x += 204; }
            };

            // ── Card shell ───────────────────────────────────────────
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });

            dgv = BuildDgv();
            var bar = BuildBar();

            // ── Pagination footer ─────────────────────────────────
            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 46,
                BackColor = Surface2
            };
            footer.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border), 0, 0, footer.Width, 0);

            // Page size selector
            footer.Controls.Add(new Label
            {
                Text = "Rows per page:",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(16, 15)
            });
            _cmbPageSize = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 8.5F),
                Size = new Size(72, 26),
                Location = new Point(110, 11)
            };
            _cmbPageSize.Items.AddRange(new object[] { "25", "50", "100", "All" });
            _cmbPageSize.SelectedIndex = 1; // default 50
            _cmbPageSize.SelectedIndexChanged += (s, e) =>
            {
                string sel = _cmbPageSize.SelectedItem?.ToString() ?? "50";
                _pageSize = sel == "All" ? int.MaxValue : int.Parse(sel);
                _pageIndex = 0;
                RenderPage();
            };
            footer.Controls.Add(_cmbPageSize);

            // Prev / Next buttons
            _btnPrev = FlatBtn("◀  Prev", Surface, TextMid, BluePale, border: true);
            _btnPrev.Size = new Size(78, 28);
            _btnPrev.Enabled = false;
            _btnPrev.Click += (s, e) => { _pageIndex--; RenderPage(); };

            _btnNext = FlatBtn("Next  ▶", Surface, TextMid, BluePale, border: true);
            _btnNext.Size = new Size(78, 28);
            _btnNext.Enabled = false;
            _btnNext.Click += (s, e) => { _pageIndex++; RenderPage(); };

            _lblPageInfo = new Label
            {
                Text = "No records",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextMid,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };

            footer.Controls.Add(_btnPrev);
            footer.Controls.Add(_lblPageInfo);
            footer.Controls.Add(_btnNext);

            // Responsive reposition of footer controls
            footer.Resize += (s, e) =>
            {
                int fw = footer.Width;
                _btnNext.Location = new Point(fw - 94, 9);
                _btnPrev.Location = new Point(fw - 180, 9);
                _lblPageInfo.Location = new Point((fw - _lblPageInfo.Width) / 2, 15);
            };

            card.Controls.Add(dgv);     // Fill — first
            card.Controls.Add(footer);  // Bottom — before bar
            card.Controls.Add(bar);     // Top  — after

            this.Controls.Add(card);
            this.Controls.Add(stats);
            this.Controls.Add(hdr);
        }

        Label StatVal(Color c) =>
            new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = c,
                AutoSize = true
            };

        Panel StatCard(string icon, Label val, string cap, Color accent, Color bg)
        {
            var p = new Panel { BackColor = bg, Size = new Size(192, 56) };
            p.Controls.Add(new Panel { Location = new Point(0, 0), Size = new Size(4, 56), BackColor = accent });
            p.Controls.Add(new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 13F),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(12, 9)
            });
            val.Location = new Point(40, 6);
            p.Controls.Add(val);
            p.Controls.Add(new Label
            {
                Text = cap,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(40, 33)
            });
            return p;
        }

        Panel BuildBar()
        {
            // Two-row toolbar: row1 = FY quick buttons, row2 = search + date range
            var bar = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Surface2 };
            bar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border), 0, bar.Height - 1, bar.Width, bar.Height - 1);

            // ── ROW 1 : Financial Year quick-select buttons ───────────
            var rowFY = new Panel { Location = new Point(0, 0), Height = 42, BackColor = Surface2 };
            rowFY.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            bar.Controls.Add(rowFY);

            rowFY.Controls.Add(new Label
            {
                Text = "Quick Filter:",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(16, 14)
            });

            // Helper to create FY pill button
            Button FYBtn(string label, int x)
            {
                var b = FlatBtn(label, Surface, TextMid, BluePale, border: true);
                b.Size = new Size(90, 26); b.Location = new Point(x, 8); b.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
                return b;
            }

            // Indian FY = April 1 → March 31
            DateTime FYStart(int yearsBack = 0)
            {
                int year = DateTime.Today.Month >= 4 ? DateTime.Today.Year : DateTime.Today.Year - 1;
                return new DateTime(year - yearsBack, 4, 1);
            }
            DateTime FYEnd(int yearsBack = 0)
            {
                int year = DateTime.Today.Month >= 4 ? DateTime.Today.Year : DateTime.Today.Year - 1;
                return new DateTime(year - yearsBack + 1, 3, 31);
            }

            var bThisFY = FYBtn("This FY", 96);
            var bLastFY = FYBtn("Last FY", 194);
            var bThisMon = FYBtn("This Month", 292);
            var bLastMon = FYBtn("Last Month", 390);
            var bThisWk = FYBtn("This Week", 488);
            var bAllTime = FYBtn("All Time", 586);

            bThisFY.Click += (s, e) => { dtpFrom.Value = FYStart(); dtpTo.Value = FYEnd(); LoadGrid(); HighlightFY(bThisFY, new[] { bThisFY, bLastFY, bThisMon, bLastMon, bThisWk, bAllTime }); };
            bLastFY.Click += (s, e) => { dtpFrom.Value = FYStart(1); dtpTo.Value = FYEnd(1); LoadGrid(); HighlightFY(bLastFY, new[] { bThisFY, bLastFY, bThisMon, bLastMon, bThisWk, bAllTime }); };
            bThisMon.Click += (s, e) => { dtpFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); dtpTo.Value = DateTime.Today; LoadGrid(); HighlightFY(bThisMon, new[] { bThisFY, bLastFY, bThisMon, bLastMon, bThisWk, bAllTime }); };
            bLastMon.Click += (s, e) => { var d = DateTime.Today.AddMonths(-1); dtpFrom.Value = new DateTime(d.Year, d.Month, 1); dtpTo.Value = new DateTime(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month)); LoadGrid(); HighlightFY(bLastMon, new[] { bThisFY, bLastFY, bThisMon, bLastMon, bThisWk, bAllTime }); };
            bThisWk.Click += (s, e) => { dtpFrom.Value = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek); dtpTo.Value = DateTime.Today; LoadGrid(); HighlightFY(bThisWk, new[] { bThisFY, bLastFY, bThisMon, bLastMon, bThisWk, bAllTime }); };
            bAllTime.Click += (s, e) => { dtpFrom.Value = new DateTime(2000, 1, 1); dtpTo.Value = DateTime.Today; LoadGrid(); HighlightFY(bAllTime, new[] { bThisFY, bLastFY, bThisMon, bLastMon, bThisWk, bAllTime }); };

            rowFY.Controls.Add(bThisFY);
            rowFY.Controls.Add(bLastFY);
            rowFY.Controls.Add(bThisMon);
            rowFY.Controls.Add(bLastMon);
            rowFY.Controls.Add(bThisWk);
            rowFY.Controls.Add(bAllTime);

            // Reposition FY buttons on resize
            bar.Resize += (s, e) => {
                rowFY.Width = bar.Width;
            };

            // ── ROW 2 : Divider + Search + Date Range + Apply ─────────
            var divider = new Panel { Location = new Point(0, 42), Height = 1, BackColor = Border };
            divider.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            bar.Controls.Add(divider);

            var row2 = new Panel { Location = new Point(0, 43), Height = 57, BackColor = Surface2 };
            row2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            bar.Controls.Add(row2);

            // Search box
            var srch = new Panel { Location = new Point(16, 13), Size = new Size(228, 32), BackColor = Color.White };
            srch.Paint += (s, e) => {
                e.Graphics.DrawRectangle(new Pen(Border), 0, 0, srch.Width - 1, srch.Height - 1);
                e.Graphics.DrawString("🔍", new Font("Segoe UI Emoji", 9F),
                    new SolidBrush(TextLight), new PointF(5, 7));
            };
            txtSearch = new TextBox
            {
                PlaceholderText = "Search invoice or client…",
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = TextDark,
                Location = new Point(27, 7),
                Size = new Size(193, 20)
            };
            txtSearch.TextChanged += (s, e) => LoadGrid();
            srch.Controls.Add(txtSearch);
            row2.Controls.Add(srch);

            // From / To labels and pickers
            var lblFrom = FLbl("From", 260, 20);
            dtpFrom = Dtp(298, 13, FYStart());   // default = start of current FY
            var lblTo = FLbl("To", 430, 20);
            dtpTo = Dtp(452, 13, DateTime.Today);

            row2.Controls.Add(lblFrom);
            row2.Controls.Add(dtpFrom);
            row2.Controls.Add(lblTo);
            row2.Controls.Add(dtpTo);

            var bApply = FlatBtn("Apply", Blue, Color.White, BlueHov);
            bApply.Size = new Size(80, 32); bApply.Location = new Point(584, 13);
            bApply.Click += (s, e) => LoadGrid();
            row2.Controls.Add(bApply);

            var bRef = FlatBtn("⟳", Surface, TextMid, BluePale, border: true);
            bRef.Size = new Size(32, 32); bRef.Location = new Point(672, 13);
            bRef.Font = new Font("Segoe UI", 11F); bRef.TabStop = false;
            bRef.Click += (s, e) => LoadGrid();
            row2.Controls.Add(bRef);

            // Reposition row2 right-side controls on resize
            bar.Resize += (s2, e2) => {
                row2.Width = bar.Width;
                divider.Width = bar.Width;
                int rw = row2.Width;
                bRef.Location = new Point(rw - 44, 13);
                bApply.Location = new Point(rw - 132, 13);
                dtpTo.Location = new Point(rw - 270, 13);
                lblTo.Location = new Point(rw - 278 - lblTo.Width, 20);
                dtpFrom.Location = new Point(rw - 278 - lblTo.Width - 12 - dtpFrom.Width, 13);
                lblFrom.Location = new Point(rw - 278 - lblTo.Width - 12 - dtpFrom.Width - lblFrom.Width - 6, 20);
            };

            // Highlight This FY by default
            HighlightFY(bThisFY, new[] { bThisFY, bLastFY, bThisMon, bLastMon, bThisWk, bAllTime });

            return bar;
        }

        // Highlight the active FY quick-filter button, reset the others
        void HighlightFY(Button active, Button[] all)
        {
            foreach (var b in all)
            {
                b.BackColor = Surface;
                b.ForeColor = TextMid;
                b.FlatAppearance.BorderColor = Border;
            }
            active.BackColor = BluePale;
            active.ForeColor = Blue;
            active.FlatAppearance.BorderColor = Blue;
        }

        Label FLbl(string t, int x, int y) =>
            new Label
            {
                Text = t,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(x, y)
            };

        DateTimePicker Dtp(int x, int y, DateTime val) =>
            new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9F),
                Size = new Size(122, 32),
                Location = new Point(x, y),
                Value = val
            };

        Button FlatBtn(string text, Color bg, Color fg, Color hover, bool border = false)
        {
            var b = new Button
            {
                Text = text,
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = border ? 1 : 0;
            b.FlatAppearance.BorderColor = Border;
            b.FlatAppearance.MouseOverBackColor = hover;
            return b;
        }

        void LoadGrid()
        {
            _allRows.Clear();
            _pageIndex = 0;
            _lastHover = -1;
            decimal sumAmt = 0, sumGrand = 0;
            try
            {
                int cid = SessionManager.CompanyProfileId;
                string from = dtpFrom.Value.Date.ToString("yyyy-MM-dd");
                string to = dtpTo.Value.Date.ToString("yyyy-MM-dd");
                string q = txtSearch.Text.Trim().ToLower();
                string cs;
                using (var db = new AppDbContext()) cs = db.Database.GetConnectionString()!;

                const string sql = @"
                    SELECT h.INVOICE_ID,
                           ISNULL(h.INVOICE_NO,'')                    AS InvNo,
                           ISNULL(a.ACC_NM,'—')                       AS Client,
                           CONVERT(VARCHAR(11),h.INVOICE_DATE,106)    AS InvDate,
                           ISNULL(h.TOTAL_AMOUNT,0)                   AS TotalAmt,
                           ISNULL(h.CGST,0) AS Cgst, ISNULL(h.SGST,0) AS Sgst,
                           ISNULL(h.IGST,0) AS Igst, ISNULL(h.GRAND_TOTAL,0) AS GrandTotal
                    FROM   INVOICE_HEADER h
                    LEFT JOIN ACCOUNTS a
                           ON TRY_CAST(a.ACCOUNT_ID AS INT)=TRY_CAST(h.CLIENT_ID AS INT)
                    WHERE  h.COMPANY_PROFILE_ID=@cid
                      AND  CAST(h.INVOICE_DATE AS DATE) BETWEEN @from AND @to
                    ORDER  BY h.INVOICE_DATE DESC, h.INVOICE_ID DESC";

                using var conn = new SqlConnection(cs);
                conn.Open();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@cid", cid);
                cmd.Parameters.AddWithValue("@from", from);
                cmd.Parameters.AddWithValue("@to", to);

                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        string inv = rdr["InvNo"].ToString()!;
                        string cli = rdr["Client"].ToString()!;
                        if (!string.IsNullOrWhiteSpace(q) &&
                            !inv.ToLower().Contains(q) &&
                            !cli.ToLower().Contains(q)) continue;
                        decimal a1 = Convert.ToDecimal(rdr["TotalAmt"]);
                        decimal g1 = Convert.ToDecimal(rdr["GrandTotal"]);
                        sumAmt += a1; sumGrand += g1;
                        _allRows.Add(new object[] {
                            rdr["INVOICE_ID"], inv, cli, rdr["InvDate"].ToString(),
                            "₹"+a1.ToString("N2"),
                            "₹"+Convert.ToDecimal(rdr["Cgst"]).ToString("N2"),
                            "₹"+Convert.ToDecimal(rdr["Sgst"]).ToString("N2"),
                            "₹"+Convert.ToDecimal(rdr["Igst"]).ToString("N2"),
                            "₹"+g1.ToString("N2")
                        });
                    }

                lblCount.Text = _allRows.Count.ToString();
                lblTotalAmt.Text = Fmt(sumAmt);
                lblTotalGrand.Text = Fmt(sumGrand);
                RenderPage();
            }
            catch (Exception ex)
            {
                lblCount.Text = "!";
                MessageBox.Show("Load error: " + ex.Message, "Invoice List",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void RenderPage()
        {
            dgv.Rows.Clear();
            _lastHover = -1;
            int total = _allRows.Count;
            int pages = _pageSize == int.MaxValue ? 1 : (int)Math.Ceiling(total / (double)_pageSize);
            if (pages == 0) pages = 1;
            if (_pageIndex >= pages) _pageIndex = pages - 1;

            int start = _pageIndex * (_pageSize == int.MaxValue ? total : _pageSize);
            int end = Math.Min(start + (_pageSize == int.MaxValue ? total : _pageSize), total);

            for (int i = start; i < end; i++)
                dgv.Rows.Add(_allRows[i]);

            if (_lblPageInfo != null)
                _lblPageInfo.Text = total == 0
                    ? "No records"
                    : $"Page {_pageIndex + 1} of {pages}  ({start + 1}–{end} of {total})";

            if (_btnPrev != null) _btnPrev.Enabled = _pageIndex > 0;
            if (_btnNext != null) _btnNext.Enabled = _pageIndex < pages - 1;

            // Reposition page info label to centre
            if (_lblPageInfo != null && _lblPageInfo.Parent != null)
            {
                _lblPageInfo.Left = (_lblPageInfo.Parent.Width - _lblPageInfo.Width) / 2;
            }
        }

        string Fmt(decimal v)
        {
            if (v >= 10_000_000) return "₹" + (v / 10_000_000M).ToString("0.0") + "Cr";
            if (v >= 100_000) return "₹" + (v / 100_000M).ToString("0.0") + "L";
            if (v >= 1_000) return "₹" + (v / 1_000M).ToString("0.0") + "K";
            return "₹" + v.ToString("N0");
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
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                ScrollBars = ScrollBars.Both
            };
            // Enable double-buffering via reflection to cut flicker
            typeof(DataGridView)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(g, true);

            // ── LIGHT header ──────────────────────────────────────
            g.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg;
            g.ColumnHeadersDefaultCellStyle.ForeColor = TextMid;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderBg;
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.ColumnHeadersHeight = 36;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            g.DefaultCellStyle.BackColor = Surface;
            g.DefaultCellStyle.ForeColor = TextMid;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.DefaultCellStyle.SelectionBackColor = BluePale;
            g.DefaultCellStyle.SelectionForeColor = TextDark;
            g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.RowTemplate.Height = 34;

            // Columns
            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "INV NO", Name = "InvNo", MinimumWidth = 90, FillWeight = 9 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CLIENT", Name = "Client", MinimumWidth = 160, FillWeight = 20 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DATE", Name = "Date", MinimumWidth = 100, FillWeight = 10 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "AMOUNT", Name = "Amt", MinimumWidth = 100, FillWeight = 10 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CGST", Name = "Cgst", MinimumWidth = 80, FillWeight = 8 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SGST", Name = "Sgst", MinimumWidth = 80, FillWeight = 8 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IGST", Name = "Igst", MinimumWidth = 80, FillWeight = 8 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "GRAND TOTAL", Name = "Grand", MinimumWidth = 120, FillWeight = 12 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Edit", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 70, MinimumWidth = 70 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Print", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 70, MinimumWidth = 70 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Pdf", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 70, MinimumWidth = 70 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Delete", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 76, MinimumWidth = 76 });

            // Alternate rows
            g.RowsAdded += (s, e) => {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                    if (i >= 0 && i < g.Rows.Count)
                        g.Rows[i].DefaultCellStyle.BackColor =
                            i % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253);
            };

            g.CellFormatting += (s, e) => {
                if (e.RowIndex < 0) return;
                if (e.ColumnIndex == g.Columns["InvNo"]?.Index)
                { e.CellStyle.ForeColor = Blue; e.CellStyle.Font = new Font("Consolas", 9F, FontStyle.Bold); }
                if (e.ColumnIndex == g.Columns["Grand"]?.Index)
                { e.CellStyle.ForeColor = Green; e.CellStyle.Font = new Font("Consolas", 9.5F, FontStyle.Bold); }
                int ei = g.Columns["Edit"]?.Index ?? -1, pi = g.Columns["Print"]?.Index ?? -1,
                    fi = g.Columns["Pdf"]?.Index ?? -1, di = g.Columns["Delete"]?.Index ?? -1;
                if (e.ColumnIndex == ei || e.ColumnIndex == pi || e.ColumnIndex == fi || e.ColumnIndex == di)
                { e.Value = ""; e.FormattingApplied = true; }
            };

            // ── Custom-painted action pills — size NEVER changes on hover → no vibration ──
            g.CellPainting += (s, e) => {
                if (e.RowIndex < 0) return;
                int ei = g.Columns["Edit"]?.Index ?? -1, pi = g.Columns["Print"]?.Index ?? -1,
                    fi = g.Columns["Pdf"]?.Index ?? -1, di = g.Columns["Delete"]?.Index ?? -1;
                if (e.ColumnIndex != ei && e.ColumnIndex != pi && e.ColumnIndex != fi && e.ColumnIndex != di) return;

                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);

                var rc = new Rectangle(
                    e.CellBounds.X + 7, e.CellBounds.Y + 6,
                    e.CellBounds.Width - 14, e.CellBounds.Height - 12);

                Color bg, fg, bdr; string lbl;
                if (e.ColumnIndex == ei) { bg = AmberPale; fg = Amber; bdr = AmberBdr; lbl = "✏  Edit"; }
                else if (e.ColumnIndex == pi) { bg = BluePale; fg = Blue; bdr = BlueBdr; lbl = "🖨  Print"; }
                else if (e.ColumnIndex == fi) { bg = PurplePale; fg = Purple; bdr = PurpleBdr; lbl = "📄  PDF"; }
                else { bg = RedPale; fg = Red; bdr = RedBdr; lbl = "🗑 Delete"; }

                var gr = e.Graphics;
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = RRect(rc, 5);
                gr.FillPath(new SolidBrush(bg), path);
                gr.DrawPath(new Pen(bdr, 1f), path);
                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                gr.DrawString(lbl, new Font("Segoe UI", 8F, FontStyle.Bold),
                    new SolidBrush(fg), rc, sf);
            };

            // ── Hover — tracked with _lastHover to prevent double invalidation → no vibration ──
            g.CellMouseEnter += (s, e) => {
                if (e.RowIndex < 0 || e.RowIndex == _lastHover) return;
                ResetHoverRow(g);
                _lastHover = e.RowIndex;
                g.Rows[e.RowIndex].DefaultCellStyle.BackColor = BluePale;
                g.InvalidateRow(e.RowIndex);
            };
            g.MouseLeave += (s, e) => ResetHoverRow(g);

            // Hand cursor on action cols
            g.CellMouseMove += (s, e) => {
                int ei = g.Columns["Edit"]?.Index ?? -1, pi = g.Columns["Print"]?.Index ?? -1,
                    fi = g.Columns["Pdf"]?.Index ?? -1, di = g.Columns["Delete"]?.Index ?? -1;
                g.Cursor = e.RowIndex >= 0 && (e.ColumnIndex == ei || e.ColumnIndex == pi
                                             || e.ColumnIndex == fi || e.ColumnIndex == di)
                    ? Cursors.Hand : Cursors.Default;
            };

            // Hairline row separators
            g.RowPostPaint += (s, e) => {
                using var p = new Pen(RowLine, 1);
                e.Graphics.DrawLine(p,
                    e.RowBounds.Left, e.RowBounds.Bottom - 1,
                    e.RowBounds.Right, e.RowBounds.Bottom - 1);
            };

            g.CellClick += (s, e) => {
                if (e.RowIndex < 0) return;
                int invId = Convert.ToInt32(g.Rows[e.RowIndex].Cells["Id"].Value);
                var form = ((Control)s).FindForm();
                int ei = g.Columns["Edit"]?.Index ?? -1, pi = g.Columns["Print"]?.Index ?? -1,
                    fi = g.Columns["Pdf"]?.Index ?? -1, di = g.Columns["Delete"]?.Index ?? -1;
                if (e.ColumnIndex == ei) OpenEdit(invId);
                if (e.ColumnIndex == pi) InvoicePrintService.PrintInvoice(invId, form);
                if (e.ColumnIndex == fi) InvoicePrintService.SavePdf(invId, form);
                if (e.ColumnIndex == di) DeleteInvoice(invId, g.Rows[e.RowIndex].Cells["InvNo"].Value?.ToString() ?? "");
            };

            return g;
        }

        void ResetHoverRow(DataGridView g)
        {
            if (_lastHover >= 0 && _lastHover < g.Rows.Count)
            {
                g.Rows[_lastHover].DefaultCellStyle.BackColor =
                    _lastHover % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253);
                g.InvalidateRow(_lastHover);
                _lastHover = -1;
            }
        }

        void DeleteInvoice(int id, string invNo)
        {
            // FIX: Two-step confirm — first warn about GST compliance gap,
            // then require a second explicit confirmation to proceed.
            var gstWarn = MessageBox.Show(
                $"⚠  GST Compliance Notice\n\n" +
                $"Deleting Invoice {invNo} will create a gap in your invoice number sequence.\n\n" +
                $"Under GST rules, gaps in invoice numbering can attract scrutiny during audits.\n" +
                $"Consider cancelling the invoice instead of deleting it.\n\n" +
                $"Do you still want to permanently delete this invoice?",
                "Delete Invoice — GST Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);   // default = No (safer)

            if (gstWarn != DialogResult.Yes) return;

            var confirm = MessageBox.Show(
                $"Last chance — permanently delete Invoice {invNo} and all its line items?\n\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button2);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var db = new AppDbContext();
                var hdr = db.InvoiceHeaders.Find(id);
                if (hdr == null) { MessageBox.Show("Invoice not found.", "Error"); return; }

                // Lines are deleted via ON DELETE CASCADE defined in the DB,
                // but we also remove them explicitly for EF to be safe
                var lines = db.InvoiceItems.Where(i => i.InvoiceId == id).ToList();
                db.InvoiceItems.RemoveRange(lines);
                db.InvoiceHeaders.Remove(hdr);
                db.SaveChanges();

                LoadGrid();   // refresh the list
            }
            catch (Exception ex)
            {
                MessageBox.Show("Delete error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void OpenEdit(int id)
        {
            Control p = this.Parent;
            while (p != null && !(p is Dashboard)) p = p.Parent;
            if (p is Dashboard d) { d.LoadPage(new UC_CreateInvoice(id)); d.HighlightNavBtn(d.BtnCreateInvoice); }
            else
            {
                var f = new Form
                {
                    Text = $"Edit Invoice #{id}",
                    Size = new Size(1200, 800),
                    StartPosition = FormStartPosition.CenterParent,
                    WindowState = FormWindowState.Maximized
                };
                f.Controls.Add(new UC_CreateInvoice(id) { Dock = DockStyle.Fill });
                f.ShowDialog(); LoadGrid();
            }
        }

        static GraphicsPath RRect(Rectangle r, int rad)
        {
            int d = rad * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}