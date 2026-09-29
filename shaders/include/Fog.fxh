// Description: Linear distance fog, applied by the pixel shaders themselves because Shader Model 3.0 ignores fixed-function fog.
// Mirrors Direct3D's table fog with a W-friendly projection matrix, i.e. the fog distance is the view-space depth.

float3 fogParams : Fog = {0.0f, 1.0f, 0.0f}; // Start distance, end distance, 1 if enabled (0 otherwise)
float3 fogColor  : FogColor;                  // Fog color in linear space

// How much of the surface color remains visible through the fog (1 = unfogged, 0 = fully fogged)
float fogFactor(float viewDepth)
{ return (fogParams.z > 0) ? saturate((fogParams.y - viewDepth) / (fogParams.y - fogParams.x)) : 1; }

// Applies fog to a color; additive passes fade to black instead, since the first pass already contributed the fog color
float3 applyFog(float3 color, float viewDepth, uniform bool firstPass)
{ return firstPass ? lerp(fogColor, color, fogFactor(viewDepth)) : color * fogFactor(viewDepth); }
