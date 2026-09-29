This directory contains the shaders used by OmegaEngine.

.fx files contain the shader source code in DirectX's HLSL format.

All techniques target Shader Model 3.0 (vs_3_0/ps_3_0), the minimum the engine requires.

Run "build.ps1" to compile the shaders to .fxo files. This requires the DirectX SDK to be installed.
The compiled files are placed in ..\src\OmegaEngine\Shaders and are packaged together with the engine.

Additional command-line arguments are passed on to fxc.exe, e.g.:

/Zi = Enable Debug information
