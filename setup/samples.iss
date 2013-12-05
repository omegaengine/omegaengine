;Set version number via command-line argument "/dVersion=X.Y"
#ifndef Version
  #define Version "0.1"
#endif

;Constants
#define AppName "OmegaEngine Samples"
#define AppNameShort "TerrainSample"
#define Company "NanoByte"
#define Website "http://omega.nanobyte.de/"

;Automatic dependency download and installation
#include "scripts\fileversion.iss"
#include "scripts\winversion.iss"
#include "scripts\products.iss"
#include "scripts\products\msi31.iss"
#include "scripts\products\vcredist.iss"
#include "scripts\products\dotnetfx20sp2.iss"
#include "scripts\products\directx.iss"

[CustomMessages]
winxpsp2_title=Windows XP Service Pack 2

;Used by downloader
appname={#AppName}

[Setup]
OutputDir=..\build\Setup
OutputBaseFilename=omegaengine-samples-{#Version}

;General settings
ShowLanguageDialog=auto
MinVersion=0,5.1
DefaultDirName={pf}\{#AppName}
AppName={#AppName}
AppVerName={#AppName} v{#Version}
AppCopyright=Copyright 2006-2012 Bastian Eicher
AppID={#AppName}
AppMutex=Terrain Sample Game,Terrain Sample Game Editor,Space Sample Game,Space Sample Game Editor
DefaultGroupName={#AppName}
AppPublisher={#Company}
AppVersion={#Version}
DisableProgramGroupPage=true
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#AppNameShort}.exe
UninstallDisplayName={#AppName}
VersionInfoVersion={#Version}
VersionInfoCompany={#Company}
VersionInfoDescription={#AppName}
VersionInfoTextVersion={#AppName} {#Version}
AppPublisherURL={#Website}
SetupIconFile=setup.ico
WizardImageFile=compiler:WizModernImage-IS.bmp
WizardSmallImageFile=compiler:WizModernSmallImage-IS.bmp
Compression=lzma/ultra
SolidCompression=true

[Languages]
Name: de; MessagesFile: compiler:Languages\German.isl
Name: en; MessagesFile: compiler:Default.isl

[InstallDelete]
;Remove obsolete files from previous versions
Name: {app}\Base; Type: filesandordirs

;Remove extracted game archives and any installed mods
Name: {app}\content; Type: filesandordirs
Name: {app}\Mods; Type: filesandordirs
Name: {app}; Type: dirifempty

[Files]
Source: ..\build\Release\*; Excludes: _portable,*.xml,*.log,*.pdb,*.vshost.exe; DestDir: {app}; Flags: ignoreversion recursesubdirs
Source: ..\content\*; DestDir: {app}\content; Flags: ignoreversion recursesubdirs

[Icons]
Name: {group}\{cm:UninstallProgram,{#AppName}}; Filename: {uninstallexe}
Name: {group}\Website; Filename: {#Website}
Name: {group}\Terrain Sample Game; Filename: {app}\TerrainSample.exe
Name: {group}\Terrain Sample Benchmark; Filename: {app}\TerrainSample.exe; Parameters: /benchmark
Name: {group}\Terrain Sample Editor; Filename: {app}\TerrainSample.Editor.exe
Name: {group}\Space Sample Game; Filename: {app}\SpaceSample.exe
Name: {group}\Space Sample Benchmark; Filename: {app}\SpaceSample.exe; Parameters: /benchmark
Name: {group}\Space Sample Editor; Filename: {app}\SpaceSample.Editor.exe

[Run]
Filename: {app}\TerrainSample.exe; Description: {cm:LaunchProgram,Terrain Sample Game}; Flags: nowait postinstall runasoriginaluser skipifsilent
Filename: {app}\SpaceSample.exe; Description: {cm:LaunchProgram,Space Sample Game}; Flags: nowait postinstall runasoriginaluser skipifsilent unchecked

[Code]
function InitializeSetup(): Boolean;
begin
	// Determine the exact Windows version, including Service pack
	initwinversion();

	// Check if vcredist and .NET 2.0 can be installed on this OS
	if not minwinspversion(5, 1, 2) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp2_title')]), mbError, MB_OK);
		exit;
	end;

	// Add all required products to the list
	msi31('3.0');
	vcredist();	
	dotnetfx20sp2();
	directx();

	Result := true;
end;
