using System;
using System.Drawing;
using System.Windows.Forms;

namespace Textile_Invoice_App
{
    /// <summary>
    /// Small modal dialog shown before RegisterCompany.
    /// Returns DialogResult.OK only when the correct passkey is entered.
    /// Change PASSKEY constant to whatever secret you want.
    /// </summary>
    public class PasskeyDialog : Form
    {
        // ── Change this to your desired passkey ───────────────────────
        private const string PASSKEY = "Anki#5889";

        // ── Palette (matches app theme) ───────────────────────────────
        static readonly Color BgDark = Color.FromArgb(13, 17, 28);
        static readonly Color BgCard = Color.FromArgb(26, 33, 52);
        static readonly Color Accent = Color.FromArgb(37, 99, 235);
        static readonly Color AccentL = Color.FromArgb(59, 130, 246);
        static readonly Color Gold = Color.FromArgb(251, 191, 36);
        static readonly Color TextMain = Color.FromArgb(226, 232, 240);
        static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
        static readonly Color TextDim = Color.FromArgb(148, 163, 184);
        static readonly Color InputBg = Color.FromArgb(15, 20, 35);
        static readonly Color CardBdr = Color.FromArgb(37, 47, 73);
        static readonly Color RedHover = Color.FromArgb(220, 38, 38);

        private TextBox _txtPasskey;

        public PasskeyDialog()
        {
            this.Text = "Admin Passkey";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = BgDark;
            this.Size = new Size(380, 230);

            BuildUI();
        }

        private void BuildUI()
        {
            // ── Card ──────────────────────────────────────────────────
            var card = new Panel
            {
                BackColor = BgCard,
                Dock = DockStyle.Fill
            };

            // Accent stripe top
            card.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Accent });

            // ── Close button ──────────────────────────────────────────
            var btnX = new Button
            {
                Text = "✕",
                Size = new Size(32, 32),
                BackColor = BgCard,
                ForeColor = TextDim,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.FlatAppearance.MouseOverBackColor = RedHover;
            btnX.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            card.Controls.Add(btnX);
            card.Resize += (s, e) => btnX.Location = new Point(card.Width - 36, 6);
            this.Shown += (s, e) => btnX.Location = new Point(card.Width - 36, 6);

            // ── Content table ─────────────────────────────────────────
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = BgCard,
                Padding = new Padding(28, 20, 28, 20),
                ColumnCount = 1,
                RowCount = 4
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // title
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // subtitle
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // input
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));  // buttons

            // Title
            tbl.Controls.Add(new Label
            {
                Text = "🔐  Enter Admin Passkey",
                Font = new Font("Segoe UI Emoji", 11F, FontStyle.Bold),
                ForeColor = TextMain,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            }, 0, 0);

            // Subtitle
            tbl.Controls.Add(new Label
            {
                Text = "Enter the administrator passkey to register a new company.",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextMuted,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 14)
            }, 0, 1);

            // Passkey input
            _txtPasskey = new TextBox
            {
                BackColor = InputBg,
                ForeColor = TextMain,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11F),
                Dock = DockStyle.Fill,
                Height = 30,
                UseSystemPasswordChar = true,
                Margin = new Padding(0, 0, 0, 14)
            };
            _txtPasskey.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; TryConfirm(); }
                if (e.KeyCode == Keys.Escape) { this.DialogResult = DialogResult.Cancel; this.Close(); }
            };
            tbl.Controls.Add(_txtPasskey, 0, 2);

            // Buttons row
            var btnRow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = BgCard,
                Margin = new Padding(0)
            };

            var btnConfirm = new Button
            {
                Text = "✓  Confirm",
                Size = new Size(120, 36),
                BackColor = Accent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.FlatAppearance.MouseOverBackColor = AccentL;
            btnConfirm.Click += (s, e) => TryConfirm();
            btnRow.Controls.Add(btnConfirm);

            var btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(80, 36),
                BackColor = BgCard,
                ForeColor = TextDim,
                Font = new Font("Segoe UI", 9.5F),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnCancel.FlatAppearance.BorderColor = CardBdr;
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 40, 62);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            btnRow.Controls.Add(btnCancel);

            tbl.Controls.Add(btnRow, 0, 3);

            card.Controls.Add(tbl);
            this.Controls.Add(card);

            // Focus passkey input when shown
            this.Shown += (s, e) => _txtPasskey.Focus();
        }

        private void TryConfirm()
        {
            if (_txtPasskey.Text.Trim() == PASSKEY)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                // Flash red and clear
                _txtPasskey.BackColor = Color.FromArgb(80, 20, 20);
                var t = new System.Windows.Forms.Timer { Interval = 900 };
                t.Tick += (s, e) =>
                {
                    _txtPasskey.BackColor = Color.FromArgb(15, 20, 35);
                    t.Stop(); t.Dispose();
                };
                t.Start();
                _txtPasskey.Clear();
                _txtPasskey.Focus();
                MessageBox.Show("Incorrect passkey. Access denied.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}