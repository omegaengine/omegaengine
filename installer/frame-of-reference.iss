;Set version number via command-line argument "/dVersion=X.Y"
#ifndef Version
  #define Version "0.1"
#endif

;Constants
#define AppName "Frame of Reference"
#define AppNameShort "Frame"
#define Company "NanoByte"
#define Website "http://www.omegaengine.de/"

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
OutputDir=..\build\Installer
OutputBaseFilename=frame-of-reference

;General settings
ShowLanguageDialog=auto
MinVersion=0,5.1
DefaultDirName={pf}\{#AppName}
AppName={#AppName}
AppVerName={#AppName} v{#Version}
AppCopyright=Copyright 2006-2012 Bastian Eicher
AppID={#AppName}
AppMutex=Frame of Reference,Frame of Reference Editor
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
Name: {group}\Frame of Reference; Filename: {app}\FrameOfReference.exe
Name: {group}\Frame of Reference Benchmark; Filename: {app}\FrameOfReference.exe; Parameters: /benchmark
Name: {group}\Frame of Reference Editor; Filename: {app}\FrameOfReference.Editor.exe

[Run]
Filename: {app}\FrameOfReference.exe; Description: {cm:LaunchProgram,Frame of Reference}; Flags: nowait postinstall runasoriginaluser skipifsilent

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
