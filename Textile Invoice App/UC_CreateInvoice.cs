using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App
{
    public partial class UC_CreateInvoice : UserControl
    {
        // ── Palette ───────────────────────────────────────────────────────────
        static readonly Color Bg = Color.FromArgb(245, 247, 250);
        static readonly Color Surface = Color.White;
        static readonly Color Surface2 = Color.FromArgb(248, 249, 252);
        static readonly Color Blue = Color.FromArgb(37, 99, 235);
        static readonly Color BlueL = Color.FromArgb(59, 130, 246);
        static readonly Color BluePale = Color.FromArgb(239, 246, 255);
        static readonly Color Green = Color.FromArgb(22, 163, 74);
        static readonly Color GreenPale = Color.FromArgb(240, 253, 244);
        static readonly Color Red = Color.FromArgb(220, 38, 38);
        static readonly Color Amber = Color.FromArgb(217, 119, 6);
        static readonly Color Border = Color.FromArgb(226, 232, 240);
        static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        static readonly Color TextMid = Color.FromArgb(71, 85, 105);
        static readonly Color TextLight = Color.FromArgb(148, 163, 184);

        // ── Controls ─────────────────────────────────────────────────────────
        Label lblInvoiceNo;
        DateTimePicker dtpInvoiceDate, dtpChallanDate;
        TextBox txtChallanNo;
        ComboBox cmbClient, cmbBroker, cmbTransport;
        Label lblClientAddr, lblClientGstin, lblClientState;
        DataGridView dgv;

        // Totals sidebar
        Label lblSubTotal, lblCgstAmt, lblSgstAmt, lblIgstAmt, lblGrandTotal, lblAmtWords;
        TextBox txtCgstPct, txtSgstPct, txtIgstPct, txtRoundup;
        Button _btnSave, _btnPrint, _btnPdf;
        Panel pnlGrandBand;   // highlighted grand total band

        // State
        int _lastSavedInvoiceId = -1;
        int _editInvoiceId = -1;
        bool _isEditMode => _editInvoiceId > 0;
        bool _recalcLock = false;
        List<DesignMaster> _designs = new();
        List<Account> _clients = new();
        List<Account> _brokers = new();
        List<Account> _transports = new();

        // ── Constructors ─────────────────────────────────────────────────────
        public UC_CreateInvoice()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Bg;
            Build();
            LoadMasterData();
            SetNextInvoiceNo();
        }

        public UC_CreateInvoice(int invoiceId) : this()
        {
            _editInvoiceId = invoiceId;
            LoadInvoiceForEdit(invoiceId);
        }

        // ════════════════════════════════════════════════════════════════════
        //  BUILD — two-pane layout
        // ════════════════════════════════════════════════════════════════════
        private void Build()
        {
            // ── Root split: left form | right totals sidebar ─────────────
            var pnlRight = BuildTotalsSidebar();          // fixed right sidebar
            pnlRight.Dock = DockStyle.Right;
            pnlRight.Width = 270;

            var pnlLeft = new Panel { Dock = DockStyle.Fill, BackColor = Bg };

            // ── Page header (inside left pane) ───────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Bg };
            var lblTitle = new Label
            {
                Text = _isEditMode ? "Edit Invoice" : "Create Invoice",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            };
            var lblSub = new Label
            {
                Text = SessionManager.CompanyName,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMid,
                AutoSize = true,
                Location = new Point(26, 34)
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);

            // ── Scrollable content ───────────────────────────────────────
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Bg };

            var sec1 = BuildSec1();
            var sec2 = BuildSec2();
            var sec3 = BuildSec3();

            int cx = 16;
            void LaySection(Panel s, ref int sy)
            {
                s.Location = new Point(cx, sy);
                s.Width = scroll.Width - cx * 2;
                s.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                scroll.Controls.Add(s);
                sy += s.Height + 10;
            }

            int sy = 10;
            LaySection(sec1, ref sy);
            LaySection(sec2, ref sy);
            LaySection(sec3, ref sy);

            // Relayout all sections when any section changes height (e.g. client strip toggle)
            void RelayoutAll()
            {
                int ry = 10;
                foreach (var s in new[] { sec1, sec2, sec3 })
                {
                    s.Location = new Point(cx, ry);
                    ry += s.Height + 10;
                }
            }

            sec2.Resize += (s, e) => RelayoutAll();

            scroll.Resize += (s, e) => {
                sec1.Width = sec2.Width = sec3.Width = scroll.Width - cx * 2;
                dgv.Width = sec3.Width;
            };

            sec3.Resize += (s, e) => dgv.Width = sec3.Width;

            pnlLeft.Controls.Add(scroll);
            pnlLeft.Controls.Add(pnlHeader);

            // Order matters: Right dock before Fill dock
            this.Controls.Add(pnlLeft);
            this.Controls.Add(pnlRight);

            // Thin separator between left and right
            var sep = new Panel { Dock = DockStyle.Right, Width = 1, BackColor = Border };
            this.Controls.Add(sep);
        }

        // ════════════════════════════════════════════════════════════════════
        //  SECTION 1 — Invoice Information
        // ════════════════════════════════════════════════════════════════════
        private Panel BuildSec1()
        {
            var p = MakeCard("📋  Invoice Information");
            int y = 44;

            // Row 1: Invoice No | Invoice Date | Challan No | Challan Date
            MakeFieldGroup(p, "Invoice No", 16, y, out Panel grpInvNo);
            lblInvoiceNo = new Label
            {
                Text = "Auto",
                Font = new Font("Consolas", 12F, FontStyle.Bold),
                ForeColor = Blue,
                AutoSize = true,
                Location = new Point(0, 22)
            };
            grpInvNo.Controls.Add(lblInvoiceNo);

            MakeFieldGroup(p, "Invoice Date", 175, y, out Panel grpDate);
            dtpInvoiceDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9.5F),
                Size = new Size(140, 28),
                Location = new Point(0, 20),
                Value = DateTime.Today
            };
            grpDate.Controls.Add(dtpInvoiceDate);

            MakeFieldGroup(p, "Challan No", 360, y, out Panel grpChNo);
            txtChallanNo = new TextBox
            {
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = TextDark,
                Size = new Size(140, 28),
                Location = new Point(0, 20)
            };
            grpChNo.Controls.Add(txtChallanNo);

            MakeFieldGroup(p, "Challan Date (optional)", 545, y, out Panel grpChDate);
            dtpChallanDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9.5F),
                Size = new Size(155, 28),
                Location = new Point(0, 20),
                Value = DateTime.Today,
                ShowCheckBox = true,
                Checked = false
            };
            grpChDate.Controls.Add(dtpChallanDate);

            p.Height = y + 62;
            return p;
        }

        // ════════════════════════════════════════════════════════════════════
        //  SECTION 2 — Party Information
        // ════════════════════════════════════════════════════════════════════
        private Panel BuildSec2()
        {
            var p = MakeCard("🏢  Party Information");
            int y = 44;

            // Client — full width top row
            MakeFieldGroup(p, "Bill To (Client) *", 16, y, out Panel grpClient);
            cmbClient = MakeCmb(0, 20, 320);
            cmbClient.SelectedIndexChanged += CmbClient_Changed;
            grpClient.Controls.Add(cmbClient);

            // Transport
            MakeFieldGroup(p, "Transport", 380, y, out Panel grpTrans);
            cmbTransport = MakeCmb(0, 20, 220);
            grpTrans.Controls.Add(cmbTransport);

            // Broker
            MakeFieldGroup(p, "Broker", 644, y, out Panel grpBroker);
            cmbBroker = MakeCmb(0, 20, 200);
            grpBroker.Controls.Add(cmbBroker);

            // Client detail strip
            var strip = new Panel
            {
                Location = new Point(14, y + 62),
                Size = new Size(860, 52),
                BackColor = BluePale
            };
            strip.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 3, BackColor = Blue });

            lblClientAddr = new Label { Text = "", Font = new Font("Segoe UI", 8.5F), ForeColor = TextMid, AutoSize = true, Location = new Point(10, 6) };
            lblClientGstin = new Label { Text = "", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = TextDark, AutoSize = true, Location = new Point(10, 22) };
            lblClientState = new Label { Text = "", Font = new Font("Segoe UI", 8F), ForeColor = TextMid, AutoSize = true, Location = new Point(10, 38) };
            strip.Controls.Add(lblClientAddr);
            strip.Controls.Add(lblClientGstin);
            strip.Controls.Add(lblClientState);
            strip.Visible = false;   // hidden until client selected

            p.Controls.Add(strip);
            p.Resize += (s, e) => strip.Width = p.Width - 28;

            // Toggle strip visibility on client change
            cmbClient.SelectedIndexChanged += (s, e) => {
                var ci = cmbClient.SelectedItem as CItem;
                strip.Visible = ci != null && ci.Id > 0;
            };

            // Height without strip showing — strip will expand it if visible
            p.Height = y + 62 + 14;   // compact: just combos + padding

            // When strip shows/hides, adjust section height
            cmbClient.SelectedIndexChanged += (s2, e2) => {
                var ci2 = cmbClient.SelectedItem as CItem;
                bool showing = ci2 != null && ci2.Id > 0;
                p.Height = showing ? y + 62 + 58 + 14 : y + 62 + 14;
            };

            return p;
        }

        // ════════════════════════════════════════════════════════════════════
        //  SECTION 3 — Items Grid
        // ════════════════════════════════════════════════════════════════════
        private Panel BuildSec3()
        {
            // Card with absolute layout — title at y=10, toolbar at y=34, grid at y=78
            var p = MakeCard("📦  Description of Goods");

            // ── Toolbar — absolutely positioned below the title ───────────
            // y=34 → sits just below the section title (y=10 + ~22px font)
            var btnAdd = MakeToolBtn("＋  Add Row", Color.FromArgb(37, 99, 235));
            var btnDel = MakeToolBtn("✕  Delete Row", Color.FromArgb(220, 38, 38));
            var btnClr = MakeToolBtn("⟳  Clear All", Color.FromArgb(100, 116, 139));

            btnAdd.Click += (s, e) => AddGridRow();
            btnDel.Click += (s, e) => DeleteGridRow();
            btnClr.Click += (s, e) => {
                if (MessageBox.Show("Clear all rows?", "Confirm", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                { dgv.Rows.Clear(); RecalcTotals(); }
            };

            // Position buttons manually — no Dock, no FlowLayout conflicts
            btnAdd.Location = new Point(12, 36);
            btnDel.Location = new Point(126, 36);
            btnClr.Location = new Point(240, 36);
            p.Controls.Add(btnAdd);
            p.Controls.Add(btnDel);
            p.Controls.Add(btnClr);

            // Tip label to the right of buttons
            var lblTip = new Label
            {
                Text = "💡 Select design → HSN & Rate auto-fill",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(356, 42)
            };
            p.Controls.Add(lblTip);

            // Thin separator line between toolbar and grid
            var divToolbar = new Panel
            {
                BackColor = Border,
                Height = 1,
                Location = new Point(0, 72),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            p.Controls.Add(divToolbar);
            p.Resize += (s, e) => divToolbar.Width = p.Width;

            // ── Grid — starts at y=76, fills remaining height ─────────────
            dgv = BuildGrid();
            dgv.Location = new Point(0, 76);
            dgv.Height = 260;
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            p.Controls.Add(dgv);

            p.Height = 76 + 260 + 8;
            return p;
        }

        // ════════════════════════════════════════════════════════════════════
        //  RIGHT SIDEBAR — Totals (always visible)
        // ════════════════════════════════════════════════════════════════════
        private Panel BuildTotalsSidebar()
        {
            var sidebar = new Panel { BackColor = Surface };

            // Top accent — absolute, no DockStyle
            var sAccent = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(270, 3),
                BackColor = Green,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            sidebar.Controls.Add(sAccent);
            sidebar.Resize += (s, e) => sAccent.Width = sidebar.Width;

            // Title
            var lblTitle = new Label
            {
                Text = "💰  Invoice Totals",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 12)
            };
            sidebar.Controls.Add(lblTitle);

            // ── Tax rate inputs ───────────────────────────────────────────
            int ty = 40;
            txtCgstPct = AddTaxRow(sidebar, "CGST %", ref ty, "2.5");
            txtSgstPct = AddTaxRow(sidebar, "SGST %", ref ty, "2.5");
            txtIgstPct = AddTaxRow(sidebar, "IGST %", ref ty, "0.0");
            txtRoundup = AddTaxRow(sidebar, "Roundup ±", ref ty, "0.00");

            txtCgstPct.TextChanged += (s, e) => RecalcTotals();
            txtSgstPct.TextChanged += (s, e) => RecalcTotals();
            txtIgstPct.TextChanged += (s, e) => RecalcTotals();
            txtRoundup.TextChanged += (s, e) => RecalcTotals();

            // ── Totals rows ───────────────────────────────────────────────
            int ay = ty + 6;
            var divTop = new Panel { BackColor = Border, Size = new Size(238, 1), Location = new Point(16, ay) };
            sidebar.Controls.Add(divTop);
            ay += 8;

            lblSubTotal = AddAmtRow(sidebar, "Sub Total", ref ay, Blue, false);
            lblCgstAmt = AddAmtRow(sidebar, "CGST", ref ay, TextMid, false);
            lblSgstAmt = AddAmtRow(sidebar, "SGST", ref ay, TextMid, false);
            lblIgstAmt = AddAmtRow(sidebar, "IGST", ref ay, TextMid, false);

            var divBot = new Panel { BackColor = Border, Size = new Size(238, 1), Location = new Point(16, ay) };
            sidebar.Controls.Add(divBot);
            ay += 4;

            // Grand Total band
            pnlGrandBand = new Panel
            {
                BackColor = GreenPale,
                Size = new Size(238, 44),
                Location = new Point(16, ay)
            };
            pnlGrandBand.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 4, BackColor = Green });
            var lblGrandLbl = new Label
            {
                Text = "GRAND TOTAL",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Green,
                AutoSize = true,
                Location = new Point(12, 6)
            };
            lblGrandTotal = new Label
            {
                Text = "₹ 0.00",
                Font = new Font("Consolas", 14F, FontStyle.Bold),
                ForeColor = Green,
                AutoSize = true,
                Location = new Point(12, 22)
            };
            pnlGrandBand.Controls.Add(lblGrandLbl);
            pnlGrandBand.Controls.Add(lblGrandTotal);
            sidebar.Controls.Add(pnlGrandBand);
            ay += 50;

            // Amount in words
            lblAmtWords = new Label
            {
                Text = "—",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                ForeColor = TextLight,
                Size = new Size(238, 48),
                Location = new Point(16, ay),
                AutoEllipsis = true
            };
            sidebar.Controls.Add(lblAmtWords);
            ay += 52;

            // ── Action buttons ────────────────────────────────────────────
            var divAct = new Panel { BackColor = Border, Size = new Size(238, 1), Location = new Point(16, ay) };
            sidebar.Controls.Add(divAct);
            ay += 10;

            _btnSave = MakeSideBtn(_isEditMode ? "✏️  Update Invoice" : "💾  Save Invoice",
                                    Green, 16, ay, 238);
            _btnSave.Click += BtnSave_Click;
            sidebar.Controls.Add(_btnSave);
            ay += 44;

            _btnPrint = MakeSideBtn("🖨  Print", Color.FromArgb(71, 85, 105), 16, ay, 115);
            _btnPdf = MakeSideBtn("📄  PDF", Color.FromArgb(124, 58, 237), 137, ay, 117);
            _btnPrint.Enabled = _btnPdf.Enabled = false;
            _btnPrint.Click += (s, e) => PrintLastInvoice();
            _btnPdf.Click += (s, e) => PdfLastInvoice();
            sidebar.Controls.Add(_btnPrint);
            sidebar.Controls.Add(_btnPdf);
            ay += 44;

            // New Invoice button
            var btnNew = MakeSideBtn("↩  New Invoice", BlueL, 16, ay, 238);
            btnNew.Click += (s, e) => { ClearForm(); SetNextInvoiceNo(); };
            sidebar.Controls.Add(btnNew);

            // Resize: keep sidebar controls full width
            sidebar.Resize += (s, e) => {
                int w = sidebar.Width - 32;
                foreach (Control c in sidebar.Controls)
                {
                    if (c is Panel dp && dp.Height == 1) dp.Width = w;
                    if (c == pnlGrandBand) pnlGrandBand.Width = w;
                    if (c == lblAmtWords) lblAmtWords.Width = w;
                }
                // reposition buttons
                _btnSave.Width = w;
                _btnPrint.Width = w / 2;
                _btnPdf.Location = new Point(16 + w / 2 + 4, _btnPdf.Top);
                _btnPdf.Width = w - w / 2 - 4;
                btnNew.Width = w;
            };

            return sidebar;
        }

        // ── Helpers for sidebar rows ──────────────────────────────────────────
        private TextBox AddTaxRow(Panel p, string label, ref int y, string defVal)
        {
            p.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = TextMid,
                AutoSize = true,
                Location = new Point(16, y + 3)
            });
            var txt = new TextBox
            {
                Text = defVal,
                Font = new Font("Consolas", 9.5F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = TextDark,
                Size = new Size(72, 26),
                Location = new Point(182, y),
                TextAlign = HorizontalAlignment.Right
            };
            p.Controls.Add(txt);
            y += 30;
            return txt;
        }

        private Label AddAmtRow(Panel p, string label, ref int y, Color valColor, bool bold)
        {
            p.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextMid,
                AutoSize = true,
                Location = new Point(16, y + 3)
            });
            var val = new Label
            {
                Text = "₹ 0.00",
                Font = new Font("Consolas", bold ? 11F : 9.5F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = valColor,
                AutoSize = true,
                Location = new Point(130, y + 2)
            };
            p.Controls.Add(val);
            y += 26;
            return val;
        }

        private Button MakeSideBtn(string text, Color bg, int x, int y, int w)
        {
            var b = new Button
            {
                Text = text,
                BackColor = bg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(w, 36),
                Location = new Point(x, y),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.15f);
            return b;
        }

        private Button MakeToolBtn(string text, Color bg)
        {
            var b = new Button
            {
                Text = text,
                BackColor = bg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(110, 28),
                Margin = new Padding(0, 4, 6, 0),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(bg, 0.15f);
            return b;
        }

        // ── Card factory — NO DockStyle anywhere, pure absolute positions ────
        private Panel MakeCard(string title)
        {
            var p = new Panel { BackColor = Surface };
            // Blue top accent bar — absolute, 3px tall, full width, anchored L+R
            var accent = new Panel
            {
                Location = new Point(0, 0),
                Height = 3,
                BackColor = Blue,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            p.Controls.Add(accent);
            p.Resize += (s, e) => accent.Width = p.Width;   // keep it full width
            // Section title label
            p.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 10)
            });
            return p;
        }

        // ── Field group: label above control ──────────────────────────────────
        private void MakeFieldGroup(Panel parent, string labelText, int x, int y, out Panel group)
        {
            group = new Panel { Location = new Point(x, y), Size = new Size(160, 52), BackColor = Surface };
            group.Controls.Add(new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(0, 0)
            });
            parent.Controls.Add(group);
        }

        // ════════════════════════════════════════════════════════════════════
        //  BUILD GRID
        // ════════════════════════════════════════════════════════════════════
        private DataGridView BuildGrid()
        {
            var g = new DataGridView
            {
                BackgroundColor = Surface,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Border,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                EditMode = DataGridViewEditMode.EditOnEnter
            };

            g.ColumnHeadersDefaultCellStyle.BackColor = Surface2;
            g.ColumnHeadersDefaultCellStyle.ForeColor = TextMid;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Surface2;
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.ColumnHeadersHeight = 36;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.BackColor = Surface;
            g.DefaultCellStyle.ForeColor = TextMid;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            g.DefaultCellStyle.SelectionBackColor = BluePale;
            g.DefaultCellStyle.SelectionForeColor = TextDark;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.RowTemplate.Height = 36;

            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Sr", Name = "Sr", ReadOnly = true, FillWeight = 4 });
            var colDesc = new DataGridViewComboBoxColumn { HeaderText = "Description", Name = "DesignId", FlatStyle = FlatStyle.Flat, FillWeight = 18 };
            g.Columns.Add(colDesc);
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "HSN", Name = "HsnCode", FillWeight = 8 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "P.CH.NO", Name = "PChNo", FillWeight = 8 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CO.CH.NO", Name = "CoChNo", FillWeight = 8 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PCS", Name = "Pcs", FillWeight = 5 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "QTY", Name = "Qty", FillWeight = 8 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "RATE", Name = "Rate", FillWeight = 8 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PER", Name = "Per", FillWeight = 5 });

            var colAmt = new DataGridViewTextBoxColumn { HeaderText = "AMOUNT", Name = "Amount", ReadOnly = true, FillWeight = 12 };
            colAmt.DefaultCellStyle.ForeColor = Green;
            colAmt.DefaultCellStyle.Font = new Font("Consolas", 9.5F, FontStyle.Bold);
            colAmt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            g.Columns.Add(colAmt);

            g.CellValueChanged += Grid_CellValueChanged;
            g.CurrentCellDirtyStateChanged += (s, e) => {
                if (g.IsCurrentCellDirty && g.CurrentCell?.OwningColumn.Name == "DesignId")
                    g.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            g.DataError += (s, e) => e.Cancel = true;

            // Keyboard shortcut: Insert = add row, Delete = delete row
            g.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Insert) { AddGridRow(); e.Handled = true; }
                if (e.KeyCode == Keys.Delete && e.Shift) { DeleteGridRow(); e.Handled = true; }
            };

            // Alternating row shading
            g.RowsAdded += (s, e) => {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                    if (i >= 0 && i < g.Rows.Count)
                        g.Rows[i].DefaultCellStyle.BackColor =
                            i % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253);
            };

            return g;
        }

        // ════════════════════════════════════════════════════════════════════
        //  LOAD MASTER DATA
        // ════════════════════════════════════════════════════════════════════
        private void LoadMasterData()
        {
            int cid = SessionManager.CompanyProfileId;
            try
            {
                using var db = new AppDbContext();
                _designs = db.DesignMasters
                    .Where(d => d.CompanyProfileId == cid)
                    .OrderBy(d => d.DesignName).ToList();

                var colDesc = dgv.Columns["DesignId"] as DataGridViewComboBoxColumn;
                if (colDesc != null)
                {
                    colDesc.DataSource = _designs;
                    colDesc.DisplayMember = "DesignName";
                    colDesc.ValueMember = "DesignId";
                }

                var all = db.Accounts.Where(a => a.CompanyProfileId == cid)
                            .OrderBy(a => a.AccNm).ToList();
                _clients = all.Where(a => (a.Type ?? "").Equals("Customer", StringComparison.OrdinalIgnoreCase)).ToList();
                _brokers = all.Where(a => (a.Type ?? "").Equals("Broker", StringComparison.OrdinalIgnoreCase)).ToList();
                _transports = all.Where(a => (a.Type ?? "").Equals("Transport", StringComparison.OrdinalIgnoreCase)).ToList();
                // If no customers found yet show all accounts so the form is still usable
                if (_clients.Count == 0)
                    _clients = all.Where(a => !(a.Type ?? "").Equals("Broker", StringComparison.OrdinalIgnoreCase)
                                           && !(a.Type ?? "").Equals("Transport", StringComparison.OrdinalIgnoreCase)).ToList();

                BindCombo(cmbClient, _clients, "-- Select Client --");
                BindCombo(cmbBroker, _brokers, "-- None --");
                BindCombo(cmbTransport, _transports, "-- None --");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Master data error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindCombo(ComboBox c, List<Account> list, string placeholder)
        {
            c.Items.Clear();
            c.Items.Add(new CItem(0, placeholder));
            foreach (var a in list) c.Items.Add(new CItem(a.AccountId, a.AccNm));
            c.DisplayMember = "Display";
            c.ValueMember = "Id";
            c.SelectedIndex = 0;
        }

        private void SetNextInvoiceNo()
        {
            try
            {
                using var db = new AppDbContext();
                var t = db.InvoiceNumberTrackers.Find(SessionManager.CompanyProfileId);
                int nextNo = (t?.CurrentInvoiceNo ?? 0) + 1;
                lblInvoiceNo.Text = InvoiceNumberHelper.Format(nextNo);
            }
            catch { lblInvoiceNo.Text = "------"; }
        }

        private void CmbClient_Changed(object sender, EventArgs e)
        {
            var item = cmbClient.SelectedItem as CItem;
            if (item == null || item.Id == 0)
            { lblClientAddr.Text = lblClientGstin.Text = lblClientState.Text = ""; return; }
            var acc = _clients.FirstOrDefault(a => a.AccountId == item.Id);
            if (acc == null) return;
            var parts = new[] { acc.BillAdd1, acc.BillAdd2, acc.BillCity, acc.BillPincode }
                            .Where(s => !string.IsNullOrWhiteSpace(s));
            lblClientAddr.Text = string.Join(", ", parts);
            lblClientGstin.Text = "GSTIN: " + (acc.Gstin ?? "—");
            lblClientState.Text = "State: " + (acc.BillState ?? "—");
        }

        // ════════════════════════════════════════════════════════════════════
        //  GRID OPERATIONS
        // ════════════════════════════════════════════════════════════════════
        private void AddGridRow()
        {
            int i = dgv.Rows.Add();
            dgv.Rows[i].Cells["Per"].Value = "Mtrs";
            dgv.Rows[i].Cells["Pcs"].Value = "0";
            dgv.Rows[i].Cells["Qty"].Value = "0.000";
            dgv.Rows[i].Cells["Rate"].Value = "0.00";
            dgv.Rows[i].Cells["Amount"].Value = "0.00";
            RenumberRows();
            // Jump to Description cell
            dgv.CurrentCell = dgv.Rows[i].Cells["DesignId"];
        }

        private void DeleteGridRow()
        {
            if (dgv.SelectedRows.Count > 0)
            { dgv.Rows.Remove(dgv.SelectedRows[0]); RenumberRows(); RecalcTotals(); }
        }

        private void RenumberRows()
        {
            for (int i = 0; i < dgv.Rows.Count; i++)
                dgv.Rows[i].Cells["Sr"].Value = i + 1;
        }

        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _recalcLock) return;
            _recalcLock = true;
            try
            {
                var row = dgv.Rows[e.RowIndex];
                if (e.ColumnIndex == dgv.Columns["DesignId"].Index)
                {
                    var dv = row.Cells["DesignId"].Value;
                    if (dv != null && dv != DBNull.Value)
                    {
                        var d = _designs.FirstOrDefault(x => x.DesignId == Convert.ToInt32(dv));
                        if (d != null)
                        {
                            row.Cells["HsnCode"].Value = d.HsnCode ?? "";
                            row.Cells["Rate"].Value = (d.DefaultRate ?? 0).ToString("0.00");
                            row.Cells["Per"].Value = d.Unit ?? "Mtrs";
                        }
                    }
                }
                if (e.ColumnIndex == dgv.Columns["Qty"].Index ||
                    e.ColumnIndex == dgv.Columns["Rate"].Index ||
                    e.ColumnIndex == dgv.Columns["DesignId"].Index)
                {
                    decimal qty = Dec(row.Cells["Qty"].Value);
                    decimal rate = Dec(row.Cells["Rate"].Value);
                    row.Cells["Amount"].Value = (qty * rate).ToString("0.00");
                }
                RecalcTotals();
            }
            finally { _recalcLock = false; }
        }

        private void RecalcTotals()
        {
            decimal sub = 0;
            foreach (DataGridViewRow r in dgv.Rows) sub += Dec(r.Cells["Amount"].Value);

            decimal cgstP = Dec(txtCgstPct?.Text);
            decimal sgstP = Dec(txtSgstPct?.Text);
            decimal igstP = Dec(txtIgstPct?.Text);
            decimal rnd = Dec(txtRoundup?.Text);
            decimal cgst = Math.Round(sub * cgstP / 100, 2);
            decimal sgst = Math.Round(sub * sgstP / 100, 2);
            decimal igst = Math.Round(sub * igstP / 100, 2);
            decimal grand = sub + cgst + sgst + igst + rnd;

            if (lblSubTotal != null) lblSubTotal.Text = "₹ " + sub.ToString("N2");
            if (lblCgstAmt != null) lblCgstAmt.Text = "₹ " + cgst.ToString("N2");
            if (lblSgstAmt != null) lblSgstAmt.Text = "₹ " + sgst.ToString("N2");
            if (lblIgstAmt != null) lblIgstAmt.Text = "₹ " + igst.ToString("N2");
            if (lblGrandTotal != null) lblGrandTotal.Text = "₹ " + grand.ToString("N2");
            if (lblAmtWords != null) lblAmtWords.Text = ToWords(grand) + " ONLY";
        }

        // ════════════════════════════════════════════════════════════════════
        //  SAVE / UPDATE
        // ════════════════════════════════════════════════════════════════════
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_isEditMode) UpdateInvoice();
            else SaveNewInvoice();
        }

        private void SaveNewInvoice()
        {
            if (!ValidateForm()) return;
            var (sub, cgstP, sgstP, igstP, rnd, cgst, sgst, igst, grand) = CalcTotals();
            int cid = SessionManager.CompanyProfileId;
            var ci = cmbClient.SelectedItem as CItem;

            try
            {
                using var db = new AppDbContext();
                using var tx = db.Database.BeginTransaction();

                var tracker = db.InvoiceNumberTrackers.Find(cid);
                int nextNo;
                if (tracker == null)
                { tracker = new InvoiceNumberTracker { CompanyProfileId = cid, CurrentInvoiceNo = 1 }; db.InvoiceNumberTrackers.Add(tracker); nextNo = 1; }
                else
                { nextNo = (tracker.CurrentInvoiceNo ?? 0) + 1; tracker.CurrentInvoiceNo = nextNo; }

                var hdr = BuildHeader(cid, InvoiceNumberHelper.Format(nextNo), sub, cgstP, sgstP, igstP, cgst, sgst, igst, rnd, grand);
                db.InvoiceHeaders.Add(hdr);
                db.SaveChanges();

                SaveItems(db, hdr.InvoiceId, cid);
                db.SaveChanges();
                tx.Commit();

                _lastSavedInvoiceId = hdr.InvoiceId;
                _btnPrint.Enabled = _btnPdf.Enabled = true;

                // ── Post-save action dialog ──────────────────────────
                var dlg = new Form
                {
                    Text = "Invoice Saved",
                    Size = new Size(360, 180),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = Color.FromArgb(245, 247, 250)
                };
                dlg.Controls.Add(new Label
                {
                    Text = $"✓  Invoice {hdr.InvoiceNo} saved!\nGrand Total: ₹{grand:N2}",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42),
                    Location = new Point(20, 16),
                    AutoSize = true
                });
                var bPrint2 = new Button
                {
                    Text = "🖨  Print",
                    Size = new Size(90, 34),
                    Location = new Point(20, 60),
                    BackColor = Color.FromArgb(71, 85, 105),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                bPrint2.FlatAppearance.BorderSize = 0;
                bPrint2.Click += (s2, e2) => { InvoicePrintService.PrintInvoice(_lastSavedInvoiceId, this.FindForm()); };
                var bPdf2 = new Button
                {
                    Text = "📄  PDF",
                    Size = new Size(90, 34),
                    Location = new Point(118, 60),
                    BackColor = Color.FromArgb(124, 58, 237),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                bPdf2.FlatAppearance.BorderSize = 0;
                bPdf2.Click += (s2, e2) => { InvoicePrintService.SavePdf(_lastSavedInvoiceId, this.FindForm()); };
                var bNew2 = new Button
                {
                    Text = "➕  New Invoice",
                    Size = new Size(110, 34),
                    Location = new Point(216, 60),
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false
                };
                bNew2.FlatAppearance.BorderSize = 0;
                bNew2.Click += (s2, e2) => { dlg.Close(); ClearForm(); SetNextInvoiceNo(); };
                dlg.Controls.Add(bPrint2); dlg.Controls.Add(bPdf2); dlg.Controls.Add(bNew2);
                dlg.ShowDialog(this.FindForm());
            }
            catch (Exception ex)
            { MessageBox.Show("Save error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void UpdateInvoice()
        {
            if (!ValidateForm()) return;
            if (MessageBox.Show("Update this invoice? Existing data will be overwritten.",
                    "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var (sub, cgstP, sgstP, igstP, rnd, cgst, sgst, igst, grand) = CalcTotals();
            int cid = SessionManager.CompanyProfileId;
            var ci = cmbClient.SelectedItem as CItem;

            try
            {
                using var db = new AppDbContext();
                using var tx = db.Database.BeginTransaction();

                var hdr = db.InvoiceHeaders.Find(_editInvoiceId);
                if (hdr == null) { MessageBox.Show("Invoice not found.", "Error"); return; }

                hdr.InvoiceDate = DateOnly.FromDateTime(dtpInvoiceDate.Value);
                hdr.ClientId = ci!.Id;
                hdr.BrokerId = (cmbBroker.SelectedItem as CItem) is CItem bi && bi.Id > 0 ? bi.Id : (int?)null;
                hdr.TransportId = (cmbTransport.SelectedItem as CItem) is CItem ti && ti.Id > 0 ? ti.Id : (int?)null;
                hdr.ChallanNo = string.IsNullOrWhiteSpace(txtChallanNo.Text) ? null : txtChallanNo.Text.Trim();
                hdr.ChallanDate = dtpChallanDate.Checked ? DateOnly.FromDateTime(dtpChallanDate.Value) : null;
                hdr.TotalAmount = sub; hdr.CgstPct = cgstP; hdr.SgstPct = sgstP; hdr.IgstPct = igstP;
                hdr.Cgst = cgst; hdr.Sgst = sgst; hdr.Igst = igst; hdr.Roundup = rnd; hdr.GrandTotal = grand;

                var old = db.InvoiceItems.Where(i => i.InvoiceId == _editInvoiceId).ToList();
                db.InvoiceItems.RemoveRange(old);
                db.SaveChanges();

                SaveItems(db, _editInvoiceId, cid);
                db.SaveChanges();
                tx.Commit();

                _lastSavedInvoiceId = _editInvoiceId;
                _btnPrint.Enabled = _btnPdf.Enabled = true;
                MessageBox.Show($"Invoice {hdr.InvoiceNo} updated!\nGrand Total: ₹{grand:N2}",
                    "Updated ✓", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            { MessageBox.Show("Update error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private bool ValidateForm()
        {
            var ci = cmbClient.SelectedItem as CItem;
            if (ci == null || ci.Id == 0)
            { MessageBox.Show("Please select a client.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); cmbClient.Focus(); return false; }
            if (dgv.Rows.Count == 0)
            { MessageBox.Show("Please add at least one item.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }

            // ── Duplicate Challan guard ──────────────────────────────
            string challan = txtChallanNo.Text.Trim();
            if (!string.IsNullOrWhiteSpace(challan))
            {
                try
                {
                    using var db = new AppDbContext();
                    var dup = db.InvoiceHeaders.FirstOrDefault(h =>
                        h.CompanyProfileId == SessionManager.CompanyProfileId &&
                        h.ChallanNo == challan &&
                        (_isEditMode ? h.InvoiceId != _editInvoiceId : true));
                    if (dup != null)
                    {
                        var res = MessageBox.Show(
                            $"Challan No \"{challan}\" already exists on Invoice {dup.InvoiceNo}.\n\n" +
                            "Do you want to continue saving anyway?",
                            "Duplicate Challan Warning",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button2);
                        if (res != DialogResult.Yes) { txtChallanNo.Focus(); return false; }
                    }
                }
                catch { /* non-critical, proceed */ }
            }

            return true;
        }

        private InvoiceHeader BuildHeader(int cid, string invNo,
            decimal sub, decimal cgstP, decimal sgstP, decimal igstP,
            decimal cgst, decimal sgst, decimal igst, decimal rnd, decimal grand)
        {
            var ci = cmbClient.SelectedItem as CItem;
            var bi = cmbBroker.SelectedItem as CItem;
            var ti = cmbTransport.SelectedItem as CItem;
            return new InvoiceHeader
            {
                CompanyProfileId = cid,
                InvoiceNo = invNo,
                InvoiceDate = DateOnly.FromDateTime(dtpInvoiceDate.Value),
                ClientId = ci!.Id,
                BrokerId = bi != null && bi.Id > 0 ? bi.Id : (int?)null,
                TransportId = ti != null && ti.Id > 0 ? ti.Id : (int?)null,
                ChallanNo = string.IsNullOrWhiteSpace(txtChallanNo.Text) ? null : txtChallanNo.Text.Trim(),
                ChallanDate = dtpChallanDate.Checked ? DateOnly.FromDateTime(dtpChallanDate.Value) : null,
                TotalAmount = sub,
                CgstPct = cgstP,
                SgstPct = sgstP,
                IgstPct = igstP,
                Cgst = cgst,
                Sgst = sgst,
                Igst = igst,
                Roundup = rnd,
                GrandTotal = grand
            };
        }

        private void SaveItems(AppDbContext db, int invoiceId, int cid)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                var dv = row.Cells["DesignId"].Value;
                db.InvoiceItems.Add(new InvoiceItem
                {
                    InvoiceId = invoiceId,
                    CompanyProfileId = cid,
                    DesignId = (dv != null && dv != DBNull.Value) ? (int?)Convert.ToInt32(dv) : null,
                    HsnCode = row.Cells["HsnCode"].Value?.ToString(),
                    PChNo = row.Cells["PChNo"].Value?.ToString(),
                    CoChNo = row.Cells["CoChNo"].Value?.ToString(),
                    Pcs = Int(row.Cells["Pcs"].Value),
                    Qty = Dec(row.Cells["Qty"].Value),
                    Rate = Dec(row.Cells["Rate"].Value),
                    Per = row.Cells["Per"].Value?.ToString() ?? "Mtrs",
                    Amount = Dec(row.Cells["Amount"].Value)
                });
            }
        }

        private (decimal sub, decimal cgstP, decimal sgstP, decimal igstP, decimal rnd,
                 decimal cgst, decimal sgst, decimal igst, decimal grand) CalcTotals()
        {
            decimal sub = 0;
            foreach (DataGridViewRow r in dgv.Rows) sub += Dec(r.Cells["Amount"].Value);
            decimal cgstP = Dec(txtCgstPct.Text), sgstP = Dec(txtSgstPct.Text),
                    igstP = Dec(txtIgstPct.Text), rnd = Dec(txtRoundup.Text);
            decimal cgst = Math.Round(sub * cgstP / 100, 2);
            decimal sgst = Math.Round(sub * sgstP / 100, 2);
            decimal igst = Math.Round(sub * igstP / 100, 2);
            decimal grand = sub + cgst + sgst + igst + rnd;
            return (sub, cgstP, sgstP, igstP, rnd, cgst, sgst, igst, grand);
        }

        private void PrintLastInvoice()
        {
            if (_lastSavedInvoiceId < 0) { MessageBox.Show("No invoice saved yet.", "Info"); return; }
            InvoicePrintService.PrintInvoice(_lastSavedInvoiceId, this.FindForm());
        }
        private void PdfLastInvoice()
        {
            if (_lastSavedInvoiceId < 0) { MessageBox.Show("No invoice saved yet.", "Info"); return; }
            InvoicePrintService.SavePdf(_lastSavedInvoiceId, this.FindForm());
        }

        private void ClearForm()
        {
            dgv.Rows.Clear();
            cmbClient.SelectedIndex = cmbBroker.SelectedIndex = cmbTransport.SelectedIndex = 0;
            dtpInvoiceDate.Value = DateTime.Today;
            txtChallanNo.Text = "";
            dtpChallanDate.Checked = false;
            txtCgstPct.Text = "2.5"; txtSgstPct.Text = "2.5";
            txtIgstPct.Text = "0.0"; txtRoundup.Text = "0.00";
            lblClientAddr.Text = lblClientGstin.Text = lblClientState.Text = "";
            _editInvoiceId = -1;
            _lastSavedInvoiceId = -1;
            _btnPrint.Enabled = _btnPdf.Enabled = false;
            RecalcTotals();
        }

        // ── Edit mode loader ──────────────────────────────────────────────────
        private void LoadInvoiceForEdit(int invoiceId)
        {
            try
            {
                using var db = new AppDbContext();
                var hdr = db.InvoiceHeaders.Find(invoiceId);
                if (hdr == null) { MessageBox.Show("Invoice not found.", "Error"); return; }

                lblInvoiceNo.Text = hdr.InvoiceNo ?? "—";
                dtpInvoiceDate.Value = hdr.InvoiceDate.ToDateTime(TimeOnly.MinValue);
                if (hdr.ChallanNo != null) txtChallanNo.Text = hdr.ChallanNo;
                if (hdr.ChallanDate.HasValue)
                { dtpChallanDate.Checked = true; dtpChallanDate.Value = hdr.ChallanDate.Value.ToDateTime(TimeOnly.MinValue); }

                SelectCombo(cmbClient, hdr.ClientId);
                CmbClient_Changed(cmbClient, EventArgs.Empty);
                if (hdr.TransportId.HasValue) SelectCombo(cmbTransport, hdr.TransportId.Value);
                if (hdr.BrokerId.HasValue) SelectCombo(cmbBroker, hdr.BrokerId.Value);

                txtCgstPct.Text = (hdr.CgstPct ?? 0).ToString("0.0#");
                txtSgstPct.Text = (hdr.SgstPct ?? 0).ToString("0.0#");
                txtIgstPct.Text = (hdr.IgstPct ?? 0).ToString("0.0#");
                txtRoundup.Text = (hdr.Roundup ?? 0).ToString("0.00");

                dgv.Rows.Clear();
                var items = db.InvoiceItems.Where(i => i.InvoiceId == invoiceId)
                              .OrderBy(i => i.ItemId).ToList();
                foreach (var it in items)
                {
                    int idx = dgv.Rows.Add();
                    var row = dgv.Rows[idx];
                    row.Cells["Sr"].Value = idx + 1;
                    row.Cells["DesignId"].Value = it.DesignId;
                    row.Cells["HsnCode"].Value = it.HsnCode ?? "";
                    row.Cells["PChNo"].Value = it.PChNo ?? "";
                    row.Cells["CoChNo"].Value = it.CoChNo ?? "";
                    row.Cells["Pcs"].Value = it.Pcs.ToString();
                    row.Cells["Qty"].Value = (it.Qty ?? 0).ToString("0.000");
                    row.Cells["Rate"].Value = (it.Rate ?? 0).ToString("0.00");
                    row.Cells["Per"].Value = it.Per ?? "Mtrs";
                    row.Cells["Amount"].Value = (it.Amount ?? 0).ToString("0.00");
                }

                RecalcTotals();
                _lastSavedInvoiceId = invoiceId;
                _btnPrint.Enabled = _btnPdf.Enabled = true;
                _btnSave.Text = "✏️  Update Invoice";
            }
            catch (Exception ex)
            { MessageBox.Show("Load error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void SelectCombo(ComboBox cmb, int accountId)
        {
            foreach (var item in cmb.Items)
                if (item is CItem ci && ci.Id == accountId) { cmb.SelectedItem = item; return; }
        }

        // ════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════════════════
        private ComboBox MakeCmb(int x, int y, int w)
            => new ComboBox
            {
                Font = new Font("Segoe UI", 9.5F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = TextDark,
                Size = new Size(w, 28),
                Location = new Point(x, y)
            };

        private decimal Dec(object v) { if (v == null || v == DBNull.Value) return 0; return decimal.TryParse(v.ToString(), out decimal d) ? d : 0; }
        private decimal Dec(string s) => decimal.TryParse(s, out decimal d) ? d : 0;
        private int Int(object v) { if (v == null || v == DBNull.Value) return 0; return int.TryParse(v.ToString(), out int i) ? i : 0; }

        // ── Amount to words ───────────────────────────────────────────────────
        private string ToWords(decimal amount)
        {
            long r = (long)Math.Floor(amount);
            int p = (int)Math.Round((amount - r) * 100);
            string w = Words(r);
            if (p > 0) w += " AND PAISE " + Words(p);
            return w.ToUpper();
        }
        private string Words(long n)
        {
            if (n == 0) return "ZERO";
            string[] o = { "", "ONE","TWO","THREE","FOUR","FIVE","SIX","SEVEN","EIGHT","NINE","TEN",
                           "ELEVEN","TWELVE","THIRTEEN","FOURTEEN","FIFTEEN","SIXTEEN",
                           "SEVENTEEN","EIGHTEEN","NINETEEN" };
            string[] t = { "", "", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY" };
            string w = "";
            if (n >= 10000000) { w += Words(n / 10000000) + " CRORE "; n %= 10000000; }
            if (n >= 100000) { w += Words(n / 100000) + " LAKH "; n %= 100000; }
            if (n >= 1000) { w += Words(n / 1000) + " THOUSAND "; n %= 1000; }
            if (n >= 100) { w += o[n / 100] + " HUNDRED "; n %= 100; }
            if (n >= 20) { w += t[n / 10] + " "; n %= 10; }
            if (n > 0) { w += o[n] + " "; }
            return w.Trim();
        }

        private class CItem
        {
            public int Id { get; }
            public string Display { get; }
            public CItem(int id, string display) { Id = id; Display = display; }
            public override string ToString() => Display;
        }
    }
}