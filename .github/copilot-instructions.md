# OmegaEngine - GitHub Copilot Guide

## Overview
3D graphics engine for .NET Framework 4.7.2/DirectX 9. 439 C# files: OmegaEngine (core), OmegaEngine.Foundation (infrastructure), OmegaGUI (GUI toolkit), AlphaFramework (game framework), Frame of Reference (sample game).

⚠️ **Windows-only (x86)** - DirectX 9/SlimDX/WinForms dependencies. DO NOT build on Linux/macOS.

## Build Commands (Windows PowerShell)

**Master build** (use this): `.\build.ps1 [Version]` (default: 1.0.0-pre)
- Runs: src\build.ps1 → src\test.ps1 → templates\build.ps1 → doc\build.ps1 → feeds\build.ps1
- Time: 2-5 minutes
- Output: `artifacts\Release\` (NuGet packages)

**Code only**: `cd src; .\build.ps1 [Version]`
- Executes: `dotnet msbuild /v:Quiet /t:Restore /t:Build /p:Configuration=Release /p:Version=$Version`
- Publishes Frame of Reference Game/Editor

**Tests** (ALWAYS build first): `cd src; .\test.ps1`
- Runs: `dotnet test --no-build --logger trx --configuration Release`
- Results: `src\UnitTests\TestResults\*.trx`
- Time: 1-2 minutes

**Docs**: `cd doc; .\build.ps1` (requires docfx 2.78.3 via `dotnet tool restore`)
**Templates**: `cd templates; .\build.ps1 [Version]` (requires nuget.exe in PATH)
**Shaders**: `cd shaders; .\build.ps1` (requires DXSDK_DIR env var; skip unless modifying shaders)

## Project Structure

**Root**: `build.ps1` (master), `GitVersion.yml`, `.editorconfig` (4 spaces, C# style)
**Solution**: `src/OmegaEngine.slnx`
**Config**: `src/Directory.Build.props` (net472, x86, TreatWarningsAsErrors=True, C# 13.0)

**Directories**:
- `src/OmegaEngine/` - Core rendering (DirectX 9)
- `src/OmegaEngine.Foundation/` - Infrastructure (storage, data structures)
- `src/OmegaGUI/` - GUI toolkit
- `src/AlphaFramework/{World,Presentation,Editor}/` - Game framework (MVP pattern)
- `src/FrameOfReference/{World,Presentation,Game,Editor}/` - Sample game
- `src/{UnitTests,AlphaFramework/UnitTests,FrameOfReference/UnitTests}/` - Tests
- `content/` - Game assets (meshes, textures, sounds)
- `shaders/` - HLSL sources (*.fx → *.fxo)
- `doc/` - Documentation (docfx)
- `templates/` - Project templates

**Dependency chain**: Foundation → Engine → GUI/AlphaFramework.Presentation → Editor

## CI Pipeline (`.github/workflows/build.yml`)

**Platform**: windows-latest | **Trigger**: push, pull_request

**Steps**: Checkout (full history) → GitVersion 5.12.x → Install VC++ 2010 x86 → Install D3DX9 DLL → Build → Test → Report

**Success criteria**: Exit code 0, all tests pass, zero warnings (TreatWarningsAsErrors=True)

**On tags**: Publish NuGet packages, deploy docs to docs.omegaengine.de

## Code Style (`.editorconfig`)

**C#**: 4 spaces, braces on new line, var for apparent types only, expression bodies for methods/properties, pattern matching (`is null`), no `this.` qualifier
**Files**: UTF-8, final newline, trim whitespace
**PowerShell/Projects**: CRLF | **JSON/YAML**: 2 spaces
**Warnings as errors** (TreatWarningsAsErrors=True) - suppressed: 1591 (XML docs), NU1900-1904 (advisories)

## Common Tasks

**Making changes**:
1. Identify project: OmegaEngine (rendering), Foundation (infrastructure), OmegaGUI (GUI), AlphaFramework (game)
2. Build: `cd src; .\build.ps1 1.0.0-dev`
3. Test: `cd src; .\test.ps1`
4. Check warnings (build fails on any warning)

**Dependencies**: SlimDX 4.0.13.44 (DirectX, net472), NanoByte.Common.WinForms 2.21.1, xunit 2.9.3, Moq 4.20.72, FluentAssertions 7.2.0

**Docs**: Edit `doc/*.md` or XML comments → `cd doc; .\build.ps1` → output: `artifacts\Documentation\`

## Common Errors

| Error | Solution |
|-------|----------|
| "FrameworkReference...not recognized" | Windows-only, don't build on Linux/macOS |
| "DirectX SDK must be installed!" | Install DirectX SDK or skip shader build |
| "nuget: command not found" | Install NuGet CLI or skip template build |
| Tests fail | Install VC++ 2010 Redist x86 + DirectX June 2010 Runtime |

## Validation Checklist

✅ Build without warnings: `cd src && .\build.ps1 1.0.0-dev`
✅ All tests pass: `cd src && .\test.ps1`
✅ Follows .editorconfig (4 spaces, braces on new line)
✅ XML docs for public APIs
✅ Windows-only verification

**Trust these validated instructions.** Search only if unclear.
