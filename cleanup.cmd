@echo off
::Removes compilation artifacts and other temporary files.

rem Compiled artifacts
rd /s /q "%~dp0build" > NUL 2>&1

rem Solution-wide
del "%~dp0src\*.userprefs" > NUL 2>&1
attrib -h "%~dp0src\*.suo" > NUL 2>&1
del "%~dp0src\*.suo" > NUL 2>&1
del "%~dp0src\*.user" > NUL 2>&1
del "%~dp0src\*.cache" > NUL 2>&1
rd /s /q "src\obj" > NUL 2>&1

rem Per-project
FOR /d %%D IN ("%~dp0src\*") DO (
  rd /s /q "%%D\obj" > NUL 2>&1
  del "%%D\*.pidb" > NUL 2>&1
  del "%%D\*.csproj.user" > NUL 2>&1
)
FOR /d %%D IN ("%~dp0src\AlphaFramework\*") DO (
  rd /s /q "%%D\obj" > NUL 2>&1
  del "%%D\*.pidb" > NUL 2>&1
  del "%%D\*.csproj.user" > NUL 2>&1
)
FOR /d %%D IN ("%~dp0src\FrameOfReference\*") DO (
  rd /s /q "%%D\obj" > NUL 2>&1
  del "%%D\*.pidb" > NUL 2>&1
  del "%%D\*.csproj.user" > NUL 2>&1
)
FOR /d %%D IN ("%~dp0templates\*") DO (
  rd /s /q "%%D\obj" > NUL 2>&1
  rd /s /q "%%D\bin" > NUL 2>&1
  del "%%D\*.pidb" > NUL 2>&1
  del "%%D\*.csproj.user" > NUL 2>&1
)
FOR /d %%D IN ("%~dp0templates\AlphaFramework\*") DO (
  rd /s /q "%%D\obj" > NUL 2>&1
  del "%%D\*.pidb" > NUL 2>&1
  del "%%D\*.csproj.user" > NUL 2>&1
)

rem NUnit logs
del "%~dp0*.VisualState.xml" > NUL 2>&1
del "%~dp0TestResult.xml" > NUL 2>&1

rem JetBrains caches
rd /s /q "%~dp0src\_ReSharper.OmegaEngine" > NUL 2>&1
rd /s /q "%~dp0src\_TeamCity.OmegaEngine" > NUL 2>&1
rd /s /q "%~dp0src\_dotTrace.OmegaEngine" > NUL 2>&1
rd /s /q "%~dp0src\_dotMemory.OmegaEngine" > NUL 2>&1