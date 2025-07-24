This folder contains the shaders used by OmegaEngine.

.fx files contain the shader source code in DirectX's HLSL format.
.fxo files contain pre-compiled shaders.

Run "compile.cmd /LD" to update the pre-compiled shaders. This requires the DirectX SDK to be installed.

Command-line arguments:

/LD = Support Shader Model 1.1
/Gec /Gis = Upgrade Shader Model 1.1 to 2.0
/Zi = Enable Debug information
