
[Setup]
AppName=League TikTok Overlay
AppVersion=1.0.0
DefaultDirName={pf}\League TikTok Overlay
DisableDirPage=yes
DefaultGroupName=League TikTok Overlay
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=LeagueTikTokOverlay-Setup
Compression=lzma
SolidCompression=yes
UninstallDisplayIcon={app}\LeagueTikTokOverlay.exe
SetupIconFile=icon.ico

#define PublishDir AddBackslash(SourcePath) + "bin\\Release\\net6.0-windows\\win-x64\\publish\\"

[Files]
Source: "{#PublishDir}*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion; Excludes: "*.WebView2\\*"

[Icons]
Name: "{autoprograms}\League TikTok Overlay"; Filename: "{app}\LeagueTikTokOverlay.exe"
Name: "{autodesktop}\League TikTok Overlay"; Filename: "{app}\LeagueTikTokOverlay.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional icons"; Flags: unchecked

[Run]
Filename: "{app}\LeagueTikTokOverlay.exe"; Description: "Launch League TikTok Overlay"; Flags: nowait postinstall skipifsilent