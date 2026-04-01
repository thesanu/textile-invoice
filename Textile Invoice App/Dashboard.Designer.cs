namespace Textile_Invoice_App
{
    partial class Dashboard
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlHeaderRight = new System.Windows.Forms.Panel();
            this.pnlHeaderAccent = new System.Windows.Forms.Panel();
            this.lblLogoIcon = new System.Windows.Forms.Label();
            this.lblAppTitle = new System.Windows.Forms.Label();
            this.lblAppSub = new System.Windows.Forms.Label();
            this.lblStatusDot = new System.Windows.Forms.Label();
            this.lblSystemOnline = new System.Windows.Forms.Label();
            this.lblCompanyName = new System.Windows.Forms.Label();
            this.lblUserAvatar = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();

            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.pnlSidebarScroll = new System.Windows.Forms.Panel();
            this.pnlUserInfo = new System.Windows.Forms.Panel();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblUserRole = new System.Windows.Forms.Label();
            this.pnlActiveNav = new System.Windows.Forms.Panel();
            this.lblNavMain = new System.Windows.Forms.Label();
            this.lblNavMasters = new System.Windows.Forms.Label();
            this.lblNavInvoicing = new System.Windows.Forms.Label();
            this.lblNavAnalytics = new System.Windows.Forms.Label();
            this.pnlNavDivider1 = new System.Windows.Forms.Panel();

            this.btnDashboard = new System.Windows.Forms.Button();
            this.btnAccounts = new System.Windows.Forms.Button();
            this.btnDesignMaster = new System.Windows.Forms.Button();
            // ── NEW ──────────────────────────────────────────────────
            this.btnWorkOrder = new System.Windows.Forms.Button();
            // ─────────────────────────────────────────────────────────
            this.btnServices = new System.Windows.Forms.Button();
            this.btnCreateInvoice = new System.Windows.Forms.Button();
            this.btnInvoiceList = new System.Windows.Forms.Button();
            this.btnReports = new System.Windows.Forms.Button();
            this.btnGstReturn = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnNavLogout = new System.Windows.Forms.Button();

            this.pnlContent = new System.Windows.Forms.Panel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);

            this.pnlHeader.SuspendLayout();
            this.pnlHeaderRight.SuspendLayout();
            this.pnlSidebar.SuspendLayout();
            this.pnlSidebarScroll.SuspendLayout();
            this.pnlUserInfo.SuspendLayout();
            this.SuspendLayout();

            // ── PALETTE ──────────────────────────────────────────────
            var bg = System.Drawing.Color.FromArgb(245, 247, 250);
            var surface = System.Drawing.Color.White;
            var surface2 = System.Drawing.Color.FromArgb(248, 249, 252);
            var sidebarBg = System.Drawing.Color.FromArgb(30, 41, 59);
            var sideHov = System.Drawing.Color.FromArgb(44, 58, 80);
            var blue = System.Drawing.Color.FromArgb(37, 99, 235);
            var blueL = System.Drawing.Color.FromArgb(59, 130, 246);
            var green = System.Drawing.Color.FromArgb(22, 163, 74);
            var red = System.Drawing.Color.FromArgb(220, 38, 38);
            var border = System.Drawing.Color.FromArgb(226, 232, 240);
            var textDark = System.Drawing.Color.FromArgb(15, 23, 42);
            var textMid = System.Drawing.Color.FromArgb(71, 85, 105);
            var textLight = System.Drawing.Color.FromArgb(148, 163, 184);
            var sideText = System.Drawing.Color.FromArgb(203, 213, 225);
            var sideMuted = System.Drawing.Color.FromArgb(100, 116, 139);

            // ════════════════════════════════════════════════════════
            //  FORM
            // ════════════════════════════════════════════════════════
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = bg;
            this.ClientSize = new System.Drawing.Size(1440, 900);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.Name = "Dashboard";
            this.Text = "Textile Invoice Management";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Dashboard_Load);
            this.Resize += new System.EventHandler(this.Dashboard_Resize);

            // ════════════════════════════════════════════════════════
            //  HEADER
            // ════════════════════════════════════════════════════════
            this.pnlHeader.BackColor = surface;
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 62;
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Header_MouseDown);
            this.pnlHeader.DoubleClick += new System.EventHandler(this.Header_DoubleClick);

            this.pnlHeaderAccent.BackColor = blue;
            this.pnlHeaderAccent.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlHeaderAccent.Height = 2;

            this.lblLogoIcon.BackColor = blue;
            this.lblLogoIcon.ForeColor = System.Drawing.Color.White;
            this.lblLogoIcon.Font = new System.Drawing.Font("Segoe UI Emoji", 16F);
            this.lblLogoIcon.Text = "🧵";
            this.lblLogoIcon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLogoIcon.Location = new System.Drawing.Point(14, 12);
            this.lblLogoIcon.Size = new System.Drawing.Size(38, 38);
            this.lblLogoIcon.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;

            this.lblAppTitle.AutoSize = true;
            this.lblAppTitle.ForeColor = textDark;
            this.lblAppTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblAppTitle.Text = "Textile Invoice Management";
            this.lblAppTitle.Location = new System.Drawing.Point(60, 12);
            this.lblAppTitle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;

            this.lblAppSub.AutoSize = true;
            this.lblAppSub.ForeColor = textLight;
            this.lblAppSub.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            this.lblAppSub.Text = "LABOUR SERVICE BILLING SYSTEM";
            this.lblAppSub.Location = new System.Drawing.Point(61, 36);
            this.lblAppSub.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;

            this.lblStatusDot.BackColor = green;
            this.lblStatusDot.Size = new System.Drawing.Size(8, 8);
            this.lblStatusDot.Location = new System.Drawing.Point(370, 28);
            this.lblStatusDot.Text = "";

            this.lblSystemOnline.AutoSize = true;
            this.lblSystemOnline.ForeColor = textMid;
            this.lblSystemOnline.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSystemOnline.Text = "System Online";
            this.lblSystemOnline.Location = new System.Drawing.Point(384, 22);

            // ── Header right panel ────────────────────────────────────
            this.pnlHeaderRight.BackColor = surface;
            this.pnlHeaderRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlHeaderRight.Width = 520;

            this.pnlHeaderRight.Resize += (s, e) => {
                int rw = this.pnlHeaderRight.Width;
                this.btnClose.Location = new System.Drawing.Point(rw - 44, 0);
                this.btnRestore.Location = new System.Drawing.Point(rw - 88, 0);
                this.btnMinimize.Location = new System.Drawing.Point(rw - 132, 0);
                this.btnLogout.Location = new System.Drawing.Point(rw - 132 - 10 - 84, 17);
                this.lblUserAvatar.Location = new System.Drawing.Point(rw - 132 - 10 - 84 - 10 - 32, 15);
                bool showCo = rw > 400;
                this.lblCompanyName.Visible = showCo;
                if (showCo)
                {
                    int avatarLeft = rw - 132 - 10 - 84 - 10 - 32;
                    this.lblCompanyName.Width = System.Math.Max(80, avatarLeft - 16);
                    this.lblCompanyName.Location = new System.Drawing.Point(8, 17);
                }
            };

            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnClose.Text = "✕";
            this.btnClose.Size = new System.Drawing.Size(44, 52);
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            this.btnRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestore.FlatAppearance.BorderSize = 0;
            this.btnRestore.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.btnRestore.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.btnRestore.BackColor = System.Drawing.Color.Transparent;
            this.btnRestore.Font = new System.Drawing.Font("Marlett", 10F);
            this.btnRestore.Text = "2";
            this.btnRestore.Size = new System.Drawing.Size(44, 52);
            this.btnRestore.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnRestore.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestore.UseVisualStyleBackColor = false;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);

            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.btnMinimize.ForeColor = System.Drawing.Color.FromArgb(50, 50, 50);
            this.btnMinimize.BackColor = System.Drawing.Color.Transparent;
            this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnMinimize.Text = "─";
            this.btnMinimize.Size = new System.Drawing.Size(44, 52);
            this.btnMinimize.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMinimize.UseVisualStyleBackColor = false;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);

            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.FlatAppearance.BorderColor = red;
            this.btnLogout.FlatAppearance.BorderSize = 1;
            this.btnLogout.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(254, 242, 242);
            this.btnLogout.ForeColor = red;
            this.btnLogout.BackColor = System.Drawing.Color.Transparent;
            this.btnLogout.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnLogout.Text = "⏻  Logout";
            this.btnLogout.Size = new System.Drawing.Size(84, 26);
            this.btnLogout.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.UseVisualStyleBackColor = false;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            this.lblUserAvatar.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
            this.lblUserAvatar.ForeColor = System.Drawing.Color.White;
            this.lblUserAvatar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblUserAvatar.Text = "AD";
            this.lblUserAvatar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUserAvatar.Size = new System.Drawing.Size(32, 32);
            this.lblUserAvatar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;

            this.lblCompanyName.AutoSize = false;
            this.lblCompanyName.ForeColor = textDark;
            this.lblCompanyName.BackColor = surface2;
            this.lblCompanyName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCompanyName.Text = "Loading…";
            this.lblCompanyName.Size = new System.Drawing.Size(190, 26);
            this.lblCompanyName.Location = new System.Drawing.Point(8, 13);
            this.lblCompanyName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.lblCompanyName.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);

            this.pnlHeaderRight.Controls.Add(this.lblCompanyName);
            this.pnlHeaderRight.Controls.Add(this.lblUserAvatar);
            this.pnlHeaderRight.Controls.Add(this.btnLogout);
            this.pnlHeaderRight.Controls.Add(this.btnMinimize);
            this.pnlHeaderRight.Controls.Add(this.btnRestore);
            this.pnlHeaderRight.Controls.Add(this.btnClose);

            this.pnlHeader.Controls.Add(this.pnlHeaderRight);
            this.pnlHeader.Controls.Add(this.pnlHeaderAccent);
            this.pnlHeader.Controls.Add(this.lblLogoIcon);
            this.pnlHeader.Controls.Add(this.lblAppTitle);
            this.pnlHeader.Controls.Add(this.lblAppSub);
            this.pnlHeader.Controls.Add(this.lblStatusDot);
            this.pnlHeader.Controls.Add(this.lblSystemOnline);

            // ════════════════════════════════════════════════════════
            //  SIDEBAR
            // ════════════════════════════════════════════════════════
            this.pnlSidebar.BackColor = sidebarBg;
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Width = 220;
            this.pnlSidebar.Name = "pnlSidebar";

            this.pnlUserInfo.BackColor = System.Drawing.Color.FromArgb(21, 32, 51);
            this.pnlUserInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlUserInfo.Height = 72;

            this.lblUserName.AutoSize = true;
            this.lblUserName.ForeColor = System.Drawing.Color.White;
            this.lblUserName.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblUserName.Text = "Loading…";
            this.lblUserName.Location = new System.Drawing.Point(16, 16);

            this.lblUserRole.AutoSize = true;
            this.lblUserRole.ForeColor = sideMuted;
            this.lblUserRole.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblUserRole.Text = "Logged In";
            this.lblUserRole.Location = new System.Drawing.Point(17, 38);

            this.pnlUserInfo.Controls.Add(this.lblUserName);
            this.pnlUserInfo.Controls.Add(this.lblUserRole);

            this.pnlSidebarScroll.BackColor = sidebarBg;
            this.pnlSidebarScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSidebarScroll.AutoScroll = true;
            this.pnlSidebarScroll.Name = "pnlSidebarScroll";

            this.pnlActiveNav.BackColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.pnlActiveNav.Size = new System.Drawing.Size(4, 36);
            this.pnlActiveNav.Location = new System.Drawing.Point(0, 12);
            this.pnlActiveNav.Name = "pnlActiveNav";

            void SL(System.Windows.Forms.Label l, string t, System.Drawing.Point p)
            {
                l.AutoSize = true;
                l.ForeColor = sideMuted;
                l.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Bold);
                l.Text = t;
                l.Location = p;
                l.TabIndex = 0;
            }

            SL(lblNavMain, "MAIN", new System.Drawing.Point(16, 12));
            SL(lblNavMasters, "MASTERS", new System.Drawing.Point(16, 80));
            SL(lblNavInvoicing, "INVOICING", new System.Drawing.Point(16, 252)); // shifted +36 for Work Order
            SL(lblNavAnalytics, "ANALYTICS", new System.Drawing.Point(16, 360)); // shifted +36

            void NB(System.Windows.Forms.Button b, string icon, string label,
                    System.Drawing.Point loc, bool active = false)
            {
                b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = sideHov;
                b.BackColor = active
                    ? System.Drawing.Color.FromArgb(44, 58, 80) : sidebarBg;
                b.ForeColor = active
                    ? System.Drawing.Color.White : sideText;
                b.Font = new System.Drawing.Font("Segoe UI Emoji", 10F);
                b.Text = icon + "  " + label;
                b.Tag = icon;
                b.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                b.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
                b.Location = loc;
                b.Size = new System.Drawing.Size(214, 36);
                b.Cursor = System.Windows.Forms.Cursors.Hand;
                b.UseVisualStyleBackColor = false;
            }

            // ── Nav buttons (Work Order inserted after Design Master) ─
            NB(btnDashboard, "📊", "Dashboard", new System.Drawing.Point(3, 28), true);
            NB(btnAccounts, "👤", "Accounts", new System.Drawing.Point(3, 92));
            NB(btnDesignMaster, "🎨", "Design Master", new System.Drawing.Point(3, 128));
            NB(btnWorkOrder, "📦", "Work Order", new System.Drawing.Point(3, 164)); // ← NEW
            NB(btnServices, "⚙️", "Services", new System.Drawing.Point(3, 200)); // shifted +36
            NB(btnCreateInvoice, "🧾", "Create Invoice", new System.Drawing.Point(3, 264)); // shifted +36
            NB(btnInvoiceList, "📋", "Invoice List", new System.Drawing.Point(3, 300)); // shifted +36
            NB(btnReports, "📈", "Reports", new System.Drawing.Point(3, 372)); // shifted +36

            this.pnlNavDivider1.BackColor = System.Drawing.Color.FromArgb(44, 58, 80);
            this.pnlNavDivider1.Location = new System.Drawing.Point(14, 412); // shifted +36
            this.pnlNavDivider1.Size = new System.Drawing.Size(192, 1);

            NB(btnSettings, "🔧", "Settings", new System.Drawing.Point(3, 420)); // shifted +36

            this.btnNavLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNavLogout.FlatAppearance.BorderSize = 0;
            this.btnNavLogout.FlatAppearance.MouseOverBackColor = sideHov;
            this.btnNavLogout.BackColor = sidebarBg;
            this.btnNavLogout.ForeColor = System.Drawing.Color.FromArgb(255, 100, 100);
            this.btnNavLogout.Font = new System.Drawing.Font("Segoe UI Emoji", 10F);
            this.btnNavLogout.Text = "🚪  Logout";
            this.btnNavLogout.Tag = "🚪";
            this.btnNavLogout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNavLogout.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.btnNavLogout.Location = new System.Drawing.Point(3, 456); // shifted +36
            this.btnNavLogout.Size = new System.Drawing.Size(214, 36);
            this.btnNavLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNavLogout.UseVisualStyleBackColor = false;
            this.btnNavLogout.Click += new System.EventHandler(this.btnNavLogout_Click);

            // ── Tooltips ──────────────────────────────────────────────
            this.toolTip1.SetToolTip(btnDashboard, "Dashboard");
            this.toolTip1.SetToolTip(btnAccounts, "Account Master");
            this.toolTip1.SetToolTip(btnDesignMaster, "Design Master");
            this.toolTip1.SetToolTip(btnWorkOrder, "Work Order");  // ← NEW
            this.toolTip1.SetToolTip(btnServices, "Service Master");
            this.toolTip1.SetToolTip(btnCreateInvoice, "Create Invoice");
            this.toolTip1.SetToolTip(btnInvoiceList, "Invoice List");
            this.toolTip1.SetToolTip(btnReports, "Reports");
            this.toolTip1.SetToolTip(btnGstReturn, "GST Return Filing");
            this.toolTip1.SetToolTip(btnSettings, "Settings");
            this.toolTip1.SetToolTip(btnNavLogout, "Logout");

            // ── Click handlers ────────────────────────────────────────
            this.btnDashboard.Click += new System.EventHandler(this.btnDashboard_Click);
            this.btnAccounts.Click += new System.EventHandler(this.btnAccounts_Click);
            this.btnDesignMaster.Click += new System.EventHandler(this.btnDesignMaster_Click);
            this.btnWorkOrder.Click += new System.EventHandler(this.btnWorkOrder_Click); // ← NEW
            this.btnServices.Click += new System.EventHandler(this.btnServices_Click);
            this.btnCreateInvoice.Click += new System.EventHandler(this.btnCreateInvoice_Click);
            this.btnInvoiceList.Click += new System.EventHandler(this.btnInvoiceList_Click);
            this.btnReports.Click += new System.EventHandler(this.btnReports_Click);
            this.btnGstReturn.Click += new System.EventHandler(this.btnGstReturn_Click);
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);

            // ── Assemble scrollable nav panel ─────────────────────────
            this.pnlSidebarScroll.Controls.Add(this.pnlActiveNav);
            this.pnlSidebarScroll.Controls.Add(this.lblNavMain);
            this.pnlSidebarScroll.Controls.Add(this.btnDashboard);
            this.pnlSidebarScroll.Controls.Add(this.lblNavMasters);
            this.pnlSidebarScroll.Controls.Add(this.btnAccounts);
            this.pnlSidebarScroll.Controls.Add(this.btnDesignMaster);
            this.pnlSidebarScroll.Controls.Add(this.btnWorkOrder);       // ← NEW
            this.pnlSidebarScroll.Controls.Add(this.btnServices);
            this.pnlSidebarScroll.Controls.Add(this.lblNavInvoicing);
            this.pnlSidebarScroll.Controls.Add(this.btnCreateInvoice);
            this.pnlSidebarScroll.Controls.Add(this.btnInvoiceList);
            this.pnlSidebarScroll.Controls.Add(this.lblNavAnalytics);
            this.pnlSidebarScroll.Controls.Add(this.btnReports);
            this.pnlSidebarScroll.Controls.Add(this.btnGstReturn);
            this.pnlSidebarScroll.Controls.Add(this.pnlNavDivider1);
            this.pnlSidebarScroll.Controls.Add(this.btnSettings);
            this.pnlSidebarScroll.Controls.Add(this.btnNavLogout);

            // ── Assemble sidebar ──────────────────────────────────────
            this.pnlSidebar.Controls.Add(this.pnlSidebarScroll);
            this.pnlSidebar.Controls.Add(this.pnlUserInfo);

            // ════════════════════════════════════════════════════════
            //  CONTENT AREA
            // ════════════════════════════════════════════════════════
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.AutoScroll = true;

            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlSidebar);
            this.Controls.Add(this.pnlHeader);

            this.pnlHeader.ResumeLayout(false); this.pnlHeader.PerformLayout();
            this.pnlHeaderRight.ResumeLayout(false);
            this.pnlSidebar.ResumeLayout(false); this.pnlSidebar.PerformLayout();
            this.pnlSidebarScroll.ResumeLayout(false);
            this.pnlUserInfo.ResumeLayout(false); this.pnlUserInfo.PerformLayout();
            this.ResumeLayout(false); this.PerformLayout();
        }
        #endregion

        // ── Field declarations ────────────────────────────────────────
        private System.Windows.Forms.Panel pnlHeader, pnlHeaderAccent, pnlHeaderRight;
        private System.Windows.Forms.Label lblLogoIcon, lblAppTitle, lblAppSub;
        private System.Windows.Forms.Label lblStatusDot, lblSystemOnline, lblUserAvatar;
        private System.Windows.Forms.Label lblCompanyName;
        private System.Windows.Forms.Button btnLogout, btnMinimize, btnRestore, btnClose;
        private System.Windows.Forms.Panel pnlSidebar, pnlSidebarScroll, pnlUserInfo, pnlActiveNav, pnlNavDivider1;
        private System.Windows.Forms.Label lblUserName, lblUserRole;
        private System.Windows.Forms.Label lblNavMain, lblNavMasters, lblNavInvoicing, lblNavAnalytics;
        private System.Windows.Forms.Button btnDashboard;
        private System.Windows.Forms.Button btnAccounts;
        private System.Windows.Forms.Button btnDesignMaster;
        private System.Windows.Forms.Button btnWorkOrder;   // ← NEW
        private System.Windows.Forms.Button btnServices;
        private System.Windows.Forms.Button btnCreateInvoice;
        private System.Windows.Forms.Button btnInvoiceList;
        private System.Windows.Forms.Button btnReports;
        private System.Windows.Forms.Button btnGstReturn;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnNavLogout;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}