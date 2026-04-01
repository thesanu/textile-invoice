namespace Textile_Invoice_App
{
    partial class Login
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.pnlCard = new System.Windows.Forms.Panel();
            this.pnlAccent = new System.Windows.Forms.Panel();
            this.lblLogo = new System.Windows.Forms.Label();
            this.lblAppName = new System.Windows.Forms.Label();
            this.lblTagline = new System.Windows.Forms.Label();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.pnlUserLine = new System.Windows.Forms.Panel();
            this.lblPass = new System.Windows.Forms.Label();
            this.TextBox2 = new System.Windows.Forms.TextBox();
            this.pnlPassLine = new System.Windows.Forms.Panel();
            this.Button1 = new System.Windows.Forms.Button();
            this.Button2 = new System.Windows.Forms.Button();
            this.lblVersion = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnMin = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();

            // ── Colour palette ────────────────────────────────────────
            var bgDark = System.Drawing.Color.FromArgb(13, 17, 28);
            var bgPanel = System.Drawing.Color.FromArgb(20, 26, 40);
            var bgCard = System.Drawing.Color.FromArgb(26, 33, 52);
            var accent = System.Drawing.Color.FromArgb(37, 99, 235);
            var accentL = System.Drawing.Color.FromArgb(59, 130, 246);
            var gold = System.Drawing.Color.FromArgb(251, 191, 36);
            var textMain = System.Drawing.Color.FromArgb(226, 232, 240);
            var textMuted = System.Drawing.Color.FromArgb(100, 116, 139);
            var textDim = System.Drawing.Color.FromArgb(148, 163, 184);
            var inputBg = System.Drawing.Color.FromArgb(15, 20, 35);
            var cardBorder = System.Drawing.Color.FromArgb(37, 47, 73);

            // ── Form ──────────────────────────────────────────────────
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = bgDark;
            this.ClientSize = new System.Drawing.Size(1100, 680);
            this.MinimumSize = new System.Drawing.Size(720, 500);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Name = "Login";
            this.Text = "Billing Invoice Management";
            this.Load += new System.EventHandler(this.Login_Load);

            // ════════════════════════════════════════════════════════
            //  WINDOW CONTROL BUTTONS
            // ════════════════════════════════════════════════════════

            // ✕ Close
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(220, 38, 38);
            this.btnExit.BackColor = bgDark;
            this.btnExit.ForeColor = System.Drawing.Color.FromArgb(200, 210, 230);
            this.btnExit.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnExit.Text = "✕";
            this.btnExit.Size = new System.Drawing.Size(42, 42);
            this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.BtnExit_Click);

            // ⬜ Restore / Maximize toggle
            this.btnRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestore.FlatAppearance.BorderSize = 0;
            this.btnRestore.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnRestore.BackColor = bgDark;
            this.btnRestore.ForeColor = System.Drawing.Color.FromArgb(200, 210, 230);
            this.btnRestore.Font = new System.Drawing.Font("Marlett", 10F);
            this.btnRestore.Text = "2";   // Marlett: 2=restore/maximise
            this.btnRestore.Size = new System.Drawing.Size(42, 42);
            this.btnRestore.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestore.UseVisualStyleBackColor = false;
            this.btnRestore.Click += new System.EventHandler(this.BtnRestore_Click);

            // ─ Minimize
            this.btnMin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMin.FlatAppearance.BorderSize = 0;
            this.btnMin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnMin.BackColor = bgDark;
            this.btnMin.ForeColor = System.Drawing.Color.FromArgb(200, 210, 230);
            this.btnMin.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.btnMin.Text = "─";
            this.btnMin.Size = new System.Drawing.Size(42, 42);
            this.btnMin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMin.UseVisualStyleBackColor = false;
            this.btnMin.Click += new System.EventHandler(this.BtnMin_Click);

            // ════════════════════════════════════════════════════════
            //  LEFT PANEL  — proportional content positioning on resize
            // ════════════════════════════════════════════════════════
            this.pnlLeft.BackColor = bgPanel;
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Width = 480;
            this.pnlLeft.Name = "pnlLeft";

            this.pnlAccent.BackColor = gold;
            this.pnlAccent.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlAccent.Width = 3;

            this.lblLogo.Text = "🧵";
            this.lblLogo.Font = new System.Drawing.Font("Segoe UI Emoji", 48F);
            this.lblLogo.ForeColor = gold;
            this.lblLogo.AutoSize = true;
            this.lblLogo.Name = "lblLogo";

            this.lblAppName.Text = "Billing Invoice";
            this.lblAppName.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            this.lblAppName.ForeColor = textMain;
            this.lblAppName.AutoSize = true;
            this.lblAppName.Name = "lblAppName";

            var pnlUnderline = new System.Windows.Forms.Panel();
            pnlUnderline.BackColor = gold;
            pnlUnderline.Size = new System.Drawing.Size(200, 3);
            pnlUnderline.Name = "pnlUnderline";

            this.lblTagline.Text = "MANAGEMENT SYSTEM";
            this.lblTagline.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTagline.ForeColor = textMuted;
            this.lblTagline.AutoSize = true;
            this.lblTagline.Name = "lblTagline";

            string[] features = {
                "✦  Multi-Company Billing",
                "✦  Design & Client Masters",
                "✦  Smart Invoice Tracking",
                "✦  Reports & Analytics"
            };
            var featureLabels = new System.Collections.Generic.List<System.Windows.Forms.Label>();
            foreach (var ft in features)
            {
                var lbl = new System.Windows.Forms.Label();
                lbl.Text = ft;
                lbl.Font = new System.Drawing.Font("Segoe UI", 10F);
                lbl.ForeColor = textDim;
                lbl.AutoSize = true;
                featureLabels.Add(lbl);
                this.pnlLeft.Controls.Add(lbl);
            }

            this.lblVersion.Text = "v2.0.2026  · BILLING SYSTEM - MANAGED BY Seema IT Solutions";
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblVersion.ForeColor = textMuted;
            this.lblVersion.AutoSize = true;
            this.lblVersion.Name = "lblVersion";

            // ── Height-proportional repositioning for left panel ──────
            this.pnlLeft.Resize += (s, e2) => {
                int h = this.pnlLeft.Height;
                int cx = 80;

                int logoY = System.Math.Max(40, (int)(h * 0.22));
                this.lblLogo.Location = new System.Drawing.Point(cx, logoY);

                int nameY = logoY + this.lblLogo.Height + 6;
                this.lblAppName.Location = new System.Drawing.Point(cx, nameY);

                int underY = nameY + this.lblAppName.Height + 4;
                pnlUnderline.Location = new System.Drawing.Point(cx, underY);

                int tagY = underY + 8;
                this.lblTagline.Location = new System.Drawing.Point(cx + 2, tagY);

                int gap = System.Math.Max(26, (int)(h * 0.046));
                int fy = tagY + 34;
                foreach (var fl in featureLabels)
                {
                    fl.Location = new System.Drawing.Point(cx + 2, fy);
                    fy += gap;
                }

                this.lblVersion.Location = new System.Drawing.Point(cx, h - 28);
            };

            this.pnlLeft.Controls.Add(this.pnlAccent);
            this.pnlLeft.Controls.Add(this.lblLogo);
            this.pnlLeft.Controls.Add(this.lblAppName);
            this.pnlLeft.Controls.Add(pnlUnderline);
            this.pnlLeft.Controls.Add(this.lblTagline);
            this.pnlLeft.Controls.Add(this.lblVersion);

            // ════════════════════════════════════════════════════════
            //  RIGHT PANEL
            // ════════════════════════════════════════════════════════
            this.pnlRight.BackColor = bgDark;
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Name = "pnlRight";

            // ── Login card ────────────────────────────────────────────
            this.pnlCard.BackColor = bgCard;
            this.pnlCard.Size = new System.Drawing.Size(380, 440);
            this.pnlCard.Name = "pnlCard";

            var topStripe = new System.Windows.Forms.Panel();
            topStripe.BackColor = accent;
            topStripe.Dock = System.Windows.Forms.DockStyle.Top;
            topStripe.Height = 4;
            this.pnlCard.Controls.Add(topStripe);

            this.lblWelcome.Text = "Welcome Back";
            this.lblWelcome.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblWelcome.ForeColor = textMain;
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Location = new System.Drawing.Point(36, 30);
            this.pnlCard.Controls.Add(this.lblWelcome);

            this.lblSubtitle.Text = "Sign in to your account to continue";
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtitle.ForeColor = textMuted;
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Location = new System.Drawing.Point(38, 64);
            this.pnlCard.Controls.Add(this.lblSubtitle);

            // Username
            this.lblUser.Text = "USERNAME";
            this.lblUser.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblUser.ForeColor = textMuted;
            this.lblUser.AutoSize = true;
            this.lblUser.Location = new System.Drawing.Point(36, 108);
            this.pnlCard.Controls.Add(this.lblUser);

            this.TextBox1.BackColor = inputBg;
            this.TextBox1.ForeColor = textDim;
            this.TextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.TextBox1.Text = "Username";
            this.TextBox1.Location = new System.Drawing.Point(36, 128);
            this.TextBox1.Size = new System.Drawing.Size(308, 30);
            this.TextBox1.TabIndex = 0;
            this.TextBox1.Enter += new System.EventHandler(this.TextBox1_Enter);
            this.TextBox1.Leave += new System.EventHandler(this.TextBox1_Leave);
            this.TextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox1_KeyDown);
            this.pnlCard.Controls.Add(this.TextBox1);

            this.pnlUserLine.BackColor = accent;
            this.pnlUserLine.Size = new System.Drawing.Size(308, 2);
            this.pnlUserLine.Location = new System.Drawing.Point(36, 160);
            this.pnlCard.Controls.Add(this.pnlUserLine);

            // Password
            this.lblPass.Text = "PASSWORD";
            this.lblPass.Font = new System.Drawing.Font("Segoe UI", 7.5F, System.Drawing.FontStyle.Bold);
            this.lblPass.ForeColor = textMuted;
            this.lblPass.AutoSize = true;
            this.lblPass.Location = new System.Drawing.Point(36, 180);
            this.pnlCard.Controls.Add(this.lblPass);

            this.TextBox2.BackColor = inputBg;
            this.TextBox2.ForeColor = textDim;
            this.TextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox2.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.TextBox2.Text = "Password";
            this.TextBox2.PasswordChar = '\0';
            this.TextBox2.Location = new System.Drawing.Point(36, 200);
            this.TextBox2.Size = new System.Drawing.Size(308, 30);
            this.TextBox2.TabIndex = 1;
            this.TextBox2.Enter += new System.EventHandler(this.TextBox2_Enter);
            this.TextBox2.Leave += new System.EventHandler(this.TextBox2_Leave);
            this.TextBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBox2_KeyDown);
            this.pnlCard.Controls.Add(this.TextBox2);

            this.pnlPassLine.BackColor = accent;
            this.pnlPassLine.Size = new System.Drawing.Size(308, 2);
            this.pnlPassLine.Location = new System.Drawing.Point(36, 232);
            this.pnlCard.Controls.Add(this.pnlPassLine);

            // Sign In
            this.Button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button1.FlatAppearance.BorderSize = 0;
            this.Button1.FlatAppearance.MouseOverBackColor = accentL;
            this.Button1.BackColor = accent;
            this.Button1.ForeColor = System.Drawing.Color.White;
            this.Button1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.Button1.Text = "SIGN  IN  →";
            this.Button1.Location = new System.Drawing.Point(36, 254);
            this.Button1.Size = new System.Drawing.Size(308, 46);
            this.Button1.TabIndex = 2;
            this.Button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button1.UseVisualStyleBackColor = false;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            this.pnlCard.Controls.Add(this.Button1);

            var pnlDivider = new System.Windows.Forms.Panel();
            pnlDivider.BackColor = cardBorder;
            pnlDivider.Size = new System.Drawing.Size(308, 1);
            pnlDivider.Location = new System.Drawing.Point(36, 316);
            this.pnlCard.Controls.Add(pnlDivider);

            this.Button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button2.FlatAppearance.BorderColor = cardBorder;
            this.Button2.FlatAppearance.BorderSize = 1;
            this.Button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(30, 40, 62);
            this.Button2.BackColor = System.Drawing.Color.FromArgb(26, 33, 52);
            this.Button2.ForeColor = textDim;
            this.Button2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Button2.Text = "New here?  Create an account";
            this.Button2.Location = new System.Drawing.Point(36, 332);
            this.Button2.Size = new System.Drawing.Size(308, 38);
            this.Button2.TabIndex = 3;
            this.Button2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button2.UseVisualStyleBackColor = false;
            this.Button2.Click += new System.EventHandler(this.Button2_Click);
            this.pnlCard.Controls.Add(this.Button2);

            // ── Forgot Password link ──────────────────────────────────
            this.btnForgotPwd = new System.Windows.Forms.Button();
            this.btnForgotPwd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnForgotPwd.FlatAppearance.BorderSize = 0;
            this.btnForgotPwd.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(20, 30, 50);
            this.btnForgotPwd.BackColor = System.Drawing.Color.Transparent;
            this.btnForgotPwd.ForeColor = System.Drawing.Color.FromArgb(59, 130, 246);
            this.btnForgotPwd.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Underline);
            this.btnForgotPwd.Text = "Forgot password?";
            this.btnForgotPwd.Location = new System.Drawing.Point(36, 376);
            this.btnForgotPwd.Size = new System.Drawing.Size(308, 24);
            this.btnForgotPwd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnForgotPwd.TabIndex = 4;
            this.btnForgotPwd.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnForgotPwd.UseVisualStyleBackColor = false;
            this.btnForgotPwd.Click += new System.EventHandler(this.BtnForgotPwd_Click);
            this.pnlCard.Controls.Add(this.btnForgotPwd);

            var lblCopy = new System.Windows.Forms.Label();
            lblCopy.Text = "© 2026 Billing Invoice Management";
            lblCopy.Font = new System.Drawing.Font("Segoe UI", 7.5F);
            lblCopy.ForeColor = System.Drawing.Color.FromArgb(60, 72, 100);
            lblCopy.AutoSize = true;
            lblCopy.Location = new System.Drawing.Point(36, 408);
            this.pnlCard.Controls.Add(lblCopy);

            // ── pnlRight Resize → re-centre card & reposition buttons ─
            this.pnlRight.Resize += (s, e2) =>
            {
                int rw = this.pnlRight.Width;
                int rh = this.pnlRight.Height;

                // Card height: 75% of panel height, clamped 440–500
                int cardH = System.Math.Max(440, System.Math.Min(500, (int)(rh * 0.75)));
                this.pnlCard.Height = cardH;

                // Centre card both axes
                this.pnlCard.Location = new System.Drawing.Point(
                    System.Math.Max(0, (rw - this.pnlCard.Width) / 2),
                    System.Math.Max(0, (rh - this.pnlCard.Height) / 2));

                // Window buttons top-right corner
                this.btnExit.Location = new System.Drawing.Point(rw - 42, 0);
                this.btnRestore.Location = new System.Drawing.Point(rw - 84, 0);
                this.btnMin.Location = new System.Drawing.Point(rw - 126, 0);
            };

            // ── Assemble pnlRight ─────────────────────────────────────
            this.pnlRight.Controls.Add(this.pnlCard);
            this.pnlRight.Controls.Add(this.btnMin);
            this.pnlRight.Controls.Add(this.btnRestore);
            this.pnlRight.Controls.Add(this.btnExit);

            // ── Assemble Form ─────────────────────────────────────────
            this.Controls.Add(this.pnlRight);   // Dock=Fill
            this.Controls.Add(this.pnlLeft);    // Dock=Left (overlays Fill)

            this.pnlLeft.ResumeLayout(false); this.pnlLeft.PerformLayout();
            this.pnlRight.ResumeLayout(false);
            this.pnlCard.ResumeLayout(false); this.pnlCard.PerformLayout();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.Panel pnlLeft, pnlRight, pnlCard, pnlAccent;
        private System.Windows.Forms.Label lblLogo, lblAppName, lblTagline, lblVersion;
        private System.Windows.Forms.Label lblWelcome, lblSubtitle, lblUser, lblPass;
        private System.Windows.Forms.Panel pnlUserLine, pnlPassLine;
        private System.Windows.Forms.TextBox TextBox1, TextBox2;
        private System.Windows.Forms.Button Button1, Button2, btnForgotPwd;
        private System.Windows.Forms.Button btnExit, btnMin, btnRestore;
    }
}