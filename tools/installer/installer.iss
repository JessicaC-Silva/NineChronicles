#define MyAppName "Nine Chronicles"
#define MyAppPublisher "Nine Corporation"
#define MyAppURL "https://nine-chronicles.com/"
#define GameExeName "Nine Chronicles.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{789AAC8F-6C36-4A84-ABB9-4FEA48EA924C}}
AppName={#MyAppName}
AppVersion=
AppVerName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={localappdata}{\}Programs{\}{#MyAppName}
DisableProgramGroupPage=yes
OutputBaseFilename="Nine Chronicles Installer"
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
CreateDesktopIcon=Create a &desktop icon
RegisterStartup=Register Nine Chronicles to the startup program

[Tasks]
Name: "CreateDesktopIcon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "RegisterStartup"; Description: "{cm:RegisterStartup}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\Updater\out\win-x64\Nine Chronicles Updater.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\windowsdesktop-runtime-3.1.3-win-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall
Source: ".\vc_redist.x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#GameExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#GameExeName}"; Tasks: CreateDesktopIcon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#GameExeName}"; Tasks: RegisterStartup

[Code]
var
  UUID: String;

function GenerateUUID(): String;
var
  UUIDLib: Variant;
begin
  UUIDLib := CreateOleObject('Scriptlet.TypeLib');
  Result := Copy(UUIDLib.GUID(), 2, 36);
end;

function UseUUID(): String;
var
  UUIDPath: String;
  LoadedUUID: AnsiString;
begin
  UUIDPath := Format('%s\planetarium\.installer_mixpanel_uuid', [ExpandConstant('{localappdata}')]);
  if (FileExists(UUIDPath)) then
  begin
    LoadStringFromFile(UUIDPath, LoadedUUID);
    Result := LoadedUUID;
  end else begin
    Result := GenerateUUID();
    SaveStringToFile(UUIDPath, Result, False);
  end
end;

function InitializeSetup(): Boolean;
begin
  UUID := UseUUID();
  Result := True;
end;

procedure MixpanelTrack(Event, UUID: String);
var
  WinHttpReq: Variant;
begin
  WinHttpReq := CreateOleObject('WinHttp.WinHttpRequest.5.1');
  WinHttpReq.Open('POST', 'https://api.mixpanel.com/track', False);
  WinHttpReq.SetRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
  WinHttpReq.Send(Format('data={"event":"%s","properties":{"token":"80a1e14b57d050536185c7459d45195a","distinct_id":"%s"}}', [Event, UUID]));
  if WinHttpReq.ResponseText = 1 then begin
    Log('Mixpanel request success.');
  end else begin
    Log('Mixpanel request failed. ' + WinHttpReq.ResponseText);
  end;
end;

procedure SendUUID(Event, UUID: String);
var
  WinHttpReq: Variant;
begin
  WinHttpReq := CreateOleObject('WinHttp.WinHttpRequest.5.1');
  WinHttpReq.Open('POST', 'https://planetariumhq.slack.com/services/hooks/slackbot?token=4hBLriaHECDGHlNNbOnwjkfk&channel=%239c-installer', False);
  WinHttpReq.SetRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
  WinHttpReq.Send('[INSTALLER] UUID: ' + UUID + ' // EVENT: ' + Event);
  if WinHttpReq.ResponseText = 'ok' then begin
    Log('Slack request success.');
  end else begin
    Log('Slack request failed. ' + WinHttpReq.ResponseText);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    Log('Install: Request Mixpanel.');
    Log('UUID: ' + UUID);
    MixpanelTrack('Installer/Start-test', UUID);
    SendUUID('5. Installer/Start', UUID);
  end;

  if CurStep = ssPostInstall then
  begin
    Log('PostInstall: Request Mixpanel.');
    Log('UUID: ' + UUID);
    MixpanelTrack('Installer/End-test', UUID);
    SendUUID('6. Installer/End', UUID);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    Log('UnInstall: Request Mixpanel.');
    Log('UUID: ' + UUID);
    MixpanelTrack('Installer/Uninstall-test', UUID);
    SendUUID('8. Installer/Uninstall', UUID);
  end;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  case CurPageID of
    wpSelectDir:
      begin
        Log('Install: Select Directory.');
        Log('UUID: ' + UUID);
        MixpanelTrack('Installer/SelectDir-test', UUID);
        SendUUID('1. Installer/SelectDir', UUID);
      end;
    wpSelectTasks:
      begin
        Log('Install: Select Tasks.');
        Log('UUID: ' + UUID);
        MixpanelTrack('Installer/SelectTasks-test', UUID);
        SendUUID('2. Installer/SelectTasks', UUID);
      end;
    wpReady:
      begin
        Log('Install: Ready.');
        Log('UUID: ' + UUID);
        MixpanelTrack('Installer/Ready-test', UUID);
        SendUUID('3. Installer/Ready', UUID);
      end;
    wpInstalling:
      begin
        Log('Install: Installing.');
        Log('UUID: ' + UUID);
        MixpanelTrack('Installer/Installing-test', UUID);
        SendUUID('4. Installer/Installing', UUID);
      end;
    wpFinished:
      begin
        Log('Install: Finished.');
        Log('UUID: ' + UUID);
        MixpanelTrack('Installer/Finished-test', UUID);
        SendUUID('7. Installer/Finished', UUID);
      end;
  end;
end;

[Run]
Filename: "{cmd}"; Parameters: "/C ""taskkill /im ""{#MyAppName}.exe"""" /f /t"

[Run]
Filename: {tmp}\windowsdesktop-runtime-3.1.3-win-x64.exe; \
    Parameters: "/q /norestart"; \
    StatusMsg: "Installing .NET Core Runtime..."

[Run]
Filename: {tmp}\vc_redist.x64.exe; \
    Parameters: "/q /norestart"; \
    StatusMsg: "Installing VC++ Redistributables..."

[Run]
Filename: {app}\Nine Chronicles Updater.exe; \
    StatusMsg: "Updating Nine Chonicles Executables..."

[Run]
Filename: "{app}\{#GameExeName}"; Flags: nowait postinstall skipifsilent


[InstallDelete]
Type: filesandordirs; Name: "{%TEMP}\.net\Nine Chronicles"
Type: filesandordirs; Name: "{%TEMP}\.net\Nine Chronicles Updater"

[UninstallRun]
Filename: "{cmd}"; Parameters: "/C ""taskkill /im ""{#MyAppName}.exe"""" /f /t"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
Type: filesandordirs; Name: "{%TEMP}\.net\Nine Chronicles"
Type: filesandordirs; Name: "{%TEMP}\.net\Nine Chronicles Updater"
