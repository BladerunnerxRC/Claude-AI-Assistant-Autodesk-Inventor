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
| Autodesk Inventor | 2024 or later (v28+) |
| Visual Studio | 2022 (Community is fine) |
| .NET Framework | 4.8 |
| Anthropic API key | [console.anthropic.com/keys](https://console.anthropic.com/keys) |

---

## Build

1. **Set Inventor path** in `ClaudeForInventor.csproj`:

   ```xml
   <InventorBin>C:\Program Files\Autodesk\Inventor 2025\Bin\</InventorBin>
   ```

2. Open the solution in Visual Studio 2022.

3. Build → **Release** (x64).

   The post-build step runs `regasm.exe` automatically to register the COM class.
   If it fails (needs elevation), run manually:

      ```powershell
      # Run as Administrator
      & "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" `
         "bin\Release\net48\ClaudeForInventor.dll" /codebase /nologo
      ```

---

## Install

Copy two files to Inventor's user add-ins folder:

```
%APPDATA%\Autodesk\Inventor 2025\Addins\
    ClaudeForInventor.dll
    ClaudeForInventor.addin
```

Also copy `Newtonsoft.Json.dll` from `bin\Release\net48\` to the same folder  
(or place it next to the DLL anywhere on `PATH`).

> **Tip:** For a shared install, use `C:\ProgramData\Autodesk\Inventor 2025\Addins\` instead.

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
