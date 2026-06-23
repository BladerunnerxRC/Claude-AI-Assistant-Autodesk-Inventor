<div align="center">

# 🤖 Claude AI for Autodesk Inventor

[![Inventor](https://img.shields.io/badge/Autodesk_Inventor-2027-F37021?style=for-the-badge&logo=autodesk&logoColor=white)](https://www.autodesk.com/products/inventor)
[![Claude](https://img.shields.io/badge/Claude_Opus_4.8-Anthropic-7C3AED?style=for-the-badge)](https://anthropic.com)
[![.NET Framework](https://img.shields.io/badge/.NET_Framework-4.8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![Windows](https://img.shields.io/badge/Windows-11-0078D6?style=for-the-badge&logo=windows&logoColor=white)](https://microsoft.com)

Embeds **Claude Opus 4.8** directly into the Autodesk Inventor ribbon.  
Open a chat panel from **Tools → Claude AI** in any Part, Assembly, or Drawing environment.

</div>

---

## ✨ Features

### 💬 Chat & Context

- 🟢 Live document context injected into every prompt — user parameters, model features, assembly occurrences, constraints, and iProperties
- 🟢 **Auto-refresh** — context bar updates automatically when switching between tabbed designs, no button click needed
- 🟢 Conversation history maintained across turns in the same session
- 🟢 Active sketch detection — reports sketch name, line count, circle count, **arc count, ellipse count, spline count, and total geometric constraint count**
- 🟢 Material reported for Part documents
- 🟢 **Selection context** — selected entities (faces, edges, features, occurrences, sketch geometry, work geometry, parameters) are appended to context automatically
- 🟢 **Assembly BOM** — recursive Bill of Materials (part number, quantity, description) included in context when an assembly is active
- 🟢 **Sub-assembly annotations** — each occurrence in an assembly is annotated with `[sub-asm: N components]` or `[Material]` so Claude understands the structure and materials without being told

### 🖥️ Responses

- 🔵 Rich markdown rendering: `# H1` / `## H2` / `### H3` headings, **bold**, *italic*, `inline code`, fenced code blocks with dark background, bullet and numbered lists, horizontal rules
- 🔵 iLogic / VB.NET code blocks highlighted in green monospace; **Copy iLogic** button activates automatically
- 🔵 C# and generic code blocks also detected and copyable

### 🎨 UI

- 🟣 Docked panel integrated into Inventor's UI via `DockableWindow` — docks right by default, floatable and re-dockable like any native Inventor panel
- 🟣 **Shift+Enter / Ctrl+Enter** insert a newline in the input box; **Enter** sends the message
- 🟣 **Right-click context menu** in the input box: Cut, Copy, Paste, Select All
- 🟣 **Right-click context menu** in the chat panel: Copy, Select All — paste selected text into any app or Inventor field
- 🟣 Space and Delete keys work correctly inside the input box (Inventor keyboard interception bypassed via low-level hook)
- 🟣 Toolbar buttons: **New Chat**, **Copy Code**, **⚙ Settings**, **▶ Run iLogic**, **Export**, **Apply Params**
- 🟣 Status bar shows model name and current state (Ready / Claude is thinking…)

### ▶ iLogic Runner

- 🟡 **▶ Run iLogic** button activates when Claude responds with an `ilogic` or `vb` code block
- 🟡 Writes the code as a rule named `Claude_Generated` in the active document and executes it immediately via the iLogic automation API
- 🟡 **Auto-retry on error** — if the iLogic rule fails, the error is automatically sent back to Claude which generates a corrected rule and re-runs it, without any manual copy/paste
- 🟡 The rule is preserved in the document so it can be inspected or edited in the iLogic editor

### 📤 Conversation Export

- 🟠 **Export** button (enabled after the first reply) saves the full chat as `.md` or `.txt`
- 🟠 Markdown export uses `## You` / `## Claude` sections with `---` separators
- 🟠 Default filename: `Claude_Inventor_YYYYMMDD_HHMM`

### 💾 Named Conversation Threads

- 🔵 **Save Conversation** (Settings menu) — saves the full chat history to a named JSON file in `%APPDATA%\ClaudeForInventor\conversations\`
- 🔵 **Load Conversation** (Settings menu) — restores any previously saved session so you can continue exactly where you left off
- 🔵 Saved conversations are sorted newest-first and display timestamp and name

### 🔧 Parameter Write-Back

- 🔵 When Claude suggests parameter changes, it outputs them in a `` ```params `` block
- 🔵 **Apply Params** button parses the block and writes each `Name = expression` value directly to the active document's `UserParameters` via the Inventor Parameters API
- 🔵 **Before → after diff** displayed in chat for each changed parameter: `✓ Width: 50 mm → 80 mm`
- 🔵 **Feature suppression** — include `FeatureName.Suppressed = True` or `False` in a params block to suppress or unsuppress any Inventor feature (fillets, chamfers, holes, etc.) without touching the feature tree manually
- 🔵 Per-parameter success/failure reported in the chat

### 🔒 Security

- 🟢 API key stored securely in the Windows registry (`HKCU\SOFTWARE\ClaudeForInventor`)

---

## 📋 Prerequisites

| Requirement | Version |
|---|---|
| 🟠 Autodesk Inventor | 2027 (adjust `<InventorBin>` path for other versions) |
| 🔵 .NET SDK | 9.0+ (`dotnet build` CLI) or Visual Studio 2022 |
| 🟣 .NET Framework | 4.8 (target runtime — ships with Windows) |
| 🟡 Anthropic API key | [console.anthropic.com/keys](https://console.anthropic.com/keys) |

---

## 🔨 Build

### 1. Set your Inventor path

Edit `ClaudeForInventor.csproj` and update `<InventorBin>` to match your installation:

```xml
<InventorBin>C:\Program Files\Autodesk\Inventor 2027\Bin\</InventorBin>
```

This path resolves both `Autodesk.Inventor.Interop.dll` and `stdole.dll`.

### 2. Build

Open an **elevated (Administrator) PowerShell** terminal in the project folder:

```powershell
dotnet build -c Release
```

Running as Administrator lets the post-build `regasm.exe` step write the COM registration directly.  
All output files (`ClaudeForInventor.dll`, `.addin`, `Newtonsoft.Json.dll`) are placed in `bin\Release\net48\`.

### 3. Register COM (if regasm failed or you built without elevation)

The post-build step may print a warning that `Autodesk.Inventor.Interop` could not be found — this means regasm ran without Inventor's Bin on the search path. Fix it manually:

```powershell
# Administrator PowerShell
$out = "C:\<full-path-to-project>\bin\Release\net48"

# Copy interop DLL alongside the add-in so regasm can resolve it
Copy-Item "C:\Program Files\Autodesk\Inventor 2027\Bin\Autodesk.Inventor.Interop.dll" $out

& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" `
   "$out\ClaudeForInventor.dll" /codebase /nologo

Remove-Item "$out\Autodesk.Inventor.Interop.dll"
```

> [!NOTE]
> The `/codebase` flag is required because the assembly is not in the GAC.  
> The unsigned-assembly warning it produces can be ignored for local development.

### 4. Updating the DLL (subsequent builds)

For code-only changes, replace `ClaudeForInventor.dll` in the Addins folder and restart Inventor.  
Re-running `regasm` is only needed if the DLL moves to a different path.

---

## 📦 Manual Install

Copy all three files from `bin\Release\net48\` to Inventor's add-ins folder:

```text
%APPDATA%\Autodesk\Inventor 2027\Addins\
    ClaudeForInventor.dll
    ClaudeForInventor.addin
    Newtonsoft.Json.dll
```

> [!TIP]
> For a machine-wide install, use `C:\ProgramData\Autodesk\Inventor 2027\Addins\` instead.

---

## 🚀 First Run

1. Launch Inventor.
2. Open any Part, Assembly, or Drawing.
3. Go to **Tools → Claude AI → Claude AI**.
4. Click **⚙ Settings** and paste your Anthropic API key.
5. Ask a question — the panel automatically injects the current document context.

---

## 💡 Usage Tips

### ▶ iLogic Generation & Execution

> [!TIP]
> Ask Claude to write an iLogic rule. When it responds with an `ilogic` or `vb` fenced block, both **Copy iLogic** and **▶ Run iLogic** activate. Run iLogic executes the code immediately in the active document. If the rule errors, Claude automatically receives the error and retries with a fix.

### 🔧 Parameter Write-Back & Feature Suppression

> [!TIP]
> Ask Claude to recommend parameter changes. It will respond with a `` ```params `` block listing `Name = expression` pairs. Click **Apply Params** to write them directly to the model — the chat shows a before → after diff for each change.  
> To suppress a feature: ask Claude to add `FeatureName.Suppressed = True` to the params block. Use `False` to unsuppress.

### 💾 Saving & Resuming Conversations

> [!TIP]
> Click **⚙ Settings → Save Conversation…** to name and save the current session. Load it back later with **Load Conversation…** to continue exactly where you left off — useful for long design review sessions across multiple work sessions.

### 🖱️ Context-Aware Selection

> [!TIP]
> Select a face, edge, feature, or component before sending your message. Claude will see the selected entity type in its context — useful for targeted questions like *"What's the best way to add a fillet to this edge?"*

### 📋 Copy from Chat

> [!TIP]
> Right-click anywhere in the chat panel to **Copy** selected text or **Select All**. Paste directly into Inventor's parameter dialog, iProperties fields, or any other application.

**Parameter-aware prompts**  
Claude sees your user parameters by name:  
*"Add a conditional to suppress Fillet_R2 when Width < 50 mm."*

**Assembly analysis**  
*"List all components that aren't grounded and have no assembly constraints."*

**BOM queries**  
*"How many unique parts are in this assembly? Which components appear more than once?"*

**Drawing automation**  
*"Write iLogic to populate the title block Part Number and Description from the active document's iProperties."*

**DFM review**  
*"Are there any features in this part that might cause issues in sheet metal manufacturing?"*

**Exporting a session**  
Click **Export** after any conversation to save it as `.md` (for notes/docs) or `.txt`.

---

## 🗑️ Manual Uninstall

1. Remove the three files from the Addins folder.
2. Unregister COM (optional):

   ```powershell
   & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" `
      "ClaudeForInventor.dll" /unregister /nologo
   ```

3. Delete the registry key (optional):

> [!CAUTION]
> `HKEY_CURRENT_USER\SOFTWARE\ClaudeForInventor`

---

Open source · Created by T.A.Smith2 (2026) and [Claude Code](https://claude.ai/claude-code)  
[github.com/BladerunnerxRC/Claude-AI-Assistant-Autodesk-Inventor](https://github.com/BladerunnerxRC/Claude-AI-Assistant-Autodesk-Inventor)
