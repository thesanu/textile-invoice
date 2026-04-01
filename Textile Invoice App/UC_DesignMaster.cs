using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App
{
    public partial class UC_DesignMaster : UserControl
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

        // ── Controls ──────────────────────────────────────────────────
        Panel pnlGrid, pnlForm;
        DataGridView dgv;
        TextBox txtSearch, txtDesignName, txtHsnCode, txtRate;
        ComboBox cmbUnit;
        Label lblFormTitle;

        // Image controls
        PictureBox picPreview;
        Button btnPickImage, btnClearImage;
        byte[]? _pendingImageBytes = null;

        int _editId = -1, _lastHover = -1;

        // ── Pagination ────────────────────────────────────────────────
        const int PAGE_SIZE = 20;
        const int THUMB_SIZE = 48;
        int _currentPage = 1, _totalPages = 1;
        Label _lblPageInfo;
        Button _btnPrev, _btnNext;
        System.Collections.Generic.List<DesignMaster> _allRows =
            new System.Collections.Generic.List<DesignMaster>();

        // ─────────────────────────────────────────────────────────────
        public UC_DesignMaster()
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

            var top = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            top.Controls.Add(new Label
            {
                Text = "Design Master",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            });
            top.Controls.Add(new Label
            {
                Text = "Manage fabric designs, labour rates and design images",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });

            var btnAdd = MkBtn("➕  Add Design", Blue, Color.White, BlueHov);
            btnAdd.Size = new Size(136, 34);
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Location = new Point(top.Width - 160, 17);
            btnAdd.Click += (s, e) => ShowForm(null);
            top.Resize += (s, e) => btnAdd.Location = new Point(top.Width - 160, 17);
            top.Controls.Add(btnAdd);

            var card = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });

            var bar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Surface2 };
            bar.Paint += (s, e) =>
            {
                using var pen = new Pen(Border);
                e.Graphics.DrawLine(pen, 0, bar.Height - 1, bar.Width, bar.Height - 1);
            };

            var srch = new Panel { Location = new Point(16, 10), Size = new Size(280, 32), BackColor = Color.White };
            srch.Paint += (s, e) =>
            {
                using var borderPen = new Pen(Border);
                using var txtBrush = new SolidBrush(TextLight);
                using var emojiFont = new Font("Segoe UI Emoji", 9F);
                e.Graphics.DrawRectangle(borderPen, 0, 0, srch.Width - 1, srch.Height - 1);
                e.Graphics.DrawString("🔍", emojiFont, txtBrush, new PointF(5, 7));
            };
            txtSearch = new TextBox
            {
                PlaceholderText = "Search by design name or HSN…",
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = TextDark,
                Location = new Point(27, 7),
                Size = new Size(245, 20)
            };
            txtSearch.TextChanged += (s, e) => { _currentPage = 1; LoadGrid(txtSearch.Text); };
            srch.Controls.Add(txtSearch);
            bar.Controls.Add(srch);

            var btnSample = MkBtn("📥 Sample CSV", Teal, Color.White, TealHov); btnSample.Size = new Size(126, 30);
            var btnImport = MkBtn("⬆  Import CSV", Violet, Color.White, VioletHov); btnImport.Size = new Size(118, 30);
            var btnExport = MkBtn("⬇  Export Excel", Green, Color.White, GreenHov); btnExport.Size = new Size(130, 30);

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

            var pager = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = Surface2 };
            pager.Paint += (s, e) =>
            {
                using var pen = new Pen(Border);
                e.Graphics.DrawLine(pen, 0, 0, pager.Width, 0);
            };

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

            dgv = BuildDgv();
            dgv.RowTemplate.Height = THUMB_SIZE + 8;

            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
            dgv.Columns.Add(new DataGridViewImageColumn
            {
                HeaderText = "IMAGE",
                Name = "Thumb",
                MinimumWidth = THUMB_SIZE + 16,
                Width = THUMB_SIZE + 16,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                DefaultCellStyle = { NullValue = null }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DESIGN NAME", Name = "Name", MinimumWidth = 200, FillWeight = 32 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "HSN CODE", Name = "Hsn", MinimumWidth = 110, FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "RATE (₹)", Name = "Rate", MinimumWidth = 110, FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "UNIT", Name = "Unit", MinimumWidth = 80, FillWeight = 10 });
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

            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                if (e.ColumnIndex == dgv.Columns["Rate"]?.Index && e.Value != null &&
                    decimal.TryParse(e.Value.ToString(), out decimal rv))
                {
                    e.CellStyle.ForeColor = Green;
                    e.CellStyle.Font = new Font("Consolas", 9F, FontStyle.Bold);
                    e.Value = "₹" + rv.ToString("0.00");
                    e.FormattingApplied = true;
                }
                int ei = dgv.Columns["Edit"]?.Index ?? -1, di = dgv.Columns["Delete"]?.Index ?? -1;
                if (e.ColumnIndex == ei || e.ColumnIndex == di)
                { e.Value = ""; e.FormattingApplied = true; }
            };

            AttachActionPainter(dgv,
                ("Edit", AmberPale, Amber, AmberBdr, "✏  Edit"),
                ("Delete", RedPale, Red, RedBdr, "🗑 Delete"));

            dgv.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex != dgv.Columns["Thumb"]?.Index) return;
                int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["Id"].Value);
                ShowFullImage(id);
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

        // ── Load / filter ─────────────────────────────────────────────
        void LoadGrid(string q)
        {
            _lastHover = -1;
            try
            {
                using var db = new AppDbContext();
                var list = db.DesignMasters
                    .Where(d => d.CompanyProfileId == SessionManager.CompanyProfileId)
                    .ToList();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.ToLower();
                    list = list.Where(d =>
                        (d.DesignName ?? "").ToLower().Contains(q) ||
                        (d.HsnCode ?? "").ToLower().Contains(q)).ToList();
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
            foreach (var d in _allRows.Skip((_currentPage - 1) * PAGE_SIZE).Take(PAGE_SIZE))
            {
                Image? thumb = BytesToThumb(d.DesignImage);
                int ri = dgv.Rows.Add(d.DesignId, thumb, d.DesignName, d.HsnCode,
                    d.DefaultRate?.ToString("0.00"), d.Unit);
                if (thumb == null)
                    dgv.Rows[ri].Cells["Thumb"].Value = null;
            }

            _lblPageInfo.Text = $"Page {_currentPage} of {_totalPages}  ({_allRows.Count} records)";
            _btnPrev.Enabled = _currentPage > 1;
            _btnNext.Enabled = _currentPage < _totalPages;

            if (_lblPageInfo.Parent != null)
                _lblPageInfo.Location = new Point(
                    (_lblPageInfo.Parent.Width - _lblPageInfo.Width) / 2,
                    (_lblPageInfo.Parent.Height - _lblPageInfo.Height) / 2);
        }

        void DgvCellPaintThumb(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != dgv.Columns["Thumb"]?.Index) return;
            if (e.Value != null) return;

            e.Handled = true;
            e.PaintBackground(e.ClipBounds, true);

            var rc = new Rectangle(
                e.CellBounds.X + 6, e.CellBounds.Y + 4,
                e.CellBounds.Width - 12, e.CellBounds.Height - 8);

            // ✅ NEVER wrap e.Graphics in using — WinForms owns it; disposing it crashes GetHdc()
            var g = e.Graphics;
            var prevSmoothing = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = RRect(rc, 6))
            using (var fillBrush = new SolidBrush(Color.FromArgb(241, 245, 249)))
            using (var borderPen = new Pen(Border, 1f))
            {
                g.FillPath(fillBrush, path);
                g.DrawPath(borderPen, path);
            }

            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var txtBrush = new SolidBrush(TextLight))
            using (var emojiFont = new Font("Segoe UI Emoji", 13F))
            {
                g.DrawString("🖼", emojiFont, txtBrush, rc, sf);
            }

            g.SmoothingMode = prevSmoothing; // restore — other painters share this Graphics
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
                    var d = db.DesignMasters.Find(id);
                    if (d != null) { db.DesignMasters.Remove(d); db.SaveChanges(); }
                    Toast("Design deleted.");
                    LoadGrid(txtSearch?.Text ?? "");
                }
                catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
            }
        }

        // ── Full-image viewer ─────────────────────────────────────────
        // FIX: draw pixels into a fresh Bitmap before disposing stream+temp
        void ShowFullImage(int designId)
        {
            try
            {
                using var db = new AppDbContext();
                var d = db.DesignMasters.Find(designId);
                if (d?.DesignImage == null || d.DesignImage.Length == 0)
                {
                    MessageBox.Show("No image stored for this design.", "View Image",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Bitmap img;
                using (var ms = new MemoryStream(d.DesignImage))
                using (var temp = Image.FromStream(ms, true, true))
                {
                    // Fully copy pixels while stream is still alive
                    img = new Bitmap(temp.Width, temp.Height, PixelFormat.Format32bppArgb);
                    using var g = Graphics.FromImage(img);
                    g.DrawImage(temp, 0, 0, temp.Width, temp.Height);
                }

                var frm = new Form
                {
                    Text = $"Design Image — {d.DesignName}",
                    Size = new Size(640, 520),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(30, 30, 30),
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false
                };
                var pic = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Image = img,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.FromArgb(30, 30, 30)
                };
                frm.Controls.Add(pic);
                frm.FormClosed += (s, e) => img.Dispose();
                frm.ShowDialog(this.FindForm());
            }
            catch (Exception ex) { MessageBox.Show("Cannot load image: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════
        //  EXCEL EXPORT
        // ════════════════════════════════════════════════════════════
        void ExportToExcel(object sender, EventArgs e)
        {
            if (_allRows.Count == 0)
            { MessageBox.Show("No records to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            using var sfd = new SaveFileDialog
            {
                Title = "Export Design Master to Excel",
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"DesignMaster_{SessionManager.CompanyName.Replace(" ", "_")}_{DateTime.Today:yyyyMMdd}.xlsx"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                var tmp = new DataGridView();
                tmp.Columns.Add("Name", "DESIGN NAME");
                tmp.Columns.Add("Hsn", "HSN CODE");
                tmp.Columns.Add("Rate", "DEFAULT RATE");
                tmp.Columns.Add("Unit", "UNIT");
                tmp.Columns.Add("Img", "HAS IMAGE");

                foreach (var d in _allRows)
                    tmp.Rows.Add(d.DesignName, d.HsnCode,
                        d.DefaultRate?.ToString("0.00"), d.Unit,
                        (d.DesignImage != null && d.DesignImage.Length > 0) ? "Yes" : "No");

                ExcelExportHelper.Export(tmp, sfd.FileName, "Design Master");
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
                FileName = "DesignMaster_Sample.csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("DesignName,HsnCode,DefaultRate,Unit");
                sb.AppendLine("Kantha Stitch,58063200,450.00,MTR");
                sb.AppendLine("Banarasi Silk,50072000,1200.00,MTR");
                sb.AppendLine("Chikan Embroidery,58041000,680.00,MTR");
                sb.AppendLine("Plain Cotton,52081100,120.00,MTR");
                sb.AppendLine("Georgette Print,54075200,320.00,MTR");
                sb.AppendLine("Handloom Khadi,63019000,550.00,PCS");

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show(
                    "Sample CSV saved.\n\n" +
                    "📌 Instructions:\n" +
                    "  1. Open the CSV in Excel.\n" +
                    "  2. Fill in your designs (one row per design).\n" +
                    "  3. Keep the header row exactly as-is.\n" +
                    "  4. Save as CSV, then use ⬆ Import CSV to load.\n\n" +
                    "Valid Units : MTR, PCS, KG, YDS, SET\n" +
                    "Note        : Images must be added individually via the Edit button.",
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
                Title = "Select Design Master Import CSV",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            int added = 0, skipped = 0;
            var skipLog = new StringBuilder();
            try
            {
                string[] lines = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                if (lines.Length < 2)
                { MessageBox.Show("The CSV has no data rows.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                string[] hdr = CsvSplit(lines[0]);
                int Idx(string col)
                {
                    for (int i = 0; i < hdr.Length; i++)
                        if (string.Equals(hdr[i].Trim(), col, StringComparison.OrdinalIgnoreCase)) return i;
                    return -1;
                }
                int iName = Idx("DesignName"), iHsn = Idx("HsnCode"),
                    iRate = Idx("DefaultRate"), iUnit = Idx("Unit");

                if (iName < 0)
                {
                    MessageBox.Show("Column 'DesignName' not found in CSV header.\nDownload the sample CSV to see the required format.",
                        "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string[] validUnits = { "MTR", "PCS", "KG", "YDS", "SET" };

                using var db = new AppDbContext();
                for (int li = 1; li < lines.Length; li++)
                {
                    if (string.IsNullOrWhiteSpace(lines[li])) continue;
                    string[] f = CsvSplit(lines[li]);
                    string name = Get(f, iName);
                    if (string.IsNullOrWhiteSpace(name))
                    { skipped++; skipLog.AppendLine($"Row {li + 1}: empty DesignName — skipped."); continue; }

                    if (db.DesignMasters.Any(d =>
                            d.CompanyProfileId == SessionManager.CompanyProfileId && d.DesignName == name))
                    { skipped++; skipLog.AppendLine($"Row {li + 1}: \"{name}\" already exists — skipped."); continue; }

                    decimal.TryParse(Get(f, iRate), out decimal rate);
                    string unit = Get(f, iUnit).ToUpper();
                    if (!validUnits.Contains(unit)) unit = "MTR";

                    db.DesignMasters.Add(new DesignMaster
                    {
                        CompanyProfileId = SessionManager.CompanyProfileId,
                        DesignName = name,
                        HsnCode = Get(f, iHsn),
                        DefaultRate = rate,
                        Unit = unit
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

            var top = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            lblFormTitle = new Label
            {
                Text = "Add New Design",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            };
            top.Controls.Add(lblFormTitle);
            top.Controls.Add(new Label
            {
                Text = "Define design name, HSN code, rate and upload an image",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });

            var card = new Panel
            {
                BackColor = Surface,
                Location = new Point(24, 68),
                Size = new Size(860, 520),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });

            var sec = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Surface2 };
            sec.Paint += (s, e) =>
            {
                using var pen = new Pen(Border);
                e.Graphics.DrawLine(pen, 0, sec.Height - 1, sec.Width, sec.Height - 1);
            };
            sec.Controls.Add(new Label
            {
                Text = "Design Details",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 10)
            });
            card.Controls.Add(sec);

            int lx = 24, fx = 180, fw = 280, y = 52, gap = 50;

            txtDesignName = FF(card, "Design Name *", lx, fx, fw, y); y += gap;

            txtHsnCode = FF(card, "HSN Code", lx, fx, 160, y);
            card.Controls.Add(FLbl("Unit", new Point(lx + 240, y + 5)));
            cmbUnit = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Surface2,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(lx + 310, y),
                Size = new Size(150, 28)
            };
            cmbUnit.Items.AddRange(new object[] { "MTR", "PCS", "KG", "YDS", "SET" });
            cmbUnit.SelectedIndex = 0;
            card.Controls.Add(cmbUnit);
            y += gap;

            txtRate = FF(card, "Default Rate (₹)", lx, fx, 160, y); y += gap;

            int imgX = 560, imgW = 220, imgH = 200, imgTopY = 52;

            card.Controls.Add(FLbl("Design Image", new Point(imgX, imgTopY - 20)));

            picPreview = new PictureBox
            {
                Location = new Point(imgX, imgTopY),
                Size = new Size(imgW, imgH),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Surface2,
                BorderStyle = BorderStyle.None,
                Cursor = Cursors.Hand
            };
            picPreview.Paint += PicPreview_Paint;
            picPreview.Click += (s, e) => PickImage();
            card.Controls.Add(picPreview);

            btnPickImage = MkBtn("🖼  Upload Image", Blue, Color.White, BlueHov);
            btnPickImage.Location = new Point(imgX, imgTopY + imgH + 8);
            btnPickImage.Size = new Size(imgW / 2 - 3, 32);
            btnPickImage.Click += (s, e) => PickImage();
            card.Controls.Add(btnPickImage);

            btnClearImage = MkBtn("✕  Clear", Surface, TextMid, Color.FromArgb(241, 245, 249), border: true);
            btnClearImage.Location = new Point(imgX + imgW / 2 + 3, imgTopY + imgH + 8);
            btnClearImage.Size = new Size(imgW / 2 - 3, 32);
            btnClearImage.Click += (s, e) => ClearImage();
            card.Controls.Add(btnClearImage);

            card.Controls.Add(new Label
            {
                Text = "JPG, PNG, BMP  ·  Max 2 MB  ·  Click image to change",
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(imgX, imgTopY + imgH + 46)
            });

            y = Math.Max(y, imgTopY + imgH + 90);
            var btnSave = MkBtn("💾  Save Design", Blue, Color.White, BlueHov);
            var btnCancel = MkBtn("✕  Cancel", Surface, TextMid, Color.FromArgb(241, 245, 249), border: true);
            btnSave.Location = new Point(fx, y); btnSave.Size = new Size(140, 36);
            btnCancel.Location = new Point(fx + 152, y); btnCancel.Size = new Size(100, 36);
            btnSave.Click += Save;
            btnCancel.Click += (s, e) => ShowGrid();
            card.Controls.Add(btnSave);
            card.Controls.Add(btnCancel);
            card.Height = y + 60;

            pnlForm.Resize += (sr, er) => card.Width = Math.Max(600, pnlForm.Width - 48);
            pnlForm.Controls.Add(card);
            pnlForm.Controls.Add(top);
            this.Controls.Add(pnlForm);
        }

        // ── Image preview painter ─────────────────────────────────────
        void PicPreview_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var rc = new Rectangle(0, 0, picPreview.Width - 1, picPreview.Height - 1);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = RRect(rc, 8);
            g.SetClip(path);

            if (picPreview.Image != null)
            {
                g.DrawImage(picPreview.Image,
                    new Rectangle(0, 0, picPreview.Width, picPreview.Height));
            }
            else
            {
                using (var fillBrush = new SolidBrush(Surface2))
                    g.FillPath(fillBrush, path);

                using var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var txtBrush = new SolidBrush(TextLight);
                using var font = new Font("Segoe UI", 9F);
                g.DrawString("🖼\nClick to upload image", font, txtBrush,
                    new RectangleF(0, 0, picPreview.Width, picPreview.Height), sf);
            }

            g.ResetClip();
            using var borderPen = new Pen(Border, 1.5f);
            g.DrawPath(borderPen, path);
        }

        // ── Image helpers ─────────────────────────────────────────────
        void PickImage()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select Design Image",
                Filter = "Image Files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All Files (*.*)|*.*"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            var fi = new FileInfo(ofd.FileName);
            if (fi.Length > 2 * 1024 * 1024)
            {
                MessageBox.Show("Image is too large (max 2 MB).\nPlease resize and try again.",
                    "Image Too Large", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _pendingImageBytes = File.ReadAllBytes(ofd.FileName);

                // FIX: load via stream so we can safely dispose after preview is built
                using var fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read);
                using var tmp = Image.FromStream(fs, true, true);
                picPreview.Image?.Dispose();
                picPreview.Image = ResizeForPreview(tmp, 220, 200);
                picPreview.Invalidate();
            }
            catch (Exception ex) { MessageBox.Show("Cannot load image: " + ex.Message); }
        }

        void ClearImage()
        {
            _pendingImageBytes = null;
            picPreview.Image?.Dispose();
            picPreview.Image = null;
            picPreview.Invalidate();
        }

        // Resize keeping aspect ratio — creates a fully independent Bitmap
        static Image ResizeForPreview(Image src, int maxW, int maxH)
        {
            float scale = Math.Min((float)maxW / src.Width, (float)maxH / src.Height);
            int w = (int)(src.Width * scale), h = (int)(src.Height * scale);
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(src, 0, 0, w, h);
            return bmp;
        }

        // FIX: draw pixels into a brand-new Bitmap while stream is still open,
        //      so the returned Image has zero dependency on the original stream.
        static Image? BytesToThumb(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            try
            {
                using var ms = new MemoryStream(bytes);
                using var temp = Image.FromStream(ms, true, true);

                var bmp = new Bitmap(temp.Width, temp.Height, PixelFormat.Format32bppArgb);
                using var g = Graphics.FromImage(bmp);
                g.DrawImage(temp, 0, 0, temp.Width, temp.Height);
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        // ────────────────────────────────────────────────────────────
        void ShowGrid()
        {
            pnlForm.Visible = false;
            pnlGrid.Visible = true;
            _editId = -1;
            LoadGrid(txtSearch?.Text ?? "");
        }

        void ShowForm(DataGridViewRow? row)
        {
            ClearForm(); ClearImage();
            _editId = -1;
            lblFormTitle.Text = "Add New Design";

            if (row != null)
            {
                _editId = Convert.ToInt32(row.Cells["Id"].Value);
                lblFormTitle.Text = "Edit Design";
                try
                {
                    using var db = new AppDbContext();
                    var d = db.DesignMasters.Find(_editId);
                    if (d == null) return;

                    txtDesignName.Text = d.DesignName ?? "";
                    txtHsnCode.Text = d.HsnCode ?? "";
                    txtRate.Text = d.DefaultRate?.ToString("0.00") ?? "";
                    int ui = cmbUnit.Items.IndexOf(d.Unit ?? "");
                    if (ui >= 0) cmbUnit.SelectedIndex = ui;

                    // FIX: load image into preview while stream is open,
                    //      ResizeForPreview creates an independent Bitmap before stream disposes
                    if (d.DesignImage != null && d.DesignImage.Length > 0)
                    {
                        _pendingImageBytes = d.DesignImage;
                        using var ms = new MemoryStream(d.DesignImage);
                        using var tmp = Image.FromStream(ms, true, true);
                        picPreview.Image?.Dispose();
                        picPreview.Image = ResizeForPreview(tmp, 220, 200);
                    }
                    picPreview.Invalidate();
                }
                catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
            }

            pnlGrid.Visible = false;
            pnlForm.Visible = true;
        }

        void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDesignName.Text))
            {
                MessageBox.Show("Design Name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            decimal.TryParse(txtRate.Text, out decimal rate);

            try
            {
                using var db = new AppDbContext();
                if (_editId == -1)
                {
                    db.DesignMasters.Add(new DesignMaster
                    {
                        CompanyProfileId = SessionManager.CompanyProfileId,
                        DesignName = txtDesignName.Text.Trim(),
                        HsnCode = txtHsnCode.Text.Trim(),
                        DefaultRate = rate,
                        Unit = cmbUnit.Text,
                        DesignImage = _pendingImageBytes
                    });
                    Toast("Design added.");
                }
                else
                {
                    var d = db.DesignMasters.Find(_editId);
                    if (d == null) return;
                    d.DesignName = txtDesignName.Text.Trim();
                    d.HsnCode = txtHsnCode.Text.Trim();
                    d.DefaultRate = rate;
                    d.Unit = cmbUnit.Text;
                    d.DesignImage = _pendingImageBytes;
                    Toast("Design updated.");
                }
                db.SaveChanges();
                ShowGrid();
            }
            catch (Exception ex) { MessageBox.Show("Save error: " + ex.Message); }
        }

        void ClearForm()
        {
            txtDesignName.Text = txtHsnCode.Text = txtRate.Text = "";
            cmbUnit.SelectedIndex = 0;
        }

        // ════════════════════════════════════════════════════════════
        //  DataGridView builder
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
            g.RowTemplate.Height = THUMB_SIZE + 8;

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
                int ei = g.Columns["Edit"]?.Index ?? -1, di = g.Columns["Delete"]?.Index ?? -1;
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
            g.CellPainting += DgvCellPaintThumb;
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

                    var rc = new Rectangle(e.CellBounds.X + 7, e.CellBounds.Y + 6,
                        e.CellBounds.Width - 14, e.CellBounds.Height - 12);

                    // ✅ Do NOT wrap e.Graphics in using — WinForms owns it
                    var gr = e.Graphics;
                    var prevSmoothing = gr.SmoothingMode;
                    gr.SmoothingMode = SmoothingMode.AntiAlias;

                    using (var path = RRect(rc, 5))
                    using (var fillBrush = new SolidBrush(bg))
                    using (var borderPen = new Pen(bdr, 1f))
                    {
                        gr.FillPath(fillBrush, path);
                        gr.DrawPath(borderPen, path);
                    }

                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    using (var fgBrush = new SolidBrush(fg))
                    using (var font = new Font("Segoe UI", 8F, FontStyle.Bold))
                    {
                        gr.DrawString(lbl, font, fgBrush, rc, sf);
                    }

                    gr.SmoothingMode = prevSmoothing;
                    break;
                }
            };
        }

        // ════════════════════════════════════════════════════════════
        //  UI helpers
        // ════════════════════════════════════════════════════════════
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
        //  CSV helpers
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
        //  Graphics helpers
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