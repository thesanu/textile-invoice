using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App
{
    public partial class UC_WorkOrder : UserControl
    {
        // ── Palette ───────────────────────────────────────────────────
        static readonly Color Bg = Color.FromArgb(245, 247, 250);
        static readonly Color Surface = Color.White;
        static readonly Color Surface2 = Color.FromArgb(248, 249, 252);
        static readonly Color HeaderBg = Color.FromArgb(241, 245, 249);
        static readonly Color Blue = Color.FromArgb(37, 99, 235);
        static readonly Color BlueHov = Color.FromArgb(29, 78, 216);
        static readonly Color BluePale = Color.FromArgb(239, 246, 255);
        static readonly Color Green = Color.FromArgb(22, 163, 74);
        static readonly Color GreenHov = Color.FromArgb(15, 118, 54);
        static readonly Color Red = Color.FromArgb(220, 38, 38);
        static readonly Color RedPale = Color.FromArgb(254, 242, 242);
        static readonly Color RedBdr = Color.FromArgb(254, 202, 202);
        static readonly Color Amber = Color.FromArgb(180, 83, 9);
        static readonly Color AmberPale = Color.FromArgb(255, 251, 235);
        static readonly Color AmberBdr = Color.FromArgb(253, 230, 138);
        static readonly Color Teal = Color.FromArgb(13, 148, 136);
        static readonly Color TealHov = Color.FromArgb(15, 118, 110);
        static readonly Color Violet = Color.FromArgb(109, 40, 217);
        static readonly Color VioletHov = Color.FromArgb(88, 28, 135);
        static readonly Color Border = Color.FromArgb(226, 232, 240);
        static readonly Color RowLine = Color.FromArgb(241, 245, 249);
        static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        static readonly Color TextMid = Color.FromArgb(71, 85, 105);
        static readonly Color TextLight = Color.FromArgb(148, 163, 184);

        // ── Tab panels ────────────────────────────────────────────────
        Panel pnlTabBar, pnlWorkOrders, pnlAlias, pnlReport;
        Button _btnTabWO, _btnTabAlias, _btnTabReport;

        // ── Work Order tab ────────────────────────────────────────────
        Panel pnlWoGrid, pnlWoForm;
        DataGridView dgvWo;
        TextBox txtWoSearch;
        Label lblWoFormTitle;
        DateTimePicker dtpWoDate;
        TextBox txtWoNo, txtWoChallanNo, txtWoRemarks;
        ComboBox cmbWoClient;
        DateTimePicker dtpWoChallanDate;
        CheckBox chkWoChallanDate;
        DataGridView dgvWoItems;
        int _woEditId = -1;
        const int WO_PAGE = 20;
        int _woPage = 1, _woTotalPages = 1;
        Label _lblWoPageInfo;
        Button _btnWoPrev, _btnWoNext;
        List<WorkOrderHeader> _woAllRows = new();

        // ── Alias tab ─────────────────────────────────────────────────
        Panel pnlAliasGrid, pnlAliasForm;
        DataGridView dgvAlias;
        TextBox txtAliasSearch;
        Label lblAliasFormTitle;
        TextBox txtPartyDesignName, txtAliasNotes;
        ComboBox cmbAliasClient, cmbAliasOurDesign;
        int _aliasEditId = -1;
        const int ALIAS_PAGE = 20;
        int _aliasPage = 1, _aliasTotalPages = 1;
        Label _lblAliasPageInfo;
        Button _btnAliasPrev, _btnAliasNext;
        List<PartyDesignAlias> _aliasAllRows = new();

        // ── Report tab ────────────────────────────────────────────────
        DateTimePicker dtpReportDate;
        DataGridView dgvReport;
        Label lblReportSummary;

        // ── Master data ───────────────────────────────────────────────
        List<Account> _accounts = new();
        List<DesignMaster> _designs = new();
        // Aliases for current selected client (used to populate PartyDesign dropdown)
        List<PartyDesignAlias> _clientAliases = new();
        bool _suppressGridEvents = false;

        // ═════════════════════════════════════════════════════════════
        public UC_WorkOrder()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Bg;
            LoadMasterData();
            // Fill panels FIRST, then Top panels
            BuildWorkOrdersTab();
            BuildAliasTab();
            BuildReportTab();
            BuildTabBar();
            ShowTab(pnlWorkOrders, _btnTabWO);
        }

        // ════════════════════════════════════════════════════════════
        //  MASTER DATA
        // ════════════════════════════════════════════════════════════
        void LoadMasterData()
        {
            try
            {
                using var db = new AppDbContext();
                int cid = SessionManager.CompanyProfileId;
                _accounts = db.Accounts
                    .Where(a => a.CompanyProfileId == cid)
                    .OrderBy(a => a.AccNm).ToList();
                _designs = db.DesignMasters
                    .Where(d => d.CompanyProfileId == cid)
                    .OrderBy(d => d.DesignName).ToList();
            }
            catch (Exception ex) { MessageBox.Show("Master data error: " + ex.Message); }
        }

        // Load aliases for the selected client and rebuild PartyDesign column
        void LoadClientAliases()
        {
            _clientAliases.Clear();
            var ci = cmbWoClient?.SelectedItem as AItem;
            if (ci == null || ci.Id == 0) return;
            try
            {
                using var db = new AppDbContext();
                _clientAliases = db.PartyDesignAliases
                    .Where(a => a.CompanyProfileId == SessionManager.CompanyProfileId
                                && a.AccountId == ci.Id)
                    .ToList();
            }
            catch { }

            // Rebuild the PartyDesign combo column with this client's alias names
            RebuildPartyDesignColumn();
        }

        // Rebuild PartyDesign combobox column items from _clientAliases
        void RebuildPartyDesignColumn()
        {
            if (dgvWoItems == null) return;
            var col = dgvWoItems.Columns["PartyDesign"] as DataGridViewComboBoxColumn;
            if (col == null) return;

            col.Items.Clear();
            col.Items.Add(""); // blank option so user can leave it empty
            foreach (var a in _clientAliases)
                col.Items.Add(a.PartyDesignName);

            // Also allow free typing: set to allow user-typed values not in list
            col.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
        }

        // ════════════════════════════════════════════════════════════
        //  TAB BAR
        // ════════════════════════════════════════════════════════════
        void BuildTabBar()
        {
            pnlTabBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Surface };
            pnlTabBar.Paint += (s, e) =>
            {
                using var pen = new Pen(Border);
                e.Graphics.DrawLine(pen, 0, pnlTabBar.Height - 1, pnlTabBar.Width, pnlTabBar.Height - 1);
                if (pnlTabBar.Tag is Button active)
                {
                    using var br = new SolidBrush(Blue);
                    e.Graphics.FillRectangle(br, active.Left, pnlTabBar.Height - 3, active.Width, 3);
                }
            };

            _btnTabWO = MkTab("📋  Work Orders");
            _btnTabAlias = MkTab("🔗  Design Alias");
            _btnTabReport = MkTab("📊  Daily Report");

            _btnTabWO.Location = new Point(0, 0);
            _btnTabAlias.Location = new Point(140, 0);
            _btnTabReport.Location = new Point(280, 0);

            _btnTabWO.Click += (s, e) => ShowTab(pnlWorkOrders, _btnTabWO);
            _btnTabAlias.Click += (s, e) => ShowTab(pnlAlias, _btnTabAlias);
            _btnTabReport.Click += (s, e) =>
            {
                ShowTab(pnlReport, _btnTabReport);
                LoadReport(dtpReportDate?.Value.Date ?? DateTime.Today);
            };

            pnlTabBar.Controls.Add(_btnTabWO);
            pnlTabBar.Controls.Add(_btnTabAlias);
            pnlTabBar.Controls.Add(_btnTabReport);
            this.Controls.Add(pnlTabBar);

            // Page header — added AFTER tab bar so it renders above it
            var top = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            top.Controls.Add(new Label
            {
                Text = "Work Order",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            });
            top.Controls.Add(new Label
            {
                Text = "Create daily work orders, manage design aliases and view reports",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });
            this.Controls.Add(top);
        }

        Button MkTab(string text)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(138, 44),
                Cursor = Cursors.Hand,
                BackColor = Surface,
                ForeColor = TextMid,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(241, 245, 249);
            return b;
        }

        void ShowTab(Panel target, Button activeBtn)
        {
            foreach (var p in new[] { pnlWorkOrders, pnlAlias, pnlReport })
                if (p != null) p.Visible = false;
            foreach (var b in new[] { _btnTabWO, _btnTabAlias, _btnTabReport })
            {
                b.ForeColor = TextMid;
                b.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }
            if (target != null) target.Visible = true;
            activeBtn.ForeColor = Blue;
            pnlTabBar.Tag = activeBtn;
            pnlTabBar.Invalidate();
        }

        // ════════════════════════════════════════════════════════════
        //  TAB 1 — WORK ORDERS
        // ════════════════════════════════════════════════════════════
        void BuildWorkOrdersTab()
        {
            pnlWorkOrders = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Visible = false };

            pnlWoGrid = new Panel { Dock = DockStyle.Fill, BackColor = Bg };
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });

            // Toolbar
            var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Surface2 };
            bar.Paint += (s, e) => { using var p = new Pen(Border); e.Graphics.DrawLine(p, 0, bar.Height - 1, bar.Width, bar.Height - 1); };

            var srch = new Panel { Location = new Point(16, 10), Size = new Size(260, 32), BackColor = Color.White };
            srch.Paint += (s, e) =>
            {
                using var bp = new Pen(Border);
                using var br = new SolidBrush(TextLight);
                using var ef = new Font("Segoe UI Emoji", 9F);
                e.Graphics.DrawRectangle(bp, 0, 0, srch.Width - 1, srch.Height - 1);
                e.Graphics.DrawString("🔍", ef, br, new PointF(5, 7));
            };
            txtWoSearch = new TextBox
            {
                PlaceholderText = "Search by WO No, client, date…",
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = TextDark,
                Location = new Point(27, 7),
                Size = new Size(225, 20)
            };
            txtWoSearch.TextChanged += (s, e) => { _woPage = 1; LoadWoGrid(txtWoSearch.Text); };
            srch.Controls.Add(txtWoSearch);
            bar.Controls.Add(srch);

            var btnAdd = MkBtn("➕  New Work Order", Blue, Color.White, BlueHov);
            var btnExport = MkBtn("⬇  Export Excel", Green, Color.White, GreenHov);
            var btnImport = MkBtn("⬆  Import CSV", Violet, Color.White, VioletHov);
            var btnSample = MkBtn("📥 Sample CSV", Teal, Color.White, TealHov);
            btnAdd.Size = new Size(150, 30); btnExport.Size = new Size(130, 30);
            btnImport.Size = new Size(118, 30); btnSample.Size = new Size(126, 30);

            btnAdd.Click += (s, e) => ShowWoForm(null);
            btnExport.Click += WoExportExcel;
            btnImport.Click += WoImportCsv;
            btnSample.Click += WoDownloadSample;

            bar.Controls.Add(btnAdd); bar.Controls.Add(btnExport);
            bar.Controls.Add(btnImport); bar.Controls.Add(btnSample);
            bar.Resize += (s, e) =>
            {
                int r = bar.Width - 12;
                btnExport.Location = new Point(r - btnExport.Width, 11);
                btnImport.Location = new Point(r - btnExport.Width - btnImport.Width - 6, 11);
                btnSample.Location = new Point(r - btnExport.Width - btnImport.Width - btnSample.Width - 12, 11);
                btnAdd.Location = new Point(r - btnExport.Width - btnImport.Width - btnSample.Width - btnAdd.Width - 18, 11);
            };

            // Pager
            var pager = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Surface2 };
            pager.Paint += (s, e) => { using var p = new Pen(Border); e.Graphics.DrawLine(p, 0, 0, pager.Width, 0); };
            _btnWoPrev = MkBtn("◀  Prev", Surface, TextMid, Color.FromArgb(241, 245, 249), true);
            _btnWoNext = MkBtn("Next  ▶", Surface, TextMid, Color.FromArgb(241, 245, 249), true);
            _btnWoPrev.Size = new Size(82, 30); _btnWoPrev.Location = new Point(12, 7);
            _btnWoNext.Size = new Size(82, 30);
            _lblWoPageInfo = new Label { Text = "Page 1 of 1  (0 records)", ForeColor = TextMid, Font = new Font("Segoe UI", 9F), AutoSize = true };
            _btnWoPrev.Click += (s, e) => { if (_woPage > 1) { _woPage--; RenderWoPage(); } };
            _btnWoNext.Click += (s, e) => { if (_woPage < _woTotalPages) { _woPage++; RenderWoPage(); } };
            pager.Controls.Add(_btnWoPrev); pager.Controls.Add(_lblWoPageInfo); pager.Controls.Add(_btnWoNext);
            pager.Resize += (s, e) =>
            {
                _lblWoPageInfo.Location = new Point((pager.Width - _lblWoPageInfo.Width) / 2, (pager.Height - _lblWoPageInfo.Height) / 2);
                _btnWoNext.Location = new Point(pager.Width - _btnWoNext.Width - 12, 7);
            };

            // Grid
            dgvWo = BuildDgv();
            dgvWo.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
            dgvWo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "WO NO", Name = "WoNo", MinimumWidth = 110, FillWeight = 12 });
            dgvWo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DATE", Name = "WoDate", MinimumWidth = 100, FillWeight = 10 });
            dgvWo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CLIENT", Name = "Client", MinimumWidth = 180, FillWeight = 24 });
            dgvWo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CHALLAN NO", Name = "Challan", MinimumWidth = 110, FillWeight = 12 });
            dgvWo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ITEMS", Name = "Items", MinimumWidth = 60, FillWeight = 6 });
            dgvWo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "STATUS", Name = "Status", MinimumWidth = 90, FillWeight = 10 });
            dgvWo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Print", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 72 });
            dgvWo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Edit", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 68 });
            dgvWo.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Delete", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 74 });

            dgvWo.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (e.ColumnIndex == dgvWo.Columns["Status"]?.Index && e.Value != null)
                {
                    string st = e.Value.ToString() ?? "";
                    e.CellStyle.ForeColor = st == "Done" ? Green : st == "InProgress" ? Amber : TextMid;
                    e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    e.FormattingApplied = true;
                }
                foreach (var n in new[] { "Print", "Edit", "Delete" })
                    if (e.ColumnIndex == dgvWo.Columns[n]?.Index)
                    { e.Value = ""; e.FormattingApplied = true; }
            };

            AttachActionPainter(dgvWo,
                ("Print", BluePale, Blue, Color.FromArgb(191, 219, 254), "🖨 Print"),
                ("Edit", AmberPale, Amber, AmberBdr, "✏  Edit"),
                ("Delete", RedPale, Red, RedBdr, "🗑 Delete"));
            dgvWo.CellClick += DgvWoClick;

            card.Controls.Add(dgvWo);
            card.Controls.Add(pager);
            card.Controls.Add(bar);
            pnlWoGrid.Controls.Add(card);

            pnlWoForm = BuildWoForm();
            pnlWorkOrders.Controls.Add(pnlWoForm);
            pnlWorkOrders.Controls.Add(pnlWoGrid);
            this.Controls.Add(pnlWorkOrders);

            LoadWoGrid("");
        }

        // ════════════════════════════════════════════════════════════
        //  WORK ORDER FORM
        // ════════════════════════════════════════════════════════════
        Panel BuildWoForm()
        {
            var frm = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Visible = false, AutoScroll = true };

            var top = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Bg };
            lblWoFormTitle = new Label
            {
                Text = "New Work Order",
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            };
            top.Controls.Add(lblWoFormTitle);

            var card = new Panel
            {
                BackColor = Surface,
                Location = new Point(24, 64),
                Size = new Size(900, 600),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });

            var sec = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Surface2 };
            sec.Paint += (s, e) => { using var p = new Pen(Border); e.Graphics.DrawLine(p, 0, sec.Height - 1, sec.Width, sec.Height - 1); };
            sec.Controls.Add(new Label { Text = "Work Order Details", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = TextDark, AutoSize = true, Location = new Point(16, 10) });
            card.Controls.Add(sec);

            int lx = 20, fx = 160, y = 52, gap = 48;

            // Row 1: WO No | Date
            txtWoNo = FF(card, "WO Number", lx, fx, 160, y);
            card.Controls.Add(FLbl("Date", new Point(lx + 260, y + 5)));
            dtpWoDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9.5F),
                Size = new Size(150, 28),
                Location = new Point(lx + 320, y),
                Value = DateTime.Today
            };
            card.Controls.Add(dtpWoDate);
            y += gap;

            // Row 2: Client — on change, reload alias list & rebuild PartyDesign column
            card.Controls.Add(FLbl("Client / Party", new Point(lx, y + 5)));
            cmbWoClient = MkCmb(fx, y, 300);
            BindAccountCombo(cmbWoClient, "-- Select Client --");
            cmbWoClient.SelectedIndexChanged += (s, e) =>
            {
                if (_suppressGridEvents) return;
                LoadClientAliases();           // reload aliases + rebuild column
                dgvWoItems.Rows.Clear();       // clear items when client changes
            };
            card.Controls.Add(cmbWoClient);
            y += gap;

            // Row 3: Challan No | Challan Date
            txtWoChallanNo = FF(card, "Challan No", lx, fx, 180, y);
            chkWoChallanDate = new CheckBox
            {
                Text = "Challan Date:",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = TextMid,
                Location = new Point(lx + 268, y + 5),
                AutoSize = true
            };
            dtpWoChallanDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9.5F),
                Size = new Size(150, 28),
                Location = new Point(lx + 380, y),
                Value = DateTime.Today,
                Enabled = false
            };
            chkWoChallanDate.CheckedChanged += (s, e) => dtpWoChallanDate.Enabled = chkWoChallanDate.Checked;
            card.Controls.Add(chkWoChallanDate); card.Controls.Add(dtpWoChallanDate);
            y += gap;

            // Row 4: Remarks
            txtWoRemarks = FF(card, "Remarks", lx, fx, 500, y); y += gap;

            // Items section
            card.Controls.Add(new Label
            {
                Text = "Work Order Items",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(lx, y)
            });
            card.Controls.Add(new Label
            {
                Text = "💡 Select client first → pick Client Design → Our Design auto-fills. Or choose Our Design manually.",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(lx + 150, y + 6)
            });
            y += 28;

            // Items toolbar
            var btnAddRow = MkBtn("＋ Add Row", Blue, Color.White, BlueHov);
            var btnDelRow = MkBtn("✕ Del Row", Red, Color.White, Color.FromArgb(185, 28, 28));
            btnAddRow.Size = new Size(100, 28); btnAddRow.Location = new Point(lx, y);
            btnDelRow.Size = new Size(96, 28); btnDelRow.Location = new Point(lx + 106, y);
            btnAddRow.Click += (s, e) => AddWoItemRow();
            btnDelRow.Click += (s, e) => DelWoItemRow();
            card.Controls.Add(btnAddRow); card.Controls.Add(btnDelRow);
            y += 36;

            // Items grid
            dgvWoItems = BuildWoItemsGrid();
            dgvWoItems.Location = new Point(lx, y);
            dgvWoItems.Height = 220;
            dgvWoItems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            card.Controls.Add(dgvWoItems);
            y += 228;

            // Save / Cancel
            var btnSave = MkBtn("💾  Save Work Order", Blue, Color.White, BlueHov);
            var btnCancel = MkBtn("✕  Cancel", Surface, TextMid, Color.FromArgb(241, 245, 249), true);
            btnSave.Size = new Size(160, 36); btnSave.Location = new Point(fx, y);
            btnCancel.Size = new Size(100, 36); btnCancel.Location = new Point(fx + 168, y);
            btnSave.Click += WoSave;
            btnCancel.Click += (s, e) => ShowWoGrid();
            card.Controls.Add(btnSave); card.Controls.Add(btnCancel);
            card.Height = y + 56;

            frm.Resize += (s, e) => card.Width = Math.Max(640, frm.Width - 48);
            card.Resize += (s, e) => dgvWoItems.Width = card.Width - lx * 2;

            frm.Controls.Add(card);
            frm.Controls.Add(top);
            return frm;
        }

        // ════════════════════════════════════════════════════════════
        //  ITEMS GRID
        //  PartyDesign  = ComboBox (client's alias names, editable)
        //  OurDesign    = ComboBox (all designs, auto-filled from alias)
        // ════════════════════════════════════════════════════════════
        DataGridView BuildWoItemsGrid()
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
            g.ColumnHeadersHeight = 34;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.BackColor = Surface;
            g.DefaultCellStyle.ForeColor = TextMid;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.DefaultCellStyle.SelectionBackColor = BluePale;
            g.DefaultCellStyle.SelectionForeColor = TextDark;
            g.DefaultCellStyle.Padding = new Padding(4, 0, 0, 0);
            g.RowTemplate.Height = 34;

            // Sr
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "#", Name = "Sr", ReadOnly = true, FillWeight = 4 });

            // ── CLIENT DESIGN NAME — ComboBox (editable, shows client's aliases) ──
            var colParty = new DataGridViewComboBoxColumn
            {
                HeaderText = "CLIENT DESIGN NAME",
                Name = "PartyDesign",
                FlatStyle = FlatStyle.Flat,
                FillWeight = 22,
                // Items will be populated by RebuildPartyDesignColumn() when client changes
                // We allow users to also type custom values via DisplayStyle
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox,
            };
            colParty.Items.Add(""); // start empty
            g.Columns.Add(colParty);

            // ── OUR DESIGN — ComboBox (all designs, auto-filled) ─────────────────
            var colDesign = new DataGridViewComboBoxColumn
            {
                HeaderText = "OUR DESIGN",
                Name = "DesignId",
                FlatStyle = FlatStyle.Flat,
                FillWeight = 22,
                DataSource = _designs.ToList(),
                DisplayMember = "DesignName",
                ValueMember = "DesignId"
            };
            g.Columns.Add(colDesign);

            // Qty / Unit / Pcs / Remarks
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "QTY", Name = "Qty", FillWeight = 10 });
            var colUnit = new DataGridViewComboBoxColumn { HeaderText = "UNIT", Name = "Unit", FlatStyle = FlatStyle.Flat, FillWeight = 8 };
            colUnit.Items.AddRange("MTR", "PCS", "KG", "YDS", "SET");
            g.Columns.Add(colUnit);
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PCS", Name = "Pcs", FillWeight = 7 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "REMARKS", Name = "ItemRemarks", FillWeight = 18 });

            // Commit combobox edits immediately
            g.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (g.IsCurrentCellDirty &&
                    (g.CurrentCell?.OwningColumn.Name == "DesignId" ||
                     g.CurrentCell?.OwningColumn.Name == "Unit" ||
                     g.CurrentCell?.OwningColumn.Name == "PartyDesign"))
                    g.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // ── KEY EVENT: when PartyDesign value changes → auto-fill OurDesign ──
            g.CellValueChanged += (s, e) =>
            {
                if (_suppressGridEvents) return;
                if (e.RowIndex < 0) return;
                if (g.Columns[e.ColumnIndex]?.Name != "PartyDesign") return;

                AutoFillOurDesign(g, e.RowIndex);
            };

            g.DataError += (s, e) => e.Cancel = true;

            // Stripe rows
            g.RowsAdded += (s, e) =>
            {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                    if (i >= 0 && i < g.Rows.Count)
                        g.Rows[i].DefaultCellStyle.BackColor = i % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253);
            };

            return g;
        }

        // ── When PartyDesign is selected, auto-fill OurDesign from alias ─────────
        void AutoFillOurDesign(DataGridView g, int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= g.Rows.Count) return;

            var row = g.Rows[rowIndex];
            string partyName = row.Cells["PartyDesign"].Value?.ToString()?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(partyName)) return;

            // Find matching alias for this client
            var alias = _clientAliases.FirstOrDefault(a =>
                string.Equals(a.PartyDesignName, partyName, StringComparison.OrdinalIgnoreCase));

            if (alias != null)
            {
                // Auto-fill OurDesign
                _suppressGridEvents = true;
                try { row.Cells["DesignId"].Value = alias.DesignId; }
                catch { }
                finally { _suppressGridEvents = false; }

                // Green flash to show it was auto-filled
                row.Cells["DesignId"].Style.BackColor = Color.FromArgb(220, 252, 231);
                var t = new System.Windows.Forms.Timer { Interval = 900 };
                t.Tick += (s, e) =>
                {
                    t.Stop();
                    if (rowIndex < g.Rows.Count)
                        g.Rows[rowIndex].Cells["DesignId"].Style.BackColor = Color.Empty;
                };
                t.Start();
            }
            // If no alias found → OurDesign stays empty, user picks manually
        }

        void AddWoItemRow()
        {
            // Validate client is selected first
            var ci = cmbWoClient.SelectedItem as AItem;
            if (ci == null || ci.Id == 0)
            {
                MessageBox.Show("Please select a client first before adding items.", "Select Client", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int i = dgvWoItems.Rows.Add();
            dgvWoItems.Rows[i].Cells["Qty"].Value = "0.00";
            dgvWoItems.Rows[i].Cells["Unit"].Value = "MTR";
            dgvWoItems.Rows[i].Cells["Pcs"].Value = "0";
            RenumberWoItems();
            dgvWoItems.CurrentCell = dgvWoItems.Rows[i].Cells["PartyDesign"];
            dgvWoItems.BeginEdit(true);
        }

        void DelWoItemRow()
        {
            if (dgvWoItems.SelectedRows.Count > 0)
            { dgvWoItems.Rows.Remove(dgvWoItems.SelectedRows[0]); RenumberWoItems(); }
        }

        void RenumberWoItems()
        {
            for (int i = 0; i < dgvWoItems.Rows.Count; i++)
                dgvWoItems.Rows[i].Cells["Sr"].Value = i + 1;
        }

        // ── Load & Render WO Grid ─────────────────────────────────────
        void LoadWoGrid(string q)
        {
            try
            {
                using var db = new AppDbContext();
                var list = db.WorkOrderHeaders
                    .Where(w => w.CompanyProfileId == SessionManager.CompanyProfileId)
                    .OrderByDescending(w => w.WoDate).ThenByDescending(w => w.WoId)
                    .ToList();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.ToLower();
                    list = list.Where(w =>
                        (w.WoNo ?? "").ToLower().Contains(q) ||
                        (w.ChallanNo ?? "").ToLower().Contains(q) ||
                        w.WoDate.ToString().Contains(q)).ToList();
                }
                _woAllRows = list;
                _woTotalPages = Math.Max(1, (int)Math.Ceiling(_woAllRows.Count / (double)WO_PAGE));
                if (_woPage > _woTotalPages) _woPage = _woTotalPages;
                RenderWoPage();
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        void RenderWoPage()
        {
            dgvWo.Rows.Clear();
            var page = _woAllRows.Skip((_woPage - 1) * WO_PAGE).Take(WO_PAGE).ToList();

            Dictionary<int, int> itemCounts = new();
            try
            {
                using var db = new AppDbContext();
                var ids = page.Select(w => w.WoId).ToList();
                itemCounts = db.WorkOrderItems
                    .Where(i => ids.Contains(i.WoId))
                    .GroupBy(i => i.WoId)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch { }

            var accLookup = _accounts.ToDictionary(a => a.AccountId, a => a.AccNm);
            foreach (var w in page)
            {
                string client = w.AccountId.HasValue && accLookup.TryGetValue(w.AccountId.Value, out var nm) ? nm : "—";
                int cnt = itemCounts.TryGetValue(w.WoId, out var c) ? c : 0;
                dgvWo.Rows.Add(w.WoId, w.WoNo, w.WoDate.ToString("dd-MM-yyyy"), client, w.ChallanNo ?? "—", cnt, w.Status);
            }

            _lblWoPageInfo.Text = $"Page {_woPage} of {_woTotalPages}  ({_woAllRows.Count} records)";
            _btnWoPrev.Enabled = _woPage > 1;
            _btnWoNext.Enabled = _woPage < _woTotalPages;
            if (_lblWoPageInfo.Parent != null)
                _lblWoPageInfo.Location = new Point(
                    (_lblWoPageInfo.Parent.Width - _lblWoPageInfo.Width) / 2,
                    (_lblWoPageInfo.Parent.Height - _lblWoPageInfo.Height) / 2);
        }

        void DgvWoClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(dgvWo.Rows[e.RowIndex].Cells["Id"].Value);

            if (e.ColumnIndex == dgvWo.Columns["Print"]?.Index) { PrintWorkOrder(id); return; }
            if (e.ColumnIndex == dgvWo.Columns["Edit"]?.Index) { ShowWoForm(id); return; }
            if (e.ColumnIndex == dgvWo.Columns["Delete"]?.Index)
            {
                string woNo = dgvWo.Rows[e.RowIndex].Cells["WoNo"].Value?.ToString() ?? "";
                if (MessageBox.Show($"Delete Work Order \"{woNo}\"?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                try
                {
                    using var db = new AppDbContext();
                    var w = db.WorkOrderHeaders.Find(id);
                    if (w != null) { db.WorkOrderHeaders.Remove(w); db.SaveChanges(); }
                    Toast("Work Order deleted.");
                    LoadWoGrid(txtWoSearch?.Text ?? "");
                }
                catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
            }
        }

        void ShowWoForm(int? editId)
        {
            ClearWoForm();
            _woEditId = -1;
            lblWoFormTitle.Text = "New Work Order";

            if (editId.HasValue && editId.Value > 0)
            {
                _woEditId = editId.Value;
                lblWoFormTitle.Text = "Edit Work Order";
                try
                {
                    using var db = new AppDbContext();
                    var w = db.WorkOrderHeaders.Find(_woEditId);
                    if (w == null) return;

                    txtWoNo.Text = w.WoNo;
                    dtpWoDate.Value = w.WoDate.ToDateTime(TimeOnly.MinValue);

                    // Set client — suppress grid clear that happens on SelectedIndexChanged
                    _suppressGridEvents = true;
                    SelectAItem(cmbWoClient, w.AccountId ?? 0);
                    _suppressGridEvents = false;

                    // Now load aliases for this client
                    LoadClientAliases();

                    txtWoChallanNo.Text = w.ChallanNo ?? "";
                    txtWoRemarks.Text = w.Remarks ?? "";
                    if (w.ChallanDate.HasValue)
                    {
                        chkWoChallanDate.Checked = true;
                        dtpWoChallanDate.Value = w.ChallanDate.Value.ToDateTime(TimeOnly.MinValue);
                    }

                    // Load items — suppress auto-fill so saved values are not overwritten
                    _suppressGridEvents = true;
                    var items = db.WorkOrderItems.Where(i => i.WoId == _woEditId).OrderBy(i => i.ItemId).ToList();
                    foreach (var it in items)
                    {
                        int ri = dgvWoItems.Rows.Add();
                        dgvWoItems.Rows[ri].Cells["Sr"].Value = ri + 1;
                        // PartyDesign: set the value — must be in the combobox items list
                        var pdCell = dgvWoItems.Rows[ri].Cells["PartyDesign"] as DataGridViewComboBoxCell;
                        if (pdCell != null && !pdCell.Items.Contains(it.PartyDesignName ?? ""))
                            pdCell.Items.Add(it.PartyDesignName ?? "");
                        dgvWoItems.Rows[ri].Cells["PartyDesign"].Value = it.PartyDesignName ?? "";
                        dgvWoItems.Rows[ri].Cells["DesignId"].Value = it.DesignId;
                        dgvWoItems.Rows[ri].Cells["Qty"].Value = (it.Qty ?? 0).ToString("0.00");
                        dgvWoItems.Rows[ri].Cells["Unit"].Value = it.Unit ?? "MTR";
                        dgvWoItems.Rows[ri].Cells["Pcs"].Value = (it.Pcs ?? 0).ToString();
                        dgvWoItems.Rows[ri].Cells["ItemRemarks"].Value = it.Remarks ?? "";
                    }
                    _suppressGridEvents = false;
                }
                catch (Exception ex)
                {
                    _suppressGridEvents = false;
                    MessageBox.Show("Load error: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    using var db = new AppDbContext();
                    var t = db.WorkOrderNumberTrackers.Find(SessionManager.CompanyProfileId);
                    int next = (t?.CurrentWoNo ?? 0) + 1;
                    txtWoNo.Text = $"WO/{DateTime.Today:yyyy}/{next:D4}";
                }
                catch { txtWoNo.Text = $"WO/{DateTime.Today:yyyy}/0001"; }
            }

            pnlWoGrid.Visible = false;
            pnlWoForm.Visible = true;
        }

        void ShowWoGrid()
        {
            pnlWoForm.Visible = false;
            pnlWoGrid.Visible = true;
            _woEditId = -1;
            _clientAliases.Clear();
            LoadWoGrid(txtWoSearch?.Text ?? "");
        }

        void ClearWoForm()
        {
            txtWoNo.Text = txtWoChallanNo.Text = txtWoRemarks.Text = "";
            dtpWoDate.Value = DateTime.Today;
            chkWoChallanDate.Checked = false;
            _suppressGridEvents = true;
            cmbWoClient.SelectedIndex = 0;
            _suppressGridEvents = false;
            _clientAliases.Clear();
            dgvWoItems.Rows.Clear();
            RebuildPartyDesignColumn(); // reset to empty
        }

        void WoSave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtWoNo.Text))
            { MessageBox.Show("Work Order number is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (dgvWoItems.Rows.Count == 0)
            { MessageBox.Show("Add at least one item.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // Commit any pending edit
            dgvWoItems.EndEdit();

            var ci = cmbWoClient.SelectedItem as AItem;
            int cid = SessionManager.CompanyProfileId;

            try
            {
                using var db = new AppDbContext();
                using var tx = db.Database.BeginTransaction();

                WorkOrderHeader hdr;
                if (_woEditId > 0)
                {
                    hdr = db.WorkOrderHeaders.Find(_woEditId)!;
                    var oldItems = db.WorkOrderItems.Where(i => i.WoId == _woEditId).ToList();
                    db.WorkOrderItems.RemoveRange(oldItems);
                    db.SaveChanges();
                }
                else
                {
                    hdr = new WorkOrderHeader { CompanyProfileId = cid };
                    var tracker = db.WorkOrderNumberTrackers.Find(cid);
                    if (tracker == null)
                    { tracker = new WorkOrderNumberTracker { CompanyProfileId = cid, CurrentWoNo = 1 }; db.WorkOrderNumberTrackers.Add(tracker); }
                    else tracker.CurrentWoNo++;
                    db.WorkOrderHeaders.Add(hdr);
                }

                hdr.WoNo = txtWoNo.Text.Trim();
                hdr.WoDate = DateOnly.FromDateTime(dtpWoDate.Value);
                hdr.AccountId = ci != null && ci.Id > 0 ? ci.Id : (int?)null;
                hdr.ChallanNo = string.IsNullOrWhiteSpace(txtWoChallanNo.Text) ? null : txtWoChallanNo.Text.Trim();
                hdr.ChallanDate = chkWoChallanDate.Checked ? DateOnly.FromDateTime(dtpWoChallanDate.Value) : (DateOnly?)null;
                hdr.Remarks = txtWoRemarks.Text.Trim();
                hdr.Status = "Pending";
                db.SaveChanges();

                foreach (DataGridViewRow row in dgvWoItems.Rows)
                {
                    var dv = row.Cells["DesignId"].Value;
                    db.WorkOrderItems.Add(new WorkOrderItem
                    {
                        WoId = hdr.WoId,
                        CompanyProfileId = cid,
                        DesignId = (dv != null && dv != DBNull.Value) ? (int?)Convert.ToInt32(dv) : null,
                        PartyDesignName = row.Cells["PartyDesign"].Value?.ToString(),
                        Qty = DecParse(row.Cells["Qty"].Value),
                        Unit = row.Cells["Unit"].Value?.ToString() ?? "MTR",
                        Pcs = IntParse(row.Cells["Pcs"].Value),
                        Remarks = row.Cells["ItemRemarks"].Value?.ToString()
                    });
                }
                db.SaveChanges();
                tx.Commit();

                Toast(_woEditId > 0 ? "Work Order updated." : "Work Order saved.");
                ShowWoGrid();
            }
            catch (Exception ex) { MessageBox.Show("Save error: " + ex.Message); }
        }

        // ── Print Work Order ──────────────────────────────────────────
        void PrintWorkOrder(int woId)
        {
            try
            {
                using var db = new AppDbContext();
                var hdr = db.WorkOrderHeaders.Find(woId); if (hdr == null) return;
                var items = db.WorkOrderItems.Where(i => i.WoId == woId).OrderBy(i => i.ItemId).ToList();
                var acc = hdr.AccountId.HasValue ? _accounts.FirstOrDefault(a => a.AccountId == hdr.AccountId) : null;
                var dLookup = _designs.ToDictionary(d => d.DesignId, d => d.DesignName);

                var pd = new PrintDocument();
                pd.PrintPage += (s, e) =>
                {
                    var g = e.Graphics;
                    float x = 40, y = 30, pw = e.PageBounds.Width - 80;
                    using var boldFont = new Font("Segoe UI", 13F, FontStyle.Bold);
                    using var normFont = new Font("Segoe UI", 9F);
                    using var smallFont = new Font("Segoe UI", 8F);
                    using var darkBrush = new SolidBrush(Color.FromArgb(15, 23, 42));
                    using var midBrush = new SolidBrush(Color.FromArgb(71, 85, 105));
                    using var blueBrush = new SolidBrush(Color.FromArgb(37, 99, 235));
                    using var borderPen = new Pen(Color.FromArgb(226, 232, 240), 1);

                    g.FillRectangle(new SolidBrush(Color.FromArgb(37, 99, 235)), x - 10, y - 10, pw + 20, 6); y += 4;
                    g.DrawString(SessionManager.CompanyName, boldFont, darkBrush, x, y);
                    g.DrawString("WORK ORDER", boldFont, blueBrush, pw - 80, y); y += 22;
                    g.DrawString($"WO No: {hdr.WoNo}   Date: {hdr.WoDate:dd-MM-yyyy}", normFont, midBrush, x, y);
                    if (acc != null) g.DrawString($"Client: {acc.AccNm}", normFont, midBrush, pw - 200, y);
                    y += 18;
                    if (!string.IsNullOrWhiteSpace(hdr.ChallanNo))
                        g.DrawString($"Challan No: {hdr.ChallanNo}" + (hdr.ChallanDate.HasValue ? $"   Date: {hdr.ChallanDate:dd-MM-yyyy}" : ""), normFont, midBrush, x, y);
                    y += 8;
                    g.DrawLine(borderPen, x - 10, y, x + pw + 10, y); y += 10;

                    float[] cw = { 30, 180, 190, 70, 60, 50 };
                    string[] ch = { "#", "CLIENT DESIGN", "OUR DESIGN", "QTY", "UNIT", "PCS" };
                    g.FillRectangle(new SolidBrush(Color.FromArgb(241, 245, 249)), x - 4, y, pw + 8, 22);
                    float cx = x;
                    for (int i2 = 0; i2 < ch.Length; i2++) { g.DrawString(ch[i2], smallFont, midBrush, cx + 2, y + 5); cx += cw[i2]; }
                    y += 24; g.DrawLine(borderPen, x - 10, y, x + pw + 10, y); y += 4;

                    int sr = 1;
                    foreach (var it in items)
                    {
                        string ourDesign = it.DesignId.HasValue && dLookup.TryGetValue(it.DesignId.Value, out var dn) ? dn : "—";
                        string[] vals = { sr.ToString(), it.PartyDesignName ?? "—", ourDesign, (it.Qty ?? 0).ToString("0.00"), it.Unit ?? "MTR", (it.Pcs ?? 0).ToString() };
                        cx = x;
                        for (int i2 = 0; i2 < vals.Length; i2++) { g.DrawString(vals[i2], normFont, darkBrush, cx + 2, y + 2); cx += cw[i2]; }
                        y += 22;
                        g.DrawLine(new Pen(Color.FromArgb(241, 245, 249), 1), x - 4, y, x + pw + 4, y);
                        sr++;
                    }
                    y += 10; g.DrawLine(borderPen, x - 10, y, x + pw + 10, y); y += 10;
                    if (!string.IsNullOrWhiteSpace(hdr.Remarks)) g.DrawString("Remarks: " + hdr.Remarks, smallFont, midBrush, x, y);
                    y += 20;
                    g.DrawString("Prepared by: ____________________", normFont, midBrush, x, y);
                    g.DrawString("Authorised by: ____________________", normFont, midBrush, pw - 120, y);
                };
                using var dlg = new PrintPreviewDialog { Document = pd, Width = 900, Height = 700 };
                dlg.ShowDialog(this.FindForm());
            }
            catch (Exception ex) { MessageBox.Show("Print error: " + ex.Message); }
        }

        // ── Excel Export ──────────────────────────────────────────────
        void WoExportExcel(object sender, EventArgs e)
        {
            if (_woAllRows.Count == 0) { MessageBox.Show("No records to export."); return; }
            using var sfd = new SaveFileDialog { Filter = "Excel Files (*.xlsx)|*.xlsx", FileName = $"WorkOrders_{DateTime.Today:yyyyMMdd}.xlsx" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                using var db = new AppDbContext();
                var tmp = new DataGridView();
                tmp.Columns.Add("WoNo", "WO NO"); tmp.Columns.Add("WoDate", "DATE"); tmp.Columns.Add("Client", "CLIENT");
                tmp.Columns.Add("Challan", "CHALLAN NO"); tmp.Columns.Add("PartyDesign", "CLIENT DESIGN");
                tmp.Columns.Add("OurDesign", "OUR DESIGN"); tmp.Columns.Add("Qty", "QTY"); tmp.Columns.Add("Unit", "UNIT");
                tmp.Columns.Add("Pcs", "PCS"); tmp.Columns.Add("Status", "STATUS"); tmp.Columns.Add("Remarks", "REMARKS");
                var accLookup = _accounts.ToDictionary(a => a.AccountId, a => a.AccNm);
                var dLookup = _designs.ToDictionary(d => d.DesignId, d => d.DesignName);
                foreach (var w in _woAllRows)
                {
                    string client = w.AccountId.HasValue && accLookup.TryGetValue(w.AccountId.Value, out var nm) ? nm : "";
                    var wItems = db.WorkOrderItems.Where(i => i.WoId == w.WoId).ToList();
                    if (wItems.Count == 0)
                        tmp.Rows.Add(w.WoNo, w.WoDate.ToString("dd-MM-yyyy"), client, w.ChallanNo, "", "", "", "", "", w.Status, w.Remarks);
                    else
                        foreach (var it in wItems)
                        {
                            string ourD = it.DesignId.HasValue && dLookup.TryGetValue(it.DesignId.Value, out var dn) ? dn : "";
                            tmp.Rows.Add(w.WoNo, w.WoDate.ToString("dd-MM-yyyy"), client, w.ChallanNo, it.PartyDesignName, ourD, it.Qty, it.Unit, it.Pcs, w.Status, w.Remarks);
                        }
                }
                ExcelExportHelper.Export(tmp, sfd.FileName, "Work Orders");
                Toast($"Exported {_woAllRows.Count} work orders.");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show("Export error: " + ex.Message); }
        }

        void WoDownloadSample(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = "WorkOrder_Sample.csv" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            var sb = new StringBuilder();
            sb.AppendLine("WoDate,ClientName,ChallanNo,PartyDesignName,OurDesignName,Qty,Unit,Pcs,Remarks");
            sb.AppendLine("30-03-2026,Harit Textile,CH-101,H1,R7,50.00,MTR,2,");
            sb.AppendLine("30-03-2026,Harit Textile,CH-101,H2,K3,30.00,MTR,1,");
            sb.AppendLine("30-03-2026,ABC Fabrics,CH-202,A11,R7,20.00,MTR,1,Urgent");
            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Sample CSV saved.", "Sample Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
        }

        void WoImportCsv(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            int added = 0, skipped = 0; var log = new StringBuilder();
            try
            {
                string[] lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                if (lines.Length < 2) { MessageBox.Show("No data rows in CSV."); return; }
                string[] hdr = CsvSplit(lines[0]);
                int Idx(string c) { for (int i2 = 0; i2 < hdr.Length; i2++) if (hdr[i2].Trim().Equals(c, StringComparison.OrdinalIgnoreCase)) return i2; return -1; }
                int iDate = Idx("WoDate"), iClient = Idx("ClientName"), iChallan = Idx("ChallanNo"),
                    iParty = Idx("PartyDesignName"), iOur = Idx("OurDesignName"),
                    iQty = Idx("Qty"), iUnit = Idx("Unit"), iPcs = Idx("Pcs"), iRem = Idx("Remarks");
                if (iDate < 0 || iParty < 0) { MessageBox.Show("Required columns not found."); return; }
                int cid = SessionManager.CompanyProfileId;
                using var db = new AppDbContext();
                using var tx = db.Database.BeginTransaction();
                var groups = new Dictionary<string, WorkOrderHeader>();
                var tracker = db.WorkOrderNumberTrackers.Find(cid);
                if (tracker == null) { tracker = new WorkOrderNumberTracker { CompanyProfileId = cid, CurrentWoNo = 0 }; db.WorkOrderNumberTrackers.Add(tracker); }
                for (int li = 1; li < lines.Length; li++)
                {
                    if (string.IsNullOrWhiteSpace(lines[li])) continue;
                    string[] f = CsvSplit(lines[li]);
                    string dateStr = Get(f, iDate), clientNm = Get(f, iClient), challanNo = Get(f, iChallan),
                           partyD = Get(f, iParty), ourDNm = Get(f, iOur);
                    string unit = Get(f, iUnit).ToUpper();
                    if (!new[] { "MTR", "PCS", "KG", "YDS", "SET" }.Contains(unit)) unit = "MTR";
                    if (!DateTime.TryParseExact(dateStr, new[] { "dd-MM-yyyy", "yyyy-MM-dd", "d-M-yyyy" },
                        null, System.Globalization.DateTimeStyles.None, out DateTime dt))
                    { skipped++; log.AppendLine($"Row {li + 1}: invalid date — skipped."); continue; }
                    string key = $"{dateStr}|{clientNm}|{challanNo}";
                    if (!groups.ContainsKey(key))
                    {
                        var acc2 = _accounts.FirstOrDefault(a => a.AccNm.Equals(clientNm, StringComparison.OrdinalIgnoreCase));
                        tracker.CurrentWoNo++;
                        var wh = new WorkOrderHeader { CompanyProfileId = cid, WoNo = $"WO/{dt.Year}/{tracker.CurrentWoNo:D4}", WoDate = DateOnly.FromDateTime(dt), AccountId = acc2?.AccountId, ChallanNo = string.IsNullOrWhiteSpace(challanNo) ? null : challanNo, Status = "Pending" };
                        db.WorkOrderHeaders.Add(wh); db.SaveChanges(); groups[key] = wh; added++;
                    }
                    var wHeader = groups[key];
                    var design = _designs.FirstOrDefault(d => d.DesignName.Equals(ourDNm, StringComparison.OrdinalIgnoreCase));
                    db.WorkOrderItems.Add(new WorkOrderItem { WoId = wHeader.WoId, CompanyProfileId = cid, DesignId = design?.DesignId, PartyDesignName = partyD, Qty = DecParse(Get(f, iQty)), Unit = unit, Pcs = IntParse(Get(f, iPcs)), Remarks = Get(f, iRem) });
                }
                db.SaveChanges(); tx.Commit();
                string msg = $"Import complete.\n\n✅ Work Orders created : {added}\n⏭ Rows skipped : {skipped}";
                if (log.Length > 0) msg += "\n\n" + log;
                MessageBox.Show(msg, "Import Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _woPage = 1; LoadWoGrid(txtWoSearch?.Text ?? "");
            }
            catch (Exception ex) { MessageBox.Show("Import error: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════
        //  TAB 2 — DESIGN ALIAS
        // ════════════════════════════════════════════════════════════
        void BuildAliasTab()
        {
            pnlAlias = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Visible = false };
            pnlAliasGrid = new Panel { Dock = DockStyle.Fill, BackColor = Bg };
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Teal });

            var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Surface2 };
            bar.Paint += (s, e) => { using var p = new Pen(Border); e.Graphics.DrawLine(p, 0, bar.Height - 1, bar.Width, bar.Height - 1); };

            var srch = new Panel { Location = new Point(16, 10), Size = new Size(280, 32), BackColor = Color.White };
            srch.Paint += (s, e) => { using var bp = new Pen(Border); using var br = new SolidBrush(TextLight); using var ef = new Font("Segoe UI Emoji", 9F); e.Graphics.DrawRectangle(bp, 0, 0, srch.Width - 1, srch.Height - 1); e.Graphics.DrawString("🔍", ef, br, new PointF(5, 7)); };
            txtAliasSearch = new TextBox { PlaceholderText = "Search by party name, design…", Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.None, BackColor = Color.White, ForeColor = TextDark, Location = new Point(27, 7), Size = new Size(245, 20) };
            txtAliasSearch.TextChanged += (s, e) => { _aliasPage = 1; LoadAliasGrid(txtAliasSearch.Text); };
            srch.Controls.Add(txtAliasSearch); bar.Controls.Add(srch);

            var btnAdd = MkBtn("➕  Add Alias", Teal, Color.White, TealHov); var btnExport = MkBtn("⬇  Export Excel", Green, Color.White, GreenHov);
            var btnImport = MkBtn("⬆  Import CSV", Violet, Color.White, VioletHov); var btnSample = MkBtn("📥 Sample CSV", Blue, Color.White, BlueHov);
            btnAdd.Size = new Size(120, 30); btnExport.Size = new Size(130, 30); btnImport.Size = new Size(118, 30); btnSample.Size = new Size(126, 30);
            btnAdd.Click += (s, e) => ShowAliasForm(-1); btnExport.Click += AliasExportExcel; btnImport.Click += AliasImportCsv; btnSample.Click += AliasDownloadSample;
            bar.Controls.Add(btnAdd); bar.Controls.Add(btnExport); bar.Controls.Add(btnImport); bar.Controls.Add(btnSample);
            bar.Resize += (s, e) => { int r = bar.Width - 12; btnExport.Location = new Point(r - btnExport.Width, 11); btnImport.Location = new Point(r - btnExport.Width - btnImport.Width - 6, 11); btnSample.Location = new Point(r - btnExport.Width - btnImport.Width - btnSample.Width - 12, 11); btnAdd.Location = new Point(r - btnExport.Width - btnImport.Width - btnSample.Width - btnAdd.Width - 18, 11); };

            var pager = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Surface2 };
            pager.Paint += (s, e) => { using var p = new Pen(Border); e.Graphics.DrawLine(p, 0, 0, pager.Width, 0); };
            _btnAliasPrev = MkBtn("◀  Prev", Surface, TextMid, Color.FromArgb(241, 245, 249), true); _btnAliasPrev.Size = new Size(82, 30); _btnAliasPrev.Location = new Point(12, 7);
            _btnAliasNext = MkBtn("Next  ▶", Surface, TextMid, Color.FromArgb(241, 245, 249), true); _btnAliasNext.Size = new Size(82, 30);
            _lblAliasPageInfo = new Label { Text = "Page 1 of 1  (0 records)", ForeColor = TextMid, Font = new Font("Segoe UI", 9F), AutoSize = true };
            _btnAliasPrev.Click += (s, e) => { if (_aliasPage > 1) { _aliasPage--; RenderAliasPage(); } };
            _btnAliasNext.Click += (s, e) => { if (_aliasPage < _aliasTotalPages) { _aliasPage++; RenderAliasPage(); } };
            pager.Controls.Add(_btnAliasPrev); pager.Controls.Add(_lblAliasPageInfo); pager.Controls.Add(_btnAliasNext);
            pager.Resize += (s, e) => { _lblAliasPageInfo.Location = new Point((_lblAliasPageInfo.Parent.Width - _lblAliasPageInfo.Width) / 2, (_lblAliasPageInfo.Parent.Height - _lblAliasPageInfo.Height) / 2); _btnAliasNext.Location = new Point(pager.Width - _btnAliasNext.Width - 12, 7); };

            dgvAlias = BuildDgv();
            dgvAlias.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
            dgvAlias.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CLIENT / PARTY", Name = "Client", MinimumWidth = 180, FillWeight = 26 });
            dgvAlias.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CLIENT DESIGN NAME", Name = "PartyDesign", MinimumWidth = 160, FillWeight = 22 });
            dgvAlias.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "→  OUR DESIGN", Name = "OurDesign", MinimumWidth = 160, FillWeight = 22 });
            dgvAlias.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "NOTES", Name = "Notes", MinimumWidth = 140, FillWeight = 18 });
            dgvAlias.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Edit", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 68 });
            dgvAlias.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Delete", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 74 });

            dgvAlias.CellFormatting += (s, e) => { if (e.RowIndex < 0) return; if (e.ColumnIndex == dgvAlias.Columns["OurDesign"]?.Index) { e.CellStyle.ForeColor = Teal; e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold); } foreach (var n in new[] { "Edit", "Delete" }) if (e.ColumnIndex == dgvAlias.Columns[n]?.Index) { e.Value = ""; e.FormattingApplied = true; } };
            AttachActionPainter(dgvAlias, ("Edit", AmberPale, Amber, AmberBdr, "✏  Edit"), ("Delete", RedPale, Red, RedBdr, "🗑 Delete"));
            dgvAlias.CellClick += DgvAliasClick;

            card.Controls.Add(dgvAlias); card.Controls.Add(pager); card.Controls.Add(bar);
            pnlAliasGrid.Controls.Add(card);
            pnlAliasForm = BuildAliasForm();
            pnlAlias.Controls.Add(pnlAliasForm); pnlAlias.Controls.Add(pnlAliasGrid);
            this.Controls.Add(pnlAlias);
            LoadAliasGrid("");
        }

        Panel BuildAliasForm()
        {
            var frm = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Visible = false, AutoScroll = true };
            var top = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Bg };
            lblAliasFormTitle = new Label { Text = "Add Design Alias", Font = new Font("Segoe UI", 15F, FontStyle.Bold), ForeColor = TextDark, AutoSize = true, Location = new Point(24, 8) };
            top.Controls.Add(lblAliasFormTitle);
            top.Controls.Add(new Label { Text = "Map a client's design name to your internal design", Font = new Font("Segoe UI", 9F), ForeColor = TextLight, AutoSize = true, Location = new Point(27, 36) });

            var card = new Panel { BackColor = Surface, Location = new Point(24, 64), Size = new Size(700, 380), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Teal });
            var sec = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Surface2 };
            sec.Paint += (s, e) => { using var p = new Pen(Border); e.Graphics.DrawLine(p, 0, sec.Height - 1, sec.Width, sec.Height - 1); };
            sec.Controls.Add(new Label { Text = "Alias Details", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = TextDark, AutoSize = true, Location = new Point(16, 10) });
            card.Controls.Add(sec);

            int lx = 20, fx = 200, fw = 320, y = 52, gap = 50;

            card.Controls.Add(FLbl("Client / Party *", new Point(lx, y + 5)));
            cmbAliasClient = MkCmb(fx, y, fw); BindAccountCombo(cmbAliasClient, "-- Select Client --");
            card.Controls.Add(cmbAliasClient); y += gap;

            txtPartyDesignName = FF(card, "Client Design Name *", lx, fx, fw, y);
            card.Controls.Add(new Label { Text = "e.g. H1, A11, B-Gold", Font = new Font("Segoe UI", 7.5F, FontStyle.Italic), ForeColor = TextLight, AutoSize = true, Location = new Point(fx + fw + 8, y + 8) });
            y += gap;

            card.Controls.Add(FLbl("Our Design *", new Point(lx, y + 5)));
            cmbAliasOurDesign = MkCmb(fx, y, fw);
            cmbAliasOurDesign.Items.Add(new DItem(0, "-- Select Your Design --"));
            foreach (var d in _designs) cmbAliasOurDesign.Items.Add(new DItem(d.DesignId, d.DesignName));
            cmbAliasOurDesign.DisplayMember = "Display"; cmbAliasOurDesign.ValueMember = "Id"; cmbAliasOurDesign.SelectedIndex = 0;
            card.Controls.Add(cmbAliasOurDesign); y += gap;

            txtAliasNotes = FF(card, "Notes", lx, fx, fw, y); y += gap;

            var info = new Panel { Location = new Point(lx, y), Size = new Size(fw + fx - lx, 52), BackColor = Color.FromArgb(239, 246, 255) };
            info.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 3, BackColor = Blue });
            info.Controls.Add(new Label { Text = "💡 Once saved, selecting this client design in a Work Order will auto-fill Our Design.", Font = new Font("Segoe UI", 8.5F), ForeColor = TextMid, AutoSize = true, Location = new Point(10, 8) });
            card.Controls.Add(info); y += 62;

            var btnSave = MkBtn("💾  Save Alias", Teal, Color.White, TealHov);
            var btnCancel = MkBtn("✕  Cancel", Surface, TextMid, Color.FromArgb(241, 245, 249), true);
            btnSave.Location = new Point(fx, y); btnSave.Size = new Size(130, 36);
            btnCancel.Location = new Point(fx + 138, y); btnCancel.Size = new Size(100, 36);
            btnSave.Click += AliasSave; btnCancel.Click += (s, e) => ShowAliasGrid();
            card.Controls.Add(btnSave); card.Controls.Add(btnCancel);
            card.Height = y + 56;
            frm.Resize += (s, e) => card.Width = Math.Max(500, frm.Width - 48);
            frm.Controls.Add(card); frm.Controls.Add(top);
            return frm;
        }

        void LoadAliasGrid(string q)
        {
            try
            {
                using var db = new AppDbContext();
                var list = db.PartyDesignAliases.Where(a => a.CompanyProfileId == SessionManager.CompanyProfileId).ToList();
                if (!string.IsNullOrWhiteSpace(q)) { q = q.ToLower(); list = list.Where(a => (a.PartyDesignName ?? "").ToLower().Contains(q) || (a.Notes ?? "").ToLower().Contains(q)).ToList(); }
                _aliasAllRows = list;
                _aliasTotalPages = Math.Max(1, (int)Math.Ceiling(_aliasAllRows.Count / (double)ALIAS_PAGE));
                if (_aliasPage > _aliasTotalPages) _aliasPage = _aliasTotalPages;
                RenderAliasPage();
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        void RenderAliasPage()
        {
            dgvAlias.Rows.Clear();
            var accLookup = _accounts.ToDictionary(a => a.AccountId, a => a.AccNm);
            var dLookup = _designs.ToDictionary(d => d.DesignId, d => d.DesignName);
            foreach (var a in _aliasAllRows.Skip((_aliasPage - 1) * ALIAS_PAGE).Take(ALIAS_PAGE))
            {
                string client = accLookup.TryGetValue(a.AccountId, out var nm) ? nm : "—";
                string design = dLookup.TryGetValue(a.DesignId, out var dn) ? dn : "—";
                dgvAlias.Rows.Add(a.AliasId, client, a.PartyDesignName, design, a.Notes);
            }
            _lblAliasPageInfo.Text = $"Page {_aliasPage} of {_aliasTotalPages}  ({_aliasAllRows.Count} records)";
            _btnAliasPrev.Enabled = _aliasPage > 1; _btnAliasNext.Enabled = _aliasPage < _aliasTotalPages;
            if (_lblAliasPageInfo.Parent != null) _lblAliasPageInfo.Location = new Point((_lblAliasPageInfo.Parent.Width - _lblAliasPageInfo.Width) / 2, (_lblAliasPageInfo.Parent.Height - _lblAliasPageInfo.Height) / 2);
        }

        void DgvAliasClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(dgvAlias.Rows[e.RowIndex].Cells["Id"].Value);
            if (e.ColumnIndex == dgvAlias.Columns["Edit"]?.Index) { ShowAliasForm(id); return; }
            if (e.ColumnIndex == dgvAlias.Columns["Delete"]?.Index)
            {
                if (MessageBox.Show("Delete this alias?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                try { using var db = new AppDbContext(); var a = db.PartyDesignAliases.Find(id); if (a != null) { db.PartyDesignAliases.Remove(a); db.SaveChanges(); } Toast("Alias deleted."); LoadAliasGrid(txtAliasSearch?.Text ?? ""); }
                catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
            }
        }

        void ShowAliasForm(int editId)
        {
            _aliasEditId = editId;
            lblAliasFormTitle.Text = editId > 0 ? "Edit Design Alias" : "Add Design Alias";
            txtPartyDesignName.Text = txtAliasNotes.Text = "";
            cmbAliasClient.SelectedIndex = cmbAliasOurDesign.SelectedIndex = 0;
            if (editId > 0)
            {
                try { using var db = new AppDbContext(); var a = db.PartyDesignAliases.Find(editId); if (a == null) return; SelectAItem(cmbAliasClient, a.AccountId); txtPartyDesignName.Text = a.PartyDesignName; SelectDItem(cmbAliasOurDesign, a.DesignId); txtAliasNotes.Text = a.Notes ?? ""; }
                catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
            }
            pnlAliasGrid.Visible = false; pnlAliasForm.Visible = true;
        }

        void ShowAliasGrid() { pnlAliasForm.Visible = false; pnlAliasGrid.Visible = true; _aliasEditId = -1; LoadAliasGrid(txtAliasSearch?.Text ?? ""); }

        void AliasSave(object sender, EventArgs e)
        {
            var ci = cmbAliasClient.SelectedItem as AItem;
            var di = cmbAliasOurDesign.SelectedItem as DItem;
            if (ci == null || ci.Id == 0) { MessageBox.Show("Select a client.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtPartyDesignName.Text)) { MessageBox.Show("Client Design Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (di == null || di.Id == 0) { MessageBox.Show("Select your design.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                using var db = new AppDbContext(); int cid = SessionManager.CompanyProfileId;
                var dup = db.PartyDesignAliases.FirstOrDefault(a => a.CompanyProfileId == cid && a.AccountId == ci.Id && a.PartyDesignName == txtPartyDesignName.Text.Trim() && (_aliasEditId > 0 ? a.AliasId != _aliasEditId : true));
                if (dup != null) { MessageBox.Show($"Alias '{txtPartyDesignName.Text.Trim()}' already exists for this client.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (_aliasEditId > 0) { var a = db.PartyDesignAliases.Find(_aliasEditId); if (a == null) return; a.AccountId = ci.Id; a.PartyDesignName = txtPartyDesignName.Text.Trim(); a.DesignId = di.Id; a.Notes = txtAliasNotes.Text.Trim(); Toast("Alias updated."); }
                else { db.PartyDesignAliases.Add(new PartyDesignAlias { CompanyProfileId = cid, AccountId = ci.Id, PartyDesignName = txtPartyDesignName.Text.Trim(), DesignId = di.Id, Notes = txtAliasNotes.Text.Trim() }); Toast("Alias added."); }
                db.SaveChanges(); ShowAliasGrid();
            }
            catch (Exception ex) { MessageBox.Show("Save error: " + ex.Message); }
        }

        void AliasExportExcel(object sender, EventArgs e)
        {
            if (_aliasAllRows.Count == 0) { MessageBox.Show("No records to export."); return; }
            using var sfd = new SaveFileDialog { Filter = "Excel Files (*.xlsx)|*.xlsx", FileName = $"DesignAliases_{DateTime.Today:yyyyMMdd}.xlsx" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                var tmp = new DataGridView(); tmp.Columns.Add("Client", "CLIENT"); tmp.Columns.Add("PartyDesign", "CLIENT DESIGN"); tmp.Columns.Add("OurDesign", "OUR DESIGN"); tmp.Columns.Add("Notes", "NOTES");
                var accLookup = _accounts.ToDictionary(a => a.AccountId, a => a.AccNm); var dLookup = _designs.ToDictionary(d => d.DesignId, d => d.DesignName);
                foreach (var a in _aliasAllRows) { string cl = accLookup.TryGetValue(a.AccountId, out var nm) ? nm : ""; string dn = dLookup.TryGetValue(a.DesignId, out var d) ? d : ""; tmp.Rows.Add(cl, a.PartyDesignName, dn, a.Notes); }
                ExcelExportHelper.Export(tmp, sfd.FileName, "Design Aliases"); Toast($"Exported {_aliasAllRows.Count} aliases.");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show("Export error: " + ex.Message); }
        }

        void AliasDownloadSample(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = "DesignAlias_Sample.csv" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            var sb = new StringBuilder(); sb.AppendLine("ClientName,PartyDesignName,OurDesignName,Notes"); sb.AppendLine("Harit Textile,H1,R7,Harit's red floral"); sb.AppendLine("Harit Textile,H2,K3,"); sb.AppendLine("ABC Fabrics,A11,R7,Same as Harit H1");
            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8); MessageBox.Show("Sample CSV saved.", "Sample Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
        }

        void AliasImportCsv(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*" };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            int added = 0, skipped = 0; var log = new StringBuilder();
            try
            {
                string[] lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8); if (lines.Length < 2) { MessageBox.Show("No data rows."); return; }
                string[] hdr = CsvSplit(lines[0]); int Idx(string c) { for (int i2 = 0; i2 < hdr.Length; i2++) if (hdr[i2].Trim().Equals(c, StringComparison.OrdinalIgnoreCase)) return i2; return -1; }
                int iCl = Idx("ClientName"), iP = Idx("PartyDesignName"), iO = Idx("OurDesignName"), iN = Idx("Notes");
                if (iCl < 0 || iP < 0 || iO < 0) { MessageBox.Show("Required columns missing."); return; }
                int cid = SessionManager.CompanyProfileId; using var db = new AppDbContext();
                for (int li = 1; li < lines.Length; li++)
                {
                    if (string.IsNullOrWhiteSpace(lines[li])) continue; string[] f = CsvSplit(lines[li]);
                    string clNm = Get(f, iCl), pD = Get(f, iP), oD = Get(f, iO);
                    if (string.IsNullOrWhiteSpace(pD)) { skipped++; log.AppendLine($"Row {li + 1}: empty party design — skipped."); continue; }
                    var acc2 = _accounts.FirstOrDefault(a => a.AccNm.Equals(clNm, StringComparison.OrdinalIgnoreCase));
                    var des = _designs.FirstOrDefault(d => d.DesignName.Equals(oD, StringComparison.OrdinalIgnoreCase));
                    if (acc2 == null) { skipped++; log.AppendLine($"Row {li + 1}: client '{clNm}' not found — skipped."); continue; }
                    if (des == null) { skipped++; log.AppendLine($"Row {li + 1}: design '{oD}' not found — skipped."); continue; }
                    if (db.PartyDesignAliases.Any(a => a.CompanyProfileId == cid && a.AccountId == acc2.AccountId && a.PartyDesignName == pD)) { skipped++; log.AppendLine($"Row {li + 1}: alias '{pD}' already exists — skipped."); continue; }
                    db.PartyDesignAliases.Add(new PartyDesignAlias { CompanyProfileId = cid, AccountId = acc2.AccountId, PartyDesignName = pD, DesignId = des.DesignId, Notes = Get(f, iN) }); added++;
                }
                db.SaveChanges(); string msg = $"Import complete.\n\n✅ Added : {added}\n⏭ Skipped : {skipped}"; if (log.Length > 0) msg += "\n\n" + log;
                MessageBox.Show(msg, "Import Result", MessageBoxButtons.OK, MessageBoxIcon.Information); _aliasPage = 1; LoadAliasGrid(txtAliasSearch?.Text ?? "");
            }
            catch (Exception ex) { MessageBox.Show("Import error: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════
        //  TAB 3 — DAILY REPORT
        // ════════════════════════════════════════════════════════════
        void BuildReportTab()
        {
            pnlReport = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Visible = false };
            var bar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Surface };
            bar.Paint += (s, e) => { using var p = new Pen(Border); e.Graphics.DrawLine(p, 0, bar.Height - 1, bar.Width, bar.Height - 1); };
            bar.Controls.Add(new Label { Text = "Report Date:", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = TextMid, AutoSize = true, Location = new Point(16, 18) });
            dtpReportDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 9.5F), Size = new Size(140, 28), Location = new Point(110, 14), Value = DateTime.Today };
            dtpReportDate.ValueChanged += (s, e) => LoadReport(dtpReportDate.Value.Date);
            bar.Controls.Add(dtpReportDate);
            var btnToday = MkBtn("📅 Today", Blue, Color.White, BlueHov); btnToday.Size = new Size(90, 28); btnToday.Location = new Point(260, 14);
            btnToday.Click += (s, e) => { dtpReportDate.Value = DateTime.Today; LoadReport(DateTime.Today); }; bar.Controls.Add(btnToday);
            var btnPrint = MkBtn("🖨 Print Report", Color.FromArgb(71, 85, 105), Color.White, Color.FromArgb(51, 65, 85)); btnPrint.Size = new Size(120, 28); btnPrint.Location = new Point(360, 14);
            btnPrint.Click += PrintDailyReport; bar.Controls.Add(btnPrint);
            var btnExport = MkBtn("⬇ Export Excel", Green, Color.White, GreenHov); btnExport.Size = new Size(120, 28);
            btnExport.Click += ReportExportExcel; bar.Controls.Add(btnExport);
            bar.Resize += (s, e) => btnExport.Location = new Point(bar.Width - 136, 14);
            pnlReport.Controls.Add(bar);
            lblReportSummary = new Label { Text = "", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = TextMid, AutoSize = true, Location = new Point(16, 65) };
            pnlReport.Controls.Add(lblReportSummary);
            var card = new Panel { BackColor = Surface, Location = new Point(16, 88), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Green });
            dgvReport = BuildDgv(); dgvReport.Dock = DockStyle.Fill; dgvReport.ReadOnly = true;
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "WO NO", Name = "RWoNo", MinimumWidth = 110, FillWeight = 12 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CLIENT", Name = "RClient", MinimumWidth = 160, FillWeight = 20 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CHALLAN NO", Name = "RChallan", MinimumWidth = 110, FillWeight = 12 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CLIENT DESIGN", Name = "RParty", MinimumWidth = 140, FillWeight = 16 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "OUR DESIGN", Name = "ROurDesign", MinimumWidth = 140, FillWeight = 16 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "QTY", Name = "RQty", MinimumWidth = 80, FillWeight = 9 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "UNIT", Name = "RUnit", MinimumWidth = 70, FillWeight = 7 });
            dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PCS", Name = "RPcs", MinimumWidth = 60, FillWeight = 6 });
            dgvReport.CellFormatting += (s, e) => { if (e.RowIndex < 0) return; if (e.ColumnIndex == dgvReport.Columns["ROurDesign"]?.Index) { e.CellStyle.ForeColor = Green; e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold); e.FormattingApplied = true; } if (e.ColumnIndex == dgvReport.Columns["RQty"]?.Index) { e.CellStyle.ForeColor = Blue; e.CellStyle.Font = new Font("Consolas", 9F, FontStyle.Bold); e.FormattingApplied = true; } };
            card.Controls.Add(dgvReport); pnlReport.Controls.Add(card);
            pnlReport.Resize += (s, e) => { card.Location = new Point(16, 92); card.Size = new Size(pnlReport.Width - 32, pnlReport.Height - 100); };
            this.Controls.Add(pnlReport); LoadReport(DateTime.Today);
        }

        void LoadReport(DateTime date)
        {
            dgvReport.Rows.Clear();
            try
            {
                using var db = new AppDbContext(); int cid = SessionManager.CompanyProfileId;
                var dateOnly = DateOnly.FromDateTime(date);
                var headers = db.WorkOrderHeaders.Where(w => w.CompanyProfileId == cid && w.WoDate == dateOnly).ToList();
                if (headers.Count == 0) { lblReportSummary.Text = $"No work orders for {date:dd MMM yyyy}."; return; }
                var hdrIds = headers.Select(h => h.WoId).ToList();
                var items = db.WorkOrderItems.Where(i => hdrIds.Contains(i.WoId)).ToList();
                var accLookup = _accounts.ToDictionary(a => a.AccountId, a => a.AccNm);
                var dLookup = _designs.ToDictionary(d => d.DesignId, d => d.DesignName);
                int totalItems = 0; decimal totalQty = 0;
                foreach (var it in items)
                {
                    var hdr = headers.First(h => h.WoId == it.WoId);
                    string cl = hdr.AccountId.HasValue && accLookup.TryGetValue(hdr.AccountId.Value, out var nm) ? nm : "—";
                    string od = it.DesignId.HasValue && dLookup.TryGetValue(it.DesignId.Value, out var dn) ? dn : "⚠ Not Mapped";
                    dgvReport.Rows.Add(hdr.WoNo, cl, hdr.ChallanNo ?? "—", it.PartyDesignName ?? "—", od, (it.Qty ?? 0).ToString("0.00"), it.Unit ?? "MTR", it.Pcs ?? 0);
                    totalItems++; totalQty += it.Qty ?? 0;
                }
                lblReportSummary.Text = $"📅 {date:dd MMM yyyy}  ·  {headers.Count} Work Order(s)  ·  {totalItems} Items  ·  Total Qty: {totalQty:0.00}";
            }
            catch (Exception ex) { MessageBox.Show("Report error: " + ex.Message); }
        }

        void PrintDailyReport(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0) { MessageBox.Show("No data to print."); return; }
            DateTime date = dtpReportDate.Value.Date;
            var pd = new PrintDocument();
            pd.PrintPage += (s2, e2) => {
                var g = e2.Graphics; float x = 40, y = 30, pw = e2.PageBounds.Width - 80;
                using var boldFont = new Font("Segoe UI", 12F, FontStyle.Bold); using var normFont = new Font("Segoe UI", 8.5F); using var smFont = new Font("Segoe UI", 7.5F);
                using var darkBr = new SolidBrush(Color.FromArgb(15, 23, 42)); using var midBr = new SolidBrush(Color.FromArgb(71, 85, 105)); using var greenBr = new SolidBrush(Color.FromArgb(22, 163, 74)); using var borderPen = new Pen(Color.FromArgb(226, 232, 240), 1);
                g.FillRectangle(new SolidBrush(Color.FromArgb(22, 163, 74)), x - 10, y - 10, pw + 20, 5); y += 3;
                g.DrawString(SessionManager.CompanyName, boldFont, darkBr, x, y); g.DrawString($"DAILY WORK ORDER REPORT — {date:dd MMMM yyyy}", boldFont, greenBr, x, y + 18); y += 40;
                g.DrawLine(borderPen, x - 10, y, x + pw + 10, y); y += 8;
                float[] cw = { 70, 110, 80, 100, 110, 55, 40, 35 }; string[] ch = { "WO NO", "CLIENT", "CHALLAN", "CLIENT DESIGN", "OUR DESIGN", "QTY", "UNIT", "PCS" };
                g.FillRectangle(new SolidBrush(Color.FromArgb(241, 245, 249)), x - 4, y, pw + 8, 20); float cx = x;
                for (int i2 = 0; i2 < ch.Length; i2++) { g.DrawString(ch[i2], smFont, midBr, cx + 2, y + 4); cx += cw[i2]; }
                y += 22; g.DrawLine(borderPen, x - 10, y, x + pw + 10, y); y += 2;
                foreach (DataGridViewRow row in dgvReport.Rows) { cx = x; string[] vals = { row.Cells["RWoNo"].Value?.ToString(), row.Cells["RClient"].Value?.ToString(), row.Cells["RChallan"].Value?.ToString(), row.Cells["RParty"].Value?.ToString(), row.Cells["ROurDesign"].Value?.ToString(), row.Cells["RQty"].Value?.ToString(), row.Cells["RUnit"].Value?.ToString(), row.Cells["RPcs"].Value?.ToString() }; for (int i2 = 0; i2 < vals.Length; i2++) { g.DrawString(vals[i2] ?? "", normFont, i2 == 4 ? greenBr : darkBr, cx + 2, y + 1); cx += cw[i2]; } y += 20; g.DrawLine(new Pen(Color.FromArgb(248, 249, 252), 1), x - 4, y, x + pw + 4, y); if (y > e2.PageBounds.Height - 60) { e2.HasMorePages = false; break; } }
                y += 8; g.DrawLine(borderPen, x - 10, y, x + pw + 10, y); y += 12; g.DrawString(lblReportSummary.Text, smFont, midBr, x, y);
            };
            using var dlg = new PrintPreviewDialog { Document = pd, Width = 960, Height = 720 }; dlg.ShowDialog(this.FindForm());
        }

        void ReportExportExcel(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0) { MessageBox.Show("No data to export."); return; }
            using var sfd = new SaveFileDialog { Filter = "Excel Files (*.xlsx)|*.xlsx", FileName = $"WorkOrderReport_{dtpReportDate.Value:yyyyMMdd}.xlsx" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                var tmp = new DataGridView(); foreach (DataGridViewColumn col in dgvReport.Columns) tmp.Columns.Add(col.Name, col.HeaderText);
                foreach (DataGridViewRow row in dgvReport.Rows) { var vals = new object[dgvReport.Columns.Count]; for (int i2 = 0; i2 < dgvReport.Columns.Count; i2++) vals[i2] = row.Cells[i2].Value ?? ""; tmp.Rows.Add(vals); }
                ExcelExportHelper.Export(tmp, sfd.FileName, $"Work Order Report {dtpReportDate.Value:dd-MM-yyyy}"); Toast("Report exported.");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show("Export error: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════
        //  SHARED HELPERS
        // ════════════════════════════════════════════════════════════
        void BindAccountCombo(ComboBox c, string placeholder)
        {
            c.Items.Clear();
            c.Items.Add(new AItem(0, placeholder));
            foreach (var a in _accounts) c.Items.Add(new AItem(a.AccountId, a.AccNm));
            c.DisplayMember = "Display"; c.ValueMember = "Id";
            c.SelectedIndex = 0;
        }

        void SelectAItem(ComboBox c, int id) { foreach (var it in c.Items) if (it is AItem a && a.Id == id) { c.SelectedItem = it; return; } }
        void SelectDItem(ComboBox c, int id) { foreach (var it in c.Items) if (it is DItem d && d.Id == id) { c.SelectedItem = it; return; } }

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
            typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(g, true);
            g.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg; g.ColumnHeadersDefaultCellStyle.ForeColor = TextMid;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderBg; g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.ColumnHeadersHeight = 36; g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.BackColor = Surface; g.DefaultCellStyle.ForeColor = TextMid; g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.DefaultCellStyle.SelectionBackColor = BluePale; g.DefaultCellStyle.SelectionForeColor = TextDark; g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.RowTemplate.Height = 34;
            g.RowsAdded += (s, e) => { for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++) if (i >= 0 && i < g.Rows.Count) g.Rows[i].DefaultCellStyle.BackColor = i % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253); };
            g.CellMouseEnter += (s, e) => { if (e.RowIndex < 0) return; int lh = (int)(g.Tag ?? -1); if (e.RowIndex == lh) return; if (lh >= 0 && lh < g.Rows.Count) { g.Rows[lh].DefaultCellStyle.BackColor = lh % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253); g.InvalidateRow(lh); } g.Tag = e.RowIndex; g.Rows[e.RowIndex].DefaultCellStyle.BackColor = BluePale; g.InvalidateRow(e.RowIndex); };
            g.MouseLeave += (s, e) => { int lh = (int)(g.Tag ?? -1); if (lh >= 0 && lh < g.Rows.Count) { g.Rows[lh].DefaultCellStyle.BackColor = lh % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253); g.InvalidateRow(lh); } g.Tag = -1; };
            g.CellMouseMove += (s, e) => { int ei = g.Columns["Edit"]?.Index ?? -1, di = g.Columns["Delete"]?.Index ?? -1, pi = g.Columns["Print"]?.Index ?? -1; g.Cursor = e.RowIndex >= 0 && (e.ColumnIndex == ei || e.ColumnIndex == di || e.ColumnIndex == pi) ? Cursors.Hand : Cursors.Default; };
            g.RowPostPaint += (s, e) => { using var p = new Pen(RowLine, 1); e.Graphics.DrawLine(p, e.RowBounds.Left, e.RowBounds.Bottom - 1, e.RowBounds.Right, e.RowBounds.Bottom - 1); };
            return g;
        }

        void AttachActionPainter(DataGridView g, params (string name, Color bg, Color fg, Color bdr, string lbl)[] cols)
        {
            g.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                foreach (var (name, bg, fg, bdr, lbl) in cols)
                {
                    if (e.ColumnIndex != (g.Columns[name]?.Index ?? -1)) continue;
                    e.Handled = true; e.PaintBackground(e.ClipBounds, true);
                    var rc = new Rectangle(e.CellBounds.X + 7, e.CellBounds.Y + 6, e.CellBounds.Width - 14, e.CellBounds.Height - 12);
                    var prev = e.Graphics.SmoothingMode; e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RRect(rc, 5)) using (var fb = new SolidBrush(bg)) using (var bp = new Pen(bdr, 1f)) { e.Graphics.FillPath(fb, path); e.Graphics.DrawPath(bp, path); }
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) using (var fb = new SolidBrush(fg)) using (var fn = new Font("Segoe UI", 8F, FontStyle.Bold)) e.Graphics.DrawString(lbl, fn, fb, rc, sf);
                    e.Graphics.SmoothingMode = prev; break;
                }
            };
        }

        TextBox FF(Panel p, string lbl, int lx, int fx, int fw, int y, bool up = false)
        {
            p.Controls.Add(FLbl(lbl, new Point(lx, y + 5)));
            var t = new TextBox { BackColor = Surface2, ForeColor = TextDark, Font = new Font("Segoe UI", 9.5F), BorderStyle = BorderStyle.FixedSingle, Location = new Point(fx, y), Size = new Size(fw, 28), CharacterCasing = up ? CharacterCasing.Upper : CharacterCasing.Normal };
            p.Controls.Add(t); return t;
        }

        Label FLbl(string t, Point p) => new Label { Text = t, ForeColor = TextMid, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = true, Location = p };

        Button MkBtn(string t, Color bg, Color fg, Color hover, bool border = false)
        {
            var b = new Button { Text = t, BackColor = bg, ForeColor = fg, Font = new Font("Segoe UI Emoji", 9F, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            b.FlatAppearance.BorderSize = border ? 1 : 0; b.FlatAppearance.BorderColor = Border; b.FlatAppearance.MouseOverBackColor = hover; return b;
        }

        ComboBox MkCmb(int x, int y, int w) => new ComboBox { Font = new Font("Segoe UI", 9.5F), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = TextDark, Size = new Size(w, 28), Location = new Point(x, y) };

        void Toast(string msg)
        {
            var f = new Form { Size = new Size(280, 46), FormBorderStyle = FormBorderStyle.None, BackColor = Green, TopMost = true, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual };
            f.Controls.Add(new Label { Text = "✓  " + msg, ForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
            var par = this.FindForm(); if (par != null) f.Location = new Point(par.Right - 300, par.Bottom - 60);
            f.Show(); var tm = new System.Windows.Forms.Timer { Interval = 2200 }; tm.Tick += (s, e) => { tm.Stop(); f.Close(); }; tm.Start();
        }

        static decimal DecParse(object v) { if (v == null || v == DBNull.Value) return 0; return decimal.TryParse(v.ToString(), out decimal d) ? d : 0; }
        static decimal DecParse(string s) => decimal.TryParse(s, out decimal d) ? d : 0;
        static int IntParse(object v) { if (v == null || v == DBNull.Value) return 0; return int.TryParse(v.ToString(), out int i) ? i : 0; }
        static int IntParse(string s) => int.TryParse(s, out int i) ? i : 0;
        static string Get(string[] f, int idx) => (idx >= 0 && idx < f.Length) ? f[idx].Trim().Trim('"') : "";

        static string[] CsvSplit(string line)
        {
            var r = new List<string>(); bool inQ = false; var cur = new StringBuilder();
            foreach (char c in line) { if (c == '"') inQ = !inQ; else if (c == ',' && !inQ) { r.Add(cur.ToString()); cur.Clear(); } else cur.Append(c); }
            r.Add(cur.ToString()); return r.ToArray();
        }

        static GraphicsPath RRect(Rectangle r, int rad)
        {
            int d = rad * 2; var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }

        class AItem { public int Id { get; } public string Display { get; } public AItem(int id, string d) { Id = id; Display = d; } public override string ToString() => Display; }
        class DItem { public int Id { get; } public string Display { get; } public DItem(int id, string d) { Id = id; Display = d; } public override string ToString() => Display; }
    }
}