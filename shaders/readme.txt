This directory contains the shaders used by OmegaEngine.

.fx files contain the shader source code in DirectX's HLSL format.

Run "build.ps1 /LD" to compile the shaders to .fxo files. This requires the DirectX SDK to be installed.
The compiled files are placed in ..\src\OmegaEngine\Shaders and are packaged together with the engine.

Command-line arguments:

/LD = Support Shader Model 1.1
/Gec /Gis = Upgrade Shader Model 1.1 to 2.0
/Zi = Enable Debug information
