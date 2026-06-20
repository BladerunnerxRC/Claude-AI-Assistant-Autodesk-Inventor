# Claude AI for Autodesk Inventor

Embeds **Claude Opus 4.8** directly into the Autodesk Inventor ribbon.  
Open a chat panel from **Tools → Claude AI** in any Part, Assembly, or Drawing environment.

---

## Features

### Chat & Context

- Live document context injected into every prompt — user parameters, model features, assembly occurrences, constraints, and iProperties
- Conversation history maintained across turns in the same session
- Active sketch detection — reports sketch name, line count, and circle count when a sketch is open for editing
- Material reported for Part documents

### Responses

- Rich markdown rendering: `# H1` / `## H2` / `### H3` headings, **bold**, *italic*, `inline code`, fenced code blocks with dark background, bullet and numbered lists, horizontal rules
- iLogic / VB.NET code blocks highlighted in green monospace; **Copy iLogic** button activates automatically
- C# and generic code blocks also detected and copyable

### UI

- Dark-themed floating panel positioned to the right of Inventor's workspace
- Space and Delete keys work correctly inside the input box (Inventor keyboard interception bypassed via low-level hook)
- New Chat, Copy Code, and Settings buttons in the toolbar
- Status bar shows model name and current state (Ready / Claude is thinking…)

### Security

- API key stored securely in the Windows registry (`HKCU\SOFTWARE\ClaudeForInventor`)

---

## Prerequisites

| Requirement | Version |
|---|---|
| Autodesk Inventor | 2027 (adjust `<InventorBin>` path for other versions) |
| .NET SDK | 9.0+ (`dotnet build` CLI) or Visual Studio 2022 |
| .NET Framework | 4.8 (target runtime — ships with Windows) |
| Anthropic API key | [console.anthropic.com/keys](https://console.anthropic.com/keys) |

---

## Build

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

> The `/codebase` flag is required because the assembly is not in the GAC.  
> The unsigned-assembly warning it produces can be ignored for local development.

### 4. Updating the DLL (subsequent builds)

For code-only changes, replace `ClaudeForInventor.dll` in the Addins folder and restart Inventor.  
Re-running `regasm` is only needed if the DLL moves to a different path.

---

## Install

Copy all three files from `bin\Release\net48\` to Inventor's add-ins folder:

```text
%APPDATA%\Autodesk\Inventor 2027\Addins\
    ClaudeForInventor.dll
    ClaudeForInventor.addin
    Newtonsoft.Json.dll
```

> **Tip:** For a machine-wide install, use `C:\ProgramData\Autodesk\Inventor 2027\Addins\` instead.

---

## First Run

1. Launch Inventor.
2. Open any Part, Assembly, or Drawing.
3. Go to **Tools → Claude AI → Claude AI**.
4. Click **⚙ Settings** and paste your Anthropic API key.
5. Ask a question — the panel automatically injects the current document context.

---

## Usage Tips

**iLogic generation**  
Ask Claude to write an iLogic rule. When it responds with an `ilogic` fenced block, the **Copy iLogic** button activates. Click it, then paste directly into the iLogic editor.

**Parameter-aware prompts**  
Claude sees your user parameters by name:  
*"Add a conditional to suppress the Fillet_R2 feature when Width < 50 mm."*

**Assembly analysis**  
*"List all components that aren't grounded and have no assembly constraints."*

**Drawing automation**  
*"Write iLogic to populate the title block Part Number and Description from the active document's iProperties."*

**DFM review**  
*"Are there any features in this part that might cause issues in sheet metal manufacturing?"*

---

## Suggested Improvements



---

## Uninstall

1. Remove the three files from the Addins folder.
2. Unregister COM (optional):

   ```powershell
   & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" `
      "ClaudeForInventor.dll" /unregister /nologo
   ```

3. Delete the registry key (optional):  
   `HKEY_CURRENT_USER\SOFTWARE\ClaudeForInventor`
