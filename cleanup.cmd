@echo off
::Removes compilation artifacts and other temporary files.

rem Clear binaries (but leave Documentation intact since it takes very long to build)
rd /s /q build\Debug > NUL 2>&1
rd /s /q build\Release > NUL 2>&1
rd /s /q build\ReleaseSDK > NUL 2>&1
rd /s /q build\NuGet > NUL 2>&1
rd /s /q build\ProjectTemplates > NUL 2>&1
rd /s /q build\Setup > NUL 2>&1
rd /s /q build\Documentation\working > NUL 2>&1
del build\Documentation\*.log > NUL 2>&1

rem Clear ReSharper's cache
rd /s /q src\_ReSharper.OmegaEngine > NUL 2>&1

rem Clear caches and per-user preferences
attrib -h src\*.suo > NUL 2>&1
del src\*.suo > NUL 2>&1
del src\*.user > NUL 2>&1
del src\*.cache > NUL 2>&1
del src\*.ncb > NUL 2>&1
del src\*.sdf > NUL 2>&1
rd /s /q src\TerrainSample\Game\obj > NUL 2>&1
rd /s /q src\TerrainSample\Editor\obj > NUL 2>&1
rd /s /q src\TerrainSample\Settings\obj > NUL 2>&1
rd /s /q src\TerrainSample\World\obj > NUL 2>&1
rd /s /q src\TerrainSample\Test.World\obj > NUL 2>&1
rd /s /q src\TerrainSample\Presentation\obj > NUL 2>&1
rd /s /q src\SpaceSample\Game\obj > NUL 2>&1
rd /s /q src\SpaceSample\Editor\obj > NUL 2>&1
rd /s /q src\SpaceSample\Settings\obj > NUL 2>&1
rd /s /q src\SpaceSample\World\obj > NUL 2>&1
rd /s /q src\SpaceSample\Test.World\obj > NUL 2>&1
rd /s /q src\SpaceSample\Presentation\obj > NUL 2>&1
rd /s /q src\Hanoi\obj > NUL 2>&1
rd /s /q src\Omega\Common\obj > NUL 2>&1
rd /s /q src\Omega\Test.Common\obj > NUL 2>&1
rd /s /q src\Omega\Engine\obj > NUL 2>&1
rd /s /q src\Omega\GUI\obj > NUL 2>&1
rd /s /q src\Omega\Editor\obj > NUL 2>&1
rd /s /q src\Modeling\obj > NUL 2>&1

rem Remove NUnit logs
del *.VisualState.xml > NUL 2>&1
del TestResult.xml > NUL 2>&1
