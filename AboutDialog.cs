using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ClaudeForInventor
{
    internal sealed class AboutDialog : Form
    {
        public AboutDialog()
        {
            Text            = "About Claude for Inventor";
            Size            = new Size(430, 310);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ShowInTaskbar   = false;
            BackColor       = Color.FromArgb(30, 30, 30);
            ForeColor       = Color.FromArgb(218, 218, 218);

            // Flower logo
            var logo = new PictureBox
            {
                Width    = 60, Height = 60,
                Left     = 20, Top    = 20,
                Image    = AppIcon.Make(60),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            var title = new Label
            {
                Text     = "Claude AI for Autodesk Inventor",
                Font     = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(218, 218, 218),
                AutoSize = true,
                Left     = 92, Top = 22
            };

            var version = new Label
            {
                Text      = $"v{ClaudeApiClient.Version}  ·  Model: {ClaudeApiClient.Model}  ·  Inventor 2027",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(110, 110, 110),
                AutoSize  = true,
                Left      = 92, Top = 48
            };

            var sep = new Label
            {
                Height    = 1,
                Left      = 20, Top = 96,
                Width     = 375,
                BackColor = Color.FromArgb(55, 55, 55)
            };

            var desc = new Label
            {
                Text = "An AI-powered design assistant embedded directly in Inventor.\n" +
                       "Provides context-aware chat, iLogic rule generation and execution,\n" +
                       "parameter write-back, and conversation export.",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(180, 180, 180),
                Left      = 20, Top = 108,
                Width     = 375,
                AutoSize  = true
            };

            var credit = new Label
            {
                Text      = "Powered by Anthropic · anthropic.com",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize  = true,
                Left      = 20, Top = 178
            };

            var openSource = new Label
            {
                Text      = "Open source code created by T.A.Smith2 (2026) and Claude Code.",
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(85, 85, 85),
                AutoSize  = true,
                Left      = 20, Top = 198
            };

            var githubLink = new LinkLabel
            {
                Text      = "github.com/BladerunnerxRC/Claude-AI-Assistant-Autodesk-Inventor",
                Font      = new Font("Segoe UI", 8f),
                Left      = 20, Top = 216,
                Width     = 375,
                AutoSize  = true,
                LinkColor = Color.FromArgb(100, 160, 220)
            };
            githubLink.LinkClicked += (_, __) =>
                Process.Start(new ProcessStartInfo("https://github.com/BladerunnerxRC/Claude-AI-Assistant-Autodesk-Inventor") { UseShellExecute = true });

            var closeBtn = new Button
            {
                Text         = "Close",
                Width        = 80, Height = 28,
                Left         = 325, Top = 245,
                BackColor    = Color.FromArgb(60, 60, 60),
                ForeColor    = Color.White,
                FlatStyle    = FlatStyle.Flat,
                DialogResult = DialogResult.OK,
                Cursor       = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderSize = 0;

            Controls.AddRange(new System.Windows.Forms.Control[]
                { logo, title, version, sep, desc, credit, openSource, githubLink, closeBtn });

            AcceptButton = closeBtn;
        }
    }
}
