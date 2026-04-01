using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App
{
    public partial class UC_AccountMaster : UserControl
    {
        // ── Palette ───────────────────────────────────────────────────
        static readonly Color Bg = Color.FromArgb(245, 247, 250);
        static readonly Color Surface = Color.White;
        static readonly Color Surface2 = Color.FromArgb(248, 249, 252);
        static readonly Color HeaderBg = Color.FromArgb(241, 245, 249);
        static readonly Color Blue = Color.FromArgb(37, 99, 235);
        static readonly Color BlueHov = Color.FromArgb(29, 78, 216);
        static readonly Color BluePale = Color.FromArgb(239, 246, 255);
        static readonly Color BlueBdr = Color.FromArgb(191, 219, 254);
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

        // ── Controls ──────────────────────────────────────────────────
        Panel pnlGrid, pnlForm;
        DataGridView dgv;

        // Toolbar search
        TextBox txtSearch;

        // Form fields – Account Info
        TextBox txtAccCode, txtAccNm, txtGroupNm, txtGstin, txtPan;
        ComboBox cmbType;

        // Form fields – Billing Address
        TextBox txtAdd1, txtAdd2, txtAdd3, txtCity, txtDistrict, txtState, txtPincode;

        // Form fields – Shipping Address
        TextBox txtShipAdd;

        // Form fields – Contact
        TextBox txtPhone1, txtPhone2, txtEmail;

        Label lblFormTitle;
        int _editId = -1, _lastHover = -1;

        // ── Pagination ────────────────────────────────────────────────
        const int PAGE_SIZE = 20;
        int _currentPage = 1, _totalPages = 1;
        Label _lblPageInfo;
        Button _btnPrev, _btnNext;
        System.Collections.Generic.List<Account> _allRows =
            new System.Collections.Generic.List<Account>();

        // ─────────────────────────────────────────────────────────────
        public UC_AccountMaster()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Bg;
            BuildGrid();
            BuildForm();
            ShowGrid();
        }

        // ════════════════════════════════════════════════════════════
        //  GRID PANEL
        // ════════════════════════════════════════════════════════════
        void BuildGrid()
        {
            pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Bg };

            // ── Page header ───────────────────────────────────────────
            var top = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            top.Controls.Add(new Label
            {
                Text = "Account Master",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            });
            top.Controls.Add(new Label
            {
                Text = "Manage clients, brokers and transport accounts",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });

            var btnAdd = MkBtn("➕  Add Account", Blue, Color.White, BlueHov);
            btnAdd.Size = new Size(148, 34);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Location = new Point(top.Width - 172, 17);
            btnAdd.Click += (s, e) => ShowForm(null);
            top.Resize += (s, e) => btnAdd.Location = new Point(top.Width - 172, 17);
            top.Controls.Add(btnAdd);

            // ── Card ──────────────────────────────────────────────────
            var card = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });

            // ── Toolbar ───────────────────────────────────────────────
            var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Surface2 };
            bar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border), 0, bar.Height - 1, bar.Width, bar.Height - 1);

            var srch = new Panel { Location = new Point(16, 10), Size = new Size(300, 32), BackColor = Color.White };
            srch.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Border), 0, 0, srch.Width - 1, srch.Height - 1);
                e.Graphics.DrawString("🔍", new Font("Segoe UI Emoji", 9F),
                    new SolidBrush(TextLight), new PointF(5, 7));
            };
            txtSearch = new TextBox
            {
                PlaceholderText = "Search by name, code, city or type…",
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = TextDark,
                Location = new Point(27, 7),
                Size = new Size(265, 20)
            };
            txtSearch.TextChanged += (s, e) => { _currentPage = 1; LoadGrid(txtSearch.Text); };
            srch.Controls.Add(txtSearch);
            bar.Controls.Add(srch);

            // Import / Export / Sample buttons
            var btnSample = MkBtn("📥 Sample CSV", Teal, Color.White, TealHov); btnSample.Size = new Size(128, 30);
            var btnImport = MkBtn("⬆  Import CSV", Violet, Color.White, VioletHov); btnImport.Size = new Size(118, 30);
            var btnExport = MkBtn("⬇  Export Excel", Green, Color.White, GreenHov); btnExport.Size = new Size(132, 30);

            btnSample.Click += DownloadSampleCsv;
            btnImport.Click += ImportFromCsv;
            btnExport.Click += ExportToExcel;

            bar.Controls.Add(btnSample);
            bar.Controls.Add(btnImport);
            bar.Controls.Add(btnExport);
            bar.Resize += (s, e) =>
            {
                int r = bar.Width - 12;
                btnExport.Location = new Point(r - btnExport.Width, 11);
                btnImport.Location = new Point(r - btnExport.Width - btnImport.Width - 6, 11);
                btnSample.Location = new Point(r - btnExport.Width - btnImport.Width - btnSample.Width - 12, 11);
            };

            // ── Pagination bar ────────────────────────────────────────
            var pager = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Surface2 };
            pager.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border), 0, 0, pager.Width, 0);

            _btnPrev = MkBtn("◀  Prev", Surface, TextMid, Color.FromArgb(241, 245, 249), border: true);
            _btnPrev.Size = new Size(82, 30); _btnPrev.Location = new Point(12, 7);
            _btnPrev.Click += (s, e) => { if (_currentPage > 1) { _currentPage--; RenderPage(); } };

            _btnNext = MkBtn("Next  ▶", Surface, TextMid, Color.FromArgb(241, 245, 249), border: true);
            _btnNext.Size = new Size(82, 30);
            _btnNext.Click += (s, e) => { if (_currentPage < _totalPages) { _currentPage++; RenderPage(); } };

            _lblPageInfo = new Label
            {
                Text = "Page 1 of 1  (0 records)",
                ForeColor = TextMid,
                Font = new Font("Segoe UI", 9F),
                AutoSize = true
            };

            pager.Controls.Add(_btnPrev);
            pager.Controls.Add(_lblPageInfo);
            pager.Controls.Add(_btnNext);
            pager.Resize += (s, e) =>
            {
                _lblPageInfo.Location = new Point(
                    (pager.Width - _lblPageInfo.Width) / 2,
                    (pager.Height - _lblPageInfo.Height) / 2);
                _btnNext.Location = new Point(pager.Width - _btnNext.Width - 12, 7);
            };

            // ── DataGridView ──────────────────────────────────────────
            // Visible columns: CODE | NAME | TYPE | CITY | DISTRICT | PHONE | GSTIN | Edit | Delete
            dgv = BuildDgv();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CODE", Name = "Code", MinimumWidth = 80, FillWeight = 7 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "NAME", Name = "Name", MinimumWidth = 180, FillWeight = 22 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "GROUP", Name = "Group", MinimumWidth = 100, FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TYPE", Name = "Type", MinimumWidth = 80, FillWeight = 8 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CITY", Name = "City", MinimumWidth = 90, FillWeight = 9 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DISTRICT", Name = "District", MinimumWidth = 90, FillWeight = 9 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PHONE", Name = "Phone", MinimumWidth = 110, FillWeight = 11 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "GSTIN", Name = "Gstin", MinimumWidth = 150, FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "",
                Name = "Edit",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 70
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "",
                Name = "Delete",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 76
            });

            AttachActionPainter(dgv,
                ("Edit", AmberPale, Amber, AmberBdr, "✏  Edit"),
                ("Delete", RedPale, Red, RedBdr, "🗑 Delete"));

            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int ei = dgv.Columns["Edit"]?.Index ?? -1,
                    di = dgv.Columns["Delete"]?.Index ?? -1;
                if (e.ColumnIndex == ei || e.ColumnIndex == di)
                { e.Value = ""; e.FormattingApplied = true; }
            };
            dgv.CellClick += DgvClick;

            card.Controls.Add(dgv);
            card.Controls.Add(pager);
            card.Controls.Add(bar);
            pnlGrid.Controls.Add(card);
            pnlGrid.Controls.Add(top);
            this.Controls.Add(pnlGrid);
            LoadGrid("");
        }

        // ── Load / filter / paginate ──────────────────────────────────
        void LoadGrid(string q)
        {
            _lastHover = -1;
            try
            {
                using var db = new AppDbContext();
                var list = db.Accounts
                    .Where(a => a.CompanyProfileId == SessionManager.CompanyProfileId)
                    .ToList();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.ToLower();
                    list = list.Where(a =>
                        (a.AccNm ?? "").ToLower().Contains(q) ||
                        (a.AccCode ?? "").ToLower().Contains(q) ||
                        (a.BillCity ?? "").ToLower().Contains(q) ||
                        (a.BillDistrict ?? "").ToLower().Contains(q) ||
                        (a.Type ?? "").ToLower().Contains(q) ||
                        (a.Gstin ?? "").ToLower().Contains(q)).ToList();
                }

                _allRows = list;
                _totalPages = Math.Max(1, (int)Math.Ceiling(_allRows.Count / (double)PAGE_SIZE));
                if (_currentPage > _totalPages) _currentPage = _totalPages;
                RenderPage();
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        void RenderPage()
        {
            dgv.Rows.Clear();
            foreach (var a in _allRows.Skip((_currentPage - 1) * PAGE_SIZE).Take(PAGE_SIZE))
                dgv.Rows.Add(
                    a.AccountId, a.AccCode, a.AccNm, a.GroupNm,
                    a.Type, a.BillCity, a.BillDistrict, a.BillPhone1, a.Gstin);

            _lblPageInfo.Text = $"Page {_currentPage} of {_totalPages}  ({_allRows.Count} records)";
            _btnPrev.Enabled = _currentPage > 1;
            _btnNext.Enabled = _currentPage < _totalPages;

            if (_lblPageInfo.Parent != null)
                _lblPageInfo.Location = new Point(
                    (_lblPageInfo.Parent.Width - _lblPageInfo.Width) / 2,
                    (_lblPageInfo.Parent.Height - _lblPageInfo.Height) / 2);
        }

        void DgvClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == dgv.Columns["Edit"]?.Index)
            {
                ShowForm(dgv.Rows[e.RowIndex]);
                return;
            }

            if (e.ColumnIndex == dgv.Columns["Delete"]?.Index)
            {
                string name = dgv.Rows[e.RowIndex].Cells["Name"].Value?.ToString() ?? "";
                if (MessageBox.Show($"Delete \"{name}\"?", "Confirm",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                try
                {
                    int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["Id"].Value);
                    using var db = new AppDbContext();

                    bool usedAsClient = db.InvoiceHeaders.Any(h => h.ClientId == id);
                    bool usedAsBroker = db.InvoiceHeaders.Any(h => h.BrokerId == id);
                    bool usedAsTransport = db.InvoiceHeaders.Any(h => h.TransportId == id);

                    if (usedAsClient || usedAsBroker || usedAsTransport)
                    {
                        string role = usedAsClient ? "client" : usedAsBroker ? "broker" : "transport";
                        MessageBox.Show(
                            $"Cannot delete \"{name}\" — it is used as a {role} in existing invoices.\n\n" +
                            "Leave the account in the system; it will not affect new invoices.",
                            "Deletion Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var a = db.Accounts.Find(id);
                    if (a != null) { db.Accounts.Remove(a); db.SaveChanges(); }
                    Toast("Account deleted.");
                    LoadGrid(txtSearch?.Text ?? "");
                }
                catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
            }
        }

        // ════════════════════════════════════════════════════════════
        //  EXCEL EXPORT  (all filtered rows)
        // ════════════════════════════════════════════════════════════
        void ExportToExcel(object sender, EventArgs e)
        {
            if (_allRows.Count == 0)
            {
                MessageBox.Show("No records to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Title = "Export Accounts to Excel",
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Accounts_{SessionManager.CompanyName.Replace(" ", "_")}_{DateTime.Today:yyyyMMdd}.xlsx"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var tmp = new DataGridView();
                // ── All DB columns in export ──────────────────────────
                tmp.Columns.Add("AccCode", "ACC CODE");
                tmp.Columns.Add("AccNm", "ACCOUNT NAME");
                tmp.Columns.Add("GroupNm", "GROUP");
                tmp.Columns.Add("Type", "TYPE");
                tmp.Columns.Add("Gstin", "GSTIN");
                tmp.Columns.Add("Pan", "PAN");
                tmp.Columns.Add("Phone1", "PHONE 1");
                tmp.Columns.Add("Phone2", "PHONE 2");
                tmp.Columns.Add("Email", "EMAIL");
                tmp.Columns.Add("Add1", "BILL ADDRESS 1");
                tmp.Columns.Add("Add2", "BILL ADDRESS 2");
                tmp.Columns.Add("Add3", "BILL ADDRESS 3");
                tmp.Columns.Add("City", "BILL CITY");
                tmp.Columns.Add("District", "BILL DISTRICT");
                tmp.Columns.Add("State", "BILL STATE");
                tmp.Columns.Add("Pincode", "BILL PINCODE");
                tmp.Columns.Add("ShipAdd", "SHIP ADDRESS");
                tmp.Columns.Add("BrokerId", "BROKER ID");
                tmp.Columns.Add("TransportId", "TRANSPORT ID");

                foreach (var a in _allRows)
                    tmp.Rows.Add(
                        a.AccCode, a.AccNm, a.GroupNm, a.Type,
                        a.Gstin, a.Pan,
                        a.BillPhone1, a.BillPhone2, a.EmailAdd,
                        a.BillAdd1, a.BillAdd2, a.BillAdd3,
                        a.BillCity, a.BillDistrict, a.BillState, a.BillPincode,
                        a.ShipAdd,
                        a.BrokerId, a.TransportId);

                ExcelExportHelper.Export(tmp, sfd.FileName, "Accounts");
                Toast($"Exported {_allRows.Count} records.");
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show("Export error:\n" + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════
        //  DOWNLOAD SAMPLE CSV
        // ════════════════════════════════════════════════════════════
        void DownloadSampleCsv(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Title = "Save Sample Import CSV",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = "AccountMaster_Sample.csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var sb = new StringBuilder();
                // Header row — must match exactly what ImportFromCsv expects
                sb.AppendLine(
                    "AccCode,AccNm,GroupNm,Type,Gstin,Pan," +
                    "Phone1,Phone2,Email," +
                    "Add1,Add2,Add3,City,District,State,Pincode," +
                    "ShipAdd,BrokerId,TransportId");

                // Sample data rows
                sb.AppendLine("A001,Sunrise Traders,Retail,Customer,24ABCDE1234F1Z5,ABCDE1234F,9876543210,,info@sunrise.com,12 Main Street,,, Surat,,Gujarat,395001,,, ");
                sb.AppendLine("A002,Sharma Fabrics,Wholesale,Customer,27XYZPQ5678G2Z3,,9988776655,,sharma@fabrics.in,45 Mill Road,Dadar,,Mumbai,,Maharashtra,400001,,,");
                sb.AppendLine("B001,Bright Brokers,Brokers,Broker,,BRTBK1234P,9111222333,,broker@bright.com,Commerce Road,Navrangpura,,Ahmedabad,,Gujarat,380001,,,");
                sb.AppendLine("T001,Fast Cargo,,Transport,,,,9911223344,,Gondal Road,,,Rajkot,,Gujarat,360001,,,");
                sb.AppendLine("S001,Global Supplier,,Supplier,29PQRST9999H3Z1,,8000100200,,supply@global.com,MG Road,,,Bangalore,,Karnataka,560001,MG Road Warehouse,,");

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show(
                    "Sample CSV saved.\n\n" +
                    "📌 Instructions:\n" +
                    "  1. Open the CSV in Excel.\n" +
                    "  2. Fill in your accounts (one row per account).\n" +
                    "  3. Keep the header row exactly as-is.\n" +
                    "  4. Save as CSV (UTF-8), then use ⬆ Import CSV to load.\n\n" +
                    "Valid Types : Customer, Supplier, Broker, Transport, Other\n" +
                    "BrokerId / TransportId : leave blank if not applicable.",
                    "Sample Ready ✓", MessageBoxButtons.OK, MessageBoxIcon.Information);

                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show("Error saving sample: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════
        //  IMPORT FROM CSV
        // ════════════════════════════════════════════════════════════
        void ImportFromCsv(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select Account Import CSV",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            int added = 0, skipped = 0;
            var skipLog = new StringBuilder();
            try
            {
                string[] lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                if (lines.Length < 2)
                {
                    MessageBox.Show("The CSV has no data rows.", "Import",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string[] hdr = CsvSplit(lines[0]);
                int Idx(string col)
                {
                    for (int i = 0; i < hdr.Length; i++)
                        if (string.Equals(hdr[i].Trim(), col, StringComparison.OrdinalIgnoreCase)) return i;
                    return -1;
                }

                // Column indices — matches sample CSV header exactly
                int iCode = Idx("AccCode"),
                    iName = Idx("AccNm"),
                    iGroup = Idx("GroupNm"),
                    iType = Idx("Type"),
                    iGstin = Idx("Gstin"),
                    iPan = Idx("Pan"),
                    iPh1 = Idx("Phone1"),
                    iPh2 = Idx("Phone2"),
                    iEmail = Idx("Email"),
                    iAdd1 = Idx("Add1"),
                    iAdd2 = Idx("Add2"),
                    iAdd3 = Idx("Add3"),
                    iCity = Idx("City"),
                    iDistrict = Idx("District"),
                    iState = Idx("State"),
                    iPincode = Idx("Pincode"),
                    iShipAdd = Idx("ShipAdd"),
                    iBrokerId = Idx("BrokerId"),
                    iTransportId = Idx("TransportId");

                if (iName < 0)
                {
                    MessageBox.Show(
                        "Column 'AccNm' not found in CSV header.\n" +
                        "Download the sample CSV to see the required format.",
                        "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string[] validTypes = { "Customer", "Supplier", "Broker", "Transport", "Other" };

                using var db = new AppDbContext();
                for (int li = 1; li < lines.Length; li++)
                {
                    if (string.IsNullOrWhiteSpace(lines[li])) continue;
                    string[] f = CsvSplit(lines[li]);

                    string accNm = Get(f, iName);
                    if (string.IsNullOrWhiteSpace(accNm))
                    {
                        skipped++;
                        skipLog.AppendLine($"Row {li + 1}: empty Account Name — skipped.");
                        continue;
                    }

                    // Duplicate check by name within same company
                    if (db.Accounts.Any(a =>
                            a.CompanyProfileId == SessionManager.CompanyProfileId &&
                            a.AccNm == accNm))
                    {
                        skipped++;
                        skipLog.AppendLine($"Row {li + 1}: \"{accNm}\" already exists — skipped.");
                        continue;
                    }

                    string type = Get(f, iType);
                    if (!validTypes.Contains(type, StringComparer.OrdinalIgnoreCase))
                        type = "Customer";

                    // Parse nullable int FK columns safely
                    int? ParseNullableInt(int idx)
                    {
                        string v = Get(f, idx);
                        return int.TryParse(v, out int n) ? n : (int?)null;
                    }

                    db.Accounts.Add(new Account
                    {
                        CompanyProfileId = SessionManager.CompanyProfileId,
                        AccCode = Get(f, iCode),
                        AccNm = accNm,
                        GroupNm = Get(f, iGroup),
                        Type = type,
                        Gstin = Get(f, iGstin),
                        Pan = Get(f, iPan),
                        BillPhone1 = Get(f, iPh1),
                        BillPhone2 = Get(f, iPh2),
                        EmailAdd = Get(f, iEmail),
                        BillAdd1 = Get(f, iAdd1),
                        BillAdd2 = Get(f, iAdd2),
                        BillAdd3 = Get(f, iAdd3),
                        BillCity = Get(f, iCity),
                        BillDistrict = Get(f, iDistrict),
                        BillState = Get(f, iState),
                        BillPincode = Get(f, iPincode),
                        ShipAdd = Get(f, iShipAdd),
                        BrokerId = ParseNullableInt(iBrokerId),
                        TransportId = ParseNullableInt(iTransportId)
                    });
                    added++;
                }
                db.SaveChanges();

                string msg = $"Import complete.\n\n✅  Added   : {added}\n⏭  Skipped : {skipped}";
                if (skipLog.Length > 0) msg += "\n\nSkip details:\n" + skipLog;
                MessageBox.Show(msg, "Import Result", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _currentPage = 1;
                LoadGrid(txtSearch?.Text ?? "");
            }
            catch (Exception ex) { MessageBox.Show("Import error:\n" + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════
        //  FORM PANEL
        // ════════════════════════════════════════════════════════════
        void BuildForm()
        {
            pnlForm = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Bg,
                Visible = false,
                AutoScroll = true
            };

            // ── Page header ───────────────────────────────────────────
            var top = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            lblFormTitle = new Label
            {
                Text = "Add New Account",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            };
            top.Controls.Add(lblFormTitle);
            top.Controls.Add(new Label
            {
                Text = "Fill in account details below",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });

            int lx = 24, fx = 180, fw = 240, y = 0, gap = 50;
            var card = FormCard(24, 68, 900, 0);

            // ─────────────────────────────────────────────────────────
            // SECTION 1 — Account Info
            // ─────────────────────────────────────────────────────────
            var sec1 = SecHeader("Account Information");
            card.Controls.Add(sec1);
            y = 48;

            // Row 1: AccCode | Type
            txtAccCode = FF(card, "Acc Code", lx, fx, 120, y);
            card.Controls.Add(FLbl("Type", new Point(lx + 200, y + 6)));
            cmbType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Surface2,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(lx + 280, y),
                Size = new Size(160, 28)
            };
            cmbType.Items.AddRange(new object[] { "Customer", "Supplier", "Broker", "Transport", "Other" });
            cmbType.SelectedIndex = 0;
            card.Controls.Add(cmbType);
            y += gap;

            // Row 2: Account Name (full width)
            txtAccNm = FF(card, "Account Name *", lx, fx, fw * 2 + 40, y); y += gap;

            // Row 3: Group | GSTIN
            txtGroupNm = FF(card, "Group Name", lx, fx, fw, y);
            txtGstin = FF(card, "GSTIN", lx + fw + 60, lx + fw + 160, fw, y, up: true);
            y += gap;

            // Row 4: PAN | Phone 1 | Phone 2
            txtPan = FF(card, "PAN", lx, fx, 140, y, up: true);
            txtPhone1 = FF(card, "Phone 1", lx + 210, lx + 290, 140, y);
            txtPhone2 = FF(card, "Phone 2", lx + 430, lx + 510, 140, y);
            y += gap;

            // Row 5: Email
            txtEmail = FF(card, "Email", lx, fx, fw, y);
            y += gap;

            // ─────────────────────────────────────────────────────────
            // SECTION 2 — Billing Address
            // ─────────────────────────────────────────────────────────
            var sec2 = SecHeader("Billing Address");
            sec2.Location = new Point(0, y); sec2.Size = new Size(card.Width, 36); sec2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            card.Controls.Add(sec2);
            y += 44;

            txtAdd1 = FF(card, "Address 1", lx, fx, fw * 2 + 40, y); y += gap;
            txtAdd2 = FF(card, "Address 2", lx, fx, fw * 2 + 40, y); y += gap;
            txtAdd3 = FF(card, "Address 3", lx, fx, fw * 2 + 40, y); y += gap;

            txtCity = FF(card, "City", lx, fx, fw, y);
            txtDistrict = FF(card, "District", lx + fw + 60, lx + fw + 160, fw, y);
            y += gap;

            txtState = FF(card, "State", lx, fx, fw, y);
            txtPincode = FF(card, "Pincode", lx + fw + 60, lx + fw + 160, 120, y);
            y += gap;

            // ─────────────────────────────────────────────────────────
            // SECTION 3 — Shipping Address
            // ─────────────────────────────────────────────────────────
            var sec3 = SecHeader("Shipping Address");
            sec3.Location = new Point(0, y); sec3.Size = new Size(card.Width, 36); sec3.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            card.Controls.Add(sec3);
            y += 44;

            txtShipAdd = FF(card, "Ship Address", lx, fx, fw * 2 + 40, y); y += gap;

            // ─────────────────────────────────────────────────────────
            // Save / Cancel
            // ─────────────────────────────────────────────────────────
            var btnSave = MkBtn("💾  Save", Blue, Color.White, BlueHov);
            var btnCancel = MkBtn("✕  Cancel", Surface, TextMid, Color.FromArgb(241, 245, 249), border: true);
            btnSave.Location = new Point(fx, y); btnSave.Size = new Size(130, 36);
            btnCancel.Location = new Point(fx + 142, y); btnCancel.Size = new Size(100, 36);
            btnSave.Click += Save;
            btnCancel.Click += (s, e) => ShowGrid();
            card.Controls.Add(btnSave);
            card.Controls.Add(btnCancel);
            card.Height = y + 60;
            card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            pnlForm.Controls.Add(card);
            pnlForm.Controls.Add(top);
            this.Controls.Add(pnlForm);
        }

        void ShowGrid()
        {
            pnlForm.Visible = false;
            pnlGrid.Visible = true;
            _editId = -1;
            LoadGrid(txtSearch?.Text ?? "");
        }

        void ShowForm(DataGridViewRow row)
        {
            ClearForm();
            _editId = -1;
            lblFormTitle.Text = "Add New Account";

            if (row != null)
            {
                _editId = Convert.ToInt32(row.Cells["Id"].Value);
                lblFormTitle.Text = "Edit Account";
                try
                {
                    using var db = new AppDbContext();
                    var a = db.Accounts.Find(_editId);
                    if (a == null) return;

                    txtAccCode.Text = a.AccCode ?? "";
                    txtAccNm.Text = a.AccNm ?? "";
                    txtGroupNm.Text = a.GroupNm ?? "";
                    txtGstin.Text = a.Gstin ?? "";
                    txtPan.Text = a.Pan ?? "";
                    txtPhone1.Text = a.BillPhone1 ?? "";
                    txtPhone2.Text = a.BillPhone2 ?? "";
                    txtEmail.Text = a.EmailAdd ?? "";
                    txtAdd1.Text = a.BillAdd1 ?? "";
                    txtAdd2.Text = a.BillAdd2 ?? "";
                    txtAdd3.Text = a.BillAdd3 ?? "";
                    txtCity.Text = a.BillCity ?? "";
                    txtDistrict.Text = a.BillDistrict ?? "";
                    txtState.Text = a.BillState ?? "";
                    txtPincode.Text = a.BillPincode ?? "";
                    txtShipAdd.Text = a.ShipAdd ?? "";

                    int ti = cmbType.Items.IndexOf(a.Type ?? "");
                    if (ti >= 0) cmbType.SelectedIndex = ti;
                }
                catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
            }

            pnlGrid.Visible = false;
            pnlForm.Visible = true;
            pnlForm.ScrollControlIntoView(lblFormTitle);
        }

        void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAccNm.Text))
            {
                MessageBox.Show("Account Name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // GSTIN format validation
            string gstin = txtGstin.Text.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(gstin))
            {
                var rx = new System.Text.RegularExpressions.Regex(
                    @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$");
                if (!rx.IsMatch(gstin))
                {
                    var res = MessageBox.Show(
                        $"GSTIN \"{gstin}\" does not appear valid.\nExpected format: 22AAAAA0000A1Z5\n\nSave anyway?",
                        "GSTIN Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
                    if (res != DialogResult.Yes) { txtGstin.Focus(); return; }
                }
            }

            try
            {
                using var db = new AppDbContext();

                if (_editId == -1)
                {
                    db.Accounts.Add(BuildEntity(new Account
                    {
                        CompanyProfileId = SessionManager.CompanyProfileId
                    }));
                    Toast("Account added.");
                }
                else
                {
                    var a = db.Accounts.Find(_editId);
                    if (a == null) return;
                    BuildEntity(a);
                    Toast("Account updated.");
                }

                db.SaveChanges();
                ShowGrid();
            }
            catch (Exception ex) { MessageBox.Show("Save error: " + ex.Message); }
        }

        // Populate (or update) an Account entity from form fields
        Account BuildEntity(Account a)
        {
            a.AccCode = txtAccCode.Text.Trim();
            a.AccNm = txtAccNm.Text.Trim();
            a.GroupNm = txtGroupNm.Text.Trim();
            a.Type = cmbType.Text;
            a.Gstin = txtGstin.Text.Trim().ToUpper();
            a.Pan = txtPan.Text.Trim().ToUpper();
            a.BillPhone1 = txtPhone1.Text.Trim();
            a.BillPhone2 = txtPhone2.Text.Trim();
            a.EmailAdd = txtEmail.Text.Trim();
            a.BillAdd1 = txtAdd1.Text.Trim();
            a.BillAdd2 = txtAdd2.Text.Trim();
            a.BillAdd3 = txtAdd3.Text.Trim();
            a.BillCity = txtCity.Text.Trim();
            a.BillDistrict = txtDistrict.Text.Trim();
            a.BillState = txtState.Text.Trim();
            a.BillPincode = txtPincode.Text.Trim();
            a.ShipAdd = txtShipAdd.Text.Trim();
            // BrokerId / TransportId are managed via InvoiceHeader — not editable here
            return a;
        }

        void ClearForm()
        {
            txtAccCode.Text = txtAccNm.Text = txtGroupNm.Text = txtGstin.Text =
            txtPan.Text = txtPhone1.Text = txtPhone2.Text = txtEmail.Text =
            txtAdd1.Text = txtAdd2.Text = txtAdd3.Text =
            txtCity.Text = txtDistrict.Text = txtState.Text = txtPincode.Text =
            txtShipAdd.Text = "";
            cmbType.SelectedIndex = 0;
        }

        // ════════════════════════════════════════════════════════════
        //  HELPERS — DataGridView
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
                EnableHeadersVisualStyles = false,
                ScrollBars = ScrollBars.Both
            };
            typeof(DataGridView)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(g, true);

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

            g.RowsAdded += (s, e) =>
            {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                    if (i >= 0 && i < g.Rows.Count)
                        g.Rows[i].DefaultCellStyle.BackColor =
                            i % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253);
            };
            g.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex == _lastHover) return;
                ResetHover(g); _lastHover = e.RowIndex;
                g.Rows[e.RowIndex].DefaultCellStyle.BackColor = BluePale;
                g.InvalidateRow(e.RowIndex);
            };
            g.MouseLeave += (s, e) => ResetHover(g);
            g.CellMouseMove += (s, e) =>
            {
                int ei = g.Columns["Edit"]?.Index ?? -1,
                    di = g.Columns["Delete"]?.Index ?? -1;
                g.Cursor = e.RowIndex >= 0 && (e.ColumnIndex == ei || e.ColumnIndex == di)
                    ? Cursors.Hand : Cursors.Default;
            };
            g.RowPostPaint += (s, e) =>
            {
                using var p = new Pen(RowLine, 1);
                e.Graphics.DrawLine(p,
                    e.RowBounds.Left, e.RowBounds.Bottom - 1,
                    e.RowBounds.Right, e.RowBounds.Bottom - 1);
            };
            return g;
        }

        void ResetHover(DataGridView g)
        {
            if (_lastHover >= 0 && _lastHover < g.Rows.Count)
            {
                g.Rows[_lastHover].DefaultCellStyle.BackColor =
                    _lastHover % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253);
                g.InvalidateRow(_lastHover);
                _lastHover = -1;
            }
        }

        void AttachActionPainter(DataGridView g,
            params (string name, Color bg, Color fg, Color bdr, string lbl)[] cols)
        {
            g.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                foreach (var (name, bg, fg, bdr, lbl) in cols)
                {
                    if (e.ColumnIndex != (g.Columns[name]?.Index ?? -1)) continue;
                    e.Handled = true;
                    e.PaintBackground(e.ClipBounds, true);
                    var rc = new Rectangle(
                        e.CellBounds.X + 7, e.CellBounds.Y + 6,
                        e.CellBounds.Width - 14, e.CellBounds.Height - 12);
                    var gr = e.Graphics;
                    gr.SmoothingMode = SmoothingMode.AntiAlias;
                    using var path = RRect(rc, 5);
                    gr.FillPath(new SolidBrush(bg), path);
                    gr.DrawPath(new Pen(bdr, 1f), path);
                    using var sf = new StringFormat
                    { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    gr.DrawString(lbl, new Font("Segoe UI", 8F, FontStyle.Bold), new SolidBrush(fg), rc, sf);
                    break;
                }
            };
        }

        // ════════════════════════════════════════════════════════════
        //  HELPERS — UI building
        // ════════════════════════════════════════════════════════════
        Panel FormCard(int x, int y, int w, int h)
        {
            var c = new Panel
            {
                BackColor = Surface,
                Location = new Point(x, y),
                Size = new Size(w, h),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            c.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });
            return c;
        }

        // Creates a coloured section divider panel (no Dock — placed by caller)
        Panel SecHeader(string title)
        {
            var sec = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Surface2
            };
            sec.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 10)
            });
            sec.Paint += (s, e) =>
            {
                using var p = new Pen(Border);
                e.Graphics.DrawLine(p, 0, sec.Height - 1, sec.Width, sec.Height - 1);
            };
            return sec;
        }

        TextBox FF(Panel p, string lbl, int lx, int fx, int fw, int y, bool up = false)
        {
            p.Controls.Add(FLbl(lbl, new Point(lx, y + 5)));
            var t = new TextBox
            {
                BackColor = Surface2,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(fx, y),
                Size = new Size(fw, 28),
                CharacterCasing = up ? CharacterCasing.Upper : CharacterCasing.Normal
            };
            p.Controls.Add(t);
            return t;
        }

        Label FLbl(string t, Point p) =>
            new Label
            {
                Text = t,
                ForeColor = TextMid,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                AutoSize = true,
                Location = p
            };

        Button MkBtn(string t, Color bg, Color fg, Color hover, bool border = false)
        {
            var b = new Button
            {
                Text = t,
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Segoe UI Emoji", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = border ? 1 : 0;
            b.FlatAppearance.BorderColor = Border;
            b.FlatAppearance.MouseOverBackColor = hover;
            return b;
        }

        void Toast(string msg)
        {
            var f = new Form
            {
                Size = new Size(280, 46),
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Green,
                TopMost = true,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual
            };
            f.Controls.Add(new Label
            {
                Text = "✓  " + msg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            var par = this.FindForm();
            if (par != null) f.Location = new Point(par.Right - 300, par.Bottom - 60);
            f.Show();
            var tm = new System.Windows.Forms.Timer { Interval = 2200 };
            tm.Tick += (s, e) => { tm.Stop(); f.Close(); };
            tm.Start();
        }

        // ════════════════════════════════════════════════════════════
        //  HELPERS — CSV
        // ════════════════════════════════════════════════════════════
        static string Get(string[] f, int idx) =>
            (idx >= 0 && idx < f.Length) ? f[idx].Trim().Trim('"') : "";

        static string[] CsvSplit(string line)
        {
            var result = new System.Collections.Generic.List<string>();
            bool inQ = false;
            var cur = new StringBuilder();
            foreach (char c in line)
            {
                if (c == '"') inQ = !inQ;
                else if (c == ',' && !inQ) { result.Add(cur.ToString()); cur.Clear(); }
                else cur.Append(c);
            }
            result.Add(cur.ToString());
            return result.ToArray();
        }

        // ════════════════════════════════════════════════════════════
        //  HELPERS — Graphics
        // ════════════════════════════════════════════════════════════
        static GraphicsPath RRect(Rectangle r, int rad)
        {
            int d = rad * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}