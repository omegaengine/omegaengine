@echo off

::Add DirectX SDK to search path, so that the shader compiler FXC can be found
if "%DXSDK_DIR%"=="" goto error_dxsdk
path %DXSDK_DIR%Utilities\bin\%PROCESSOR_ARCHITECTURE%;%path%

::Compile all *.fx files in the directory the script is located
FOR %%A IN ("%~dp0*.fx") DO fxc /nologo %* /Tfx_2_0 /Fo"%%Ao" "%%A"
exit /b 0

:error_dxsdk
echo DirectX SDK must be installed! >&2
exit /b 2
