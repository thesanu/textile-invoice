using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App
{
    public class RegisterCompany : Form
    {
        static readonly Color BgDark = Color.FromArgb(13, 17, 28);
        static readonly Color BgPanel = Color.FromArgb(20, 26, 40);
        static readonly Color BgCard = Color.FromArgb(26, 33, 52);
        static readonly Color Accent = Color.FromArgb(37, 99, 235);
        static readonly Color AccentL = Color.FromArgb(59, 130, 246);
        static readonly Color Gold = Color.FromArgb(251, 191, 36);
        static readonly Color TextMain = Color.FromArgb(226, 232, 240);
        static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
        static readonly Color TextDim = Color.FromArgb(148, 163, 184);
        static readonly Color InputBg = Color.FromArgb(15, 20, 35);
        static readonly Color CardBdr = Color.FromArgb(37, 47, 73);

        private Panel _pnlStep1, _pnlStep2;
        private Label _lblStep1, _lblStep2;

        private TextBox _txtCoName, _txtGstin, _txtPan, _txtPhone,
                        _txtEmail, _txtCity, _txtState, _txtPincode,
                        _txtAdd1, _txtAdd2;
        private PictureBox _picLogo;
        private byte[] _logoBytes;

        private TextBox _txtFullName, _txtUsername, _txtPassword, _txtConfirm;

        private Point _dragStart;

        public RegisterCompany()
        {
            this.Text = "Register New Company";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgDark;
            this.Size = new Size(860, 640);
            this.MinimumSize = new Size(680, 500);

            BuildChrome();
            BuildBody();
        }

        // ── Chrome ────────────────────────────────────────────────────
        private void BuildChrome()
        {
            var bar = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = BgPanel };
            bar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) _dragStart = e.Location; };
            bar.MouseMove += (s, e) => { if (e.Button == MouseButtons.Left && WindowState == FormWindowState.Normal) { Left += e.X - _dragStart.X; Top += e.Y - _dragStart.Y; } };
            bar.Controls.Add(new Label { Text = "🧵  Register New Company", Font = new Font("Segoe UI Emoji", 11F, FontStyle.Bold), ForeColor = TextMain, AutoSize = true, Location = new Point(16, 11) });
            bar.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 2, BackColor = Gold });
            var btnX = new Button { Text = "✕", Size = new Size(38, 38), BackColor = BgPanel, ForeColor = TextMain, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 38, 38);
            btnX.Click += (s, e) => this.Close();
            bar.Resize += (s, e) => btnX.Location = new Point(bar.Width - 42, 4);
            this.Shown += (s, e) => btnX.Location = new Point(bar.Width - 42, 4);
            bar.Controls.Add(btnX);

            var strip = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.FromArgb(18, 23, 38) };
            _lblStep1 = new Label { Text = "  1  Company Profile  ", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = false, Size = new Size(160, 22), Location = new Point(16, 6), TextAlign = ContentAlignment.MiddleCenter };
            _lblStep2 = new Label { Text = "  2  Admin User  ", Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), AutoSize = false, Size = new Size(140, 22), Location = new Point(194, 6), TextAlign = ContentAlignment.MiddleCenter };
            strip.Controls.Add(_lblStep1);
            strip.Controls.Add(new Label { Text = "›", Font = new Font("Segoe UI", 12F), ForeColor = TextMuted, AutoSize = true, Location = new Point(178, 6) });
            strip.Controls.Add(_lblStep2);

            this.Controls.Add(strip);  // LIFO: strip added first → docks below bar
            this.Controls.Add(bar);    // bar added last  → docks at very top
        }

        // ── Body ──────────────────────────────────────────────────────
        private void BuildBody()
        {
            var body = new Panel { Dock = DockStyle.Fill, BackColor = BgDark };
            _pnlStep1 = BuildStep1(); _pnlStep1.Dock = DockStyle.Fill;
            _pnlStep2 = BuildStep2(); _pnlStep2.Dock = DockStyle.Fill;
            body.Controls.Add(_pnlStep2);  // LIFO
            body.Controls.Add(_pnlStep1);
            this.Controls.Add(body);
            ShowStep(1);
        }

        // ════════════════════════════════════════════════════════════
        //  STEP 1  —  one big TableLayoutPanel, rows top-to-bottom
        //             columns are percentages → adapts to any width
        // ════════════════════════════════════════════════════════════
        private Panel BuildStep1()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = BgDark };

            // Master table: 1 column, grows downward
            // Each "section" is a row; we build it top-to-bottom so
            // no DockStyle ordering tricks are needed at all.
            var master = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = BgCard,
                Dock = DockStyle.Top,
                ColumnCount = 1
            };
            master.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // ── ROW 0: blue accent stripe ─────────────────────────
            master.RowStyles.Add(new RowStyle(SizeType.Absolute, 3));
            master.Controls.Add(new Panel { BackColor = Accent, Dock = DockStyle.Fill }, 0, 0);

            // ── ROW 1: section header ─────────────────────────────
            master.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            var hdr = new Panel { BackColor = Color.FromArgb(20, 27, 46), Dock = DockStyle.Fill };
            hdr.Controls.Add(new Label { Text = "🏢  Company Information", Font = new Font("Segoe UI Emoji", 9.5F, FontStyle.Bold), ForeColor = TextDim, AutoSize = true, Location = new Point(14, 10) });
            master.Controls.Add(hdr, 0, 1);

            // ── ROW 2: field grid ─────────────────────────────────
            master.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var grid = BuildFieldGrid();
            master.Controls.Add(grid, 0, 2);

            // ── ROW 3: divider ────────────────────────────────────
            master.RowStyles.Add(new RowStyle(SizeType.Absolute, 16));
            var divRow = new Panel { BackColor = BgCard, Dock = DockStyle.Fill, Padding = new Padding(14, 7, 14, 0) };
            divRow.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 1, BackColor = CardBdr });
            master.Controls.Add(divRow, 0, 3);

            // ── ROW 4: logo ───────────────────────────────────────
            master.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            master.Controls.Add(BuildLogoRow(), 0, 4);

            // ── ROW 5: action buttons ─────────────────────────────
            master.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            master.Controls.Add(BuildStep1Buttons(), 0, 5);

            // Top spacer — guarantees first row is never clipped
            // This is a plain panel with no children, just height.
            var topSpacer = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = BgDark };

            scroll.Resize += (s, e) => { if (scroll.ClientSize.Width > 10) master.Width = scroll.ClientSize.Width; };
            this.Shown += (s, e) => { if (scroll.ClientSize.Width > 10) master.Width = scroll.ClientSize.Width; };

            // IMPORTANT: with DockStyle.Top, controls added LAST dock to the TOP first (LIFO).
            // So add master first (docks second = below spacer),
            // then add spacer last (docks first = very top).
            scroll.Controls.Add(master);
            scroll.Controls.Add(topSpacer);
            return scroll;
        }

        // 4-column percentage grid for all field rows
        private TableLayoutPanel BuildFieldGrid()
        {
            var g = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = BgCard,
                Dock = DockStyle.Top,
                Padding = new Padding(14, 10, 14, 10),
                ColumnCount = 4,
                RowCount = 7
            };
            // LabelL(16%) | InputL(34%) | LabelR(14%) | InputR(36%)
            g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16f));
            g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
            g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14f));
            g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36f));
            for (int i = 0; i < 7; i++) g.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Row 0: Company Name | Phone
            Row2(g, 0, "Company Name *", out _txtCoName, "Phone", out _txtPhone);
            // Row 1: GSTIN | PAN
            Row2(g, 1, "GSTIN", out _txtGstin, "PAN", out _txtPan, uleft: true, uright: true);
            // Row 2: Email (full width)
            RowFull(g, 2, "Email", out _txtEmail);
            // Row 3: City | State
            Row2(g, 3, "City", out _txtCity, "State", out _txtState);
            // Row 4: Pincode (left only)
            RowLeft(g, 4, "Pincode", out _txtPincode);
            // Row 5: Address Line 1 (full)
            RowFull(g, 5, "Address Line 1", out _txtAdd1);
            // Row 6: Address Line 2 (full)
            RowFull(g, 6, "Address Line 2", out _txtAdd2);

            return g;
        }

        private Panel BuildLogoRow()
        {
            var wrap = new Panel { BackColor = BgCard, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(14, 6, 14, 6) };

            var flow = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, BackColor = BgCard, WrapContents = false, Dock = DockStyle.Top };

            flow.Controls.Add(new Label { Text = "🖼️  Company Logo  (optional — shown on invoice)", Font = new Font("Segoe UI Emoji", 9F, FontStyle.Bold), ForeColor = TextDim, AutoSize = true, Margin = new Padding(0, 0, 0, 6) });

            var logoLine = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, BackColor = BgCard, WrapContents = false };

            _picLogo = new PictureBox { Size = new Size(80, 80), BackColor = InputBg, SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(0, 0, 10, 0) };
            DrawLogoPlaceholder();
            logoLine.Controls.Add(_picLogo);

            var btnCol = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, BackColor = BgCard };
            var btnPick = MkBtn3("📁  Choose Image", 148, 28); btnPick.Click += PickLogo; btnCol.Controls.Add(btnPick);
            var btnClear = MkBtn3("✕  Clear", 70, 28); btnClear.Click += (s, e) => { _logoBytes = null; DrawLogoPlaceholder(); }; btnCol.Controls.Add(btnClear);
            btnCol.Controls.Add(new Label { Text = "PNG / JPG / BMP  •  max 1 MB  •  recommended 300×300 px", Font = new Font("Segoe UI", 7.5F), ForeColor = TextMuted, AutoSize = true, Margin = new Padding(0, 4, 0, 0) });
            logoLine.Controls.Add(btnCol);

            flow.Controls.Add(logoLine);
            wrap.Controls.Add(flow);
            return wrap;
        }

        private Panel BuildStep1Buttons()
        {
            var p = new Panel { BackColor = BgCard, Dock = DockStyle.Top, Height = 58, Padding = new Padding(14, 10, 14, 10) };
            var btnNext = MkBtn1("Next  →", 148, 38); btnNext.Click += GoStep2; p.Controls.Add(btnNext);
            var btnCancel = MkBtn2("Cancel", 90, 38); btnCancel.Click += (s, e) => this.Close(); p.Controls.Add(btnCancel);
            btnNext.Location = new Point(14, 10);
            btnCancel.Location = new Point(170, 10);
            return p;
        }

        // ════════════════════════════════════════════════════════════
        //  STEP 2
        // ════════════════════════════════════════════════════════════
        private Panel BuildStep2()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = BgDark };

            var master = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = BgCard,
                Dock = DockStyle.Top,
                ColumnCount = 1
            };
            master.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // Row 0: stripe
            master.RowStyles.Add(new RowStyle(SizeType.Absolute, 3));
            master.Controls.Add(new Panel { BackColor = Accent, Dock = DockStyle.Fill }, 0, 0);

            // Row 1: header
            master.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            var hdr = new Panel { BackColor = Color.FromArgb(20, 27, 46), Dock = DockStyle.Fill };
            hdr.Controls.Add(new Label { Text = "👤  Admin User Account", Font = new Font("Segoe UI Emoji", 9.5F, FontStyle.Bold), ForeColor = TextDim, AutoSize = true, Location = new Point(14, 10) });
            master.Controls.Add(hdr, 0, 1);

            // Row 2: field grid
            master.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var g = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = BgCard,
                Dock = DockStyle.Top,
                Padding = new Padding(14, 10, 14, 10),
                ColumnCount = 2,
                RowCount = 6
            };
            g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
            g.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));
            for (int i = 0; i < 6; i++) g.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var infoLbl = MkLbl("This user will be the administrator for the new company.");
            infoLbl.Margin = new Padding(2, 0, 2, 8);
            g.Controls.Add(infoLbl, 0, 0); g.SetColumnSpan(infoLbl, 2);

            RowSimple(g, 1, "Full Name *", out _txtFullName);
            RowSimple(g, 2, "Username *", out _txtUsername);
            RowSimple(g, 3, "Password *", out _txtPassword, pwd: true);
            RowSimple(g, 4, "Confirm Password *", out _txtConfirm, pwd: true);

            var hint = MkLbl("ⓘ  Password must be at least 8 characters.");
            hint.Margin = new Padding(2, 4, 2, 4);
            g.Controls.Add(hint, 0, 5); g.SetColumnSpan(hint, 2);

            master.Controls.Add(g, 0, 2);

            // Row 3: buttons
            master.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var btnRow = new Panel { BackColor = BgCard, Dock = DockStyle.Top, Height = 58, Padding = new Padding(14, 10, 14, 10) };
            var btnBack = MkBtn2("← Back", 90, 38); btnBack.Click += (s, e) => ShowStep(1); btnRow.Controls.Add(btnBack);
            var btnSave = MkBtn1("✓  Register Company", 200, 38); btnSave.Click += SaveAll; btnRow.Controls.Add(btnSave);
            btnBack.Location = new Point(14, 10);
            btnSave.Location = new Point(112, 10);
            master.Controls.Add(btnRow, 0, 3);

            var topSpacer2 = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = BgDark };
            scroll.Resize += (s, e) => { if (scroll.ClientSize.Width > 10) master.Width = scroll.ClientSize.Width; };
            this.Shown += (s, e) => { if (scroll.ClientSize.Width > 10) master.Width = scroll.ClientSize.Width; };
            scroll.Controls.Add(master);
            scroll.Controls.Add(topSpacer2);
            return scroll;
        }

        // ════════════════════════════════════════════════════════════
        //  GRID ROW HELPERS
        // ════════════════════════════════════════════════════════════
        void Row2(TableLayoutPanel g, int row,
                  string lL, out TextBox tbL, string lR, out TextBox tbR,
                  bool uleft = false, bool uright = false)
        {
            g.Controls.Add(MkFieldLabel(lL), 0, row);
            tbL = MkInput(uleft); g.Controls.Add(tbL, 1, row);
            g.Controls.Add(MkFieldLabel(lR), 2, row);
            tbR = MkInput(uright); g.Controls.Add(tbR, 3, row);
        }

        void RowFull(TableLayoutPanel g, int row, string lbl, out TextBox tb)
        {
            g.Controls.Add(MkFieldLabel(lbl), 0, row);
            tb = MkInput(); tb.Dock = DockStyle.Fill;
            g.Controls.Add(tb, 1, row);
            g.SetColumnSpan(tb, 3);
        }

        void RowLeft(TableLayoutPanel g, int row, string lbl, out TextBox tb)
        {
            g.Controls.Add(MkFieldLabel(lbl), 0, row);
            tb = MkInput();
            g.Controls.Add(tb, 1, row);
        }

        void RowSimple(TableLayoutPanel g, int row, string lbl, out TextBox tb, bool pwd = false)
        {
            g.Controls.Add(MkFieldLabel(lbl), 0, row);
            tb = MkInput(pwd: pwd); tb.Dock = DockStyle.Fill;
            g.Controls.Add(tb, 1, row);
        }

        // ── control factories ─────────────────────────────────────────
        Label MkFieldLabel(string t) => new Label
        {
            Text = t,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = TextMuted,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(2, 4, 6, 4)
        };

        Label MkLbl(string t) => new Label
        {
            Text = t,
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = TextMuted,
            AutoSize = true
        };

        TextBox MkInput(bool upper = false, bool pwd = false) => new TextBox
        {
            BackColor = InputBg,
            ForeColor = TextMain,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10F),
            Dock = DockStyle.Fill,
            Height = 26,
            CharacterCasing = upper ? CharacterCasing.Upper : CharacterCasing.Normal,
            UseSystemPasswordChar = pwd,
            Margin = new Padding(2, 4, 4, 4)
        };

        Button MkBtn1(string t, int w, int h)
        {
            var b = new Button { Text = t, Size = new Size(w, h), BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = AccentL; return b;
        }
        Button MkBtn2(string t, int w, int h)
        {
            var b = new Button { Text = t, Size = new Size(w, h), BackColor = BgCard, ForeColor = TextDim, Font = new Font("Segoe UI", 9.5F), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, UseVisualStyleBackColor = false };
            b.FlatAppearance.BorderColor = CardBdr; b.FlatAppearance.BorderSize = 1; b.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 40, 62); return b;
        }
        Button MkBtn3(string t, int w, int h)
        {
            var b = new Button { Text = t, Size = new Size(w, h), BackColor = Color.FromArgb(30, 40, 62), ForeColor = TextDim, Font = new Font("Segoe UI", 8.5F), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, UseVisualStyleBackColor = false, Margin = new Padding(0, 0, 6, 4) };
            b.FlatAppearance.BorderSize = 0; b.FlatAppearance.MouseOverBackColor = Color.FromArgb(44, 58, 88); return b;
        }

        // ════════════════════════════════════════════════════════════
        //  NAV / LOGO / SAVE
        // ════════════════════════════════════════════════════════════
        void ShowStep(int step)
        {
            _pnlStep1.Visible = step == 1; _pnlStep2.Visible = step == 2;
            _lblStep1.BackColor = step == 1 ? Accent : Color.FromArgb(40, 50, 72); _lblStep1.ForeColor = step == 1 ? Color.White : TextMuted;
            _lblStep2.BackColor = step == 2 ? Accent : Color.FromArgb(40, 50, 72); _lblStep2.ForeColor = step == 2 ? Color.White : TextMuted;
        }

        void GoStep2(object s, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtCoName.Text)) { Flash(_txtCoName, "Company Name is required."); return; }
            ShowStep(2);
        }

        void PickLogo(object s, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            if (new FileInfo(dlg.FileName).Length > 1_048_576) { MessageBox.Show("Image must be ≤ 1 MB."); return; }
            _logoBytes = File.ReadAllBytes(dlg.FileName);
            try { using var ms = new MemoryStream(_logoBytes); _picLogo.Image = Image.FromStream(ms); }
            catch { _logoBytes = null; }
        }

        void DrawLogoPlaceholder()
        {
            var bmp = new Bitmap(80, 80);
            using var g = Graphics.FromImage(bmp);
            g.Clear(InputBg);
            using var pen = new Pen(CardBdr, 1.5f);
            g.DrawRectangle(pen, 1, 1, 77, 77);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("No Logo", new Font("Segoe UI", 8F), new SolidBrush(TextMuted), new RectangleF(0, 0, 80, 80), sf);
            _picLogo.Image = bmp;
        }

        void SaveAll(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtFullName.Text)) { Flash(_txtFullName, "Full Name is required."); return; }
            if (string.IsNullOrWhiteSpace(_txtUsername.Text)) { Flash(_txtUsername, "Username is required."); return; }
            if (string.IsNullOrWhiteSpace(_txtPassword.Text)) { Flash(_txtPassword, "Password is required."); return; }
            if (_txtPassword.Text.Length < 8) { Flash(_txtPassword, "Password must be at least 8 characters."); return; }
            if (_txtPassword.Text != _txtConfirm.Text) { Flash(_txtConfirm, "Passwords do not match."); return; }
            try
            {
                using var db = new AppDbContext();
                if (db.AppUsers.Any(u => u.Username == _txtUsername.Text.Trim())) { Flash(_txtUsername, "Username already taken."); return; }
                var co = new CompanyProfile
                {
                    CompanyName = _txtCoName.Text.Trim(),
                    Gstin = _txtGstin.Text.Trim().ToUpper(),
                    Pan = _txtPan.Text.Trim().ToUpper(),
                    Phone = _txtPhone.Text.Trim(),
                    Email = _txtEmail.Text.Trim(),
                    City = _txtCity.Text.Trim(),
                    State = _txtState.Text.Trim(),
                    Pincode = _txtPincode.Text.Trim(),
                    Address1 = _txtAdd1.Text.Trim(),
                    Address2 = _txtAdd2.Text.Trim(),
                    Status = "Active"
                };
                var logoProp = typeof(CompanyProfile).GetProperty("LogoImage");
                if (logoProp != null && _logoBytes != null) logoProp.SetValue(co, _logoBytes);
                db.CompanyProfiles.Add(co); db.SaveChanges();
                db.InvoiceNumberTrackers.Add(new InvoiceNumberTracker { CompanyProfileId = co.CompanyProfileId, CurrentInvoiceNo = 1 });
                db.AppUsers.Add(new AppUser { CompanyProfileId = co.CompanyProfileId, FullName = _txtFullName.Text.Trim(), Username = _txtUsername.Text.Trim(), PasswordHash = _txtPassword.Text.Trim(), IsActive = true, CreatedDate = DateTime.Now });
                db.SaveChanges();
                MessageBox.Show($"✓  \"{co.CompanyName}\" registered!\n\nUsername: {_txtUsername.Text.Trim()}\n\nYou can now log in.", "Registration Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex) { MessageBox.Show("Error:\n" + ex.Message, "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        void Flash(TextBox tb, string msg)
        {
            tb.BackColor = Color.FromArgb(60, 20, 20);
            var t = new System.Windows.Forms.Timer { Interval = 1400 };
            t.Tick += (s, e) => { tb.BackColor = InputBg; t.Stop(); t.Dispose(); };
            t.Start(); tb.Focus();
            MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}