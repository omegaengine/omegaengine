Param ([Switch]$Deploy, [Switch]$Machine)
$ErrorActionPreference = "Stop"

$RootDir = $(Split-Path -Path $MyInvocation.MyCommand.Definition -Parent)
pushd $RootDir

$TargetDir = Join-Path $RootDir ..\artifacts\Templates
if (Test-Path "$TargetDir\ProjectTemplates") { rm "$TargetDir\ProjectTemplates\*.zip" }
if (!(Test-Path "$TargetDir\ProjectTemplates")) { mkdir "$TargetDir\ProjectTemplates" | Out-Null }

# Adding zip.exe to PATH
$env:PATH = "$env:PATH;$RootDir"

echo "Packaging WinForms template..."
pushd WinForms
zip.exe -q -9 -r "$TargetDir\ProjectTemplates\WinForms.zip" . --exclude obj bin
popd

echo "Packaging Fullscreen template..."
pushd Fullscreen
zip.exe -q -9 -r "$TargetDir\ProjectTemplates\Fullscreen.zip" . --exclude obj bin
popd

echo "Packaging AlphaFramework template..."
pushd AlphaFramework
zip.exe -q -9 -r "$TargetDir\ProjectTemplates\AlphaFramework.zip" . --exclude obj bin
popd



if (Test-Path "$TargetDir\omegaengine-templates.vsix") { rm "$TargetDir\omegaengine-templates.vsix" }

echo "Creating VSIX extension..."
pushd vsix
zip.exe -q -9 -r "$TargetDir\omegaengine-templates.vsix" .
popd

echo "Adding templates to extension..."
pushd $TargetDir
zip.exe -q -9 -r "$TargetDir\omegaengine-templates.vsix" ProjectTemplates
popd

echo "Adding NuGet packages to extension..."
pushd ..\artifacts
copy ..\src\packages\ICSharpCode.SharpZipLib.Patched.*\*.nupkg Packages -Force
copy ..\src\packages\NanoByte.Common.*\*.nupkg Packages -Force
copy ..\src\packages\SlimDX.*\*.nupkg Packages -Force
zip.exe -q -9 -r "$TargetDir\omegaengine-templates.vsix" Packages
rm Packages\ICSharpCode.SharpZipLib.Patched.*.nupkg -Force
rm Packages\NanoByte.Common.*.nupkg -Force
rm Packages\SlimDX.*.nupkg -Force
popd

popd
