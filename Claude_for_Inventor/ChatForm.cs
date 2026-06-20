using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ClaudeForInventor
{
    internal sealed class ChatForm : Form
    {
        // ── State ─────────────────────────────────────────────────────────────
        private readonly ClaudeApiClient         _api     = new ClaudeApiClient();
        private readonly List<ChatMessage>       _history = new List<ChatMessage>();
        private string  _documentContext = string.Empty;
        private string? _lastCode;
        private string  _lastCodeLanguage = "Code";
        private List<(string Name, string Expression)>? _lastParams;

        // ── Controls ──────────────────────────────────────────────────────────
        private readonly RichTextBox           _chatBox;
        private readonly TextBox               _inputBox;
        private readonly Button                _sendBtn;
        private readonly Button                _newChatBtn;
        private readonly Button                _copyCodeBtn;
        private readonly Button                _settingsBtn;
        private readonly Button                _runILogicBtn;
        private readonly Button                _exportBtn;
        private readonly Button                _applyParamsBtn;
        private readonly Label                 _contextLabel;
        private readonly ToolStripStatusLabel  _statusLabel;
        private readonly ToolStripStatusLabel  _modelLabel;

        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color BgBase      = Color.FromArgb(24, 24, 24);
        private static readonly Color BgPanel     = Color.FromArgb(34, 34, 34);
        private static readonly Color BgInput     = Color.FromArgb(42, 42, 42);
        private static readonly Color BgToolbar   = Color.FromArgb(28, 28, 28);
        private static readonly Color CodeBg      = Color.FromArgb(30, 32, 30);
        private static readonly Color Coral       = Color.FromArgb(204, 75, 55);
        private static readonly Color UserBlue    = Color.FromArgb(130, 190, 255);
        private static readonly Color TextMain    = Color.FromArgb(218, 218, 218);
        private static readonly Color TextDim     = Color.FromArgb(110, 110, 110);
        private static readonly Color ItalicText  = Color.FromArgb(185, 185, 185);
        private static readonly Color CodeGreen   = Color.FromArgb(180, 220, 170);
        private static readonly Color H1Color     = Color.FromArgb(240, 190, 120);  // warm gold
        private static readonly Color H2Color     = Color.FromArgb(160, 210, 160);  // soft green
        private static readonly Color H3Color     = Color.FromArgb(140, 175, 215);  // soft blue
        private static readonly Color BulletAccent= Color.FromArgb(130, 190, 255);
        private static readonly Color HrColor     = Color.FromArgb(65, 65, 65);

        // ── Fonts ─────────────────────────────────────────────────────────────
        private static readonly Font FontBody     = new Font("Segoe UI", 9.5f);
        private static readonly Font FontBodyBold = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        private static readonly Font FontBodyItal = new Font("Segoe UI", 9.5f, FontStyle.Italic);
        private static readonly Font FontLabel    = new Font("Segoe UI", 9f,   FontStyle.Bold);
        private static readonly Font FontH1       = new Font("Segoe UI", 13f,  FontStyle.Bold);
        private static readonly Font FontH2       = new Font("Segoe UI", 11f,  FontStyle.Bold);
        private static readonly Font FontH3       = new Font("Segoe UI", 10f,  FontStyle.Bold);
        private static readonly Font FontCode     = new Font("Consolas", 9f);
        private static readonly Font FontCodeBold = new Font("Consolas", 9f, FontStyle.Bold);
        private static readonly Font FontSystem   = new Font("Segoe UI", 8.5f, FontStyle.Italic);

        // ─────────────────────────────────────────────────────────────────────

        public ChatForm()
        {
            Text            = "Claude AI — Inventor Assistant";
            Size            = new Size(540, 720);
            MinimumSize     = new Size(300, 400);
            FormBorderStyle = FormBorderStyle.None;   // DockableWindow provides the frame
            BackColor       = BgBase;
            ForeColor       = TextMain;
            Font            = new Font("Segoe UI", 9f);
            ShowInTaskbar   = false;

            // ── Status strip ─────────────────────────────────────────────────
            var statusStrip = new StatusStrip { BackColor = Color.FromArgb(18, 18, 18), SizingGrip = false };
            _statusLabel = new ToolStripStatusLabel("Ready") { ForeColor = TextDim, Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _modelLabel  = new ToolStripStatusLabel(ClaudeApiClient.Model) { ForeColor = Color.FromArgb(80, 80, 80) };
            statusStrip.Items.Add(_statusLabel);
            statusStrip.Items.Add(_modelLabel);

            // ── Toolbar ──────────────────────────────────────────────────────
            var toolbar = new Panel { Height = 36, BackColor = BgToolbar, Padding = new Padding(6, 4, 6, 0) };
            _newChatBtn   = MakeToolBtn("New Chat",    0);
            _copyCodeBtn  = MakeToolBtn("Copy Code",  84);
            _settingsBtn  = MakeToolBtn("⚙ Settings", 168);
            _runILogicBtn = MakeToolBtn("▶ Run iLogic", 256);
            _runILogicBtn.Width = 90;
            _exportBtn      = MakeToolBtn("Export",       350);
            _exportBtn.Width = 72;
            _applyParamsBtn = MakeToolBtn("Apply Params", 426);
            _applyParamsBtn.Width = 92;
            _copyCodeBtn   .Enabled = false;
            _runILogicBtn  .Enabled = false;
            _exportBtn     .Enabled = false;
            _applyParamsBtn.Enabled = false;
            _newChatBtn    .Click += (_, __) => ClearChat();
            _copyCodeBtn   .Click += OnCopyCode;
            var settingsMenu = new ContextMenuStrip { BackColor = Color.FromArgb(48, 48, 48), ForeColor = TextMain, ShowImageMargin = false };
            settingsMenu.Items.Add("⚙  Settings…", null, (_, __) => { using var d = new SettingsDialog(); d.ShowDialog(this); });
            settingsMenu.Items.Add(new ToolStripSeparator());
            settingsMenu.Items.Add("ℹ  About",      null, (_, __) => { using var d = new AboutDialog();  d.ShowDialog(this); });
            settingsMenu.Items.Add("?  Help",        null, (_, __) => { using var d = new HelpDialog();   d.ShowDialog(this); });
            _settingsBtn.Click += (_, __) => settingsMenu.Show(_settingsBtn, new Point(0, _settingsBtn.Height));
            _runILogicBtn  .Click += (_, __) => RunILogicRule();
            _exportBtn     .Click += (_, __) => ExportChat();
            _applyParamsBtn.Click += (_, __) => ApplyParameters();
            toolbar.Controls.AddRange(new Control[] { _newChatBtn, _copyCodeBtn, _settingsBtn, _runILogicBtn, _exportBtn, _applyParamsBtn });

            // ── Input panel ───────────────────────────────────────────────────
            var inputPanel = new Panel { Height = 90, BackColor = BgPanel, Padding = new Padding(8) };
            _inputBox = new TextBox
            {
                Multiline   = true,
                BackColor   = BgInput,
                ForeColor   = TextMain,
                BorderStyle = BorderStyle.None,
                ScrollBars  = ScrollBars.Vertical,
                Font        = new Font("Segoe UI", 9.5f),
                Dock        = DockStyle.Fill,
                AcceptsReturn = true        // Shift+Enter = newline; Enter = send (handled below)
            };
            _inputBox.KeyDown += OnInputKeyDown;

            _sendBtn = new Button
            {
                Text      = "Send",
                Width     = 64,
                Dock      = DockStyle.Right,
                BackColor = Coral,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            _sendBtn.FlatAppearance.BorderSize = 0;
            _sendBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 95, 70);
            _sendBtn.Click += (_, __) => SendMessage();
            inputPanel.Controls.Add(_inputBox);
            inputPanel.Controls.Add(_sendBtn);

            // ── Context panel ─────────────────────────────────────────────────
            var contextPanel = new Panel { Height = 40, BackColor = Color.FromArgb(30, 38, 48), Padding = new Padding(10, 6, 10, 6) };
            _contextLabel = new Label
            {
                Dock      = DockStyle.Fill,
                ForeColor = Color.FromArgb(130, 180, 230),
                Font      = new Font("Segoe UI", 8.5f),
                Text      = "📐 No document open",
                AutoEllipsis = true
            };
            contextPanel.Controls.Add(_contextLabel);

            // ── Chat display ──────────────────────────────────────────────────
            _chatBox = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = BgBase,
                ForeColor   = TextMain,
                Font        = new Font("Segoe UI", 9.5f),
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                WordWrap    = true,
                Padding     = new Padding(8)
            };

            // ── Dock order (bottom-to-top): strip → toolbar → input → context → fill ──
            statusStrip.Dock  = DockStyle.Bottom;
            toolbar.Dock      = DockStyle.Bottom;
            inputPanel.Dock   = DockStyle.Bottom;
            contextPanel.Dock = DockStyle.Top;
            _chatBox.Dock     = DockStyle.Fill;

            Controls.Add(_chatBox);
            Controls.Add(contextPanel);
            Controls.Add(inputPanel);
            Controls.Add(toolbar);
            Controls.Add(statusStrip);

            AppendSystem("Claude Opus 4.8 connected.\n" +
                         "Ask anything about your Inventor design. " +
                         "I can generate iLogic rules, explain features, audit parameters, and more.\n" +
                         "Press Enter to send · Shift+Enter for a newline.");

            Load       += (_, __) => InstallKeyboardHook();
            FormClosed += (_, __) => UninstallKeyboardHook();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public
        // ─────────────────────────────────────────────────────────────────────

        public void RefreshContext()
        {
            _documentContext   = InventorContextBuilder.Build(StandardAddInServer.InventorApp);
            _contextLabel.Text = "📐 " + InventorContextBuilder.SummaryLine(StandardAddInServer.InventorApp);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Messaging
        // ─────────────────────────────────────────────────────────────────────

        private void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }

        private async void SendMessage()
        {
            var text = _inputBox.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            _inputBox.Clear();
            AppendUser(text);
            SetBusy(true);

            try
            {
                RefreshContext();

                // Append header and snapshot RTF so we can replace raw stream with formatted text
                AppendRun("\nClaude\n", Coral, FontLabel);
                var rtfSnapshot = _chatBox.Rtf;
                bool firstChunk = true;

                var reply = await _api.SendAsync(text, _history, _documentContext, chunk =>
                {
                    void Apply()
                    {
                        if (firstChunk)
                        {
                            firstChunk = false;
                            _statusLabel.Text = "Claude is responding…";
                        }
                        AppendRun(chunk, TextMain, FontBody);
                        ScrollToEnd();
                    }
                    if (_chatBox.InvokeRequired) _chatBox.Invoke((Action)Apply);
                    else Apply();
                });

                _history.Add(new ChatMessage("user",      text));
                _history.Add(new ChatMessage("assistant", reply));
                _exportBtn.Enabled = true;

                // Swap raw stream text for formatted markdown
                _chatBox.Rtf = rtfSnapshot;
                RenderMarkdown(reply);
                AppendRun("\n", TextMain, FontBody);
                ScrollToEnd();

                TryExtractCode(reply);
                TryExtractParams(reply);
            }
            catch (Exception ex)
            {
                AppendSystem($"⚠ Error: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ClearChat()
        {
            _history.Clear();
            _chatBox.Clear();
            _lastCode            = null;
            _lastCodeLanguage    = "Code";
            _copyCodeBtn .Enabled   = false;
            _copyCodeBtn .Text      = "Copy Code";
            _copyCodeBtn .BackColor = Color.FromArgb(55, 55, 55);
            _runILogicBtn.Enabled   = false;
            _runILogicBtn.BackColor = Color.FromArgb(55, 55, 55);
            _exportBtn     .Enabled   = false;
            _applyParamsBtn.Enabled   = false;
            _applyParamsBtn.BackColor = Color.FromArgb(55, 55, 55);
            _lastParams               = null;
            AppendSystem("Chat cleared — new conversation started.");
        }

        private void OnCopyCode(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastCode))
            {
                Clipboard.SetText(_lastCode);
                _copyCodeBtn.Text = "✓ Copied!";
                var t = new Timer { Interval = 1800 };
                t.Tick += (_, __) => { _copyCodeBtn.Text = $"Copy {_lastCodeLanguage}"; t.Stop(); t.Dispose(); };
                t.Start();
            }
        }

        private void RunILogicRule()
        {
            if (string.IsNullOrEmpty(_lastCode)) return;

            var app = StandardAddInServer.InventorApp;
            if (app?.ActiveDocument == null)
            {
                AppendSystem("⚠ No document open — cannot run iLogic rule.");
                return;
            }

            const string ILogicAddinGuid = "{3BDD8D79-2179-4B11-8A5A-257B1C0263AC}";
            const string RuleName        = "Claude_Generated";

            _runILogicBtn.Enabled = false;
            try
            {
                var addin = app.ApplicationAddIns.ItemById[ILogicAddinGuid];
                if (addin == null)
                {
                    AppendSystem("⚠ iLogic add-in not found. Ensure iLogic is installed.");
                    return;
                }

                dynamic iLogic = addin.Automation;
                var doc = app.ActiveDocument;

                // Create or update the rule
                string? existing = null;
                try { existing = iLogic.GetRuleText(doc, RuleName); } catch { }

                if (string.IsNullOrEmpty(existing))
                    iLogic.AddRule(doc, RuleName, _lastCode);
                else
                    iLogic.SetRuleText(doc, RuleName, _lastCode);

                iLogic.RunRule(doc, RuleName);
                AppendSystem($"✓ iLogic rule \"{RuleName}\" ran successfully.");
            }
            catch (Exception ex)
            {
                AppendSystem($"⚠ iLogic run failed: {ex.Message}");
            }
            finally
            {
                _runILogicBtn.Enabled = true;
            }
        }

        private void ExportChat()
        {
            if (_history.Count == 0) return;

            using var dlg = new SaveFileDialog
            {
                Title            = "Export chat",
                Filter           = "Markdown (*.md)|*.md|Plain text (*.txt)|*.txt",
                FilterIndex      = 1,
                DefaultExt       = "md",
                FileName         = $"Claude_Inventor_{DateTime.Now:yyyyMMdd_HHmm}"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            bool isMarkdown = string.Equals(
                System.IO.Path.GetExtension(dlg.FileName), ".md",
                StringComparison.OrdinalIgnoreCase);

            try
            {
                var sb = new StringBuilder();
                var doc = InventorContextBuilder.SummaryLine(StandardAddInServer.InventorApp);

                if (isMarkdown)
                {
                    sb.AppendLine($"# Claude for Inventor — Chat Export");
                    sb.AppendLine($"*Document: {doc}*  ");
                    sb.AppendLine($"*Exported: {DateTime.Now:yyyy-MM-dd HH:mm}*");
                    sb.AppendLine();
                    foreach (var msg in _history)
                    {
                        sb.AppendLine("---");
                        sb.AppendLine();
                        sb.AppendLine(msg.Role == "user" ? "## You" : "## Claude");
                        sb.AppendLine();
                        sb.AppendLine(msg.Content);
                        sb.AppendLine();
                    }
                }
                else
                {
                    var hr = new string('=', 72);
                    sb.AppendLine("Claude for Inventor — Chat Export");
                    sb.AppendLine($"Document : {doc}");
                    sb.AppendLine($"Exported : {DateTime.Now:yyyy-MM-dd HH:mm}");
                    sb.AppendLine(hr);
                    foreach (var msg in _history)
                    {
                        sb.AppendLine();
                        sb.AppendLine(msg.Role == "user" ? "You:" : "Claude:");
                        sb.AppendLine();
                        sb.AppendLine(msg.Content);
                        sb.AppendLine(new string('-', 72));
                    }
                }

                System.IO.File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                AppendSystem($"✓ Chat exported to {System.IO.Path.GetFileName(dlg.FileName)}");
            }
            catch (Exception ex)
            {
                AppendSystem($"⚠ Export failed: {ex.Message}");
            }
        }

        private void TryExtractParams(string response)
        {
            _lastParams             = null;
            _applyParamsBtn.Enabled = false;
            _applyParamsBtn.BackColor = Color.FromArgb(55, 55, 55);

            foreach (var fence in new[] { "```params", "```parameters" })
            {
                int fencePos = response.IndexOf(fence, StringComparison.OrdinalIgnoreCase);
                if (fencePos < 0) continue;

                int lineEnd = response.IndexOf('\n', fencePos);
                if (lineEnd < 0) continue;

                int blockEnd = response.IndexOf("```", lineEnd + 1, StringComparison.Ordinal);
                if (blockEnd < 0) continue;

                var block  = response.Substring(lineEnd + 1, blockEnd - lineEnd - 1);
                var parsed = ParseParamBlock(block);
                if (parsed.Count == 0) continue;

                _lastParams             = parsed;
                _applyParamsBtn.Enabled = true;
                _applyParamsBtn.BackColor = Color.FromArgb(70, 50, 110);
                break;
            }
        }

        private static List<(string Name, string Expression)> ParseParamBlock(string block)
        {
            var result = new List<(string, string)>();
            foreach (var rawLine in block.Split('\n'))
            {
                var line = rawLine.Trim('\r', ' ', '\t');
                if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.StartsWith("#"))
                    continue;

                int eq = line.IndexOf('=');
                if (eq <= 0) continue;

                var name = line.Substring(0, eq).Trim();
                var expr = line.Substring(eq + 1).Trim();
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(expr))
                    result.Add((name, expr));
            }
            return result;
        }

        private void ApplyParameters()
        {
            if (_lastParams == null || _lastParams.Count == 0) return;

            var app = StandardAddInServer.InventorApp;
            if (app?.ActiveDocument == null)
            {
                AppendSystem("⚠ No document open — cannot apply parameters.");
                return;
            }

            var doc = app.ActiveDocument;
            Inventor.Parameters? parameters = null;
            try
            {
                parameters = doc.DocumentType switch
                {
                    Inventor.DocumentTypeEnum.kPartDocumentObject     => ((Inventor.PartDocument)doc)    .ComponentDefinition.Parameters,
                    Inventor.DocumentTypeEnum.kAssemblyDocumentObject => ((Inventor.AssemblyDocument)doc).ComponentDefinition.Parameters,
                    _ => null
                };
            }
            catch (Exception ex)
            {
                AppendSystem($"⚠ Cannot access parameters: {ex.Message}");
                return;
            }

            if (parameters == null)
            {
                AppendSystem("⚠ Parameter write-back is only supported for Part and Assembly documents.");
                return;
            }

            int ok = 0, fail = 0;
            var sb = new StringBuilder();
            foreach (var (name, expr) in _lastParams)
            {
                try
                {
                    bool found = false;

                    // User parameters first — safest to modify
                    foreach (Inventor.UserParameter up in parameters.UserParameters)
                    {
                        if (!string.Equals(up.Name, name, StringComparison.OrdinalIgnoreCase)) continue;
                        up.Expression = expr;
                        found = true;
                        break;
                    }

                    // Fallback: any parameter (model params, reference params, etc.)
                    if (!found)
                    {
                        foreach (Inventor.Parameter p in parameters)
                        {
                            if (!string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) continue;
                            p.Expression = expr;
                            found = true;
                            break;
                        }
                    }

                    if (!found) throw new KeyNotFoundException($"Parameter \"{name}\" not found.");
                    sb.AppendLine($"  ✓  {name} = {expr}");
                    ok++;
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  ✗  {name}: {ex.Message}");
                    fail++;
                }
            }

            AppendSystem($"Applied {ok} of {ok + fail} parameter(s):\n{sb.ToString().TrimEnd()}");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Rendering
        // ─────────────────────────────────────────────────────────────────────

        private void AppendUser(string text)
        {
            AppendRun("\nYou\n", UserBlue, FontLabel);
            AppendRun(text + "\n", TextMain, FontBody);
            ScrollToEnd();
        }

        private void AppendAssistant(string response)
        {
            AppendRun("\nClaude\n", Coral, FontLabel);
            RenderMarkdown(response);
            AppendRun("\n", TextMain, FontBody);
            ScrollToEnd();
        }

        private void AppendSystem(string text)
        {
            AppendRun(text + "\n", TextDim, FontSystem);
            ScrollToEnd();
        }

        private void AppendRun(string text, Color color, Font font, Color? bgColor = null)
        {
            _chatBox.SelectionStart     = _chatBox.TextLength;
            _chatBox.SelectionFont      = font;
            _chatBox.SelectionColor     = color;
            _chatBox.SelectionBackColor = bgColor ?? BgBase;
            _chatBox.AppendText(text);
        }

        private void ScrollToEnd()
        {
            _chatBox.SelectionStart = _chatBox.TextLength;
            _chatBox.ScrollToCaret();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Markdown rendering
        // ─────────────────────────────────────────────────────────────────────

        private void RenderMarkdown(string text)
        {
            var lines      = text.Split('\n');
            bool inFence   = false;
            bool inTable   = false;
            var fenceLines = new List<string>();
            var tableRows  = new List<string>();

            foreach (var rawLine in lines)
            {
                var line = rawLine.TrimEnd('\r');

                // ── Code fences ──────────────────────────────────────────────
                if (!inFence && line.TrimStart().StartsWith("```"))
                {
                    if (inTable) { RenderTable(tableRows); tableRows.Clear(); inTable = false; }
                    inFence = true;
                    fenceLines.Clear();
                    continue;
                }
                if (inFence)
                {
                    if (line.TrimStart().StartsWith("```")) { RenderCodeBlock(fenceLines); inFence = false; }
                    else fenceLines.Add(line);
                    continue;
                }

                // ── Tables ───────────────────────────────────────────────────
                if (line.TrimStart().StartsWith("|"))
                {
                    inTable = true;
                    tableRows.Add(line);
                    continue;
                }
                if (inTable) { RenderTable(tableRows); tableRows.Clear(); inTable = false; }

                RenderTextLine(line);
            }

            if (inFence && fenceLines.Count > 0) RenderCodeBlock(fenceLines);
            if (inTable && tableRows .Count > 0) RenderTable(tableRows);
        }

        private void RenderTextLine(string line)
        {
            if (line.StartsWith("# "))
            {
                AppendRun(line.Substring(2).Trim() + "\n", H1Color, FontH1);
                return;
            }
            if (line.StartsWith("## "))
            {
                AppendRun(line.Substring(3).Trim() + "\n", H2Color, FontH2);
                return;
            }
            if (line.StartsWith("### "))
            {
                AppendRun(line.Substring(4).Trim() + "\n", H3Color, FontH3);
                return;
            }
            if (line == "---" || line == "___")
            {
                AppendRun("────────────────────────────────────\n", HrColor, FontBody);
                return;
            }
            if (line.StartsWith("- ") || line.StartsWith("* "))
            {
                AppendRun("  • ", BulletAccent, FontBodyBold);
                RenderInline(line.Substring(2));
                AppendRun("\n", TextMain, FontBody);
                return;
            }
            if (IsNumberedListItem(line, out var num, out var rest))
            {
                AppendRun($"  {num} ", BulletAccent, FontBodyBold);
                RenderInline(rest);
                AppendRun("\n", TextMain, FontBody);
                return;
            }
            if (string.IsNullOrWhiteSpace(line))
            {
                AppendRun("\n", TextMain, FontBody);
                return;
            }
            RenderInline(line);
            AppendRun("\n", TextMain, FontBody);
        }

        private void RenderCodeBlock(List<string> lines)
        {
            AppendRun(string.Join("\n", lines) + "\n", CodeGreen, FontCode, CodeBg);
            _chatBox.SelectionStart     = _chatBox.TextLength;
            _chatBox.SelectionBackColor = BgBase;
        }

        private void RenderTable(List<string> rows)
        {
            // Parse rows, skipping the separator line and detecting the header
            var parsed     = new List<string[]>();
            bool hasHeader = false;

            foreach (var raw in rows)
            {
                var row = raw.Trim();
                if (string.IsNullOrEmpty(row)) continue;

                if (IsTableSeparator(row)) { hasHeader = parsed.Count > 0; continue; }

                var parts = row.Split('|');
                var cells = new List<string>();
                for (int i = 0; i < parts.Length; i++)
                {
                    if (i == 0 && parts[i].Trim().Length == 0) continue;
                    if (i == parts.Length - 1 && parts[i].Trim().Length == 0) continue;
                    cells.Add(parts[i].Trim());
                }
                if (cells.Count > 0) parsed.Add(cells.ToArray());
            }
            if (parsed.Count == 0) return;

            // Column count and minimum widths (content + 2 spaces padding)
            int cols = 0;
            foreach (var r in parsed) cols = Math.Max(cols, r.Length);
            var widths = new int[cols];
            foreach (var r in parsed)
                for (int c = 0; c < r.Length; c++)
                    widths[c] = Math.Max(widths[c], r[c].Length + 2);

            // Border strings using box-drawing characters
            var segs   = new string[cols];
            for (int c = 0; c < cols; c++) segs[c] = new string('─', widths[c]);
            string top    = "┌" + string.Join("┬", segs) + "┐";
            string mid    = "├" + string.Join("┼", segs) + "┤";
            string bottom = "└" + string.Join("┴", segs) + "┘";

            var tableBg = Color.FromArgb(26, 28, 36);
            AppendRun(top + "\n", TextDim, FontCode, tableBg);

            for (int r = 0; r < parsed.Count; r++)
            {
                bool isHeader = hasHeader && r == 0;
                var sb = new StringBuilder("│");
                for (int c = 0; c < cols; c++)
                {
                    var cell = c < parsed[r].Length ? parsed[r][c] : "";
                    sb.Append(' ').Append(cell.PadRight(widths[c] - 1)).Append('│');
                }
                var color = isHeader ? H2Color : TextMain;
                var font  = isHeader ? FontCodeBold : FontCode;
                AppendRun(sb.ToString() + "\n", color, font, tableBg);
                if (isHeader) AppendRun(mid + "\n", TextDim, FontCode, tableBg);
            }

            AppendRun(bottom + "\n", TextDim, FontCode, tableBg);
            _chatBox.SelectionStart     = _chatBox.TextLength;
            _chatBox.SelectionBackColor = BgBase;
        }

        private static bool IsTableSeparator(string line)
        {
            bool hasDash = false;
            foreach (char c in line)
            {
                if (c == '-') { hasDash = true; continue; }
                if (c == '|' || c == ':' || c == ' ') continue;
                return false;
            }
            return hasDash;
        }

        private void RenderInline(string text)
        {
            int pos = 0;
            while (pos < text.Length)
            {
                // Bold: **...**
                if (pos + 1 < text.Length && text[pos] == '*' && text[pos + 1] == '*')
                {
                    int end = text.IndexOf("**", pos + 2, StringComparison.Ordinal);
                    if (end >= 0)
                    {
                        AppendRun(text.Substring(pos + 2, end - pos - 2), TextMain, FontBodyBold);
                        pos = end + 2;
                        continue;
                    }
                }
                // Italic: *...*
                if (text[pos] == '*')
                {
                    int end = text.IndexOf('*', pos + 1);
                    if (end >= 0)
                    {
                        AppendRun(text.Substring(pos + 1, end - pos - 1), ItalicText, FontBodyItal);
                        pos = end + 1;
                        continue;
                    }
                }
                // Inline code: `...`
                if (text[pos] == '`')
                {
                    int end = text.IndexOf('`', pos + 1);
                    if (end >= 0)
                    {
                        AppendRun(text.Substring(pos + 1, end - pos - 1), CodeGreen, FontCode);
                        pos = end + 1;
                        continue;
                    }
                }
                // Plain text — run until next special char
                int next = text.Length;
                for (int i = pos + 1; i < text.Length; i++)
                {
                    if (text[i] == '*' || text[i] == '`') { next = i; break; }
                }
                AppendRun(text.Substring(pos, next - pos), TextMain, FontBody);
                pos = next;
            }
        }

        private static bool IsNumberedListItem(string line, out string number, out string content)
        {
            int i = 0;
            while (i < line.Length && char.IsDigit(line[i])) i++;
            if (i > 0 && i + 1 < line.Length && line[i] == '.' && line[i + 1] == ' ')
            {
                number  = line.Substring(0, i + 1);
                content = line.Substring(i + 2);
                return true;
            }
            number = content = string.Empty;
            return false;
        }

        /// <summary>
        /// Extracts the first code block and its language tag for the copy button.
        /// </summary>
        private void TryExtractCode(string response)
        {
            int fence = response.IndexOf("```", StringComparison.Ordinal);
            if (fence < 0) return;

            // Language tag is text between ``` and first newline
            int tagEnd  = response.IndexOf('\n', fence + 3);
            var langTag = tagEnd >= 0
                ? response.Substring(fence + 3, tagEnd - fence - 3).Trim().ToLowerInvariant()
                : string.Empty;

            int codeStart = tagEnd >= 0 ? tagEnd + 1 : fence + 3;
            int codeEnd   = response.IndexOf("```", codeStart, StringComparison.Ordinal);
            if (codeEnd < 0) return;

            _lastCode = response.Substring(codeStart, codeEnd - codeStart).TrimEnd();

            _lastCodeLanguage = langTag switch
            {
                "ilogic"      => "iLogic",
                "vb" or "vbnet" or "visualbasic" => "VB",
                "csharp" or "cs" => "C#",
                ""            => "Code",
                _             => langTag.Length > 8 ? "Code" : langTag.ToUpperInvariant()
            };

            _copyCodeBtn.Enabled   = true;
            _copyCodeBtn.Text      = $"Copy {_lastCodeLanguage}";
            _copyCodeBtn.BackColor = Color.FromArgb(45, 100, 55);

            bool isILogic = langTag is "ilogic" or "vb" or "vbnet" or "visualbasic";
            _runILogicBtn.Enabled   = isILogic;
            _runILogicBtn.BackColor = isILogic ? Color.FromArgb(40, 90, 35) : Color.FromArgb(55, 55, 55);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Keyboard hook – Inventor owns the message pump so ProcessCmdKey never
        // fires. WH_KEYBOARD_LL intercepts Space/Delete before Inventor's
        // accelerator table can eat them.
        // ─────────────────────────────────────────────────────────────────────

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN     = 0x0100;
        private const int VK_RETURN      = 0x0D;
        private const int VK_SHIFT       = 0x10;
        private const int VK_SPACE       = 0x20;
        private const int VK_DELETE      = 0x2E;

        [DllImport("user32.dll")] private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] private static extern bool   UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] private static extern short  GetKeyState(int nVirtKey);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc? _hookProc;   // field ref prevents GC
        private IntPtr _hookHandle = IntPtr.Zero;

        private void InstallKeyboardHook()
        {
            _hookProc   = OnKeyboardHook;
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, IntPtr.Zero, 0);
        }

        private void UninstallKeyboardHook()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
        }

        private IntPtr OnKeyboardHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (int)wParam == WM_KEYDOWN && _inputBox.Focused)
            {
                int vk = Marshal.ReadInt32(lParam);
                if (vk == VK_RETURN)
                {
                    bool shiftDown = (GetKeyState(VK_SHIFT) & 0x8000) != 0;
                    if (!shiftDown)
                    {
                        _inputBox.BeginInvoke((Action)SendMessage);
                        return (IntPtr)1;   // consume — prevent newline insertion
                    }
                    // Shift+Enter: fall through so TextBox inserts the newline
                }
                if (vk == VK_SPACE)
                {
                    _inputBox.SelectedText = " ";
                    return (IntPtr)1;
                }
                if (vk == VK_DELETE)
                {
                    if (_inputBox.SelectionLength > 0)
                        _inputBox.SelectedText = string.Empty;
                    else if (_inputBox.SelectionStart < _inputBox.Text.Length)
                    {
                        int pos = _inputBox.SelectionStart;
                        _inputBox.Text           = _inputBox.Text.Remove(pos, 1);
                        _inputBox.SelectionStart = pos;
                    }
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        private void SetBusy(bool busy)
        {
            _sendBtn.Enabled  = !busy;
            _inputBox.Enabled = !busy;
            _statusLabel.Text = busy ? "Claude is thinking…" : "Ready";
            Cursor            = busy ? Cursors.WaitCursor : Cursors.Default;
        }

        private Button MakeToolBtn(string text, int left)
        {
            var btn = new Button
            {
                Text      = text,
                Left      = left, Top = 3, Width = 80, Height = 28,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8f),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(70, 70, 70);
            return btn;
        }
    }
}
