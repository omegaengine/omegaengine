// Description: A general surface shader with optional normal and specular maps
//
// Techniques:
// - ColoredPerVertex (ps_1_1, plain color, per-vertex lighting)
// - ColoredPerPixel (ps_2_0, plain color, per-pixel lighting)
// - ColoredEmissiveOnly (ps_1_1, plain color, no lighting)
// - TexturedPerVertex (ps_1_1, textured, per-vertex lighting)
// - TexturedPerPixel (ps_2_0, textured, per-pixel lighting)
// - TexturedPerPixelNormalMap (ps_2_0, textured, per-pixel lighting, normal map)
// - TexturedPerPixelSpecularMap (ps_2_0, textured, per-pixel lighting, specular map)
// - TexturedPerPixelNormalSpecularMap (ps_2_0, textured, per-pixel lighting, normal map + specular map)
// - TexturedPerPixelEmissiveMap (ps_2_0, textured, per-pixel lighting, emissive map)
// - TexturedPerPixelNormalEmissiveMap (ps_2_0, textured, per-pixel lighting, normal map + emissive map)
// - TexturedPerPixelNormalSpecularEmissiveMap (ps_2_0, textured, per-pixel lighting, normal map + specular map + emissive map)
// - TexturedEmissiveOnly (ps_1_1, textured, no lighting)
// - TexturedEmissiveMapOnly (ps_2_0, textured, no lighting, emissive map)
//
// Passes:
// - AmbientLight (Light1 must be an ambient-only light, must be calld as first pass)
// - TwoDirLights (Light1 and Light2 must be directional lights, must be calld as first pass)
// - TwoDirLightsAdd (Light1 and Light2 must be directional lights, must not be calld as first pass)
// - OneDirLight (Light1 must be a directional light, must be calld as first pass)
// - OneDirLightAdd (Light1 must be a directional light, must not be calld as first pass)
// - OnePointLight (Light1 must be a point light, must be calld as first pass)
// - OnePointLightAdd (Light1 must be a point light, must not be calld as first pass)

//---------------- Parameters ----------------

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
> = 0.8;

// Camera
float4x4 world                 : World;
float4x4 worldViewProjection   : WorldViewProjection;
float4x4 worldInverseTranspose : WorldInverseTranspose;
float4x4 viewInverse           : ViewInverse;

// Lights
float specularPower    : SpecularPower < string UIWidget = "slider"; float UIMin = 1.0; float UIMax = 128.0; float UIStep = 1.0; > = 30.0f;
float4 emissiveColor   : Emissive = {0.0f, 0.0f, 0.0f, 1.0f};

// Light 1
float4 lightDirection1 : Direction < string Object = "Light1"; string Space = "World"; >;
float4 lightPosition1  : Position < string Object = "Light1"; string Space = "World"; >;
float3 attenuation1    : Attenuation < string Object = "Light1"; > = {1.0f, 0.0f, 0.0f};
float4 diffuseColor1   : Diffuse < string Object = "Light1"; > = {1.0f, 1.0f, 1.0f, 1.0f};
float4 specularColor1  : Specular < string Object = "Light1"; > = {1.0f, 1.0f, 1.0f, 1.0f};
float4 ambientColor1   : Ambient = {0.1f, 0.1f, 0.1f, 1.0f};

// Light 2
float4 lightDirection2 : Direction < string Object = "Light2"; string Space = "World"; >;
//float4 lightPosition2  : Position < string Object = "Light2"; string Space = "World"; >;
float3 attenuation2    : Attenuation < string Object = "Light2"; > = {1.0f, 0.0f, 0.0f};
float4 diffuseColor2   : Diffuse < string Object = "Light2"; > = {0.0f, 0.0f, 0.0f, 1.0f};
float4 specularColor2  : Specular < string Object = "Light2"; > = {0.0f, 0.0f, 0.0f, 1.0f};
float4 ambientColor2   : Ambient = {0.0f, 0.0f, 0.0f, 1.0f};


//---------------- Texture samplers ----------------

int FilterMode : FILTERMODE = 2; // 2 = Linear, 3 = Anisotropic

texture DiffuseTexture : Diffuse < string ResourceName = "default_color.dds"; >;
sampler2D diffuseSampler : register(s0) = sampler_state
{
  texture = <DiffuseTexture>;
  MinFilter = <FilterMode>; MagFilter = <FilterMode>; MipFilter = linear; MipFilter = linear;
};

texture NormalTexture : Normal < string ResourceName = "default_bump_normal.dds"; >;
sampler2D normalSampler : register(s1) = sampler_state
{
  texture = <NormalTexture>;
  MinFilter = <FilterMode>; MagFilter = <FilterMode>; MipFilter = linear;
};

texture SpecularTexture : Specular < string ResourceName = "default_gloss.dds"; >;
sampler2D specularSampler : register(s2) = sampler_state
{
  texture = <SpecularTexture>;
  MinFilter = <FilterMode>; MagFilter = <FilterMode>; MipFilter = linear;
};

texture EmissiveTexture : Emissive;
sampler2D emissiveSampler : register(s3) = sampler_state
{
  texture = <EmissiveTexture>;
  MinFilter = <FilterMode>; MagFilter = <FilterMode>; MipFilter = linear;
};


//---------------- Structs ----------------

struct lightComponents {
  float4 diffuseAmbient;
  float4 specular;
};

struct inTextured {
  float3 entityPos : POSITION;
  float2 texCoord  : texCoord0;
  float3 normal    : NORMAL;
  float3 binormal  : BINORMAL;
  float3 tangent   : TANGENT;
};

struct inColored {
  float3 entityPos : POSITION;
  float3 normal    : NORMAL;
  float3 binormal  : BINORMAL;
  float3 tangent   : TANGENT;
};

struct outTexturedPerPixel {
  float4 pos      : POSITION;
  float3 position : texCoord0;
  float3 worldPos : texCoord1;
  float3 normal   : texCoord2;
  float3 binormal : texCoord3;
  float3 tangent  : texCoord4;
  float2 texCoord : texCoord5;
};

struct outTexturedAmbient {
  float4 pos      : POSITION;
  float2 texCoord : texCoord0;
  float4 ambColor : COLOR0;
};

struct outTexturedPerVertex {
  float4 pos          : POSITION;
  float2 texCoord     : texCoord0;
  float4 diffAmbColor : COLOR0;
  float4 specCol      : COLOR1;
};

struct outColoredPerPixel {
  float4 pos      : POSITION;
  float3 position : texCoord0;
  float3 worldPos : texCoord1;
  float3 normal   : texCoord2;
  float3 binormal : texCoord3;
  float3 tangent  : texCoord4;
};

struct outColoredPerVertex {
  float4 pos        : POSITION;
  float4 finalColor : COLOR0;
};


//---------------- Helper functions ----------------

// Translate to world space
float3 transWorld(float3 position)
{ return mul(float4(position, 1.0), world).xyz; }

// Translate to projection space
float4 transProj(float3 position)
{ return mul(float4(position, 1.0), worldViewProjection); }

// Translate to world space and normalize
float3 transNorm(float3 normal)
{ normal = mul(float4(normal, 0), worldInverseTranspose).xyz; return normalize(normal); }

// Apply the normal map to the normal vector
float3 ApplyNormalMap(float2 texCoord, float3 normal, float3 binormal, float3 tangent)
{
    // Map 0 to -2, 0.5 to 0 and 1 to 2
    float3 mapNormal = 4 * (tex2D(normalSampler, texCoord).rgb - 0.5);
    normal = normal + mapNormal.x*tangent + mapNormal.y*binormal;
    return normalize(normal);
}

// Read the specular map
float4 ReadSpecMap(float2 texCoord)
{ return tex2D(specularSampler, texCoord); }

// Read the emissive map
float4 ReadEmissiveMap(float2 texCoord)
{ return tex2D(emissiveSampler, texCoord); }

// Calculate the lighting components for one directional light
lightComponents CalcDirLight(float3 transPos, float3 normal, float3 lightDir,
  float4 diffuseColor, float4 specularColor, float4 ambientColor)
{
    // Renormalize to prevent anti-aliasing causing glitches
    normal = normalize(normal);

    // Calculate additional vectors
    float3 eye = normalize(viewInverse[3].xyz - transPos);
    float3 halfAngle = normalize(eye + lightDir);

    // Calculate the diffuse and specular contributions
    lightComponents OUT;
    float4 litV = lit(dot(normal, lightDir), dot(halfAngle, normal), specularPower);
    OUT.diffuseAmbient = litV.x * ambientColor + litV.y * diffuseColor;
    OUT.specular = litV.z * specularColor;

    return OUT;
}

// Calculate the lighting components for two directional lights
lightComponents CalcTwoDirLights(float3 transPos, float3 normal,
  float3 lightDir1, float3 lightDir2,
  float4 diffCol1, float4 diffCol2, float4 specCol1, float4 specCol2,
  float4 ambCol1, float4 ambCol2)
{
    // Calculate separate light values
    lightComponents light1 = CalcDirLight(transPos, normal, lightDir1, diffCol1, specCol1, ambCol1);
    lightComponents light2 = CalcDirLight(transPos, normal, lightDir2, diffCol2, specCol2, ambCol2);

    // Add lights together
    lightComponents OUT;
    OUT.diffuseAmbient = light1.diffuseAmbient + light2.diffuseAmbient;
    OUT.specular = light1.specular + light2.specular;
    return OUT;
}

// Calculate the lighting components for one point light
lightComponents CalcPointLight(float3 worldPos, float3 transPos, float3 normal, float3 lightPos,
  float4 diffuseColor, float4 specularColor, float4 ambientColor, float3 att)
{
    // Convert point to directional
    float3 lightDir = lightPos - worldPos;
    float lightDist = length(lightDir);
    float attenuation = 1 / (att.x + att.y * lightDist + att.z * lightDist * lightDist);

    // Simulate point-lighting by using pixel-wise directional-lighting
    lightComponents OUT = CalcDirLight(transPos, normal, normalize(lightDir), diffuseColor, specularColor, ambientColor);
    OUT.diffuseAmbient *= attenuation;
    OUT.specular *= attenuation;

    return OUT;
}


//---------------- Vertex shaders ----------------

outTexturedPerPixel VS_TexturedPerPixel(inTextured IN) 
{
    // Transform data into world space
    outTexturedPerPixel OUT;
    OUT.pos = transProj(IN.entityPos); OUT.position = OUT.pos.xyz;
    OUT.worldPos = transWorld(IN.entityPos);
    OUT.normal = transNorm(IN.normal);
    OUT.binormal = transNorm(IN.binormal);
    OUT.tangent = transNorm(IN.tangent);

    OUT.texCoord = IN.texCoord;

    return OUT;
}

outTexturedAmbient VS_TexturedAmbient(inTextured IN, uniform float4 ambCol) 
{
    // Transform data into world space
    outTexturedAmbient OUT;
    OUT.pos = transProj(IN.entityPos);
    OUT.texCoord = IN.texCoord;

    // Set lighting values
    OUT.ambColor = ambCol + emissiveColor;

    return OUT;
}

outTexturedPerVertex VS_TexturedPerVertex(inTextured IN,  // Overload for two directional lights
  uniform bool firstPass,
  uniform float3 lightDir1, uniform float3 lightDir2,
  uniform float4 diffCol1, uniform float4 diffCol2, uniform float4 specCol1, uniform float4 specCol2,
  uniform float4 ambCol1, uniform float4 ambCol2) 
{
    // Transform data into world space
    outTexturedPerVertex OUT;
    OUT.pos = transProj(IN.entityPos);
    OUT.texCoord = IN.texCoord;

    // Output diffuse-ambient & specular colors
    lightComponents components = CalcTwoDirLights(OUT.pos, transNorm(IN.normal),
      lightDir1, lightDir2, diffCol1, diffCol2, specCol1, specCol2, ambCol1, ambCol2);
    OUT.diffAmbColor = components.diffuseAmbient;
    if (firstPass) OUT.diffAmbColor += emissiveColor; // Add emisive light only once (since it's technically not related to a specific light source)
    OUT.specCol = components.specular;

    return OUT;
}

outTexturedPerVertex VS_TexturedPerVertex(inTextured IN,  // Overload for one directional light
  uniform bool firstPass, uniform float3 lightDir, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol) 
{
    // Transform data into world space
    outTexturedPerVertex OUT;
    OUT.pos = transProj(IN.entityPos);
    OUT.texCoord = IN.texCoord;

    // Output diffuse-ambient & specular colors
    lightComponents components = CalcDirLight(OUT.pos, transNorm(IN.normal), lightDir, diffCol, specCol, ambCol);
    OUT.diffAmbColor = components.diffuseAmbient;
    if (firstPass) OUT.diffAmbColor += emissiveColor; // Add emisive light only once (since it's technically not related to a specific light source)
    OUT.specCol = components.specular;

    return OUT;
}

outTexturedPerVertex VS_TexturedPerVertex(inTextured IN,  // Overload for one point light
  uniform bool firstPass, uniform float3 lightPos, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol, uniform float3 att) 
{
    // Transform data into world space
    outTexturedPerVertex OUT;
    OUT.pos = transProj(IN.entityPos);
    OUT.texCoord = IN.texCoord;

    // Output diffuse-ambient & specular colors
    lightComponents components = CalcPointLight(transWorld(IN.entityPos), OUT.pos, transNorm(IN.normal), lightPos, diffCol, specCol, ambCol, att);
    OUT.diffAmbColor = components.diffuseAmbient;
    if (firstPass) OUT.diffAmbColor += emissiveColor; // Add emisive light only once (since it's technically not related to a specific light source)
    OUT.specCol = components.specular;

    return OUT;
}

outColoredPerPixel VS_ColoredPerPixel(inTextured IN) 
{
    // Transform data into world space
    outColoredPerPixel OUT;
    OUT.pos = transProj(IN.entityPos); OUT.position = OUT.pos.xyz;
    OUT.worldPos = transWorld(IN.entityPos);
    OUT.normal = transNorm(IN.normal);
    OUT.binormal = transNorm(IN.binormal);
    OUT.tangent = transNorm(IN.tangent);

    return OUT;
}

outColoredPerVertex VS_ColoredAmbient(inColored IN, uniform float4 ambCol) 
{
    // Transform data into world space
    outColoredPerVertex OUT;
    OUT.pos = transProj(IN.entityPos);

    // Set lighting values
    OUT.finalColor = ambCol;

    return OUT;
}

outColoredPerVertex VS_ColoredPerVertex(inColored IN,  // Overload for two directional lights
  uniform bool firstPass, uniform float3 lightDir1, uniform float3 lightDir2,
  uniform float4 diffCol1, uniform float4 diffCol2, uniform float4 specCol1, uniform float4 specCol2,
  uniform float4 ambCol1, uniform float4 ambCol2) 
{
    // Transform data into world space
    outColoredPerVertex OUT;
    OUT.pos = transProj(IN.entityPos);

    // Output final color
    lightComponents components = CalcTwoDirLights(OUT.pos, transNorm(IN.normal),
      lightDir1, lightDir2, diffCol1, diffCol2, specCol1, specCol2, ambCol1, ambCol2);
    OUT.finalColor = components.diffuseAmbient + components.specular;
    if (firstPass) OUT.finalColor += emissiveColor; // Add emisive light only once (since it's technically not related to a specific light source)

    return OUT;
}

outColoredPerVertex VS_ColoredPerVertex(inColored IN,  // Overload for one directional light
  uniform bool firstPass, uniform float3 lightDir, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol)
{
    // Transform data into world space
    outColoredPerVertex OUT;
    OUT.pos = transProj(IN.entityPos);

    // Output final color
    lightComponents components = CalcDirLight(OUT.pos, transNorm(IN.normal), lightDir, diffCol, specCol, ambCol);
    OUT.finalColor = components.diffuseAmbient + components.specular;
    if (firstPass) OUT.finalColor += emissiveColor; // Add emisive light only once (since it's technically not related to a specific light source)

    return OUT;
}

outColoredPerVertex VS_ColoredPerVertex(inColored IN,  // Overload for one point light
  uniform bool firstPass, uniform float3 lightPos, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol, uniform float3 att)
{
    // Transform data into world space
    outColoredPerVertex OUT;
    OUT.pos = transProj(IN.entityPos);

    // Output final color
    lightComponents components = CalcPointLight(transWorld(IN.entityPos), OUT.pos, transNorm(IN.normal), lightPos, diffCol, specCol, ambCol, att);
    OUT.finalColor = components.diffuseAmbient + components.specular;
    if (firstPass) OUT.finalColor += emissiveColor; // Add emisive light only once (since it's technically not related to a specific light source)

    return OUT;
}


//---------------- Pixel shaders ----------------

float4 PS_TexturedPerVertex(outTexturedPerVertex IN, uniform bool useEmissiveMap, uniform bool firstPass) : COLOR
{
    // Sample diffuse texture
    float4 diffuseMap;
    diffuseMap = tex2D(diffuseSampler, IN.texCoord);
    if (useEmissiveMap) diffuseMap.rgb = ReadEmissiveMap(IN.texCoord).rgb; // Keep original alpha-channel

    // Output final color
    float3 diffCol = IN.diffAmbColor.rgb * diffuseMap.rgb;
    // Bake in the specular color...
    if (firstPass) return float4(diffCol + IN.specCol.rgb, diffuseMap.a); // ... and pass the alpha map through on the first pass
    else return float4(diffCol * diffuseMap.a + IN.specCol.rgb, 1); // ... and bake in the alpha map for all additional passes since they use additive blending
}

float4 PS_TexturedPerPixel(outTexturedPerPixel IN,  // Overload for two directional lights
  uniform bool useNormalMap, uniform bool useSpecularMap, uniform bool useEmissiveMap, uniform bool firstPass,
  uniform float3 lightDir1, uniform float3 lightDir2,
  uniform float4 diffCol1, uniform float4 diffCol2, uniform float4 specCol1, uniform float4 specCol2,
  uniform float4 ambCol1, uniform float4 ambCol2) : COLOR
{
    // Sample diffuse texture
    float4 diffuseMap = tex2D(diffuseSampler, IN.texCoord);

    // Determine effective normal vector
    float3 normal = useNormalMap ? ApplyNormalMap(IN.texCoord, IN.normal, IN.binormal, IN.tangent) : IN.normal;

    // Determine effective specular factor
    float4 specMap = useSpecularMap ? ReadSpecMap(IN.texCoord) : 1;

    // Calculate diffuse-ambient & specular colors
    lightComponents components = CalcTwoDirLights(IN.worldPos, normal,
      lightDir1, lightDir2, diffCol1, diffCol2, specCol1 * specMap, specCol2 * specMap, ambCol1, ambCol2);
    if (firstPass) // Add emisive light only once (since it's technically not related to a specific light source)
        components.diffuseAmbient += useEmissiveMap ? (emissiveColor * ReadEmissiveMap(IN.texCoord)) : emissiveColor;

    // Output final color
    float3 diffAmbCol = components.diffuseAmbient.rgb * diffuseMap.rgb;
    // Bake in the specular color...
    if (firstPass) return float4(diffAmbCol + components.specular.rgb, diffuseMap.a); // ... and pass the alpha map through on the first pass
    else return float4(diffAmbCol * diffuseMap.a + components.specular.rgb, 1); // ... and bake in the alpha map for all additional passes since they use additive blending
}

float4 PS_TexturedPerPixel(outTexturedPerPixel IN,  // Overload for one directional or point light
  uniform bool useNormalMap, uniform bool useSpecularMap, uniform bool useEmissiveMap, uniform bool firstPass, uniform bool pointLight,
  uniform float3 lightDirPos, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol, uniform float3 att) : COLOR
{
    // Sample diffuse texture
    float4 diffuseMap = tex2D(diffuseSampler, IN.texCoord);

    // Determine effective normal vector
    float3 normal = useNormalMap ? ApplyNormalMap(IN.texCoord, IN.normal, IN.binormal, IN.tangent) : IN.normal;

    // Determine effective specular factor
    float4 specMap = useSpecularMap ? ReadSpecMap(IN.texCoord) : 1;

    // Calculate diffuse-ambient & specular colors
    lightComponents components;
    if (pointLight) components = CalcPointLight(IN.worldPos, IN.position, normal, lightDirPos, diffCol, specCol * specMap, ambCol, att);
    else components = CalcDirLight(IN.worldPos, normal, lightDirPos, diffCol, specCol * specMap, ambCol);
    if (firstPass) // Add emisive light only once (since it's technically not related to a specific light source)
        components.diffuseAmbient += useEmissiveMap ? (emissiveColor * ReadEmissiveMap(IN.texCoord)) : emissiveColor;

    // Output final color
    float3 diffAmbCol = components.diffuseAmbient.rgb * diffuseMap.rgb;
    // Bake in the specular color...
    if (firstPass) return float4(diffAmbCol + components.specular.rgb, diffuseMap.a); // ... and pass the alpha map through on the first pass
    else return float4(diffAmbCol * diffuseMap.a + components.specular.rgb, 1);; // ... and bake in the alpha map for all additional passes since they use additive blending
}

float4 PS_ColoredPerPixel(outColoredPerPixel IN,  // Overload for two directional lights
  uniform bool firstPass,
  uniform float3 lightDir1, uniform float3 lightDir2,
  uniform float4 diffCol1, uniform float4 diffCol2, uniform float4 specCol1, uniform float4 specCol2,
  uniform float4 ambCol1, uniform float4 ambCol2) : COLOR
{
    // Calculate diffuse-ambient & specular colors
    lightComponents components = CalcTwoDirLights(IN.worldPos, IN.normal,
      lightDir1, lightDir2, diffCol1, diffCol2, specCol1, specCol2, ambCol1, ambCol2);

    // Output final color
    return components.diffuseAmbient + components.specular;
}

float4 PS_ColoredPerPixel(outColoredPerPixel IN,  // Overload for one directional light
  uniform bool firstPass, uniform float3 lightDir, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol) : COLOR
{
    // Calculate diffuse-ambient & specular colors
    lightComponents components = CalcDirLight(IN.worldPos, IN.normal, lightDir, diffCol, specCol, ambCol);

    // Output final color
    return components.diffuseAmbient + components.specular;
}

float4 PS_ColoredPerPixel(outColoredPerPixel IN,  // Overload for one point light
  uniform bool firstPass, uniform float3 lightPos, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol, uniform float3 att) : COLOR
{
    // Calculate diffuse-ambient & specular colors
    lightComponents components = CalcPointLight(IN.worldPos, IN.position, IN.normal, lightPos, diffCol, specCol, ambCol, att);

    // Output final color
    return components.diffuseAmbient + components.specular;
}


//---------------- Techniques ----------------

#include "General.Techniques.fxh"

technique FXComposerTest {
  pass OneDirLight {
    VertexShader = compile vs_1_1 VS_TexturedPerPixel();
    PixelShader = compile ps_2_0 PS_TexturedPerPixel(true/*useNormalMap*/, false/*useSpecularMap*/, false/*useGlowMap*/, /*firstPass*/true,
    false/*pointLight*/, -lightDirection1, OneLightColor, attenuation1);
  }
}

technique ColoredPerVertex {
  ColoredPerVertexMacro
}

technique ColoredPerPixel {
  ColoredPerPixelMacro
}

technique ColoredEmissiveOnly {
  pass Emissive {
    VertexShader = compile vs_1_1 VS_ColoredAmbient(/*ambCol*/0);
    PixelShader = null;
  }
}

technique TexturedPerVertex {
  TexturedPerVertexMacro
}

technique TexturedPerPixel < string Script = " Pass=OnePointLight;"; > {
  TexturedPerPixelMacro(false, false, false)
}

technique TexturedPerPixelNormalMap < string Script = " Pass=OnePointLight;"; > {
  TexturedPerPixelMacro(true, false, false)
}

technique TexturedPerPixelSpecularMap < string Script = " Pass=OnePointLight;"; > {
  TexturedPerPixelMacro(false, true, false)
}

technique TexturedPerPixelNormalSpecularMap < string Script = " Pass=OnePointLight;"; > {
  TexturedPerPixelMacro(true, true, false)
}

technique TexturedPerPixelEmissiveMap < string Script = " Pass=OnePointLight;"; > {
  TexturedPerPixelMacro(false, false, true)
}

technique TexturedPerPixelNormalEmissiveMap < string Script = " Pass=OnePointLight;"; > {
  TexturedPerPixelMacro(true, false, true)
}

technique TexturedPerPixelNormalSpecularEmissiveMap < string Script = " Pass=OnePointLight;"; > {
  TexturedPerPixelMacro(true, true, true)
}

technique TexturedEmissiveOnly {
  pass Emissive {
    VertexShader = compile vs_1_1 VS_TexturedAmbient(/*ambCol*/0);
    PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/true);
  }
}

technique TexturedEmissiveMapOnly {
  pass Emissive {
    VertexShader = compile vs_1_1 VS_TexturedAmbient(/*ambCol*/0);
    PixelShader = compile ps_2_0 PS_TexturedPerVertex(/*useEmissive*/true, /*firstPass*/true);
  }
}
