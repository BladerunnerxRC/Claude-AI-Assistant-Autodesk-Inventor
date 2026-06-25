using System.Drawing;
using System.Windows.Forms;

namespace ClaudeForInventor
{
    internal sealed class HelpDialog : Form
    {
        private static readonly Color BgBase   = Color.FromArgb(24, 24, 24);
        private static readonly Color Coral    = Color.FromArgb(204, 75, 55);
        private static readonly Color Gold     = Color.FromArgb(240, 190, 120);
        private static readonly Color Green    = Color.FromArgb(160, 210, 160);
        private static readonly Color Blue     = Color.FromArgb(140, 175, 215);
        private static readonly Color TextMain = Color.FromArgb(218, 218, 218);
        private static readonly Color TextDim  = Color.FromArgb(130, 130, 130);
        private static readonly Color CodeGreen= Color.FromArgb(180, 220, 170);

        public HelpDialog()
        {
            Text            = "Claude for Inventor — Help";
            Size            = new Size(620, 720);
            MinimumSize     = new Size(500, 500);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition   = FormStartPosition.CenterParent;
            ShowInTaskbar   = false;
            BackColor       = BgBase;
            ForeColor       = TextMain;

            var rtb = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = BgBase,
                ForeColor   = TextMain,
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                Font        = new Font("Segoe UI", 9.5f),
                Padding     = new Padding(12)
            };

            Controls.Add(rtb);
            PopulateHelp(rtb);
            rtb.SelectionStart = 0;
            rtb.ScrollToCaret();
        }

        private void PopulateHelp(RichTextBox r)
        {
            void H1(string t)
            {
                r.SelectionFont  = new Font("Segoe UI", 15f, FontStyle.Bold);
                r.SelectionColor = Coral;
                r.AppendText(t + "\n");
            }
            void H2(string t)
            {
                r.SelectionFont  = new Font("Segoe UI", 11f, FontStyle.Bold);
                r.SelectionColor = Gold;
                r.AppendText("\n" + t + "\n");
            }
            void H3(string t)
            {
                r.SelectionFont  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                r.SelectionColor = Green;
                r.AppendText(t + "\n");
            }
            void Body(string t)
            {
                r.SelectionFont  = new Font("Segoe UI", 9.5f);
                r.SelectionColor = TextMain;
                r.AppendText(t + "\n");
            }
            void Bullet(string t)
            {
                r.SelectionFont  = new Font("Segoe UI", 9.5f);
                r.SelectionColor = TextMain;
                r.SelectionIndent = 16;
                r.AppendText("•  " + t + "\n");
                r.SelectionIndent = 0;
            }
            void Code(string t)
            {
                r.SelectionFont  = new Font("Consolas", 9f);
                r.SelectionColor = CodeGreen;
                r.SelectionIndent = 16;
                r.AppendText(t + "\n");
                r.SelectionIndent = 0;
            }
            void Dim(string t)
            {
                r.SelectionFont  = new Font("Segoe UI", 8.5f, FontStyle.Italic);
                r.SelectionColor = TextDim;
                r.AppendText(t + "\n");
            }
            void NL() { r.AppendText("\n"); }

            // ─── Content ───────────────────────────────────────────────────

            H1("Claude AI for Autodesk Inventor");
            Dim("An AI design assistant embedded directly in the Inventor ribbon.");

            // ── Getting Started ──────────────────────────────────────────────
            H2("Getting Started");
            Body("1.  Build the add-in and copy the output files to the Inventor Addins folder.");
            Body("2.  Launch Inventor and open any Part, Assembly, or Drawing.");
            Body("3.  Go to  Tools → Claude AI → Claude AI  to open the panel.");
            Body("4.  Click  ⚙  in the toolbar, choose Settings, and paste your Anthropic API key.");
            Body("5.  Type a question and press  Enter  (or click Send).");
            NL();
            Dim("API keys are available at console.anthropic.com/keys and are stored locally in the Windows registry.");

            // ── The Chat Panel ───────────────────────────────────────────────
            H2("The Chat Panel");
            H3("Sending messages");
            Bullet("Enter — sends the current message.");
            Bullet("Shift + Enter — inserts a newline within the message.");
            Bullet("The Send button always works regardless of keyboard focus.");
            NL();
            H3("Attaching files for reference");
            Body("You can give Claude any external file as reference material three ways:");
            Bullet("Drag & drop — drag one or more files from Explorer and drop them anywhere on the panel.");
            Bullet("📎 button — click the paperclip below the Send button to browse and pick files.");
            Bullet("Right-click the input box and choose  📎 Attach File…");
            Body("Attached files appear as blue chips in the strip above the toolbar. Click any chip to remove it before sending.");
            Body("Text and code files are read and sent in full (up to 50 KB each). Binary files are referenced by name and size only.");
            Dim("Supported text types: .txt .cs .vb .js .ts .json .xml .yaml .md .csv .html .css .sql .py .bat .ps1 .ini .cfg .log .ilogic .xaml");
            NL();
            H3("Document context");
            Body("Every message automatically includes a snapshot of the active document:");
            Bullet("Part — user parameters, model parameters, features, active sketch, material, iProperties.");
            Bullet("Assembly — top-level occurrences, constraints, user parameters, iProperties.");
            Bullet("Drawing — sheet names, sizes, view counts.");
            NL();
            H3("Selection context");
            Body("Select entities in Inventor before sending a message and Claude will see them:");
            Bullet("Faces (plane / cylinder / cone / sphere / torus)");
            Bullet("Edges (line / arc / circle / ellipse)");
            Bullet("Features, occurrences, work geometry, sketch entities, parameters");
            Dim("Tip: select a specific face or feature to ask targeted questions about it.");

            // ── Toolbar Buttons ──────────────────────────────────────────────
            H2("Toolbar Buttons");
            H3("New Chat");
            Body("Clears the conversation history, attached files, and resets the panel to start a fresh session.");
            NL();
            H3("Copy Code  /  Copy iLogic  /  Copy VB");
            Body("Activates when Claude responds with a code block. Copies the first code block to the clipboard. The label reflects the detected language (iLogic, VB, C#, etc.).");
            NL();
            H3("▶ Run iLogic");
            Body("Activates for iLogic or VB code blocks. Writes the code into the active document as a rule named  Claude_Generated  and executes it immediately via the iLogic automation API. The rule is preserved in the document for inspection.");
            Dim("Requires iLogic to be installed. Part and Assembly documents only.");
            NL();
            H3("Export");
            Body("Saves the full conversation to a file. Opens a save dialog with two format options:");
            Bullet(".md  — Markdown with ## You / ## Claude sections and --- separators.");
            Bullet(".txt — Plain text with = / - dividers.");
            Dim("Default filename: Claude_Inventor_YYYYMMDD_HHMM");
            NL();
            H3("Apply Params");
            Body("Activates when Claude responds with a  ```params  block. Parses each  Name = expression  line and writes the values directly to the document's user parameters via the Inventor Parameters API.");
            Bullet("User parameters are updated first; falls back to model parameters if the name is not found in user parameters.");
            Bullet("Per-parameter success / failure is reported in the chat.");
            Dim("Part and Assembly documents only.");
            NL();
            H3("⚙ Settings menu");
            Body("Clicking the gear button opens a small menu:");
            Bullet("Settings… — enter or update your Anthropic API key.");
            Bullet("About — version and model information.");
            Bullet("Help — this window.");
            Bullet("💾 Save Conversation… — save the current chat to a named file for later.");
            Bullet("📂 Load Conversation… — reopen a previously saved conversation and continue it.");
            Dim("Conversations are saved as JSON files in your AppData\\Roaming\\ClaudeForInventor folder.");

            // ── iLogic Generation ────────────────────────────────────────────
            H2("iLogic Generation");
            Body("Claude is prompted to wrap all iLogic and VB.NET code in  ```ilogic  fences. Ask naturally:");
            Code("\"Write an iLogic rule that suppresses Fillet_R2 when Width < 50 mm\"");
            Code("\"Generate a rule to set Part Number from the file name\"");
            Body("When Claude responds, the  Copy iLogic  and  ▶ Run iLogic  buttons activate.");
            Bullet("Copy iLogic — paste into the Inventor iLogic editor manually.");
            Bullet("▶ Run iLogic — execute immediately without opening the editor.");
            NL();
            Body("To reference an existing rule, attach the  .ilogic  file before asking Claude to modify it:");
            Code("\"Update the attached rule to also suppress Chamfer_C1 when Depth < 20 mm\"");

            // ── Parameter Write-Back ─────────────────────────────────────────
            H2("Parameter Write-Back");
            Body("Ask Claude to suggest parameter values or optimise your design:");
            Code("\"Suggest values for Width, Height, and Thickness for a 2:1 aspect ratio\"");
            Code("\"Resize this part to fit a 100×50 mm envelope\"");
            Body("Claude will include a  ```params  block in its response:");
            Code("Width = 100 mm");
            Code("Height = 50 mm");
            Code("Thickness = 3 mm");
            Body("Click  Apply Params  to write these directly to the model. Inventor recalculates immediately.");

            // ── File Reference Examples ───────────────────────────────────────
            H2("File Reference Examples");
            Body("Attach any text or data file to give Claude extra context beyond the live document:");
            Bullet("Attach a  .csv  spec sheet: \"Does my current Width parameter meet this spec?\"");
            Bullet("Attach a  .json  config: \"Generate an iLogic rule that reads these values.\"");
            Bullet("Attach a  .txt  customer requirement: \"Summarise what design changes are needed.\"");
            Bullet("Attach multiple  .cs  or  .vb  files: \"Review these for consistency.\"");
            Dim("File contents are included in the message sent to Claude but are not stored in the conversation history beyond that turn.");

            // ── Tips ─────────────────────────────────────────────────────────
            H2("Tips & Tricks");
            Bullet("Name your parameters clearly — Claude references them by name from the live context.");
            Bullet("Select a specific feature or face before asking for advice to get targeted answers.");
            Bullet("Use New Chat between unrelated topics to keep context relevant.");
            Bullet("Export the chat as .md after a complex design session to keep a record.");
            Bullet("Save long sessions via  ⚙ → Save Conversation…  so you can pick up where you left off.");
            Bullet("If Run iLogic fails, check the error message in the chat — it will name the missing parameter or API call.");
            Bullet("The panel docks right by default but can be dragged to any side or floated.");
            NL();
        }
    }
}
