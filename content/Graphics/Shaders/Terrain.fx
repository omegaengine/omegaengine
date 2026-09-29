// Description: A shader that blends multiple textures together
// no specular lighting
// optional occlusion interval map for shadows
//
// All techniques require Shader Model 3.0.
//
// Compiled at runtime by TerrainShader with these preprocessor defines:
// - LIGHTING: 1 to generate the lit techniques, 0 to generate the unlit one
// - TEXTURE_MASK: Bitmask of the textures a terrain subset uses (bit 0 = Texture1, ..., bit 15 = Texture16)
//
// Techniques:
// - Simple (up to 16 textures, no lighting; only if LIGHTING is 0)
// - Light (up to 16 textures, per-pixel lighting; only if LIGHTING is 1)
// - LightDetail (up to 16 textures, per-pixel lighting, double sampling; only if LIGHTING is 1)
// - Black (all black)
// - Depth (outputs normalized camera-relative depth as grayscale)
//
// Passes:
// - AmbientLight (Light1 must be an ambient-only light, must be called as first pass)
// - TwoDirLights (Light1 and Light2 must be directional lights, must be called as first pass)
// - TwoDirLightsAdd (Light1 and Light2 must be directional lights, additive, must not be called as first pass)
// - OneDirLight (Light1 must be a directional light, must be called as first pass)
// - OneDirLightAdd (Light1 must be a directional light, additive, must not be called as first pass)
// - OnePointLight (Light1 must be a point light, must be called as first pass)
// - OnePointLightAdd (Light1 must be a point light, additive, must not be called as first pass)

// Defaults for compiling this file standalone, e.g. with fxc.exe
#ifndef LIGHTING
#define LIGHTING 1
#endif
#ifndef TEXTURE_MASK
#define TEXTURE_MASK 0xFFFF
#endif

static const float PI = 3.14159265;

//---------------- Parameters ----------------

// Camera
float3 cameraPosition          : CameraPosition;
float4x4 world                 : World;
float4x4 worldViewProjection   : WorldViewProjection;
float4x4 worldViewProjInv      : WorldViewProjectionInverse;
float4x4 worldInverseTranspose : WorldInverseTranspose;
float4x4 viewInverse           : ViewInverse;
float nearClip                 : NearClip;
float farClip                  : FarClip;

// Light 1
float4 lightDirection1 : Direction < string Object = "Light1"; string Space = "World"; >;
float4 lightPosition1  : Position < string Object = "Light1"; string Space = "World"; >;
float3 attenuation1    : Attenuation < string Object = "Light1"; > = {1.0f, 0.0f, 0.0f};
float4 diffuseColor1   : Diffuse < string Object = "Light1"; > = {1.0f, 1.0f, 1.0f, 1.0f};
float4 ambientColor1   : Ambient = {0.1f, 0.1f, 0.1f, 1.0f};

// Light 2
float4 lightDirection2 : Direction < string Object = "Light2"; string Space = "World"; >;
float3 attenuation2    : Attenuation < string Object = "Light2"; > = {1.0f, 0.0f, 0.0f};
float4 diffuseColor2   : Diffuse < string Object = "Light2"; > = {0.0f, 0.0f, 0.0f, 1.0f};
float4 ambientColor2   : Ambient = {0.0f, 0.0f, 0.0f, 1.0f};

float shadowTwilightSize = 0.05;

// UV blending
float BlendDistance = 400;
float BlendWidth = 700;

// Fog (copy of shaders/include/Fog.fxh, since this file is compiled at runtime without access to that include directory)
float3 fogParams : Fog = {0.0f, 1.0f, 0.0f}; // Start distance, end distance, 1 if enabled (0 otherwise)
float3 fogColor  : FogColor;                  // Fog color in linear space


//---------------- Textures ----------------

int FilterMode : FILTERMODE = 2; // 2 = Linear, 3 = Anisotropic

// Declared individually rather than as arrays: SurfaceShader assigns the material's diffuse maps to Diffuse parameters in declaration order,
// and the effect framework does not apply the states (e.g. sRGBTexture) of sampler array elements
#define TEXTURE(i) \
texture Texture##i : Diffuse; \
sampler2D texture##i##Sampler = sampler_state \
{ \
  texture = <Texture##i>; \
  AddressU = wrap; AddressV = wrap; \
  MinFilter = <FilterMode>; MagFilter = <FilterMode>; MipFilter = linear; \
  sRGBTexture = TRUE; \
};
TEXTURE(1)  TEXTURE(2)  TEXTURE(3)  TEXTURE(4)
TEXTURE(5)  TEXTURE(6)  TEXTURE(7)  TEXTURE(8)
TEXTURE(9)  TEXTURE(10) TEXTURE(11) TEXTURE(12)
TEXTURE(13) TEXTURE(14) TEXTURE(15) TEXTURE(16)

// Shader Model 3.0 has no integer bit operations, so bits are tested with division instead
static const int textureBits[16] = {0x1, 0x2, 0x4, 0x8, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800, 0x1000, 0x2000, 0x4000, 0x8000};
bool textureEnabled(int i)
{ return (TEXTURE_MASK / textureBits[i]) % 2 == 1; }


//---------------- Structs ----------------

struct inLight
{
    float3 entityPos          : POSITION;  // Position in object space
    float3 normal             : NORMAL;    // Normal vector in object space
    float2 texCoord           : TEXCOORD0; // Texture coordinates
    float4 occlusionIntervals : TEXCOORD1; // Angles when light 1 and 2 fist become visible and first become covered again
    float4 texWeights1        : TEXCOORD2;
    float4 texWeights2        : TEXCOORD3;
    float4 texWeights3        : TEXCOORD4;
    float4 texWeights4        : TEXCOORD5;
    float4 color              : COLOR0;
};

struct inSimple
{
    float3 entityPos          : POSITION;  // Position in object space
    float2 texCoord           : TEXCOORD0; // Texture coordinates
    float4 occlusionIntervals : TEXCOORD1; // Angles when light 1 and 2 fist become visible and first become covered again
    float4 texWeights1        : TEXCOORD2;
    float4 texWeights2        : TEXCOORD3;
    float4 texWeights3        : TEXCOORD4;
    float4 texWeights4        : TEXCOORD5;
    float4 color              : COLOR0;
};

#if LIGHTING
struct outLight
{
    float4 pos                : POSITION;  // Position in clip space
    float2 texCoord           : TEXCOORD0; // Texture coordinates
    float3 worldPos           : TEXCOORD1; // Position in world space
    float3 normal             : TEXCOORD2; // Normal vector in world space
    float4 occlusionIntervals : TEXCOORD3; // Angles when light 1 and 2 fist become visible and first become covered again
    float4 texWeights1        : TEXCOORD4;
    float4 texWeights2        : TEXCOORD5;
    float4 texWeights3        : TEXCOORD6;
    float4 texWeights4        : TEXCOORD7;
    float2 depth              : TEXCOORD8; // Distance from the camera in clip space (x) and view space (y)
    float4 color              : COLOR0;
};
#endif

#if !LIGHTING
struct outSimple
{
    float4 pos         : POSITION;  // Position in clip space
    float2 texCoord    : TEXCOORD0; // Texture coordinates
    float4 texWeights1 : TEXCOORD1;
    float4 texWeights2 : TEXCOORD2;
    float4 texWeights3 : TEXCOORD3;
    float4 texWeights4 : TEXCOORD4;
    float fogDepth     : TEXCOORD5; // Distance from the camera in view space
};
#endif

struct outBlack {
  float4 pos      : POSITION;  // Position in clip space
  float fogDepth  : TEXCOORD0; // Distance from the camera in view space
};

struct outDepth {
  float4 pos   : POSITION;  // Position in clip space
  float depth  : TEXCOORD0; // Normalized camera-relative depth
};


//---------------- Helper functions ----------------

// Translate position vector to world space
float3 transWorld(float3 position)
{ return mul(float4(position, 1.0), world).xyz; }

// Translate position vector to projection/clip space
float4 transProj(float3 position)
{ return mul(float4(position, 1.0), worldViewProjection); }

// Translate normal vector to world space and normalize
float3 transNorm(float3 normal)
{ return normalize(mul(float4(normal, 0.0), worldInverseTranspose).xyz); }

// How much of the surface color remains visible through the fog (1 = unfogged, 0 = fully fogged)
float fogFactor(float viewDepth)
{ return (fogParams.z > 0) ? saturate((fogParams.y - viewDepth) / (fogParams.y - fogParams.x)) : 1; }

// Applies fog to a color; additive passes fade to black instead, since the first pass already contributed the fog color
float4 applyFog(float4 color, float viewDepth, uniform bool firstPass)
{ return float4(firstPass ? lerp(fogColor, color.rgb, fogFactor(viewDepth)) : color.rgb * fogFactor(viewDepth), color.a); }

#if LIGHTING

float4 calcDirLight(float3 normal, float3 lightDir,
  float4 diffuseColor, float4 ambientColor) // Overload without shadows
{
    float diffuseFactor = saturate(dot(normal, lightDir));

    return diffuseColor * diffuseFactor + ambientColor;
}

float shadowSmoothstep(float focus, float angle)
{
    return smoothstep(focus, focus + shadowTwilightSize, angle);
}

float4 calcDirLight(float3 normal, float3 lightDir,
  float4 diffuseColor, float4 ambientColor, float4 occlusionIntervals) // Overload with shadows (light angles)
{
    // Calculate the angle between the light direction and an arrow pointing straight to the right
    float angle = atan2(lightDir.y, lightDir.x);

    float shadowFactor = saturate(
        min(shadowSmoothstep(occlusionIntervals.x, angle), 1 - shadowSmoothstep(occlusionIntervals.y, angle)) +
        min(shadowSmoothstep(occlusionIntervals.z, angle), 1 - shadowSmoothstep(occlusionIntervals.w, angle)));

    return calcDirLight(normal, lightDir, diffuseColor * shadowFactor, ambientColor);
}

float4 calcTwoDirLights(float3 normal,
  float3 lightDir1, float3 lightDir2,
  float4 diffCol1, float4 diffCol2,
  float4 ambCol1, float4 ambCol2,
  float4 occlusionIntervals1, float4 occlusionIntervals2)
{
    // Calculate separate light values
    float4 light1 = calcDirLight(normal, lightDir1, diffCol1, ambCol1, occlusionIntervals1);
    float4 light2 = calcDirLight(normal, lightDir2, diffCol2, ambCol2, occlusionIntervals2);

    // Add lights together
    return light1 + light2;
}

float4 calcPointLight(float3 worldPos, float3 normal, float3 lightPos,
  float4 diffuseColor, float4 ambientColor, float3 att)
{
    // Convert point to directional
    float3 lightDir = lightPos - worldPos;
    float lightDist = length(lightDir);
    float attenuation = 1 / (att.x + att.y * lightDist + att.z * lightDist * lightDist);

    // Simulate point-lighting by using pixel-wise directional-lighting
    return calcDirLight(normal, normalize(lightDir), diffuseColor, ambientColor) * attenuation;
}
#endif


//---------------- Vertex shaders ----------------

#if LIGHTING
outLight VS_Light(inLight IN)
{
    outLight OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);
    OUT.depth = OUT.pos.zw;
    OUT.worldPos = transWorld(IN.entityPos);
    OUT.normal = transNorm(IN.normal);

    OUT.texWeights1 = IN.texWeights1;
    OUT.texWeights2 = IN.texWeights2;
    OUT.texWeights3 = IN.texWeights3;
    OUT.texWeights4 = IN.texWeights4;

    OUT.texCoord = IN.texCoord;
    OUT.color = IN.color;
    OUT.occlusionIntervals = IN.occlusionIntervals;

    return OUT;
}
#endif

#if !LIGHTING
outSimple VS_Simple(inSimple IN)
{
    outSimple OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);
    OUT.fogDepth = OUT.pos.w;

    OUT.texWeights1 = IN.texWeights1;
    OUT.texWeights2 = IN.texWeights2;
    OUT.texWeights3 = IN.texWeights3;
    OUT.texWeights4 = IN.texWeights4;

    OUT.texCoord = IN.texCoord;

    return OUT;
}
#endif

// Position-only; works for both the lit and unlit vertex layouts since neither is actually referenced beyond entityPos
outBlack VS_Black(inSimple IN)
{
    outBlack OUT;

    OUT.pos = transProj(IN.entityPos);
    OUT.fogDepth = OUT.pos.w;

    return OUT;
}

// Position-only; works for both the lit and unlit vertex layouts since neither is actually referenced beyond entityPos
outDepth VS_Depth(inSimple IN)
{
    outDepth OUT;

    OUT.pos = transProj(IN.entityPos);

    float dist = length(transWorld(IN.entityPos) - cameraPosition);
    OUT.depth = saturate((dist - nearClip) / (farClip - nearClip));

    return OUT;
}


//---------------- Pixel shaders ----------------

// Adds the weighted color of texture i (counting from 1); compiled out if the texture is not enabled in TEXTURE_MASK
#define BLEND_TEXTURE(i) if (textureEnabled(i - 1)) color += tex2D(texture##i##Sampler, texCoord) * texWeights[(i - 1) / 4][(i - 1) % 4];

// Blends the textures enabled in TEXTURE_MASK
float4 PS_Helper(float2 texCoord, float4 texWeights1, float4 texWeights2, float4 texWeights3, float4 texWeights4)
{
    float4 texWeights[4] = {texWeights1, texWeights2, texWeights3, texWeights4};

    float4 color = 0;
    BLEND_TEXTURE(1)  BLEND_TEXTURE(2)  BLEND_TEXTURE(3)  BLEND_TEXTURE(4)
    BLEND_TEXTURE(5)  BLEND_TEXTURE(6)  BLEND_TEXTURE(7)  BLEND_TEXTURE(8)
    BLEND_TEXTURE(9)  BLEND_TEXTURE(10) BLEND_TEXTURE(11) BLEND_TEXTURE(12)
    BLEND_TEXTURE(13) BLEND_TEXTURE(14) BLEND_TEXTURE(15) BLEND_TEXTURE(16)
    return color;
}

#if LIGHTING
// Samples every texture twice, at a coarse scale for high camera distances and at a fine scale for low camera distances
float4 PS_HelperDetail(float zDepth, float2 texCoord, float4 texWeights1, float4 texWeights2, float4 texWeights3, float4 texWeights4)
{
    float4 farColor = PS_Helper(texCoord * 0.8, texWeights1, texWeights2, texWeights3, texWeights4);
    float4 nearColor = PS_Helper(texCoord * 3.9, texWeights1, texWeights2, texWeights3, texWeights4);

    // Calculate distance blend factor
    float blendFactor = clamp((zDepth-BlendDistance)/BlendWidth, 0.05, 0.95);
    return farColor*blendFactor + nearColor*(1-blendFactor);
}

float4 PS_Light(outLight IN, uniform bool detail, uniform bool firstPass,  // Overload for ambient-only light
  uniform float4 ambCol) : COLOR
{
    float4 texColor = detail
        ? PS_HelperDetail(IN.depth.x, IN.texCoord, IN.texWeights1, IN.texWeights2, IN.texWeights3, IN.texWeights4)
        : PS_Helper(IN.texCoord, IN.texWeights1, IN.texWeights2, IN.texWeights3, IN.texWeights4);
    return applyFog(texColor * ambCol * IN.color, IN.depth.y, firstPass);
}

float4 PS_Light(outLight IN, uniform bool detail, uniform bool firstPass,  // Overload for two directional lights
  uniform float3 lightDir1, uniform float3 lightDir2,
  uniform float4 diffCol1, uniform float4 diffCol2,
  uniform float4 ambCol1, uniform float4 ambCol2) : COLOR
{
    float4 diffAmbColor = calcTwoDirLights(IN.normal,
      lightDir1, lightDir2, diffCol1, diffCol2, ambCol1, ambCol2, IN.occlusionIntervals, IN.occlusionIntervals);
    float4 texColor = detail
        ? PS_HelperDetail(IN.depth.x, IN.texCoord, IN.texWeights1, IN.texWeights2, IN.texWeights3, IN.texWeights4)
        : PS_Helper(IN.texCoord, IN.texWeights1, IN.texWeights2, IN.texWeights3, IN.texWeights4);
    return applyFog(texColor * diffAmbColor * IN.color, IN.depth.y, firstPass);
}

float4 PS_Light(outLight IN, uniform bool detail, uniform bool firstPass,  // Overload for one directional light
  uniform float3 lightDir, uniform float4 diffCol, uniform float4 ambCol) : COLOR
{
    float4 diffAmbColor = calcDirLight(IN.normal, lightDir, diffCol, ambCol, IN.occlusionIntervals);
    float4 texColor = detail
        ? PS_HelperDetail(IN.depth.x, IN.texCoord, IN.texWeights1, IN.texWeights2, IN.texWeights3, IN.texWeights4)
        : PS_Helper(IN.texCoord, IN.texWeights1, IN.texWeights2, IN.texWeights3, IN.texWeights4);
    return applyFog(texColor * diffAmbColor * IN.color, IN.depth.y, firstPass);
}

float4 PS_Light(outLight IN, uniform bool detail, uniform bool firstPass,  // Overload for one point light
  uniform float3 lightPos, uniform float4 diffCol, uniform float4 ambCol, uniform float3 att) : COLOR
{
    float4 diffAmbColor = calcPointLight(IN.worldPos, IN.normal, lightPos, diffCol, ambCol, att);
    float4 texColor = detail
        ? PS_HelperDetail(IN.depth.x, IN.texCoord, IN.texWeights1, IN.texWeights2, IN.texWeights3, IN.texWeights4)
        : PS_Helper(IN.texCoord, IN.texWeights1, IN.texWeights2, IN.texWeights3, IN.texWeights4);
    return applyFog(texColor * diffAmbColor * IN.color, IN.depth.y, firstPass);
}
#endif

#if !LIGHTING
float4 PS_Simple(outSimple IN) : COLOR
{
    return applyFog(PS_Helper(IN.texCoord, IN.texWeights1, IN.texWeights2, IN.texWeights3, IN.texWeights4), IN.fogDepth, /*firstPass*/true);
}
#endif

float4 PS_Black(outBlack IN) : COLOR
{ return applyFog(float4(0, 0, 0, 1), IN.fogDepth, /*firstPass*/true); }

float4 PS_Depth(outDepth IN) : COLOR
{ return float4(IN.depth, IN.depth, IN.depth, 1); }


//---------------- Techniques ----------------

#define ADDITIVE_STATES ZWriteEnable = false; ZFunc = LessEqual; CullMode = None; AlphaBlendEnable = true; SrcBlend = One; DestBlend = One;

#if !LIGHTING
technique Simple
{
  pass NoLights
  {
    VertexShader = compile vs_3_0 VS_Simple();
    PixelShader = compile ps_3_0 PS_Simple();
  }
}
#endif

#if LIGHTING
#define LIGHT_PASSES(detail) \
  pass AmbientLight \
  { \
    VertexShader = compile vs_3_0 VS_Light(); \
    PixelShader = compile ps_3_0 PS_Light(detail, /*firstPass*/true, ambientColor1); \
  } \
  pass TwoDirLights \
  { \
    VertexShader = compile vs_3_0 VS_Light(); \
    PixelShader = compile ps_3_0 PS_Light(detail, /*firstPass*/true, -lightDirection1.xyz, -lightDirection2.xyz, diffuseColor1, diffuseColor2, ambientColor1, ambientColor2); \
  } \
  pass TwoDirLightsAdd \
  { \
    ADDITIVE_STATES \
    VertexShader = compile vs_3_0 VS_Light(); \
    PixelShader = compile ps_3_0 PS_Light(detail, /*firstPass*/false, -lightDirection1.xyz, -lightDirection2.xyz, diffuseColor1, diffuseColor2, ambientColor1, ambientColor2); \
  } \
  pass OneDirLight \
  { \
    VertexShader = compile vs_3_0 VS_Light(); \
    PixelShader = compile ps_3_0 PS_Light(detail, /*firstPass*/true, -lightDirection1.xyz, diffuseColor1, ambientColor1); \
  } \
  pass OneDirLightAdd \
  { \
    ADDITIVE_STATES \
    VertexShader = compile vs_3_0 VS_Light(); \
    PixelShader = compile ps_3_0 PS_Light(detail, /*firstPass*/false, -lightDirection1.xyz, diffuseColor1, ambientColor1); \
  } \
  pass OnePointLight \
  { \
    VertexShader = compile vs_3_0 VS_Light(); \
    PixelShader = compile ps_3_0 PS_Light(detail, /*firstPass*/true, lightPosition1.xyz, diffuseColor1, ambientColor1, attenuation1); \
  } \
  pass OnePointLightAdd \
  { \
    ADDITIVE_STATES \
    VertexShader = compile vs_3_0 VS_Light(); \
    PixelShader = compile ps_3_0 PS_Light(detail, /*firstPass*/false, lightPosition1.xyz, diffuseColor1, ambientColor1, attenuation1); \
  }

technique Light
{
  LIGHT_PASSES(/*detail*/false)
}

technique LightDetail
{
  LIGHT_PASSES(/*detail*/true)
}
#endif

technique Black
{
  pass Black
  {
    VertexShader = compile vs_3_0 VS_Black();
    PixelShader = compile ps_3_0 PS_Black();
  }
}

technique Depth
{
  pass Depth
  {
    VertexShader = compile vs_3_0 VS_Depth();
    PixelShader = compile ps_3_0 PS_Depth();
  }
}
