@echo off
::Removes compilation artifacts and other temporary files.

rem Clear binaries (but leave Documentation intact since it takes very long to "%~dp0build)
rd /s /q "%~dp0build\Debug" > NUL 2>&1
rd /s /q "%~dp0build\Release" > NUL 2>&1
rd /s /q "%~dp0build\ReleaseSDK" > NUL 2>&1
rd /s /q "%~dp0build\Packages" > NUL 2>&1
rd /s /q "%~dp0build\ProjectTemplates" > NUL 2>&1
rd /s /q "%~dp0build\Setup" > NUL 2>&1
rd /s /q "%~dp0build\Documentation\working" > NUL 2>&1
del "%~dp0build\Documentation\*.log" > NUL 2>&1

rem Clear JetBrains caches
rd /s /q "%~dp0src\_ReSharper.OmegaEngine" > NUL 2>&1
rd /s /q "%~dp0src\_dotTrace.OmegaEngine" > NUL 2>&1
rd /s /q "%~dp0src\_TeamCity.OmegaEngine" > NUL 2>&1

rem Clear caches and per-user preferences
attrib -h "%~dp0src\*.suo" > NUL 2>&1
del "%~dp0src\*.suo" > NUL 2>&1
del "%~dp0src\*.user" > NUL 2>&1
del "%~dp0src\*.cache" > NUL 2>&1
del "%~dp0src\*.ncb" > NUL 2>&1
del "%~dp0src\*.sdf" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\Game\obj" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\Editor\obj" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\Settings\obj" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\World\obj" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\Test.World\obj" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\Presentation\obj" > NUL 2>&1
rd /s /q "%~dp0src\SpaceSample\Game\obj" > NUL 2>&1
rd /s /q "%~dp0src\SpaceSample\Editor\obj" > NUL 2>&1
rd /s /q "%~dp0src\SpaceSample\Settings\obj" > NUL 2>&1
rd /s /q "%~dp0src\SpaceSample\World\obj" > NUL 2>&1
rd /s /q "%~dp0src\SpaceSample\Test.World\obj" > NUL 2>&1
rd /s /q "%~dp0src\SpaceSample\Presentation\obj" > NUL 2>&1
rd /s /q "%~dp0src\Omega\Common\obj" > NUL 2>&1
rd /s /q "%~dp0src\Omega\Test.Common\obj" > NUL 2>&1
rd /s /q "%~dp0src\Omega\Engine\obj" > NUL 2>&1
rd /s /q "%~dp0src\Omega\GUI\obj" > NUL 2>&1
rd /s /q "%~dp0src\Omega\Editor\obj" > NUL 2>&1
rd /s /q "%~dp0src\Modeling\obj" > NUL 2>&1

rem Remove NUnit logs
del "%~dp0*.VisualState.xml" > NUL 2>&1
del "%~dp0TestResult.xml" > NUL 2>&1
