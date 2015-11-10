; -- TowerDefender.iss --
[Setup]
AppName=Windows Tower Defender (Game Logic Release)
AppVersion=1.0
DefaultDirName={pf}\Windows Tower Defender
UninstallDisplayIcon={app}\game.exe
DefaultGroupName=Windows Tower Defender

[Files]
Source: "game.exe"; DestDir: "{app}"
Source: "game_Data\*"; DestDir: "{app}\game_Data"; Flags: recursesubdirs

[Icons]
Name: "{group}\Windows Tower Defender"; Filename: "{app}\game.exe"

; NOTE: Most apps do not need registry entries to be pre-created. If you
; don't know what the registry is or if you need to use it, then chances are
; you don't need a [Registry] section.

[Registry]
; Start "Software\My Company\My Program" keys under HKEY_CURRENT_USER
; and HKEY_LOCAL_MACHINE. The flags tell it to always delete the
; "My Program" keys upon uninstall, and delete the "My Company" keys
; if there is nothing left in them.
Root: HKCU; Subkey: "Software\CST TechPros"; Flags: uninsdeletekeyifempty
Root: HKCU; Subkey: "Software\CST TechPros\Windows Tower Defender"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\CST TechPros"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\CST TechPros\Windows Tower Defender"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\CST TechPros\Windows Tower Defender\Settings"; ValueType: string; ValueName: "Path"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "towerdefender"; ValueType: string; ValueName: ""; ValueData: "URL:towerdefender"; Flags: uninsdeletekey
Root: HKCR; Subkey: "towerdefender"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""; Flags: uninsdeletekey
Root: HKCR; Subkey: "towerdefender\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "C:\Windows\System32\url.dll,0"; Flags: uninsdeletekey
Root: HKCR; Subkey: "towerdefender\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\game.exe"" %1"; Flags: uninsdeletekey
