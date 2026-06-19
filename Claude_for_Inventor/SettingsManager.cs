using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ClaudeForInventor
{
    // ──────────────────────────────────────────────────────────────────────────
    // SettingsManager — stores API key in HKCU registry
    // ──────────────────────────────────────────────────────────────────────────

    internal static class SettingsManager
    {
        private const string RegKey    = @"SOFTWARE\ClaudeForInventor";
        private const string KeyName   = "ApiKey";

        public static string GetApiKey()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegKey);
                return key?.GetValue(KeyName)?.ToString() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        public static void SetApiKey(string value)
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegKey);
            key.SetValue(KeyName, value?.Trim() ?? string.Empty);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // SettingsDialog
    // ──────────────────────────────────────────────────────────────────────────

    internal sealed class SettingsDialog : Form
    {
        private readonly TextBox _keyBox;

        public SettingsDialog()
        {
            Text            = "Claude AI — Settings";
            Size            = new Size(460, 180);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = Color.FromArgb(36, 36, 36);
            ForeColor       = Color.FromArgb(210, 210, 210);
            Font            = new Font("Segoe UI", 9f);

            // ── Labels ────────────────────────────────────────────────────────
            Controls.Add(new Label
            {
                Text     = "Cloudflare Worker Secret",
                Left     = 14, Top = 14, Width = 220, Height = 18,
                ForeColor = Color.FromArgb(160, 180, 210)
            });

            // ── Key TextBox ───────────────────────────────────────────────────
            _keyBox = new TextBox
            {
                Left         = 14, Top = 36, Width = 360, Height = 24,
                BackColor     = Color.FromArgb(50, 50, 50),
                ForeColor     = Color.White,
                BorderStyle   = BorderStyle.FixedSingle,
                PasswordChar  = '●',
                Text          = SettingsManager.GetApiKey(),
                Font          = new Font("Consolas", 9f)
            };
            Controls.Add(_keyBox);

            // ── Show / Hide ───────────────────────────────────────────────────
            var showBtn = MakeButton("Show", 380, 35, 52, 26);
            showBtn.Click += (_, __) =>
            {
                _keyBox.PasswordChar = _keyBox.PasswordChar == '●' ? '\0' : '●';
                showBtn.Text = _keyBox.PasswordChar == '●' ? "Show" : "Hide";
            };
            Controls.Add(showBtn);

            // ── Link ─────────────────────────────────────────────────────────
            var link = new LinkLabel
            {
                Text       = "Set WORKER_SECRET in Cloudflare → Worker → Settings → Variables",
                Left       = 14, Top = 68, Width = 410, Height = 18,
                LinkColor  = Color.FromArgb(100, 160, 255),
                ActiveLinkColor = Color.FromArgb(150, 200, 255),
                ForeColor  = Color.FromArgb(100, 160, 255)
            };
            link.LinkClicked += (_, __) =>
                System.Diagnostics.Process.Start("https://dash.cloudflare.com");
            Controls.Add(link);

            // ── Model info ────────────────────────────────────────────────────
            Controls.Add(new Label
            {
                Text      = $"Model: {ClaudeApiClient.Model}",
                Left      = 14, Top = 92, Width = 300, Height = 18,
                ForeColor = Color.FromArgb(120, 120, 120),
                Font      = new Font("Segoe UI", 8f)
            });

            // ── Save ─────────────────────────────────────────────────────────
            var saveBtn = MakeButton("Save", 14, 118, 80, 28);
            saveBtn.BackColor = Color.FromArgb(204, 75, 55);
            saveBtn.ForeColor = Color.White;
            saveBtn.Click += (_, __) =>
            {
                SettingsManager.SetApiKey(_keyBox.Text);
                MessageBox.Show("API key saved.", "Settings Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            };
            Controls.Add(saveBtn);

            // ── Cancel ────────────────────────────────────────────────────────
            var cancelBtn = MakeButton("Cancel", 102, 118, 70, 28);
            cancelBtn.Click += (_, __) => Close();
            Controls.Add(cancelBtn);

            AcceptButton = saveBtn;
            CancelButton = cancelBtn;
        }

        private static Button MakeButton(string text, int left, int top, int w, int h)
        {
            var btn = new Button
            {
                Text      = text,
                Left      = left, Top = top, Width = w, Height = h,
                BackColor = Color.FromArgb(55, 55, 55),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize  = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 75, 75);
            return btn;
        }
    }
}
