@echo off

if '%1'=='' goto error_arg
if '%2'=='' goto error_arg
if '%3'=='' goto error_arg

::Add DirectX SDK to search path, so that the shader compiler FXC can be found
if "%DXSDK_DIR%"=="" goto error_dxsdk
path %DXSDK_DIR%Utilities\bin\%PROCESSOR_ARCHITECTURE%;%path%



::Enable Debug information (must keep optimization on or Shader Model limits will be passed)
if "%1"=="Debug" set debug=/Zi

::Support Shader Model 1.1
if "%2"=="SM11" set legacy=/LD

::Upgrade Shader Model 1.1 to 2.0
if "%2"=="SM20" set legacy=/Gec /Gis



::Compile all *.fx files in the directoy the script is located
::%~3 = The compiled shader target directory
::%~dp0*.fx --> %%A = Shader source files (HLSL)
::%~dp0*.fx --> %%~nA.fxo =  Compiled shader
FOR %%A IN ("%~dp0*.fx") DO fxc /nologo %debug% %legacy% /Tfx_2_0 /Fo"%~3\%%~nA.fxo" "%%A"
exit /b 0



:error_arg
echo Missing arguments! >&2
echo Usage: compile Debug/Release SM11/SM20 TargetDir >&2
exit /b 1

:error_dxsdk
echo DirectX SDK must be installed! >&2
exit /b 2
