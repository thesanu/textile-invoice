using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Textile_Invoice_App.Models;

namespace Textile_Invoice_App
{
    public partial class Login : Form
    {
        private static readonly Color InputActiveBg = Color.FromArgb(22, 28, 45);
        private static readonly Color InputNormalBg = Color.FromArgb(15, 20, 35);
        private static readonly Color TextActive = Color.FromArgb(226, 232, 240);
        private static readonly Color TextPlaceholder = Color.FromArgb(100, 116, 139);
        private static readonly Color AccentBlue = Color.FromArgb(37, 99, 235);
        private static readonly Color LineActive = Color.FromArgb(59, 130, 246);
        private static readonly Color LineNormal = Color.FromArgb(37, 99, 235);

        private bool _registerOpen = false;   // guard against double-open

        public Login()
        {
            InitializeComponent();
            TextBox2.UseSystemPasswordChar = false;
            Button1.MouseEnter += (s, e) => Button1.BackColor = Color.FromArgb(59, 130, 246);
            Button1.MouseLeave += (s, e) => Button1.BackColor = AccentBlue;
            this.ActiveControl = TextBox1;

            // ── FIX: Button2.Click is already wired in Login_Designer.cs
            // DO NOT wire it again here — doing so caused the form to open twice.
            // Button2.Click += Button2_Click;   ← REMOVED
        }

        private void Login_Load(object sender, EventArgs e) => CenterCard();

        private void CenterCard()
        {
            pnlCard.Location = new Point(
                (pnlRight.Width - pnlCard.Width) / 2,
                (pnlRight.Height - pnlCard.Height) / 2);
        }

        // ── LOGIN ─────────────────────────────────────────────────────
        private void Button1_Click(object sender, EventArgs e)
        {
            string username = TextBox1.Text.Trim();
            string password = TextBox2.Text.Trim();

            if (username == "Username" || password == "Password" || string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter your credentials.", "Input Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                TextBox1.Focus();
                return;
            }

            try
            {
                using (var db = new AppDbContext())
                {
                    var user = db.AppUsers.FirstOrDefault(u =>
                        u.Username == username &&
                        u.PasswordHash == password &&
                        u.IsActive == true);

                    if (user == null)
                    {
                        ShakeCard();
                        MessageBox.Show("Invalid Username or Password.",
                            "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var company = db.CompanyProfiles
                        .FirstOrDefault(c => c.CompanyProfileId == user.CompanyProfileId);

                    if (company == null)
                    {
                        MessageBox.Show("Company profile not found for this user.",
                            "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    SessionManager.UserId = user.UserId;
                    SessionManager.Username = user.Username;
                    SessionManager.FullName = user.FullName ?? user.Username;
                    SessionManager.CompanyProfileId = user.CompanyProfileId;
                    SessionManager.CompanyName = company.CompanyName;

                    var dashboard = new Dashboard();
                    dashboard.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── REGISTER NEW COMPANY ──────────────────────────────────────
        private void Button2_Click(object sender, EventArgs e)
        {
            if (_registerOpen) return;

            // ── Passkey check ─────────────────────────────────────────
            // Only authorised admins who know the passkey can create a company.
            using var dlg = new PasskeyDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            _registerOpen = true;
            try
            {
                var reg = new RegisterCompany();
                reg.ShowDialog(this);
            }
            finally
            {
                _registerOpen = false;
            }
        }

        // ── FORGOT PASSWORD ───────────────────────────────────────────
        private void BtnForgotPwd_Click(object sender, EventArgs e)
        {
            ShowForgotPasswordDialog();
        }

        private void ShowForgotPasswordDialog()
        {
            // ── Palette (matches dark theme) ──────────────────────────
            var BgDark = Color.FromArgb(13, 17, 28);
            var BgCard = Color.FromArgb(26, 33, 52);
            var Accent = Color.FromArgb(37, 99, 235);
            var AccentL = Color.FromArgb(59, 130, 246);
            var TextMain = Color.FromArgb(226, 232, 240);
            var TextMuted = Color.FromArgb(100, 116, 139);
            var TextDim = Color.FromArgb(148, 163, 184);
            var InputBg = Color.FromArgb(15, 20, 35);
            var Red = Color.FromArgb(220, 38, 38);
            var Green = Color.FromArgb(22, 163, 74);

            using var frm = new Form
            {
                Text = "Reset Password",
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                BackColor = BgDark,
                Size = new Size(420, 520),
                MinimumSize = new Size(360, 480)
            };

            // ── Card ──────────────────────────────────────────────────
            var card = new Panel
            {
                BackColor = BgCard,
                Location = new Point(20, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                            AnchorStyles.Right | AnchorStyles.Bottom
            };
            card.Controls.Add(new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = Accent
            });

            // ── Close (✕) ─────────────────────────────────────────────
            var btnX = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = BgCard,
                ForeColor = TextDim,
                Size = new Size(34, 34),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.FlatAppearance.MouseOverBackColor = Red;
            btnX.Click += (s, e) => frm.Close();
            card.Controls.Add(btnX);

            // Helper — add a labelled input inside the card
            int iy = 54;
            TextBox AddInput(string labelText, bool isPass = false)
            {
                card.Controls.Add(new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    ForeColor = TextMuted,
                    AutoSize = true,
                    Location = new Point(28, iy)
                });
                iy += 20;
                var txt = new TextBox
                {
                    BackColor = InputBg,
                    ForeColor = TextMain,
                    BorderStyle = BorderStyle.None,
                    Font = new Font("Segoe UI", 11F),
                    Size = new Size(card.Width - 56, 28),
                    Location = new Point(28, iy),
                    UseSystemPasswordChar = isPass,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                card.Controls.Add(txt);
                iy += 28;
                var line = new Panel
                {
                    BackColor = Accent,
                    Size = new Size(card.Width - 56, 1),
                    Location = new Point(28, iy),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                card.Controls.Add(line);
                iy += 22;
                return txt;
            }

            // ── Title ─────────────────────────────────────────────────
            card.Controls.Add(new Label
            {
                Text = "🔑  Reset Your Password",
                Font = new Font("Segoe UI Emoji", 13F, FontStyle.Bold),
                ForeColor = TextMain,
                AutoSize = true,
                Location = new Point(28, 16)
            });

            var txtPasskey = AddInput("ADMIN PASSKEY", isPass: true);
            var txtUsername = AddInput("YOUR USERNAME");
            var txtNewPass = AddInput("NEW PASSWORD", isPass: true);
            var txtConfPass = AddInput("CONFIRM NEW PASSWORD", isPass: true);

            // ── Error label ───────────────────────────────────────────
            var lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Red,
                AutoSize = false,
                Size = new Size(card.Width - 56, 32),
                Location = new Point(28, iy),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(lblError);
            iy += 36;

            // ── Reset button ──────────────────────────────────────────
            var btnReset = new Button
            {
                Text = "✓  Reset Password",
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(card.Width - 56, 44),
                Location = new Point(28, iy),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.FlatAppearance.MouseOverBackColor = AccentL;
            card.Controls.Add(btnReset);

            // ── Resize card to fill form ──────────────────────────────
            frm.Resize += (s, e) =>
            {
                card.Size = new Size(frm.ClientSize.Width - 40,
                                     frm.ClientSize.Height - 40);
                btnX.Location = new Point(card.Width - 38, 4);
            };
            frm.Controls.Add(card);

            // ── Reset logic ───────────────────────────────────────────
            void TryReset()
            {
                lblError.Text = "";

                // 1. Passkey check (same constant as PasskeyDialog)
                const string PASSKEY = "Anki#5889";
                if (txtPasskey.Text.Trim() != PASSKEY)
                {
                    lblError.Text = "Incorrect admin passkey.";
                    txtPasskey.Clear(); txtPasskey.Focus();
                    // Flash red
                    txtPasskey.BackColor = Color.FromArgb(80, 20, 20);
                    var t = new System.Windows.Forms.Timer { Interval = 900 };
                    t.Tick += (s, e) => { txtPasskey.BackColor = InputBg; t.Stop(); t.Dispose(); };
                    t.Start();
                    return;
                }

                // 2. Username must exist
                string uname = txtUsername.Text.Trim();
                if (string.IsNullOrWhiteSpace(uname))
                { lblError.Text = "Please enter your username."; txtUsername.Focus(); return; }

                // 3. Password strength
                if (txtNewPass.Text.Length < 6)
                { lblError.Text = "New password must be at least 6 characters."; txtNewPass.Focus(); return; }

                // 4. Passwords match
                if (txtNewPass.Text != txtConfPass.Text)
                { lblError.Text = "Passwords do not match."; txtConfPass.Clear(); txtConfPass.Focus(); return; }

                // 5. Look up the user in the DB
                try
                {
                    using var db = new AppDbContext();
                    var user = db.AppUsers.FirstOrDefault(u =>
                        u.Username == uname && u.IsActive == true);

                    if (user == null)
                    {
                        lblError.Text = "Username not found or account is inactive.";
                        txtUsername.Focus();
                        return;
                    }

                    user.PasswordHash = txtNewPass.Text;   // consistent with login
                    db.SaveChanges();

                    MessageBox.Show(
                        $"✓  Password for \"{uname}\" has been reset successfully.\n\nYou can now sign in with your new password.",
                        "Password Reset",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    frm.Close();
                }
                catch (Exception ex)
                {
                    lblError.Text = "Error: " + ex.Message;
                }
            }

            btnReset.Click += (s, e) => TryReset();

            // Enter key submits from any field
            foreach (Control c in card.Controls)
                if (c is TextBox tb)
                    tb.KeyDown += (s, e) =>
                    {
                        if (((KeyEventArgs)e).KeyCode == Keys.Enter)
                        { ((KeyEventArgs)e).SuppressKeyPress = true; TryReset(); }
                        if (((KeyEventArgs)e).KeyCode == Keys.Escape)
                            frm.Close();
                    };

            frm.ShowDialog(this);
        }

        // ── USERNAME placeholder ──────────────────────────────────────
        private void TextBox1_Enter(object sender, EventArgs e)
        {
            if (TextBox1.Text == "Username") { TextBox1.Text = ""; TextBox1.ForeColor = TextActive; }
            TextBox1.BackColor = InputActiveBg;
            pnlUserLine.BackColor = LineActive;
        }
        private void TextBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBox1.Text)) { TextBox1.Text = "Username"; TextBox1.ForeColor = TextPlaceholder; }
            TextBox1.BackColor = InputNormalBg;
            pnlUserLine.BackColor = LineNormal;
        }
        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; TextBox2.Focus(); }
        }

        // ── PASSWORD placeholder ──────────────────────────────────────
        private void TextBox2_Enter(object sender, EventArgs e)
        {
            if (TextBox2.Text == "Password") { TextBox2.Text = ""; TextBox2.ForeColor = TextActive; TextBox2.UseSystemPasswordChar = true; }
            TextBox2.BackColor = InputActiveBg;
            pnlPassLine.BackColor = LineActive;
        }
        private void TextBox2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBox2.Text)) { TextBox2.UseSystemPasswordChar = false; TextBox2.Text = "Password"; TextBox2.ForeColor = TextPlaceholder; }
            TextBox2.BackColor = InputNormalBg;
            pnlPassLine.BackColor = LineNormal;
        }
        private void TextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Button1.PerformClick(); }
        }

        // ── WINDOW CONTROLS ───────────────────────────────────────────
        private void BtnExit_Click(object sender, EventArgs e) => Application.Exit();
        private void BtnMin_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;
        private void BtnRestore_Click(object sender, EventArgs e) =>
            this.WindowState = this.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;

        // ── BORDERLESS DRAG ───────────────────────────────────────────
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x84) m.Result = new System.IntPtr(0x2);
        }

        // ── SHAKE on wrong password ───────────────────────────────────
        private async void ShakeCard()
        {
            int origX = pnlCard.Left;
            int[] steps = { -10, 10, -8, 8, -5, 5, -3, 3, 0 };
            foreach (int offset in steps)
            {
                pnlCard.Left = origX + offset;
                await System.Threading.Tasks.Task.Delay(28);
            }
            pnlCard.Left = origX;
        }
    }
}