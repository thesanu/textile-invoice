using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Textile_Invoice_App.Models;

// ══════════════════════════════════════════════════════════════════
//  UC_ServiceMaster
// ══════════════════════════════════════════════════════════════════
namespace Textile_Invoice_App
{
    public partial class UC_ServiceMaster : UserControl
    {
        static readonly Color Bg = Color.FromArgb(245, 247, 250);
        static readonly Color Surface = Color.White;
        static readonly Color Surface2 = Color.FromArgb(248, 249, 252);
        static readonly Color HeaderBg = Color.FromArgb(241, 245, 249);
        static readonly Color Blue = Color.FromArgb(37, 99, 235);
        static readonly Color BlueHov = Color.FromArgb(29, 78, 216);
        static readonly Color BluePale = Color.FromArgb(239, 246, 255);
        static readonly Color Green = Color.FromArgb(22, 163, 74);
        static readonly Color Red = Color.FromArgb(220, 38, 38);
        static readonly Color RedPale = Color.FromArgb(254, 242, 242);
        static readonly Color RedBdr = Color.FromArgb(254, 202, 202);
        static readonly Color Amber = Color.FromArgb(180, 83, 9);
        static readonly Color AmberPale = Color.FromArgb(255, 251, 235);
        static readonly Color AmberBdr = Color.FromArgb(253, 230, 138);
        static readonly Color Border = Color.FromArgb(226, 232, 240);
        static readonly Color RowLine = Color.FromArgb(241, 245, 249);
        static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        static readonly Color TextMid = Color.FromArgb(71, 85, 105);
        static readonly Color TextLight = Color.FromArgb(148, 163, 184);

        Panel pnlGrid, pnlForm;
        DataGridView dgv;
        TextBox txtSearch, txtName, txtDesc;
        Label lblTitle;
        int _editId = -1, _lastHover = -1;

        public UC_ServiceMaster()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Bg;
            BuildGrid();
            BuildForm();
            ShowGrid();
        }

        void BuildGrid()
        {
            pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Bg };

            var top = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            top.Controls.Add(new Label
            {
                Text = "Service Master",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            });
            top.Controls.Add(new Label
            {
                Text = "Manage services offered by " + SessionManager.CompanyName,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });

            var btnAdd = MkBtn("➕  Add Service", Blue, Color.White, BlueHov);
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
                e.Graphics.DrawLine(new Pen(Border), 0, bar.Height - 1, bar.Width, bar.Height - 1);

            var srch = new Panel { Location = new Point(16, 10), Size = new Size(280, 32), BackColor = Color.White };
            srch.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Border), 0, 0, srch.Width - 1, srch.Height - 1);
                e.Graphics.DrawString("🔍", new Font("Segoe UI Emoji", 9F),
                    new SolidBrush(TextLight), new PointF(5, 7));
            };
            txtSearch = new TextBox
            {
                PlaceholderText = "Search services…",
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                ForeColor = TextDark,
                Location = new Point(27, 7),
                Size = new Size(245, 20)
            };
            txtSearch.TextChanged += (s, e) => LoadGrid(txtSearch.Text);
            srch.Controls.Add(txtSearch);
            bar.Controls.Add(srch);

            dgv = BuildDgv();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", Visible = false });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "SERVICE NAME", Name = "Name", MinimumWidth = 200, FillWeight = 25 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "DESCRIPTION", Name = "Desc", MinimumWidth = 300, FillWeight = 45 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Edit", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 70 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Delete", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 76 });

            dgv.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int ei = dgv.Columns["Edit"]?.Index ?? -1, di = dgv.Columns["Delete"]?.Index ?? -1;
                if (e.ColumnIndex == ei || e.ColumnIndex == di) { e.Value = ""; e.FormattingApplied = true; }
            };
            AttachActionPainter(dgv,
                ("Edit", AmberPale, Amber, AmberBdr, "✏  Edit"),
                ("Delete", RedPale, Red, RedBdr, "🗑 Delete"));
            dgv.CellClick += DgvClick;

            card.Controls.Add(dgv);
            card.Controls.Add(bar);
            pnlGrid.Controls.Add(card);
            pnlGrid.Controls.Add(top);
            this.Controls.Add(pnlGrid);
            LoadGrid("");
        }

        void LoadGrid(string q)
        {
            dgv.Rows.Clear(); _lastHover = -1;
            try
            {
                using var db = new AppDbContext();
                var list = db.ServiceMasters
                    .Where(s => s.CompanyProfileId == SessionManager.CompanyProfileId).ToList();
                if (!string.IsNullOrWhiteSpace(q))
                {
                    q = q.ToLower();
                    list = list.Where(s => (s.ServiceName ?? "").ToLower().Contains(q)).ToList();
                }
                foreach (var s in list) dgv.Rows.Add(s.ServiceId, s.ServiceName, s.Description);
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        void DgvClick(object s, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == dgv.Columns["Edit"]?.Index)
                ShowForm(dgv.Rows[e.RowIndex]);
            if (e.ColumnIndex == dgv.Columns["Delete"]?.Index &&
                MessageBox.Show("Delete this service?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["Id"].Value);
                    using var db = new AppDbContext();
                    var sv = db.ServiceMasters.Find(id);
                    if (sv != null) { db.ServiceMasters.Remove(sv); db.SaveChanges(); }
                    Toast("Deleted."); LoadGrid(txtSearch.Text);
                }
                catch (Exception ex) { MessageBox.Show("Delete error: " + ex.Message); }
            }
        }

        void BuildForm()
        {
            pnlForm = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Visible = false };

            var top = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            lblTitle = new Label
            {
                Text = "Add Service",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            };
            top.Controls.Add(lblTitle);
            top.Controls.Add(new Label
            {
                Text = "Define the service name and description",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });

            var card = new Panel
            {
                BackColor = Surface,
                Location = new Point(24, 68),
                Size = new Size(640, 260),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });
            var sec = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Surface2 };
            sec.Controls.Add(new Label
            {
                Text = "Service Details",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 12)
            });
            card.Controls.Add(sec);

            int lx = 24, fx = 170, y = 60;
            card.Controls.Add(FLbl("Service Name *", new Point(lx, y + 5)));
            txtName = new TextBox
            {
                BackColor = Surface2,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(fx, y),
                Size = new Size(380, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(txtName); y += 50;

            card.Controls.Add(FLbl("Description", new Point(lx, y + 5)));
            txtDesc = new TextBox
            {
                Multiline = true,
                BackColor = Surface2,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(fx, y),
                Size = new Size(380, 64),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(txtDesc); y += 80;

            var btnSave = MkBtn("💾  Save", Blue, Color.White, BlueHov);
            btnSave.Location = new Point(fx, y); btnSave.Size = new Size(120, 36);
            btnSave.Click += Save;
            var btnCancel = MkBtn("✕  Cancel", Surface, TextMid,
                Color.FromArgb(241, 245, 249), border: true);
            btnCancel.Location = new Point(fx + 132, y); btnCancel.Size = new Size(100, 36);
            btnCancel.Click += (s, e) => ShowGrid();
            card.Controls.Add(btnSave); card.Controls.Add(btnCancel);
            card.Height = y + 60;

            pnlForm.Resize += (sr, er) => { card.Width = System.Math.Max(360, pnlForm.Width - 48); };
            pnlForm.Controls.Add(card); pnlForm.Controls.Add(top);
            this.Controls.Add(pnlForm);
        }

        void ShowGrid()
        {
            pnlForm.Visible = false; pnlGrid.Visible = true;
            _editId = -1; LoadGrid(txtSearch?.Text ?? "");
        }

        void ShowForm(DataGridViewRow row)
        {
            txtName.Text = txtDesc.Text = ""; _editId = -1; lblTitle.Text = "Add Service";
            if (row != null)
            {
                _editId = Convert.ToInt32(row.Cells["Id"].Value); lblTitle.Text = "Edit Service";
                try
                {
                    using var db = new AppDbContext();
                    var sv = db.ServiceMasters.Find(_editId);
                    if (sv != null) { txtName.Text = sv.ServiceName ?? ""; txtDesc.Text = sv.Description ?? ""; }
                }
                catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
            }
            pnlGrid.Visible = false; pnlForm.Visible = true;
        }

        void Save(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Service Name required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                using var db = new AppDbContext();
                if (_editId == -1)
                {
                    db.ServiceMasters.Add(new ServiceMaster
                    {
                        CompanyProfileId = SessionManager.CompanyProfileId,
                        ServiceName = txtName.Text.Trim(),
                        Description = txtDesc.Text.Trim()
                    });
                    Toast("Service added.");
                }
                else
                {
                    var sv = db.ServiceMasters.Find(_editId);
                    if (sv == null) return;
                    sv.ServiceName = txtName.Text.Trim();
                    sv.Description = txtDesc.Text.Trim();
                    Toast("Service updated.");
                }
                db.SaveChanges(); ShowGrid();
            }
            catch (Exception ex) { MessageBox.Show("Save error: " + ex.Message); }
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
            typeof(DataGridView)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(g, true);
            g.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg;
            g.ColumnHeadersDefaultCellStyle.ForeColor = TextMid;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderBg;
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            g.ColumnHeadersHeight = 40;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.BackColor = Surface;
            g.DefaultCellStyle.ForeColor = TextMid;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.DefaultCellStyle.SelectionBackColor = BluePale;
            g.DefaultCellStyle.SelectionForeColor = TextDark;
            g.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            g.RowTemplate.Height = 42;
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
                e.Graphics.DrawLine(p, e.RowBounds.Left, e.RowBounds.Bottom - 1,
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
                g.InvalidateRow(_lastHover); _lastHover = -1;
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
                    e.Handled = true; e.PaintBackground(e.ClipBounds, true);
                    var rc = new Rectangle(e.CellBounds.X + 7, e.CellBounds.Y + 10,
                        e.CellBounds.Width - 14, e.CellBounds.Height - 20);
                    var gr = e.Graphics; gr.SmoothingMode = SmoothingMode.AntiAlias;
                    using var path = RRect(rc, 5);
                    gr.FillPath(new SolidBrush(bg), path);
                    gr.DrawPath(new Pen(bdr, 1f), path);
                    using var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    gr.DrawString(lbl, new Font("Segoe UI", 8F, FontStyle.Bold), new SolidBrush(fg), rc, sf);
                    break;
                }
            };
        }

        Label FLbl(string t, Point p) => new Label
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
            tm.Tick += (s, e2) => { tm.Stop(); tm.Dispose(); f.Close(); f.Dispose(); };
            tm.Start();
        }

        static GraphicsPath RRect(Rectangle r, int rad)
        {
            int d = rad * 2; var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }
    }


    // ══════════════════════════════════════════════════════════════════
    //  UC_Reports
    // ══════════════════════════════════════════════════════════════════
    public partial class UC_Reports : UserControl
    {
        static readonly Color Bg = Color.FromArgb(245, 247, 250);
        static readonly Color Surface = Color.White;
        static readonly Color Surface2 = Color.FromArgb(248, 249, 252);
        static readonly Color HeaderBg = Color.FromArgb(241, 245, 249);
        static readonly Color Blue = Color.FromArgb(37, 99, 235);
        static readonly Color BlueHov = Color.FromArgb(29, 78, 216);
        static readonly Color BluePale = Color.FromArgb(239, 246, 255);
        static readonly Color Green = Color.FromArgb(22, 163, 74);
        static readonly Color Amber = Color.FromArgb(180, 83, 9);
        static readonly Color Border = Color.FromArgb(226, 232, 240);
        static readonly Color RowLine = Color.FromArgb(241, 245, 249);
        static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        static readonly Color TextMid = Color.FromArgb(71, 85, 105);
        static readonly Color TextLight = Color.FromArgb(148, 163, 184);

        DataGridView dgv;
        ComboBox cmbReport;
        DateTimePicker dtpFrom, dtpTo;

        public UC_Reports()
        {
            this.Dock = DockStyle.Fill; this.BackColor = Bg; Build();
        }

        void Build()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            top.Controls.Add(new Label
            {
                Text = "Reports",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            });
            top.Controls.Add(new Label
            {
                Text = SessionManager.CompanyName + " — filtered reports",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });

            var card = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });

            var bar = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Surface2 };
            bar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border), 0, bar.Height - 1, bar.Width, bar.Height - 1);

            bar.Controls.Add(new Label
            {
                Text = "Report:",
                ForeColor = TextMid,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 20)
            });
            cmbReport = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(72, 15),
                Size = new Size(200, 30)
            };
            cmbReport.Items.AddRange(new object[]
            {
                "Invoice Summary", "Client-wise Total", "Design-wise Total",
                "Monthly Summary", "GST Summary", "HSN Summary"
            });
            cmbReport.SelectedIndex = 0;
            bar.Controls.Add(cmbReport);

            bar.Controls.Add(new Label
            {
                Text = "From:",
                ForeColor = TextMid,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(290, 20)
            });
            dtpFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(330, 15),
                Size = new Size(112, 30),
                Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
            };
            bar.Controls.Add(dtpFrom);

            bar.Controls.Add(new Label
            {
                Text = "To:",
                ForeColor = TextMid,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(452, 20)
            });
            dtpTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(476, 15),
                Size = new Size(112, 30),
                Value = DateTime.Today
            };
            bar.Controls.Add(dtpTo);

            var btnRun = new Button
            {
                Text = "▶  Run Report",
                BackColor = Blue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 32),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnRun.FlatAppearance.BorderSize = 0;
            btnRun.FlatAppearance.MouseOverBackColor = BlueHov;
            btnRun.Click += RunReport;
            bar.Controls.Add(btnRun);

            var btnExport = new Button
            {
                Text = "📥  Export CSV",
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 32),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.FlatAppearance.MouseOverBackColor = Color.FromArgb(15, 118, 56);
            btnExport.Click += ExportCsv;
            bar.Controls.Add(btnExport);

            var btnExcelExp = new Button
            {
                Text = "📊  Export Excel",
                BackColor = Color.FromArgb(33, 115, 70),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 32),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnExcelExp.FlatAppearance.BorderSize = 0;
            btnExcelExp.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 83, 45);
            btnExcelExp.Click += ExportExcel;
            bar.Controls.Add(btnExcelExp);

            bar.Resize += (s, e) =>
            {
                btnExcelExp.Location = new Point(bar.Width - btnExcelExp.Width - 12, 13);
                btnExport.Location = new Point(bar.Width - btnExcelExp.Width - btnExport.Width - 16, 13);
                btnRun.Location = new Point(bar.Width - btnExcelExp.Width - btnExport.Width - btnRun.Width - 24, 13);
            };

            dgv = new DataGridView
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
                EnableHeadersVisualStyles = false
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = HeaderBg;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextMid;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = HeaderBg;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgv.ColumnHeadersHeight = 40;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.DefaultCellStyle.BackColor = Surface;
            dgv.DefaultCellStyle.ForeColor = TextMid;
            dgv.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgv.DefaultCellStyle.SelectionBackColor = BluePale;
            dgv.RowTemplate.Height = 42;
            dgv.RowsAdded += (s, e) =>
            {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                    if (i >= 0 && i < dgv.Rows.Count)
                        dgv.Rows[i].DefaultCellStyle.BackColor =
                            i % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253);
            };
            dgv.RowPostPaint += (s, e) =>
            {
                using var p = new Pen(RowLine, 1);
                e.Graphics.DrawLine(p, e.RowBounds.Left, e.RowBounds.Bottom - 1,
                    e.RowBounds.Right, e.RowBounds.Bottom - 1);
            };
            // ── FIX: Wire CellFormatting ONCE here, not inside RunReport ──
            // This prevents the handler from accumulating on each run click.
            var _fntMono = new Font("Consolas", 9F, FontStyle.Bold);
            dgv.CellFormatting += (s2, e2) =>
            {
                if (e2.RowIndex < 0 || e2.ColumnIndex < 0 || e2.ColumnIndex >= dgv.Columns.Count) return;
                var col = dgv.Columns[e2.ColumnIndex];
                if (col.HeaderText.Contains("TOTAL") || col.HeaderText.Contains("AMOUNT"))
                {
                    e2.CellStyle.ForeColor = Green;
                    e2.CellStyle.Font = _fntMono;
                    e2.FormattingApplied = true;
                }
            };

            card.Controls.Add(dgv);
            card.Controls.Add(bar);
            this.Controls.Add(card);
            this.Controls.Add(top);
        }

        void RunReport(object sender, EventArgs e)
        {
            dgv.Columns.Clear(); dgv.Rows.Clear();
            int cid = SessionManager.CompanyProfileId;
            DateOnly dateFrom = DateOnly.FromDateTime(dtpFrom.Value);
            DateOnly dateTo = DateOnly.FromDateTime(dtpTo.Value);
            try
            {
                using var db = new AppDbContext();
                switch (cmbReport.SelectedIndex)
                {
                    case 0:
                        dgv.Columns.Add("InvNo", "INV NO");
                        dgv.Columns.Add("Client", "CLIENT");
                        dgv.Columns.Add("Date", "DATE");
                        dgv.Columns.Add("Total", "TOTAL");
                        dgv.Columns.Add("Grand", "GRAND TOTAL");
                        var il = db.InvoiceHeaders
                            .Where(h => h.CompanyProfileId == cid && h.InvoiceDate >= dateFrom && h.InvoiceDate <= dateTo)
                            .Join(db.Accounts, h => h.ClientId, a => a.AccountId,
                                (h, a) => new { h, ClientName = a.AccNm })
                            .OrderByDescending(x => x.h.InvoiceDate).ToList();
                        foreach (var r in il)
                            dgv.Rows.Add(r.h.InvoiceNo, r.ClientName,
                                r.h.InvoiceDate.ToString("dd MMM yyyy"),
                                "₹" + (r.h.TotalAmount ?? 0).ToString("N2"),
                                "₹" + (r.h.GrandTotal ?? 0).ToString("N2"));
                        break;

                    case 1:
                        dgv.Columns.Add("Client", "CLIENT");
                        dgv.Columns.Add("Count", "INVOICES");
                        dgv.Columns.Add("Total", "GRAND TOTAL");
                        var cl = db.InvoiceHeaders
                            .Where(h => h.CompanyProfileId == cid && h.InvoiceDate >= dateFrom && h.InvoiceDate <= dateTo)
                            .Join(db.Accounts, h => h.ClientId, a => a.AccountId,
                                (h, a) => new { h, ClientName = a.AccNm })
                            .AsEnumerable()
                            .GroupBy(x => x.ClientName)
                            .Select(g => new { Client = g.Key, Count = g.Count(), Total = g.Sum(x => x.h.GrandTotal ?? 0) })
                            .OrderByDescending(x => x.Total).ToList();
                        foreach (var r in cl)
                            dgv.Rows.Add(r.Client, r.Count, "₹" + r.Total.ToString("N2"));
                        break;

                    case 2:
                        dgv.Columns.Add("Design", "DESIGN");
                        dgv.Columns.Add("Qty", "TOTAL QTY");
                        dgv.Columns.Add("Amt", "TOTAL AMOUNT");
                        var dl = db.InvoiceItems
                            .Where(i => i.CompanyProfileId == cid)
                            .Join(db.InvoiceHeaders, i => i.InvoiceId, h => h.InvoiceId, (i, h) => new { i, h })
                            .Where(x => x.h.InvoiceDate >= dateFrom && x.h.InvoiceDate <= dateTo)
                            .GroupJoin(db.DesignMasters, x => x.i.DesignId, d => d.DesignId,
                                (x, ds) => new { x.i, DesignName = ds.Select(d => d.DesignName).FirstOrDefault() ?? "— Free Text —" })
                            .AsEnumerable()
                            .GroupBy(x => x.DesignName)
                            .Select(g => new { Design = g.Key, Qty = g.Sum(x => x.i.Qty ?? 0), Amt = g.Sum(x => x.i.Amount ?? 0) })
                            .OrderByDescending(x => x.Amt).ToList();
                        foreach (var r in dl)
                            dgv.Rows.Add(r.Design, r.Qty.ToString("N2"), "₹" + r.Amt.ToString("N2"));
                        break;

                    case 3:
                        dgv.Columns.Add("Month", "MONTH");
                        dgv.Columns.Add("Count", "INVOICES");
                        dgv.Columns.Add("Total", "GRAND TOTAL");
                        var ml = db.InvoiceHeaders
                            .Where(h => h.CompanyProfileId == cid && h.InvoiceDate >= dateFrom && h.InvoiceDate <= dateTo)
                            .AsEnumerable()
                            .GroupBy(h => new { h.InvoiceDate.Year, h.InvoiceDate.Month })
                            .Select(g => new
                            {
                                SortKey = new DateTime(g.Key.Year, g.Key.Month, 1),
                                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                                Count = g.Count(),
                                Total = g.Sum(h => h.GrandTotal ?? 0)
                            })
                            .OrderBy(x => x.SortKey).ToList();
                        foreach (var r in ml)
                            dgv.Rows.Add(r.Month, r.Count, "₹" + r.Total.ToString("N2"));
                        break;

                    case 4: // GST Summary
                        dgv.Columns.Add("InvNo", "INV NO");
                        dgv.Columns.Add("Client", "CLIENT");
                        dgv.Columns.Add("Date", "DATE");
                        dgv.Columns.Add("Taxable", "TAXABLE AMT");
                        dgv.Columns.Add("Cgst", "CGST");
                        dgv.Columns.Add("Sgst", "SGST");
                        dgv.Columns.Add("Igst", "IGST");
                        dgv.Columns.Add("Grand", "GRAND TOTAL");
                        var gst = db.InvoiceHeaders
                            .Where(h => h.CompanyProfileId == cid && h.InvoiceDate >= dateFrom && h.InvoiceDate <= dateTo)
                            .Join(db.Accounts, h => h.ClientId, a => a.AccountId,
                                (h, a) => new { h, ClientName = a.AccNm })
                            .OrderByDescending(x => x.h.InvoiceDate).ToList();
                        decimal totTaxable = 0, totCgst = 0, totSgst = 0, totIgst = 0, totGrand = 0;
                        foreach (var r in gst)
                        {
                            decimal tx = r.h.TotalAmount ?? 0, cg = r.h.Cgst ?? 0,
                                    sg = r.h.Sgst ?? 0, ig = r.h.Igst ?? 0,
                                    gt = r.h.GrandTotal ?? 0;
                            totTaxable += tx; totCgst += cg; totSgst += sg; totIgst += ig; totGrand += gt;
                            dgv.Rows.Add(r.h.InvoiceNo, r.ClientName,
                                r.h.InvoiceDate.ToString("dd MMM yyyy"),
                                "₹" + tx.ToString("N2"), "₹" + cg.ToString("N2"),
                                "₹" + sg.ToString("N2"), "₹" + ig.ToString("N2"),
                                "₹" + gt.ToString("N2"));
                        }
                        int ti = dgv.Rows.Add("", "TOTAL", "",
                            "₹" + totTaxable.ToString("N2"), "₹" + totCgst.ToString("N2"),
                            "₹" + totSgst.ToString("N2"), "₹" + totIgst.ToString("N2"),
                            "₹" + totGrand.ToString("N2"));
                        foreach (DataGridViewCell cell in dgv.Rows[ti].Cells)
                        {
                            cell.Style.BackColor = Color.FromArgb(240, 253, 244);
                            cell.Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                            cell.Style.ForeColor = Color.FromArgb(22, 163, 74);
                        }
                        break;

                    case 5: // HSN Summary
                        dgv.Columns.Add("Hsn", "HSN CODE");
                        dgv.Columns.Add("Desc", "DESCRIPTION");
                        dgv.Columns.Add("Qty", "TOTAL QTY");
                        dgv.Columns.Add("Taxable", "TAXABLE AMT");
                        dgv.Columns.Add("CgstPct", "CGST %");
                        dgv.Columns.Add("CgstAmt", "CGST AMT");
                        dgv.Columns.Add("SgstPct", "SGST %");
                        dgv.Columns.Add("SgstAmt", "SGST AMT");
                        dgv.Columns.Add("IgstPct", "IGST %");
                        dgv.Columns.Add("IgstAmt", "IGST AMT");
                        dgv.Columns.Add("Total", "TOTAL AMT");
                        // FIX: Group by (HsnCode + all three rate %) so mixed-rate
                        // invoices for the same HSN each get their own row — matching
                        // the GSTR-1 HSN summary filing requirement.
                        var hsn = db.InvoiceItems
                            .Where(i => i.CompanyProfileId == cid)
                            .Join(db.InvoiceHeaders, i => i.InvoiceId, h => h.InvoiceId, (i, h) => new { i, h })
                            .Where(x => x.h.InvoiceDate >= dateFrom && x.h.InvoiceDate <= dateTo)
                            .AsEnumerable()
                            .GroupBy(x => new
                            {
                                Hsn = x.i.HsnCode ?? "—",
                                CgstPct = x.h.CgstPct ?? 0,
                                SgstPct = x.h.SgstPct ?? 0,
                                IgstPct = x.h.IgstPct ?? 0
                            })
                            .Select(g => new
                            {
                                g.Key.Hsn,
                                Desc = db.DesignMasters.FirstOrDefault(d => d.HsnCode == g.Key.Hsn)?.DesignName ?? "—",
                                Qty = g.Sum(x => x.i.Qty ?? 0),
                                Taxable = g.Sum(x => x.i.Amount ?? 0),
                                g.Key.CgstPct,
                                g.Key.SgstPct,
                                g.Key.IgstPct
                            })
                            .OrderBy(x => x.Hsn).ThenBy(x => x.CgstPct).ToList();
                        foreach (var r in hsn)
                        {
                            decimal cgstA = Math.Round(r.Taxable * r.CgstPct / 100, 2);
                            decimal sgstA = Math.Round(r.Taxable * r.SgstPct / 100, 2);
                            decimal igstA = Math.Round(r.Taxable * r.IgstPct / 100, 2);
                            decimal tot = r.Taxable + cgstA + sgstA + igstA;
                            dgv.Rows.Add(r.Hsn, r.Desc, r.Qty.ToString("N3"),
                                "₹" + r.Taxable.ToString("N2"),
                                r.CgstPct + "%", "₹" + cgstA.ToString("N2"),
                                r.SgstPct + "%", "₹" + sgstA.ToString("N2"),
                                r.IgstPct + "%", "₹" + igstA.ToString("N2"),
                                "₹" + tot.ToString("N2"));
                        }
                        break;
                }
            }
            catch (Exception ex) { MessageBox.Show("Report error: " + ex.Message); }
        }

        void ExportCsv(object sender, EventArgs e)
        {
            if (dgv.Columns.Count == 0 || dgv.Rows.Count == 0)
            {
                MessageBox.Show("Please run a report first before exporting.",
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string reportName = cmbReport.SelectedItem?.ToString() ?? "Report";
            using var sfd = new SaveFileDialog
            {
                Title = "Export Report as CSV",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"{reportName.Replace(" ", "_")}_{DateTime.Today:yyyyMMdd}.csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                var sb = new System.Text.StringBuilder();
                var headers = new System.Collections.Generic.List<string>();
                foreach (DataGridViewColumn col in dgv.Columns)
                    headers.Add($"\"{col.HeaderText}\"");
                sb.AppendLine(string.Join(",", headers));
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    var cells = new System.Collections.Generic.List<string>();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        string val = cell.Value?.ToString() ?? "";
                        cells.Add($"\"{val.Replace("\"", "\"\"")}\"");
                    }
                    sb.AppendLine(string.Join(",", cells));
                }
                System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                MessageBox.Show($"Report exported successfully!\n\n{sfd.FileName}",
                    "Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // FIX 1 — ExportExcel was incorrectly placed OUTSIDE UC_Reports closing brace in the original
        void ExportExcel(object sender, EventArgs e)
        {
            if (dgv.Columns.Count == 0 || dgv.Rows.Count == 0)
            {
                MessageBox.Show("Please run a report first before exporting.",
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string reportName = cmbReport.SelectedItem?.ToString() ?? "Report";
            using var sfd = new SaveFileDialog
            {
                Title = "Export Report as Excel",
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"{reportName.Replace(" ", "_")}_{DateTime.Today:yyyyMMdd}.xlsx"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            try
            {
                ExcelExportHelper.Export(dgv, sfd.FileName, reportName);
                // FIX 2 — was a raw multiline string literal
                MessageBox.Show($"Report exported to Excel successfully!\n\n{sfd.FileName}",
                    "Exported", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel export error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }   // ← end of UC_Reports


    // ══════════════════════════════════════════════════════════════════
    //  UC_Settings  — tab-based organised layout
    // ══════════════════════════════════════════════════════════════════
    public partial class UC_Settings : UserControl
    {
        static readonly Color Bg = Color.FromArgb(245, 247, 250);
        static readonly Color Surface = Color.White;
        static readonly Color Surface2 = Color.FromArgb(248, 249, 252);
        static readonly Color Blue = Color.FromArgb(37, 99, 235);
        static readonly Color BlueHov = Color.FromArgb(29, 78, 216);
        static readonly Color BluePale = Color.FromArgb(239, 246, 255);
        static readonly Color Green = Color.FromArgb(22, 163, 74);
        static readonly Color GreenPale = Color.FromArgb(240, 253, 244);
        static readonly Color Red = Color.FromArgb(220, 38, 38);
        static readonly Color Amber = Color.FromArgb(180, 83, 9);
        static readonly Color AmberPale = Color.FromArgb(255, 251, 235);
        static readonly Color Border = Color.FromArgb(226, 232, 240);
        static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        static readonly Color TextMid = Color.FromArgb(71, 85, 105);
        static readonly Color TextLight = Color.FromArgb(148, 163, 184);

        TextBox txtName, txtGstin, txtPan, txtPhone, txtEmail;
        TextBox txtCity, txtState, txtPincode, txtAdd1, txtAdd2, txtInvPrefix;
        DataGridView dgvUsers;
        Panel _activeTabContent;
        Button _activeTabBtn;

        public UC_Settings()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Bg;
            Build();
            LoadData();
        }

        void Build()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Bg };
            top.Controls.Add(new Label
            {
                Text = "Settings",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, 8)
            });
            top.Controls.Add(new Label
            {
                Text = "Configure your company profile, users and data backup",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(27, 42)
            });

            var tabBar = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Surface };
            tabBar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border), 0, tabBar.Height - 1, tabBar.Width, tabBar.Height - 1);

            var tabCompany = BuildTabCompany();
            var tabUsers = BuildTabUsers();
            var tabBackup = BuildTabBackup();

            var tabAbout = BuildTabAbout();

            foreach (var tp in new[] { tabCompany, tabUsers, tabBackup, tabAbout })
            {
                tp.Dock = DockStyle.Fill;
                tp.Visible = false;
                this.Controls.Add(tp);
            }

            var indicator = new Panel
            {
                BackColor = Blue,
                Size = new Size(160, 3),
                Location = new Point(0, 45)
            };
            tabBar.Controls.Add(indicator);

            Button MakeTab(string icon, string label, Panel content, int x)
            {
                var btn = new Button
                {
                    Text = icon + "  " + label,
                    Font = new Font("Segoe UI Emoji", 9F, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Surface,
                    ForeColor = TextMid,
                    Size = new Size(160, 48),
                    Location = new Point(x, 0),
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false,
                    TabStop = false
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(248, 249, 252);
                btn.Click += (s, e) => SwitchTab(btn, content, indicator,
                    new[] { tabCompany, tabUsers, tabBackup, tabAbout }, tabBar);
                tabBar.Controls.Add(btn);
                return btn;
            }

            var btnCompany = MakeTab("🏢", "Company Profile", tabCompany, 0);
            var btnUsers = MakeTab("👥", "Users", tabUsers, 160);
            var btnBackup = MakeTab("🗄", "Backup & Data", tabBackup, 320);
            var btnAbout = MakeTab("ℹ️", "About", tabAbout, 480);

            SwitchTab(btnCompany, tabCompany, indicator,
                new[] { tabCompany, tabUsers, tabBackup, tabAbout }, tabBar);

            this.Controls.Add(tabBar);
            this.Controls.Add(top);
        }

        // ════════════════════════════════════════════════════════════
        //  TAB 4 — ABOUT
        // ════════════════════════════════════════════════════════════
        Panel BuildTabAbout()
        {
            var tab = new Panel { BackColor = Bg, AutoScroll = true };

            var card = new Panel
            {
                BackColor = Surface,
                Location = new Point(24, 20),
                Size = new Size(520, 340),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });

            var sec = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Surface2 };
            sec.Controls.Add(new Label
            {
                Text = "ℹ️  About This Application",
                Font = new Font("Segoe UI Emoji", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 12)
            });
            card.Controls.Add(sec);

            int y = 60;
            void Row(string label, string value, Color valColor)
            {
                card.Controls.Add(new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = TextMid,
                    AutoSize = true,
                    Location = new Point(24, y)
                });
                card.Controls.Add(new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = valColor,
                    AutoSize = true,
                    Location = new Point(200, y)
                });
                y += 32;
            }

            Row("Application", Program.AppName, TextDark);
            Row("Version", "v" + Program.AppVersion, Blue);
            Row("Build Year", Program.BuildDate, TextMid);
            Row("Company", SessionManager.CompanyName, TextDark);
            Row("Database", GetDbServer(), TextMid);
            Row("Framework", ".NET 6 / WinForms", TextMid);

            y += 8;
            card.Controls.Add(new Panel
            {
                BackColor = Border,
                Location = new Point(24, y),
                Size = new Size(460, 1)
            });
            y += 16;

            card.Controls.Add(new Label
            {
                Text = "© " + Program.BuildDate + " " + Program.AppName + "\n" +
                            "All rights reserved. Unauthorized copying is prohibited.",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(24, y)
            });

            card.Height = y + 48;
            tab.Controls.Add(card);
            return tab;
        }

        string GetDbServer()
        {
            try
            {
                using var db = new AppDbContext();
                var cs = db.Database.GetConnectionString() ?? "";
                // Extract Server value only — don't expose credentials
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cs);
                return builder.DataSource;
            }
            catch { return "—"; }
        }

        void SwitchTab(Button btn, Panel content, Panel indicator,
            Panel[] allTabs, Panel tabBar)
        {
            foreach (var tp in allTabs) tp.Visible = false;
            foreach (Button b in tabBar.Controls.OfType<Button>())
            { b.ForeColor = TextMid; b.BackColor = Surface; }
            content.Visible = true;
            btn.ForeColor = Blue;
            btn.BackColor = BluePale;
            indicator.Location = new Point(btn.Left, 45);
            _activeTabBtn = btn;
            _activeTabContent = content;
        }

        // ════════════════════════════════════════════════════════════
        //  TAB 1 — COMPANY PROFILE
        // ════════════════════════════════════════════════════════════
        Panel BuildTabCompany()
        {
            var tab = new Panel { BackColor = Bg, AutoScroll = true };

            var card = new Panel
            {
                BackColor = Surface,
                Location = new Point(24, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });

            var sec = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Surface2 };
            sec.Controls.Add(new Label
            {
                Text = "🏢  Company Information",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 12)
            });
            card.Controls.Add(sec);

            int y = 64, gap = 52;
            int lx = 24, fx = 200, fw = 280;
            int rx = fx + fw + 40, rfx = rx + (fx - lx);

            AddFieldLabel(card, "Company Name *", lx, y);
            txtName = AddField(card, fx, y, fw * 2 + 40);
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            y += gap;

            AddFieldLabel(card, "GSTIN", lx, y);
            txtGstin = AddField(card, fx, y, fw, upper: true);
            AddFieldLabel(card, "PAN", rx, y);
            txtPan = AddField(card, rfx, y, 160, upper: true);
            y += gap;

            AddFieldLabel(card, "Phone", lx, y);
            txtPhone = AddField(card, fx, y, 180);
            AddFieldLabel(card, "Email", rx, y);
            txtEmail = AddField(card, rfx, y, fw);
            y += gap;

            AddFieldLabel(card, "City", lx, y);
            txtCity = AddField(card, fx, y, 180);
            AddFieldLabel(card, "State", rx, y);
            txtState = AddField(card, rfx, y, 180);
            y += gap;

            AddFieldLabel(card, "Pincode", lx, y);
            txtPincode = AddField(card, fx, y, 130);
            y += gap;

            card.Controls.Add(new Panel
            {
                BackColor = Border,
                Height = 1,
                Location = new Point(0, y - 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            });
            card.Controls.Add(new Label
            {
                Text = "Address Details",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(lx, y)
            });
            y += 24;

            AddFieldLabel(card, "Address Line 1", lx, y);
            txtAdd1 = AddField(card, fx, y, fw * 2 + 40);
            txtAdd1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            y += gap;

            AddFieldLabel(card, "Address Line 2", lx, y);
            txtAdd2 = AddField(card, fx, y, fw * 2 + 40);
            txtAdd2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            y += gap + 8;

            // ── Invoice Number Prefix ─────────────────────────────────
            card.Controls.Add(new Panel
            {
                BackColor = Border,
                Height = 1,
                Location = new Point(0, y - 8),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            });
            card.Controls.Add(new Label
            {
                Text = "Invoice Numbering",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(lx, y)
            });
            y += 24;

            AddFieldLabel(card, "Invoice Prefix", lx, y);
            txtInvPrefix = AddField(card, fx, y, 160, upper: true);
            txtInvPrefix.PlaceholderText = "e.g. RR or INV";
            var lblPreview = new Label
            {
                Text = "Preview: " + InvoiceNumberHelper.Preview(1),
                Font = new Font("Consolas", 9F),
                ForeColor = Blue,
                AutoSize = true,
                Location = new Point(fx + 172, y + 6)
            };
            card.Controls.Add(lblPreview);
            txtInvPrefix.TextChanged += (s, e) =>
            {
                InvoiceNumberHelper.SetPrefix(txtInvPrefix.Text.Trim().ToUpper());
                lblPreview.Text = "Preview: " + InvoiceNumberHelper.Preview(1);
            };
            y += gap;
            card.Controls.Add(new Label
            {
                Text = "Format: PREFIX/YYMM/000001  —  leave blank for plain 000001",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Italic),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(fx, y - 28)
            });

            var btnSave = MkBtn("💾  Save Company Profile", Blue, BlueHov);
            btnSave.Size = new Size(200, 38);
            btnSave.Location = new Point(fx, y);
            btnSave.Click += SaveData;
            card.Controls.Add(btnSave);

            card.Controls.Add(new Label
            {
                Text = "This information appears on your printed invoices.",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(fx + 212, y + 11)
            });

            y += 56;
            card.Height = y;

            tab.Resize += (s, e) =>
            {
                card.Width = System.Math.Max(500, tab.Width - 48);
                int cw = card.Width - fx - 24;
                txtName.Width = System.Math.Min(fw * 2 + 40, cw);
                txtAdd1.Width = System.Math.Min(fw * 2 + 40, cw);
                txtAdd2.Width = System.Math.Min(fw * 2 + 40, cw);
                var div = card.Controls.OfType<Panel>().FirstOrDefault(p => p.Height == 1);
                if (div != null) div.Width = card.Width;
            };

            tab.Controls.Add(card);
            return tab;
        }

        // ════════════════════════════════════════════════════════════
        //  TAB 2 — USERS
        // ════════════════════════════════════════════════════════════
        Panel BuildTabUsers()
        {
            var tab = new Panel { BackColor = Bg };

            var card = new Panel
            {
                BackColor = Surface,
                Location = new Point(24, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Blue });

            var sec = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Surface2 };
            sec.Controls.Add(new Label
            {
                Text = "👥  User Accounts",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 12)
            });
            var btnAddUser = MkBtn("➕  Add User", Blue, BlueHov);
            btnAddUser.Size = new Size(110, 28);
            btnAddUser.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAddUser.Location = new Point(sec.Width - 126, 8);
            sec.Resize += (s, e) => btnAddUser.Location = new Point(sec.Width - 126, 8);
            btnAddUser.Click += ShowAddUserDialog;
            sec.Controls.Add(btnAddUser);
            card.Controls.Add(sec);

            var banner = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = BluePale };
            banner.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 3, BackColor = Blue });
            banner.Controls.Add(new Label
            {
                Text = "ℹ️  Each user belongs to this company only. Use Enable/Disable to control access without deleting accounts.",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextMid,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 8, 0)
            });
            card.Controls.Add(banner);

            dgvUsers = BuildUserDgv();
            dgvUsers.Dock = DockStyle.Fill;
            card.Controls.Add(dgvUsers);

            tab.Resize += (s, e) =>
            {
                card.Width = System.Math.Max(400, tab.Width - 48);
                card.Height = System.Math.Max(300, tab.Height - 48);
            };

            tab.Controls.Add(card);
            LoadUserGrid();
            return tab;
        }

        // ════════════════════════════════════════════════════════════
        //  TAB 3 — BACKUP & DATA
        // ════════════════════════════════════════════════════════════
        Panel BuildTabBackup()
        {
            var tab = new Panel { BackColor = Bg, AutoScroll = true };
            var cardBk = MakeCard("🗄  Database Backup", Amber, 20);

            string markerFile = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "TextileInvoiceApp", "last_backup.txt");

            string lastTs = "Never";
            if (System.IO.File.Exists(markerFile))
                try { lastTs = System.IO.File.ReadAllText(markerFile).Trim(); } catch { }

            int by = 60;
            var pnlStatus = new Panel
            {
                BackColor = lastTs == "Never" ? Color.FromArgb(255, 251, 235) : GreenPale,
                Location = new Point(24, by),
                Size = new Size(600, 48),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var dotColor = lastTs == "Never" ? Amber : Green;
            pnlStatus.Controls.Add(new Panel { Location = new Point(14, 18), Size = new Size(10, 10), BackColor = dotColor });
            var lblStatus = new Label
            {
                Text = lastTs == "Never"
                    ? "⚠️  No backup found — we recommend backing up before major data entry."
                    : $"✓  Last backup: {lastTs}",
                Font = new Font("Segoe UI", 9F, lastTs == "Never" ? FontStyle.Regular : FontStyle.Bold),
                ForeColor = dotColor,
                AutoSize = true,
                Location = new Point(34, 15)
            };
            pnlStatus.Controls.Add(lblStatus);
            cardBk.Controls.Add(pnlStatus);
            by += 60;

            cardBk.Controls.Add(new Label
            {
                Text = "A full SQL Server backup includes all invoices, accounts, designs, services and company settings.\n" +
                       "Save the .bak file to an external drive or cloud storage (Google Drive, OneDrive) for safety.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMid,
                Location = new Point(24, by),
                Size = new Size(620, 36),
                AutoSize = false
            });
            by += 48;

            var btnBackup = MkBtn("🗄  Backup Now", Amber, Color.FromArgb(146, 64, 14));
            btnBackup.Size = new Size(160, 40); btnBackup.Location = new Point(24, by);
            btnBackup.Font = new Font("Segoe UI Emoji", 10F, FontStyle.Bold);
            btnBackup.Click += (s, e) => DoBackup(lblStatus, pnlStatus, markerFile);
            cardBk.Controls.Add(btnBackup);
            cardBk.Controls.Add(new Label
            {
                Text = "💡 Recommended: backup daily or before any bulk data entry session.",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(200, by + 12)
            });
            by += 58;
            cardBk.Height = by;

            var cardInfo = MakeCard("ℹ️  Application Information", Blue, cardBk.Bottom + 20);
            int iy = 60;
            void InfoRow(string label, string value, Color valColor)
            {
                cardInfo.Controls.Add(new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = TextMid,
                    AutoSize = true,
                    Location = new Point(24, iy)
                });
                cardInfo.Controls.Add(new Label
                {
                    Text = value,
                    Font = new Font("Consolas", 9F),
                    ForeColor = valColor,
                    AutoSize = true,
                    Location = new Point(220, iy)
                });
                iy += 30;
            }
            InfoRow("Application", "Textile Invoice Management System", TextDark);
            InfoRow("Version", "v2.0.2026", Blue);
            InfoRow("Company", SessionManager.CompanyName, TextDark);
            InfoRow("Logged in as", SessionManager.FullName + " (" + SessionManager.Username + ")", TextMid);
            InfoRow("Developed by", "Seema IT Solutions", Amber);
            iy += 8;
            cardInfo.Height = iy;

            var cardDanger = MakeCard("⚠️  Danger Zone", Red, cardInfo.Bottom + 20);
            int dy2 = 60;
            var warnBanner = new Panel
            {
                BackColor = Color.FromArgb(254, 242, 242),
                Location = new Point(24, dy2),
                Size = new Size(600, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            warnBanner.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 3, BackColor = Red });
            warnBanner.Controls.Add(new Label
            {
                Text = "Actions in this section are irreversible. Proceed with extreme caution.",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Red,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            });
            cardDanger.Controls.Add(warnBanner);
            dy2 += 52;
            cardDanger.Controls.Add(new Label
            {
                Text = "Change Your Password",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(24, dy2)
            });
            cardDanger.Controls.Add(new Label
            {
                Text = "Update the password for your current login account.",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextMid,
                AutoSize = true,
                Location = new Point(24, dy2 + 22)
            });
            var btnChgPwd = new Button
            {
                Text = "🔑  Change Password",
                Font = new Font("Segoe UI Emoji", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(254, 242, 242),
                ForeColor = Red,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(160, 34),
                Location = new Point(440, dy2 + 8),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnChgPwd.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
            btnChgPwd.FlatAppearance.BorderSize = 1;
            btnChgPwd.FlatAppearance.MouseOverBackColor = Color.FromArgb(252, 230, 230);
            btnChgPwd.Click += (s, e) =>
                ShowChangePwdDialog(SessionManager.UserId, SessionManager.Username);
            cardDanger.Controls.Add(btnChgPwd);
            dy2 += 68;
            cardDanger.Height = dy2;

            tab.Resize += (s, e) =>
            {
                int cw = System.Math.Max(400, tab.Width - 48);
                cardBk.Width = cw; cardInfo.Width = cw; cardDanger.Width = cw;
                pnlStatus.Width = cw - 48; warnBanner.Width = cw - 48;
                btnChgPwd.Location = new Point(cw - 184, btnChgPwd.Top);
            };

            tab.Controls.Add(cardDanger);
            tab.Controls.Add(cardInfo);
            tab.Controls.Add(cardBk);
            return tab;
        }

        // ════════════════════════════════════════════════════════════
        //  STARTUP BACKUP REMINDER (called from Program.cs)
        // ════════════════════════════════════════════════════════════
        public static void CheckBackupReminder()
        {
            string markerFile = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "TextileInvoiceApp", "last_backup.txt");

            bool shouldRemind = false;
            string reason = "";

            if (!System.IO.File.Exists(markerFile))
            {
                shouldRemind = true;
                reason = "You have never taken a database backup.";
            }
            else
            {
                try
                {
                    string ts = System.IO.File.ReadAllText(markerFile).Trim();
                    if (DateTime.TryParseExact(ts, "dd MMM yyyy, hh:mm tt",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out DateTime lastDt))
                    {
                        int daysSince = (int)(DateTime.Now - lastDt).TotalDays;
                        if (daysSince >= 7)
                        {
                            shouldRemind = true;
                            reason = $"Your last backup was {daysSince} days ago ({ts}).";
                        }
                    }
                }
                catch { shouldRemind = true; reason = "Could not read last backup date."; }
            }

            if (!shouldRemind) return;

            // FIX 3 — was raw multiline string literal with broken interpolation
            var result = System.Windows.Forms.MessageBox.Show(
                $"⚠️  Backup Reminder\n\n{reason}\n\n" +
                "Regular backups protect your invoice data from accidental loss.\n\n" +
                "Go to Settings → Backup & Data to create a backup now.\n\n" +
                "Remind me again tomorrow?",
                "Database Backup Reminder",
                System.Windows.Forms.MessageBoxButtons.YesNo,
                System.Windows.Forms.MessageBoxIcon.Warning);

            if (result == System.Windows.Forms.DialogResult.No)
            {
                try
                {
                    string snoozeTs = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(markerFile)!);
                    System.IO.File.WriteAllText(markerFile, snoozeTs);
                }
                catch { }
            }
        }

        Panel MakeCard(string title, Color accent, int top)
        {
            var card = new Panel
            {
                BackColor = Surface,
                Location = new Point(24, top),
                Size = new Size(700, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = accent });
            var sec = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Surface2 };
            sec.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 12)
            });
            card.Controls.Add(sec);
            return card;
        }

        // ════════════════════════════════════════════════════════════
        //  COMPANY DATA
        // ════════════════════════════════════════════════════════════
        void LoadData()
        {
            try
            {
                using var db = new AppDbContext();
                var c = db.CompanyProfiles.Find(SessionManager.CompanyProfileId);
                if (c == null) return;
                txtName.Text = c.CompanyName;
                txtGstin.Text = c.Gstin ?? "";
                txtPan.Text = c.Pan ?? "";
                txtPhone.Text = c.Phone ?? "";
                txtEmail.Text = c.Email ?? "";
                txtCity.Text = c.City ?? "";
                txtState.Text = c.State ?? "";
                txtPincode.Text = c.Pincode ?? "";
                txtAdd1.Text = c.Address1 ?? "";
                txtAdd2.Text = c.Address2 ?? "";
                string savedPrefix = InvoiceNumberHelper.LoadPrefix();
                txtInvPrefix.Text = savedPrefix;
                InvoiceNumberHelper.SetPrefix(savedPrefix);
            }
            catch (Exception ex) { MessageBox.Show("Load error: " + ex.Message); }
        }

        void SaveData(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Company Name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // FIX: Validate company GSTIN format if provided
            string compGstin = txtGstin.Text.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(compGstin))
            {
                var gstinRegex = new System.Text.RegularExpressions.Regex(
                    @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$");
                if (!gstinRegex.IsMatch(compGstin))
                {
                    var res = MessageBox.Show(
                        $"Company GSTIN \"{compGstin}\" does not appear to be in a valid format.\n\n" +
                        "Expected: 22AAAAA0000A1Z5 (15 characters)\n\nSave anyway?",
                        "GSTIN Validation Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
                    if (res != DialogResult.Yes) { txtGstin.Focus(); return; }
                }
            }
            try
            {
                using var db = new AppDbContext();
                var c = db.CompanyProfiles.Find(SessionManager.CompanyProfileId);
                if (c == null) return;
                c.CompanyName = txtName.Text.Trim();
                c.Gstin = txtGstin.Text.Trim();
                c.Pan = txtPan.Text.Trim();
                c.Phone = txtPhone.Text.Trim();
                c.Email = txtEmail.Text.Trim();
                c.City = txtCity.Text.Trim();
                c.State = txtState.Text.Trim();
                c.Pincode = txtPincode.Text.Trim();
                c.Address1 = txtAdd1.Text.Trim();
                c.Address2 = txtAdd2.Text.Trim();
                db.SaveChanges();
                SessionManager.CompanyName = c.CompanyName;
                InvoiceNumberHelper.SetPrefix(txtInvPrefix.Text.Trim().ToUpper());
                InvoiceNumberHelper.SavePrefix(txtInvPrefix.Text.Trim().ToUpper());
                Toast("✓  Company profile saved successfully!");
            }
            catch (Exception ex) { MessageBox.Show("Save error: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════
        //  USER GRID
        // ════════════════════════════════════════════════════════════
        DataGridView BuildUserDgv()
        {
            var g = new DataGridView
            {
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
                ScrollBars = ScrollBars.Vertical
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Surface2;
            g.ColumnHeadersDefaultCellStyle.ForeColor = TextMid;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Surface2;
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            g.ColumnHeadersHeight = 40;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.DefaultCellStyle.BackColor = Surface;
            g.DefaultCellStyle.ForeColor = TextMid;
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            g.DefaultCellStyle.SelectionBackColor = BluePale;
            g.DefaultCellStyle.SelectionForeColor = TextDark;
            g.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            g.RowTemplate.Height = 42;

            g.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserId", Visible = false });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "USERNAME", Name = "Username", MinimumWidth = 130, FillWeight = 20 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "FULL NAME", Name = "FullName", MinimumWidth = 180, FillWeight = 28 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "STATUS", Name = "Status", MinimumWidth = 90, FillWeight = 12 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "ChgPwd", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 130 });
            g.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Name = "Toggle", ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, Width = 110 });

            g.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int cp = g.Columns["ChgPwd"]?.Index ?? -1, ct = g.Columns["Toggle"]?.Index ?? -1;
                if (e.ColumnIndex == cp || e.ColumnIndex == ct)
                { e.Value = ""; e.FormattingApplied = true; }
                if (e.ColumnIndex == g.Columns["Status"]?.Index)
                {
                    bool active = e.Value?.ToString() == "Active";
                    e.CellStyle.ForeColor = active ? Green : Red;
                    e.CellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    e.FormattingApplied = true;
                }
            };

            g.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int cp = g.Columns["ChgPwd"]?.Index ?? -1, ct = g.Columns["Toggle"]?.Index ?? -1;
                if (e.ColumnIndex != cp && e.ColumnIndex != ct) return;
                e.Handled = true; e.PaintBackground(e.ClipBounds, true);
                var rc = new Rectangle(e.CellBounds.X + 6, e.CellBounds.Y + 9,
                    e.CellBounds.Width - 12, e.CellBounds.Height - 18);
                Color bg, fg, bdr; string lbl;
                if (e.ColumnIndex == cp)
                { bg = BluePale; fg = Blue; bdr = Color.FromArgb(191, 219, 254); lbl = "🔑  Change Pwd"; }
                else
                {
                    bool isActive = g.Rows[e.RowIndex].Cells["Status"].Value?.ToString() == "Active";
                    bg = isActive ? Color.FromArgb(254, 242, 242) : GreenPale;
                    fg = isActive ? Red : Green;
                    bdr = isActive ? Color.FromArgb(254, 202, 202) : Color.FromArgb(187, 247, 208);
                    lbl = isActive ? "🚫  Disable" : "✅  Enable";
                }
                var gr = e.Graphics;
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var path = RRect(rc, 5);
                gr.FillPath(new SolidBrush(bg), path);
                gr.DrawPath(new Pen(bdr), path);
                using var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                gr.DrawString(lbl, new Font("Segoe UI", 8F, FontStyle.Bold), new SolidBrush(fg), rc, sf);
            };

            g.CellMouseMove += (s, e) =>
            {
                int cp = g.Columns["ChgPwd"]?.Index ?? -1, ct = g.Columns["Toggle"]?.Index ?? -1;
                g.Cursor = e.RowIndex >= 0 && (e.ColumnIndex == cp || e.ColumnIndex == ct)
                    ? Cursors.Hand : Cursors.Default;
            };

            g.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int uid = Convert.ToInt32(g.Rows[e.RowIndex].Cells["UserId"].Value);
                string un = g.Rows[e.RowIndex].Cells["Username"].Value?.ToString() ?? "";
                int cp = g.Columns["ChgPwd"]?.Index ?? -1, ct = g.Columns["Toggle"]?.Index ?? -1;
                if (e.ColumnIndex == cp) ShowChangePwdDialog(uid, un);
                if (e.ColumnIndex == ct) ToggleUserActive(uid, un);
            };

            g.RowsAdded += (s, e) =>
            {
                for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                    if (i >= 0 && i < g.Rows.Count)
                        g.Rows[i].DefaultCellStyle.BackColor =
                            i % 2 == 0 ? Surface : Color.FromArgb(250, 251, 253);
            };
            g.RowPostPaint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(241, 245, 249), 1);
                e.Graphics.DrawLine(p, e.RowBounds.Left, e.RowBounds.Bottom - 1,
                    e.RowBounds.Right, e.RowBounds.Bottom - 1);
            };
            return g;
        }

        void LoadUserGrid()
        {
            if (dgvUsers == null) return;
            dgvUsers.Rows.Clear();
            try
            {
                using var db = new AppDbContext();
                var users = db.AppUsers
                    .Where(u => u.CompanyProfileId == SessionManager.CompanyProfileId).ToList();
                foreach (var u in users)
                    dgvUsers.Rows.Add(u.UserId, u.Username, u.FullName ?? u.Username,
                        u.IsActive == true ? "Active" : "Inactive");
            }
            catch (Exception ex) { MessageBox.Show("Load users error: " + ex.Message); }
        }

        void ShowAddUserDialog(object sender, EventArgs e)
        {
            using var dlg = new Form
            {
                Text = "Add New User",
                Size = new Size(420, 360),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Bg
            };
            int dy = 20;
            TextBox AddF(string lbl)
            {
                dlg.Controls.Add(new Label
                {
                    Text = lbl,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = TextMid,
                    AutoSize = true,
                    Location = new Point(24, dy)
                });
                dy += 22;
                var t = new TextBox
                {
                    Font = new Font("Segoe UI", 10F),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Surface2,
                    ForeColor = TextDark,
                    Size = new Size(360, 28),
                    Location = new Point(24, dy)
                };
                dlg.Controls.Add(t); dy += 44; return t;
            }
            var tUser = AddF("Username *");
            var tFull = AddF("Full Name *");
            var tPwd = AddF("Password *"); tPwd.UseSystemPasswordChar = true;
            var tConf = AddF("Confirm Password *"); tConf.UseSystemPasswordChar = true;

            var btnOk = new Button
            {
                Text = "✓  Add User",
                BackColor = Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 36),
                Location = new Point(24, dy),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnCx = new Button
            {
                Text = "Cancel",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(90, 36),
                Location = new Point(174, dy),
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            dlg.Controls.Add(btnOk); dlg.Controls.Add(btnCx);
            dlg.CancelButton = btnCx;

            btnOk.Click += (s2, e2) =>
            {
                if (string.IsNullOrWhiteSpace(tUser.Text)) { MessageBox.Show("Username is required."); return; }
                if (string.IsNullOrWhiteSpace(tFull.Text)) { MessageBox.Show("Full Name is required."); return; }
                if (tPwd.Text.Length < 6) { MessageBox.Show("Password must be at least 6 characters."); return; }
                if (tPwd.Text != tConf.Text) { MessageBox.Show("Passwords do not match."); return; }
                try
                {
                    using var db = new AppDbContext();
                    if (db.AppUsers.Any(u => u.Username == tUser.Text.Trim()))
                    { MessageBox.Show("Username already exists."); return; }
                    db.AppUsers.Add(new AppUser
                    {
                        CompanyProfileId = SessionManager.CompanyProfileId,
                        Username = tUser.Text.Trim(),
                        PasswordHash = tPwd.Text,
                        FullName = tFull.Text.Trim(),
                        IsActive = true
                    });
                    db.SaveChanges();
                    dlg.DialogResult = DialogResult.OK; dlg.Close();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            { LoadUserGrid(); Toast("✓  User added successfully!"); }
        }

        void ShowChangePwdDialog(int userId, string username)
        {
            using var dlg = new Form
            {
                Text = $"Change Password — {username}",
                Size = new Size(400, 270),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Bg
            };
            int dy = 20;
            TextBox AddPwd(string lbl)
            {
                dlg.Controls.Add(new Label
                {
                    Text = lbl,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                    ForeColor = TextMid,
                    AutoSize = true,
                    Location = new Point(24, dy)
                });
                dy += 22;
                var t = new TextBox
                {
                    Font = new Font("Segoe UI", 10F),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Surface2,
                    ForeColor = TextDark,
                    Size = new Size(340, 28),
                    Location = new Point(24, dy),
                    UseSystemPasswordChar = true
                };
                dlg.Controls.Add(t); dy += 44; return t;
            }
            var tNew = AddPwd("New Password *");
            var tConf = AddPwd("Confirm New Password *");

            var btnOk = new Button
            {
                Text = "✓  Change Password",
                BackColor = Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(168, 36),
                Location = new Point(24, dy),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnCx = new Button
            {
                Text = "Cancel",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(90, 36),
                Location = new Point(202, dy),
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            dlg.Controls.Add(btnOk); dlg.Controls.Add(btnCx);
            dlg.CancelButton = btnCx;

            btnOk.Click += (s, e) =>
            {
                if (tNew.Text.Length < 6) { MessageBox.Show("Password must be at least 6 characters."); return; }
                if (tNew.Text != tConf.Text) { MessageBox.Show("Passwords do not match."); return; }
                try
                {
                    using var db = new AppDbContext();
                    var u = db.AppUsers.Find(userId);
                    if (u == null) return;
                    u.PasswordHash = tNew.Text;
                    db.SaveChanges();
                    dlg.DialogResult = DialogResult.OK; dlg.Close();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                Toast("✓  Password changed successfully!");
        }

        void ToggleUserActive(int userId, string username)
        {
            try
            {
                using var db = new AppDbContext();
                var u = db.AppUsers.Find(userId);
                if (u == null) return;
                bool nowActive = !(u.IsActive == true);
                if (!nowActive && userId == SessionManager.UserId)
                {
                    MessageBox.Show("You cannot disable your own account.", "Not Allowed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string action = nowActive ? "enable" : "disable";
                if (MessageBox.Show($"Are you sure you want to {action} user \"{username}\"?",
                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                u.IsActive = nowActive;
                db.SaveChanges();
                LoadUserGrid();
                Toast($"✓  User {(nowActive ? "enabled" : "disabled")} successfully!");
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════
        //  DATABASE BACKUP
        // ════════════════════════════════════════════════════════════
        void DoBackup(Label lblStatus, Panel pnlStatus, string markerFile)
        {
            string dbName, serverCs;
            try
            {
                using var db = new AppDbContext();
                serverCs = db.Database.GetConnectionString()!;
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(serverCs);
                dbName = builder.InitialCatalog;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot read connection info: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Title = "Save Database Backup",
                Filter = "SQL Server Backup (*.bak)|*.bak",
                FileName = $"{dbName}_backup_{DateTime.Now:yyyyMMdd_HHmm}.bak",
                DefaultExt = "bak"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            string backupPath = sfd.FileName;
            var prog = new Form
            {
                Text = "Backing up…",
                Size = new Size(360, 110),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ControlBox = false
            };
            prog.Controls.Add(new Label
            {
                Text = "⏳  Creating backup, please wait…",
                Font = new Font("Segoe UI", 10F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            prog.Show(this.FindForm());
            prog.Refresh();

            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(serverCs);
                conn.Open();
                string safePath = backupPath.Replace("'", "''");
                string sql = "BACKUP DATABASE [" + dbName + "] " +
                             "TO DISK = N'" + safePath + "' " +
                             "WITH FORMAT, MEDIANAME = N'TextileInvoice', " +
                             "NAME = N'" + dbName + " Full Backup', " +
                             "COMPRESSION, STATS = 10";
                using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
                cmd.CommandTimeout = 300;
                cmd.ExecuteNonQuery();

                string ts = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt");
                try
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(markerFile)!);
                    System.IO.File.WriteAllText(markerFile, ts);
                    lblStatus.Text = $"✓  Last backup: {ts}";
                    lblStatus.ForeColor = Green;
                    pnlStatus.BackColor = GreenPale;
                }
                catch { }

                prog.Close();
                var result = MessageBox.Show(
                    $"✓  Backup completed successfully!\n\nSaved to:\n{backupPath}\n\nOpen the folder?",
                    "Backup Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{backupPath}\"");
            }
            catch (Exception ex)
            {
                prog.Close();
                MessageBox.Show(
                    "Backup failed:\n\n" + ex.Message + "\n\n" +
                    "Ensure the SQL Server service account has write permission to the target folder.",
                    "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════════
        void AddFieldLabel(Panel p, string text, int x, int y) =>
            p.Controls.Add(new Label
            {
                Text = text,
                ForeColor = TextMid,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(x, y + 6)
            });

        TextBox AddField(Panel p, int x, int y, int w, bool upper = false)
        {
            var t = new TextBox
            {
                BackColor = Surface2,
                ForeColor = TextDark,
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(x, y),
                Size = new Size(w, 28),
                CharacterCasing = upper ? CharacterCasing.Upper : CharacterCasing.Normal
            };
            p.Controls.Add(t); return t;
        }

        TextBox FF(Panel p, string lbl, int lx, int fx, int fw, int y, bool up = false)
        {
            AddFieldLabel(p, lbl, lx, y);
            return AddField(p, fx, y, fw, up);
        }

        Button MkBtn(string t, Color bg, Color hover)
        {
            var b = new Button
            {
                Text = t,
                BackColor = bg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Emoji", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = hover;
            return b;
        }

        void Toast(string msg)
        {
            var f = new Form
            {
                Size = new Size(300, 46),
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Green,
                TopMost = true,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual
            };
            f.Controls.Add(new Label
            {
                Text = msg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            var par = this.FindForm();
            if (par != null) f.Location = new Point(par.Right - 320, par.Bottom - 60);
            f.Show();
            var tm = new System.Windows.Forms.Timer { Interval = 2200 };
            tm.Tick += (s, e2) => { tm.Stop(); tm.Dispose(); f.Close(); f.Dispose(); };
            tm.Start();
        }

        static System.Drawing.Drawing2D.GraphicsPath RRect(Rectangle r, int rad)
        {
            int d = rad * 2;
            var p = new System.Drawing.Drawing2D.GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }
    }
}