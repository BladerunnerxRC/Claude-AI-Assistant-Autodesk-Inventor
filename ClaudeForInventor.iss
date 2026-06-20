; Claude AI for Autodesk Inventor — Inno Setup 6 installer script
; Download Inno Setup 6: https://jrsoftware.org/isinfo.php
;
; NOTE: The Anthropic API key is entered by the user at first run and stored
; at runtime in HKCU\SOFTWARE\ClaudeForInventor. It is NOT bundled here and
; is NOT touched by install or uninstall.
;
; How to build:
;   1. dotnet build -c Release       (from Claude_for_Inventor\ folder)
;   2. Open this file in the Inno Setup Compiler IDE and press F9
;   Output: Installer\ClaudeForInventor_v1.1_Setup.exe
;
; Logs are written to: C:\ProgramData\ClaudeForInventor\Logs\

#define AppName      "Claude AI for Autodesk Inventor"
#define AppVersion   "1.1"
#define AppPublisher "T.A.Smith2"
#define AppURL       "https://github.com/BladerunnerxRC/Autodesk-Inventor"
#define DllSrc       "Claude_for_Inventor\bin\Release\net48"
#define RegAsm       "{win}\Microsoft.NET\Framework64\v4.0.30319\regasm.exe"
#define LogBase      "{commonappdata}\ClaudeForInventor\Logs"

; ── Setup metadata ────────────────────────────────────────────────────────────

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppId={{A3F2B8C1-7E4D-4F9A-B5C2-D8E1F3A6B9C4}

; Files are installed to the Inventor 2027 machine-wide Addins folder.
; Directory picker is hidden — the destination is fixed.
DefaultDirName={commonappdata}\Autodesk\Inventor 2027\Addins
DisableDirPage=yes
DisableProgramGroupPage=yes

OutputBaseFilename=ClaudeForInventor_v{#AppVersion}_Setup
OutputDir=Installer
Compression=lzma2
SolidCompression=yes

; regasm.exe requires elevation
PrivilegesRequired=admin

WizardStyle=modern
UninstallDisplayName={#AppName} v{#AppVersion}

; Capture full Inno Setup install log — saved permanently in DeinitializeSetup
SetupLogging=yes

; ── Language ──────────────────────────────────────────────────────────────────

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

; ── Files ─────────────────────────────────────────────────────────────────────
; Only the three runtime files. No credentials, no registry values.

[Files]
Source: "{#DllSrc}\ClaudeForInventor.dll";   DestDir: "{app}"; Flags: ignoreversion
Source: "{#DllSrc}\ClaudeForInventor.addin"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#DllSrc}\Newtonsoft.Json.dll";     DestDir: "{app}"; Flags: ignoreversion

; ── COM registration ──────────────────────────────────────────────────────────

[Run]
Filename: "{#RegAsm}"; Parameters: """{app}\ClaudeForInventor.dll"" /codebase /nologo"; Flags: runhidden; StatusMsg: "Registering COM component..."

[UninstallRun]
Filename: "{#RegAsm}"; Parameters: """{app}\ClaudeForInventor.dll"" /unregister /nologo"; Flags: runhidden

; ── Pascal code ───────────────────────────────────────────────────────────────

[Code]

{ Returns the log directory path, creating it if it does not exist. }
function EnsureLogDir(): String;
var
  Dir : String;
begin
  Dir := ExpandConstant('{#LogBase}');
  if not DirExists(Dir) then
    ForceDirectories(Dir);
  Result := Dir;
end;

{ ── Install log ─────────────────────────────────────────────────────────────
  Called by Inno Setup when the wizard closes (success or failure).
  Copies the full Inno Setup session log to the permanent log folder.      }
procedure DeinitializeSetup();
var
  Dest : String;
begin
  Dest := EnsureLogDir() + '\Install_'
        + GetDateTimeString('yyyymmdd_hhnnss', #0, #0) + '.log';
  FileCopy(ExpandConstant('{log}'), Dest, False);
end;

{ ── Uninstall log ───────────────────────────────────────────────────────────
  Fires after files and registry entries are removed.                       }
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  LogFile : String;
  Lines   : TArrayOfString;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    LogFile := EnsureLogDir() + '\Uninstall_'
             + GetDateTimeString('yyyymmdd_hhnnss', #0, #0) + '.log';

    SetArrayLength(Lines, 6);
    Lines[0] := 'Claude AI for Autodesk Inventor — Uninstall Log';
    Lines[1] := '================================================';
    Lines[2] := 'Date/Time    : ' + GetDateTimeString('yyyy-mm-dd hh:nn:ss', '-', ':');
    Lines[3] := 'Version      : {#AppVersion}';
    Lines[4] := 'Removed from : ' + ExpandConstant('{app}');
    Lines[5] := 'Status       : Uninstall completed successfully.';

    SaveStringsToFile(LogFile, Lines, False);
  end;
end;

{ ── Pre-flight checks ───────────────────────────────────────────────────────
  Warn about missing .NET Framework and missing Inventor installation.      }
function InitializeSetup(): Boolean;
var
  RegAsmPath : String;
  AddinsPath : String;
begin
  Result := True;

  RegAsmPath := ExpandConstant('{win}\Microsoft.NET\Framework64\v4.0.30319\regasm.exe');
  if not FileExists(RegAsmPath) then
    MsgBox(
      '.NET Framework 4.8 (64-bit) does not appear to be installed.' + #13#10 +
      'COM registration will fail without it.' + #13#10 + #13#10 +
      'Download: https://dotnet.microsoft.com/download/dotnet-framework/net48',
      mbError, MB_OK);

  AddinsPath := ExpandConstant('{commonappdata}\Autodesk\Inventor 2027\Addins');
  if not DirExists(AddinsPath) then
    if MsgBox(
      'The Autodesk Inventor 2027 Addins folder was not found:' + #13#10 +
      AddinsPath + #13#10 + #13#10 +
      'Inventor 2027 may not be installed on this machine. The' + #13#10 +
      'folder will be created, but the add-in will only load'   + #13#10 +
      'once Inventor is installed.' + #13#10 + #13#10 +
      'Continue anyway?',
      mbConfirmation, MB_YESNO) = IDNO then
        Result := False;
end;
