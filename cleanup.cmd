@echo off
::Removes compilation artifacts and other temporary files.

rem Clear binaries
rd /s /q "%~dp0build"

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
rd /s /q "%~dp0src\Common\obj" > NUL 2>&1
rd /s /q "%~dp0src\Test.Common\obj" > NUL 2>&1
rd /s /q "%~dp0src\OmegaEngine\obj" > NUL 2>&1
rd /s /q "%~dp0src\OmegaGUI\obj" > NUL 2>&1
rd /s /q "%~dp0src\AlphaEditor\obj" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\Game\obj" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\Editor\obj" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\World\obj" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\Test.World\obj" > NUL 2>&1
rd /s /q "%~dp0src\TerrainSample\Presentation\obj" > NUL 2>&1

rem Remove NUnit logs
del "%~dp0*.VisualState.xml" > NUL 2>&1
del "%~dp0TestResult.xml" > NUL 2>&1
