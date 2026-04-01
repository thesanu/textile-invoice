using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App
{
    // ════════════════════════════════════════════════════════════════
    //  RegisterForm
    //  Opens when admin clicks "Create an account" and enters passkey.
    //  Creates a new CompanyProfile + AppUser in one transaction.
    //  Returns DialogResult.OK only when save succeeds.
    // ════════════════════════════════════════════════════════════════
    public class RegisterForm : Form
    {
        // ── Palette (matches Login dark theme) ───────────────────────
        private static readonly Color BgDark = Color.FromArgb(13, 17, 28);
        private static readonly Color BgPanel = Color.FromArgb(20, 26, 40);
        private static readonly Color BgCard = Color.FromArgb(26, 33, 52);
        private static readonly Color AccentBlue = Color.FromArgb(37, 99, 235);
        private static readonly Color AccentBlueL = Color.FromArgb(59, 130, 246);
        private static readonly Color Gold = Color.FromArgb(251, 191, 36);
        private static readonly Color TextMain = Color.FromArgb(226, 232, 240);
        private static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
        private static readonly Color TextDim = Color.FromArgb(148, 163, 184);
        private static readonly Color InputBg = Color.FromArgb(15, 20, 35);
        private static readonly Color CardBorder = Color.FromArgb(37, 47, 73);
        private static readonly Color Red = Color.FromArgb(220, 38, 38);

        // ── Input fields ─────────────────────────────────────────────
        private TextBox txtCompanyName, txtGstin, txtPan, txtPhone, txtCity, txtState;
        private TextBox txtUsername, txtPassword, txtConfirmPassword, txtFullName;

        public RegisterForm()
        {
            BuildForm();
        }

        private void BuildForm()
        {
            // ── Form setup ───────────────────────────────────────────
            this.Text = "Create New Account";
            this.Size = new Size(560, 720);
            this.MinimumSize = new Size(480, 660);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BgDark;

            // ── Outer card ───────────────────────────────────────────
            var pnlCard = new Panel
            {
                BackColor = BgCard,
                Size = new Size(500, 660),
                Location = new Point(30, 30)
            };
            pnlCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // Top accent stripe
            pnlCard.Controls.Add(new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = AccentBlue
            });

            // Scrollable inner panel
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BgCard,
                Padding = new Padding(32, 16, 32, 16)
            };

            int y = 20;

            // Title
            scroll.Controls.Add(Lbl("🧵  Create New Account", 18, TextMain, bold: true, loc: new Point(0, y))); y += 38;
            scroll.Controls.Add(Lbl("Fill in company and login details below", 9, TextMuted, loc: new Point(2, y))); y += 36;

            // ── Company Details ──────────────────────────────────────
            scroll.Controls.Add(SectionLabel("COMPANY DETAILS", y)); y += 28;

            txtCompanyName = AddField(scroll, "Company Name *", ref y);
            txtGstin = AddField(scroll, "GSTIN", ref y, upper: true);
            txtPan = AddField(scroll, "PAN", ref y, upper: true);
            txtPhone = AddField(scroll, "Phone", ref y);
            txtCity = AddField(scroll, "City", ref y);
            txtState = AddField(scroll, "State", ref y);

            y += 8;

            // ── Login Credentials ────────────────────────────────────
            scroll.Controls.Add(SectionLabel("LOGIN CREDENTIALS", y)); y += 28;

            txtFullName = AddField(scroll, "Full Name *", ref y);
            txtUsername = AddField(scroll, "Username *", ref y);
            txtPassword = AddField(scroll, "Password *", ref y, isPassword: true);
            txtConfirmPassword = AddField(scroll, "Confirm Password *", ref y, isPassword: true);

            y += 16;

            // ── Buttons ──────────────────────────────────────────────
            var btnCreate = new Button
            {
                Text = "✓  Create Account",
                BackColor = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(200, 42),
                Location = new Point(0, y),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.FlatAppearance.MouseOverBackColor = AccentBlueL;
            btnCreate.Click += BtnCreate_Click;
            scroll.Controls.Add(btnCreate);

            var btnCancel = new Button
            {
                Text = "✕  Cancel",
                BackColor = BgCard,
                ForeColor = TextDim,
                Font = new Font("Segoe UI", 10F),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 42),
                Location = new Point(210, y),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = CardBorder;
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 40, 62);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            scroll.Controls.Add(btnCancel);

            y += 60;

            // Close (✕) button top-right
            var btnX = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = BgDark,
                ForeColor = TextDim,
                Size = new Size(36, 36),
                Location = new Point(this.Width - 46, 4),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.FlatAppearance.MouseOverBackColor = Red;
            btnX.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnX);

            pnlCard.Controls.Add(scroll);
            this.Controls.Add(pnlCard);

            // Recentre card on resize
            this.Resize += (s, e) => {
                pnlCard.Width = this.ClientSize.Width - 60;
                pnlCard.Height = this.ClientSize.Height - 60;
                btnX.Location = new Point(this.ClientSize.Width - 46, 4);
            };
        }

        // ════════════════════════════════════════════════════════════
        //  SAVE
        // ════════════════════════════════════════════════════════════
        private void BtnCreate_Click(object sender, EventArgs e)
        {
            // ── Validation ───────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
            { Err("Company Name is required."); txtCompanyName.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            { Err("Full Name is required."); txtFullName.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            { Err("Username is required."); txtUsername.Focus(); return; }

            if (txtUsername.Text.Trim().Length < 3)
            { Err("Username must be at least 3 characters."); txtUsername.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            { Err("Password is required."); txtPassword.Focus(); return; }

            if (txtPassword.Text.Length < 6)
            { Err("Password must be at least 6 characters."); txtPassword.Focus(); return; }

            if (txtPassword.Text != txtConfirmPassword.Text)
            { Err("Passwords do not match. Please re-enter."); txtConfirmPassword.Focus(); return; }

            try
            {
                using var db = new AppDbContext();

                // Check username not already taken
                bool taken = db.AppUsers.Any(u =>
                    u.Username == txtUsername.Text.Trim());
                if (taken)
                { Err($"Username \"{txtUsername.Text.Trim()}\" is already in use. Choose another."); txtUsername.Focus(); return; }

                using var tx = db.Database.BeginTransaction();

                // 1. Create company profile
                var company = new CompanyProfile
                {
                    CompanyName = txtCompanyName.Text.Trim(),
                    Gstin = txtGstin.Text.Trim().ToUpper(),
                    Pan = txtPan.Text.Trim().ToUpper(),
                    Phone = txtPhone.Text.Trim(),
                    City = txtCity.Text.Trim(),
                    State = txtState.Text.Trim()
                };
                db.CompanyProfiles.Add(company);
                db.SaveChanges();   // get auto-generated CompanyProfileId

                // 2. Create user
                var user = new AppUser
                {
                    CompanyProfileId = company.CompanyProfileId,
                    Username = txtUsername.Text.Trim(),
                    PasswordHash = txtPassword.Text,   // plain text — consistent with existing login
                    FullName = txtFullName.Text.Trim(),
                    IsActive = true
                };
                db.AppUsers.Add(user);

                // 3. Create invoice number tracker for this company
                db.InvoiceNumberTrackers.Add(new InvoiceNumberTracker
                {
                    CompanyProfileId = company.CompanyProfileId,
                    CurrentInvoiceNo = 0
                });

                db.SaveChanges();
                tx.Commit();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating account:\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════
        //  UI HELPERS
        // ════════════════════════════════════════════════════════════
        private TextBox AddField(Panel p, string label, ref int y,
            bool isPassword = false, bool upper = false)
        {
            p.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(2, y)
            });
            y += 20;

            var txt = new TextBox
            {
                BackColor = InputBg,
                ForeColor = TextMain,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11F),
                Size = new Size(436, 28),
                Location = new Point(0, y),
                UseSystemPasswordChar = isPassword,
                CharacterCasing = upper ? CharacterCasing.Upper : CharacterCasing.Normal
            };
            p.Controls.Add(txt);
            y += 28;

            // Underline
            var line = new Panel
            {
                BackColor = AccentBlue,
                Size = new Size(436, 1),
                Location = new Point(0, y)
            };
            p.Controls.Add(line);
            y += 20;

            return txt;
        }

        private Label SectionLabel(string text, int y) =>
            new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(0, y)
            };

        private Label Lbl(string text, float size, Color color,
            bool bold = false, Point loc = default) =>
            new Label
            {
                Text = text,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = color,
                AutoSize = true,
                Location = loc
            };

        private void Err(string msg) =>
            MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}