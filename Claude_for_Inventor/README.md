# Claude AI for Autodesk Inventor

Embeds **Claude Opus 4.8** directly into the Autodesk Inventor ribbon.  
Open a chat panel from **Tools → Claude AI** in any Part, Assembly, or Drawing environment.

---

## Features

- Live document context injected into every prompt (parameters, features, components, iProperties)
- iLogic / VB.NET code detection with one-click copy
- Conversation history maintained across turns in the same session
- Dark-themed WinForms panel that docks alongside Inventor's UI
- API key stored securely in the Windows registry (HKCU)

---

## Prerequisites

| Requirement | Version |
|---|---|
| Autodesk Inventor | 2027 (adjust path below for other versions) |
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

This path is used to resolve both `Autodesk.Inventor.Interop.dll` and `stdole.dll`.

### 2. Build (CLI — recommended)

Open an **elevated (Administrator) PowerShell** terminal in the project folder, then:

```powershell
dotnet build -c Release
```

Running as Administrator allows the post-build `regasm.exe` step to write the COM
registration directly. If the build succeeded but regasm printed a warning about
`Autodesk.Inventor.Interop` not being found, register manually (step 3).

### 3. Register COM (if regasm failed or you built without elevation)

Open an **Administrator** PowerShell and run:

```powershell
# Add Inventor's Bin to PATH so regasm can resolve Autodesk.Inventor.Interop
$env:PATH += ";C:\Program Files\Autodesk\Inventor 2027\Bin"

& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" `
   "C:\<full-path-to-project>\bin\Release\net48\ClaudeForInventor.dll" `
   /codebase /nologo
```

> The `/codebase` flag is required because the assembly is not in the GAC.
> The unsigned-assembly warning it produces can be ignored for local development.

---

## Install

Copy these files to Inventor's user add-ins folder:

```text
%APPDATA%\Autodesk\Inventor 2027\Addins\
    ClaudeForInventor.dll
    ClaudeForInventor.addin
    Newtonsoft.Json.dll        ← from bin\Release\net48\
```

> **Tip:** For a machine-wide install, use `C:\ProgramData\Autodesk\Inventor 2027\Addins\` instead.

---

## First Run

1. Launch Inventor.
2. Open any Part, Assembly, or Drawing.
3. Go to **Tools → Claude AI → Claude AI** (the button).
4. Click **⚙ Settings** and paste your Anthropic API key.
5. Ask a question — the panel automatically injects the current document context.

---

## Usage Tips

**iLogic generation**  
Ask Claude to write an iLogic rule. When it responds with a `ilogic` code fence,  
the **Copy iLogic** button activates. Click it, then paste into the iLogic editor.

**Parameter-aware prompts**  
Claude sees your user parameters by name. You can ask:  
*"Add a conditional to suppress the Fillet_R2 feature when Width < 50 mm."*

**Assembly analysis**  
With an assembly open: *"List all components that aren't grounded and have no constraints."*

**Drawing automation**  
*"Write iLogic to set the title block Part Number field from the active document's Part Number iProperty."*

---

## Suggested Improvements (next steps)

- **Selection context** — detect `InventorApp.ActiveDocument.SelectSet` and append selected entity info
- **Streaming responses** — pipe SSE chunks to the RichTextBox for faster perceived response
- **iLogic runner** — execute the generated VB rule directly via `iLogicVb.RunRule`
- **Conversation export** — save the chat as `.md` or `.txt`
- **Dockable panel** — replace floating `Form` with a docked `DockableWindow` using Inventor's UI API
- **Parameter edit** — parse Claude's parameter suggestions and write them back via the Parameters API

---

## Uninstall

1. Remove the two files from the Addins folder.
2. Unregister COM (optional):

   ```cmd
   regasm.exe "ClaudeForInventor.dll" /unregister /nologo
   ```

3. Delete the registry key (optional):  
   `HKEY_CURRENT_USER\SOFTWARE\ClaudeForInventor`
