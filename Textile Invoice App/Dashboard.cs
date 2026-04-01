using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Textile_Invoice_App
{
    public partial class Dashboard : Form
    {
        private const int SIDEBAR_FULL = 220;
        private const int SIDEBAR_COLLAPSED = 58;
        private const int COLLAPSE_BREAKPOINT = 1280;

        private Button _activeNav = null;

        public Dashboard()
        {
            InitializeComponent();
            this.Resize += (s, e) => AdaptSidebar();
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            lblCompanyName.Text = SessionManager.CompanyName;
            lblUserName.Text = SessionManager.FullName;

            // Avatar initials
            var parts = (SessionManager.FullName ?? "AD").Split(' ');
            lblUserAvatar.Text = parts.Length >= 2
                ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                : SessionManager.FullName?.Length >= 2
                    ? SessionManager.FullName[..2].ToUpper()
                    : "AD";

            // Wire GST Return button at runtime
            btnGstReturn.FlatStyle = FlatStyle.Flat;
            btnGstReturn.FlatAppearance.BorderSize = 0;
            btnGstReturn.FlatAppearance.MouseOverBackColor = Color.FromArgb(44, 58, 80);
            btnGstReturn.BackColor = Color.FromArgb(30, 41, 59);
            btnGstReturn.ForeColor = Color.FromArgb(203, 213, 225);
            btnGstReturn.Font = new Font("Segoe UI Emoji", 10F);
            btnGstReturn.Tag = "🧾";
            btnGstReturn.TextAlign = ContentAlignment.MiddleLeft;
            btnGstReturn.Padding = new Padding(14, 0, 0, 0);
            btnGstReturn.Cursor = Cursors.Hand;
            btnGstReturn.UseVisualStyleBackColor = false;
            btnGstReturn.Click += btnGstReturn_Click;
            pnlSidebarScroll.Controls.Add(btnGstReturn);

            UpdateRestoreIcon();
            AdaptSidebar();
            LoadPage(new UC_Dashboard());
            HighlightNav(btnDashboard);

            // ── Show last-backup notification after login ─────────────
            var t = new System.Windows.Forms.Timer { Interval = 800 };
            t.Tick += (s2, e2) =>
            {
                t.Stop(); t.Dispose();
                ShowBackupNotification();
            };
            t.Start();
        }

        // ════════════════════════════════════════════════════════════
        //  LAST BACKUP NOTIFICATION
        // ════════════════════════════════════════════════════════════
        private static readonly string BackupMarkerFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "last_backup.txt");

        private void ShowBackupNotification()
        {
            DateTime? lastBackup = null;
            try
            {
                if (File.Exists(BackupMarkerFile))
                {
                    string text = File.ReadAllText(BackupMarkerFile).Trim();
                    if (DateTime.TryParse(text, out DateTime dt))
                        lastBackup = dt;
                }
            }
            catch { }

            string msg;
            Color bgColor;
            string icon;

            if (lastBackup == null)
            {
                msg = "⚠  No backup found. Please back up your database soon.";
                bgColor = Color.FromArgb(180, 83, 9);
                icon = "⚠";
            }
            else
            {
                TimeSpan age = DateTime.Now - lastBackup.Value;
                string ageStr = age.TotalDays >= 1
                    ? $"{(int)age.TotalDays}d ago"
                    : age.TotalHours >= 1
                        ? $"{(int)age.TotalHours}h ago"
                        : "just now";

                string formatted = lastBackup.Value.ToString("dd MMM yyyy  HH:mm");

                if (age.TotalDays >= 7)
                {
                    msg = $"⚠  Last backup: {formatted}  ({ageStr}) — backup overdue!";
                    bgColor = Color.FromArgb(180, 83, 9);
                    icon = "⚠";
                }
                else
                {
                    msg = $"✅  Last backup: {formatted}  ({ageStr})";
                    bgColor = Color.FromArgb(22, 163, 74);
                    icon = "✅";
                }
            }

            var banner = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                BackColor = bgColor,
                TopMost = true,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(460, 50),
                Opacity = 0.96
            };

            var lbl = new Label
            {
                Text = msg,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };

            var btnDismiss = new Button
            {
                Text = "✕",
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Size = new Size(32, 32),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                UseVisualStyleBackColor = false
            };
            btnDismiss.FlatAppearance.BorderSize = 0;
            btnDismiss.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
            btnDismiss.Click += (s, e) => { banner.Close(); };
            banner.Controls.Add(lbl);
            banner.Controls.Add(btnDismiss);
            banner.Resize += (s, e) =>
                btnDismiss.Location = new Point(banner.Width - 36, 9);

            banner.Location = new Point(
                this.Right - banner.Width - 16,
                this.Bottom - banner.Height - 16);
            banner.Show(this);

            int autoClose = lastBackup != null && (DateTime.Now - lastBackup.Value).TotalDays < 7
                ? 8000 : 12000;
            var autoDismiss = new System.Windows.Forms.Timer { Interval = autoClose };
            autoDismiss.Tick += (s, e) =>
            {
                autoDismiss.Stop(); autoDismiss.Dispose();
                if (!banner.IsDisposed) banner.Close();
            };
            autoDismiss.Start();
        }

        public static void RecordBackupTaken()
        {
            try
            {
                File.WriteAllText(BackupMarkerFile,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch { }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }

        // ── Page loader ───────────────────────────────────────────────
        public void LoadPage(UserControl uc)
        {
            pnlContent.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(uc);
        }

        // ── Sidebar adapt ─────────────────────────────────────────────
        private void AdaptSidebar()
        {
            bool collapse = this.Width < COLLAPSE_BREAKPOINT;
            pnlSidebar.SuspendLayout();

            if (collapse)
            {
                pnlSidebar.Width = SIDEBAR_COLLAPSED;
                lblNavMain.Visible = lblNavMasters.Visible =
                lblNavInvoicing.Visible = lblNavAnalytics.Visible = false;
                pnlNavDivider1.Visible = false;
                lblUserName.Visible = lblUserRole.Visible = false;
                pnlUserInfo.Height = 48;
                pnlActiveNav.Visible = false;

                // ── UPDATED: includes btnWorkOrder ────────────────────
                Button[] btns = { btnDashboard, btnAccounts, btnDesignMaster, btnWorkOrder,
                                  btnServices, btnCreateInvoice, btnInvoiceList,
                                  btnReports, btnGstReturn, btnSettings, btnNavLogout };
                string[] icons = { "📊", "👤", "🎨", "📦", "⚙️", "🧾", "📋", "📈", "🧾", "🔧", "🚪" };

                int y = 4;
                for (int i = 0; i < btns.Length; i++)
                {
                    btns[i].Text = icons[i];
                    btns[i].Tag = icons[i];
                    btns[i].TextAlign = ContentAlignment.MiddleCenter;
                    btns[i].Padding = new Padding(0);
                    btns[i].Font = new Font("Segoe UI Emoji", 14F);
                    btns[i].Location = new Point(0, y);
                    btns[i].Size = new Size(SIDEBAR_COLLAPSED, 40);
                    y += 40;
                }
            }
            else
            {
                pnlSidebar.Width = SIDEBAR_FULL;
                lblNavMain.Visible = lblNavMasters.Visible =
                lblNavInvoicing.Visible = lblNavAnalytics.Visible = true;
                pnlNavDivider1.Visible = true;
                lblUserName.Visible = lblUserRole.Visible = true;
                pnlUserInfo.Height = 72;
                pnlActiveNav.Visible = true;

                const int BTN_H = 36;
                const int BTN_GAP = 2;
                const int LBL_H = 18;
                const int SEC_TOP = 8;
                const int SEC_BOT = 4;
                const int BTN_W = 214;
                const int BTN_X = 3;

                void PlaceLabel(Label lbl, int y) => lbl.Location = new Point(16, y);

                void PlaceBtn(Button b, string icon, string label, int y)
                {
                    b.Text = icon + "  " + label;
                    b.Tag = icon;
                    b.TextAlign = ContentAlignment.MiddleLeft;
                    b.Padding = new Padding(14, 0, 0, 0);
                    b.Font = new Font("Segoe UI Emoji", 10F);
                    b.Location = new Point(BTN_X, y);
                    b.Size = new Size(BTN_W, BTN_H);
                }

                int y = SEC_TOP;
                PlaceLabel(lblNavMain, y); y += LBL_H + SEC_BOT;
                PlaceBtn(btnDashboard, "📊", "Dashboard", y); y += BTN_H + BTN_GAP;

                y += SEC_TOP;
                PlaceLabel(lblNavMasters, y); y += LBL_H + SEC_BOT;
                PlaceBtn(btnAccounts, "👤", "Accounts", y); y += BTN_H + BTN_GAP;
                PlaceBtn(btnDesignMaster, "🎨", "Design Master", y); y += BTN_H + BTN_GAP;
                // ── NEW: Work Order under Masters ─────────────────────
                PlaceBtn(btnWorkOrder, "📦", "Work Order", y); y += BTN_H + BTN_GAP;
                PlaceBtn(btnServices, "⚙️", "Services", y); y += BTN_H + BTN_GAP;

                y += SEC_TOP;
                PlaceLabel(lblNavInvoicing, y); y += LBL_H + SEC_BOT;
                PlaceBtn(btnCreateInvoice, "🧾", "Create Invoice", y); y += BTN_H + BTN_GAP;
                PlaceBtn(btnInvoiceList, "📋", "Invoice List", y); y += BTN_H + BTN_GAP;

                y += SEC_TOP;
                PlaceLabel(lblNavAnalytics, y); y += LBL_H + SEC_BOT;
                PlaceBtn(btnReports, "📈", "Reports", y); y += BTN_H + BTN_GAP;
                PlaceBtn(btnGstReturn, "🧾", "GST Return", y); y += BTN_H + BTN_GAP;

                y += SEC_TOP;
                pnlNavDivider1.Location = new Point(14, y);
                pnlNavDivider1.Size = new Size(192, 1);
                y += 8;
                PlaceBtn(btnSettings, "🔧", "Settings", y); y += BTN_H + BTN_GAP;
                PlaceBtn(btnNavLogout, "🚪", "Logout", y);

                if (_activeNav != null)
                    pnlActiveNav.Location = new Point(0, _activeNav.Top);
            }

            pnlSidebar.ResumeLayout(true);
            UpdateRestoreIcon();
        }

        private void UpdateRestoreIcon()
        {
            if (btnRestore != null)
                btnRestore.Text = "2";
        }

        // ── Nav highlight ─────────────────────────────────────────────
        public void HighlightNavBtn(Button btn) => HighlightNav(btn);
        public Button BtnCreateInvoice => btnCreateInvoice;
        public Button BtnInvoiceList => btnInvoiceList;

        private void HighlightNav(Button btn)
        {
            // ── UPDATED: includes btnWorkOrder ────────────────────────
            Button[] all = { btnDashboard, btnAccounts, btnDesignMaster, btnWorkOrder,
                             btnServices, btnCreateInvoice, btnInvoiceList,
                             btnReports, btnGstReturn, btnSettings, btnNavLogout };
            Color sidebarBg = Color.FromArgb(30, 41, 59);
            Color sidebarHov = Color.FromArgb(44, 58, 80);

            foreach (var b in all)
            {
                b.BackColor = sidebarBg;
                b.ForeColor = Color.FromArgb(148, 163, 184);
                b.FlatAppearance.MouseOverBackColor = sidebarHov;
            }
            btn.BackColor = Color.FromArgb(30, 58, 138);
            btn.ForeColor = Color.White;
            pnlActiveNav.Location = new Point(0, btn.Top);
            _activeNav = btn;
        }

        // ════════════════════════════════════════════════════════════
        //  NAV CLICK HANDLERS
        // ════════════════════════════════════════════════════════════
        private void btnDashboard_Click(object sender, EventArgs e) { HighlightNav(btnDashboard); LoadPage(new UC_Dashboard()); }
        private void btnAccounts_Click(object sender, EventArgs e) { HighlightNav(btnAccounts); LoadPage(new UC_AccountMaster()); }
        private void btnDesignMaster_Click(object sender, EventArgs e) { HighlightNav(btnDesignMaster); LoadPage(new UC_DesignMaster()); }
        // ── NEW ──────────────────────────────────────────────────────
        private void btnWorkOrder_Click(object sender, EventArgs e) { HighlightNav(btnWorkOrder); LoadPage(new UC_WorkOrder()); }
        // ─────────────────────────────────────────────────────────────
        private void btnServices_Click(object sender, EventArgs e) { HighlightNav(btnServices); LoadPage(new UC_ServiceMaster()); }
        private void btnCreateInvoice_Click(object sender, EventArgs e) { HighlightNav(btnCreateInvoice); LoadPage(new UC_CreateInvoice()); }
        private void btnInvoiceList_Click(object sender, EventArgs e) { HighlightNav(btnInvoiceList); LoadPage(new UC_InvoiceList()); }
        private void btnReports_Click(object sender, EventArgs e) { HighlightNav(btnReports); LoadPage(new UC_Reports()); }
        private void btnGstReturn_Click(object sender, EventArgs e) { HighlightNav(btnGstReturn); LoadPage(new UC_GstReturn()); }
        private void btnSettings_Click(object sender, EventArgs e) { HighlightNav(btnSettings); LoadPage(new UC_Settings()); }

        private void btnNavLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Logout from " + SessionManager.CompanyName + "?",
                "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SessionManager.Clear();
                var login = new Login();
                login.Show();
                this.Close();
            }
        }

        // ── Header controls ───────────────────────────────────────────
        private void btnClose_Click(object sender, EventArgs e) => Application.Exit();
        private void btnMinimize_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;
        private void btnLogout_Click(object sender, EventArgs e) => btnNavLogout_Click(sender, e);

        private void btnRestore_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                var screen = Screen.FromControl(this).WorkingArea;
                this.Size = new Size(
                    (int)(screen.Width * 0.80),
                    (int)(screen.Height * 0.85));
                this.Location = new Point(
                    screen.Left + (screen.Width - this.Width) / 2,
                    screen.Top + (screen.Height - this.Height) / 2);
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
            UpdateRestoreIcon();
        }

        // ── Borderless drag ───────────────────────────────────────────
        private System.Drawing.Point _dragStart;
        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragStart = e.Location;
                pnlHeader.MouseMove += Header_MouseMove;
                pnlHeader.MouseUp += Header_MouseUp;
            }
        }
        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.WindowState == FormWindowState.Normal)
            {
                this.Left += e.X - _dragStart.X;
                this.Top += e.Y - _dragStart.Y;
            }
        }
        private void Header_MouseUp(object sender, MouseEventArgs e)
        {
            pnlHeader.MouseMove -= Header_MouseMove;
            pnlHeader.MouseUp -= Header_MouseUp;
        }

        private void Header_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal : FormWindowState.Maximized;
            UpdateRestoreIcon();
        }

        private void Dashboard_Resize(object sender, EventArgs e) => AdaptSidebar();
    }
}