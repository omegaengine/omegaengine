// Description: A general-purpose surface shader.
//
// Techniques:
// - ColoredPerVertex (ps_1_1, plain color, per-vertex lighting)
// - Colored (ps_2_0, plain color, per-pixel lighting)
// - ColoredEmissiveOnly (ps_1_1, plain color, plain emissive lighting only)
// - TexturedPerVertex (ps_1_1, textured, per-vertex lighting)
// - Textured (ps_2_0, textured, per-pixel lighting)
// - TexturedNormalMap (ps_2_0, textured, per-pixel lighting, normal map)
// - TexturedSpecularMap (ps_2_0, textured, per-pixel lighting, specular map)
// - TexturedNormalSpecularMap (ps_2_0, textured, per-pixel lighting, normal map + specular map)
// - TexturedEmissiveMap (ps_2_0, textured, per-pixel lighting, emissive map)
// - TexturedNormalEmissiveMap (ps_2_0, textured, per-pixel lighting, normal map + emissive map)
// - TexturedNormalSpecularEmissiveMap (ps_2_0, textured, per-pixel lighting, normal map + specular map + emissive map)
// - TexturedEmissiveOnly (ps_1_1, textured, plain emissive lighting only)
// - TexturedEmissiveMapOnly (ps_2_0, textured, emissive map lighting only)
//
// Passes:
// - AmbientLight (Light1 must be an ambient-only light, must be called as first pass)
// - TwoDirLights (Light1 and Light2 must be directional lights, must be called as first pass)
// - TwoDirLightsAdd (Light1 and Light2 must be directional lights, additive, must not be called as first pass)
// - OneDirLight (Light1 must be a directional light, must be called as first pass)
// - OneDirLightAdd (Light1 must be a directional light, additive, must not be called as first pass)
// - OnePointLight (Light1 must be a point light, must be called as first pass)
// - OnePointLightAdd (Light1 must be a point light, additive, must not be called as first pass)

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

// General lighting
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
float3 attenuation2    : Attenuation < string Object = "Light2"; > = {1.0f, 0.0f, 0.0f};
float4 diffuseColor2   : Diffuse < string Object = "Light2"; > = {0.0f, 0.0f, 0.0f, 1.0f};
float4 specularColor2  : Specular < string Object = "Light2"; > = {0.0f, 0.0f, 0.0f, 1.0f};
float4 ambientColor2   : Ambient = {0.0f, 0.0f, 0.0f, 1.0f};


//---------------- Textures ----------------

int FilterMode : FILTERMODE = 2; // 2 = Linear, 3 = Anisotropic

texture DiffuseTexture : Diffuse < string ResourceName = "default_color.dds"; >;
sampler2D diffuseSampler : register(s0) = sampler_state
{
  texture = <DiffuseTexture>;
  MinFilter = <FilterMode>; MagFilter = <FilterMode>; MipFilter = linear;
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
  float4 diffuseAmbient; // Combined diffuse and ambient color
  float4 specular;       // Specular color
};

struct inTextured {
  float3 entityPos : POSITION;  // Position in object space
  float2 texCoord  : TEXCOORD0; // Texture coordinates
  float3 normal    : NORMAL;    // Normal vector in object space
  float3 binormal  : BINORMAL;  // Binormal vector in object space
  float3 tangent   : TANGENT;   // Tangent vector in object space
};

struct inColored {
  float3 entityPos : POSITION; // Position in object space
  float3 normal    : NORMAL;   // Normal vector in object space
  float3 binormal  : BINORMAL; // Binormal vector in object space
  float3 tangent   : TANGENT;  // Tangent vector in object space
};

struct outTextured {
  float4 pos      : POSITION;  // Position in clip space
  float3 position : TEXCOORD0; // Position in clip space without superfluous fourth dimension
  float3 worldPos : TEXCOORD1; // Position in world space
  float3 normal   : TEXCOORD2; // Normal vector in world space
  float3 binormal : TEXCOORD3; // Binormal vector in world space
  float3 tangent  : TEXCOORD4; // Tangent vector in world space
  float2 texCoord : TEXCOORD5; // Texture coordinates
};

struct outTexturedPerVertex {
  float4 pos          : POSITION;  // Position in clip space
  float2 texCoord     : TEXCOORD0; // Texture coordinates
  float4 diffAmbColor : COLOR0;    // Combined diffuse and ambient color
  float4 specCol      : COLOR1;    // Specular color
};

struct outColored {
  float4 pos      : POSITION;  // Position in clip space
  float3 position : TEXCOORD0; // Position in clip space without superfluous fourth dimension
  float3 worldPos : TEXCOORD1; // Position in world space
  float3 normal   : TEXCOORD2; // Normal vector in world space
  float3 binormal : TEXCOORD3; // Binormal vector in world space
  float3 tangent  : TEXCOORD4; // Tangent vector in world space
};

struct outColoredPerVertex {
  float4 pos        : POSITION; // Position in clip space
  float4 finalColor : COLOR0;   // The effective color including lighting
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

// Apply the normal map to the normal vector
float3 applyNormalMap(float2 texCoord, float3 normal, float3 binormal, float3 tangent)
{
    // Map 0 to -2, 0.5 to 0 and 1 to 2
    float3 mapNormal = 4 * (tex2D(normalSampler, texCoord).rgb - 0.5);
    return normalize(normal + mapNormal.x*tangent + mapNormal.y*binormal);
}

// Read the specular map
float4 readSpecMap(float2 texCoord)
{ return tex2D(specularSampler, texCoord); }

// Read the emissive map
float3 readEmissiveMap(float2 texCoord)
{ return tex2D(emissiveSampler, texCoord).rgb; }

// Calculate the lighting components for one directional light
lightComponents calcDirLight(float3 transPos, float3 normal, float3 lightDir,
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
lightComponents calcTwoDirLights(float3 transPos, float3 normal,
  float3 lightDir1, float3 lightDir2,
  float4 diffCol1, float4 diffCol2, float4 specCol1, float4 specCol2,
  float4 ambCol1, float4 ambCol2)
{
    // Calculate separate light values
    lightComponents light1 = calcDirLight(transPos, normal, lightDir1, diffCol1, specCol1, ambCol1);
    lightComponents light2 = calcDirLight(transPos, normal, lightDir2, diffCol2, specCol2, ambCol2);

    // Add lights together
    lightComponents OUT;
    OUT.diffuseAmbient = light1.diffuseAmbient + light2.diffuseAmbient;
    OUT.specular = light1.specular + light2.specular;
    return OUT;
}

// Calculate the lighting components for one point light
lightComponents calcPointLight(float3 worldPos, float3 transPos, float3 normal, float3 lightPos,
  float4 diffuseColor, float4 specularColor, float4 ambientColor, float3 att)
{
    // Convert point to directional
    float3 lightDir = lightPos - worldPos;
    float lightDist = length(lightDir);
    float attenuation = 1 / (att.x + att.y * lightDist + att.z * lightDist * lightDist);

    // Simulate point-lighting by using pixel-wise directional-lighting
    lightComponents OUT = calcDirLight(transPos, normal, normalize(lightDir), diffuseColor, specularColor, ambientColor);
    OUT.diffuseAmbient *= attenuation;
    OUT.specular *= attenuation;

    return OUT;
}


//---------------- Vertex shaders ----------------

outTextured VS_Textured(inTextured IN)
{
    outTextured OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);
    OUT.position = OUT.pos.xyz;
    OUT.worldPos = transWorld(IN.entityPos);
    OUT.normal = transNorm(IN.normal);
    OUT.binormal = transNorm(IN.binormal);
    OUT.tangent = transNorm(IN.tangent);

    OUT.texCoord = IN.texCoord;

    return OUT;
}

outTexturedPerVertex VS_TexturedAmbient(inTextured IN, uniform float4 ambCol)
{
    outTexturedPerVertex OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);
    OUT.texCoord = IN.texCoord;

    // Apply lighting
    OUT.diffAmbColor = ambCol + emissiveColor;
    OUT.specCol = 0;

    return OUT;
}

outTexturedPerVertex VS_TexturedPerVertexTwoDirLights(inTextured IN,
  uniform bool firstPass,
  uniform float3 lightDir1, uniform float3 lightDir2,
  uniform float4 diffCol1, uniform float4 diffCol2, uniform float4 specCol1, uniform float4 specCol2,
  uniform float4 ambCol1, uniform float4 ambCol2)
{
    outTexturedPerVertex OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);
    OUT.texCoord = IN.texCoord;

    // Apply lighting
    lightComponents components = calcTwoDirLights(OUT.pos, transNorm(IN.normal),
      lightDir1, lightDir2, diffCol1, diffCol2, specCol1, specCol2, ambCol1, ambCol2);
    OUT.diffAmbColor = components.diffuseAmbient;
    if (firstPass) OUT.diffAmbColor += emissiveColor; // Add emissive light only once (since it's technically not related to a specific light source)
    OUT.specCol = components.specular;

    return OUT;
}

outTexturedPerVertex VS_TexturedPerVertexOneDirLight(inTextured IN,
  uniform bool firstPass, uniform float3 lightDir, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol)
{
    outTexturedPerVertex OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);
    OUT.texCoord = IN.texCoord;

    // Apply lighting
    lightComponents components = calcDirLight(OUT.pos, transNorm(IN.normal), lightDir, diffCol, specCol, ambCol);
    OUT.diffAmbColor = components.diffuseAmbient;
    if (firstPass) OUT.diffAmbColor += emissiveColor; // Add emissive light only once (since it's technically not related to a specific light source)
    OUT.specCol = components.specular;

    return OUT;
}

outTexturedPerVertex VS_TexturedPerVertexOnePointLight(inTextured IN,
  uniform bool firstPass, uniform float3 lightPos, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol, uniform float3 att)
{
    outTexturedPerVertex OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);
    OUT.texCoord = IN.texCoord;

    // Apply lighting
    lightComponents components = calcPointLight(transWorld(IN.entityPos), OUT.pos, transNorm(IN.normal), lightPos, diffCol, specCol, ambCol, att);
    OUT.diffAmbColor = components.diffuseAmbient;
    if (firstPass) OUT.diffAmbColor += emissiveColor; // Add emissive light only once (since it's technically not related to a specific light source)
    OUT.specCol = components.specular;

    return OUT;
}

outColored VS_Colored(inColored IN)
{
    outColored OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);
    OUT.position = OUT.pos.xyz;
    OUT.worldPos = transWorld(IN.entityPos);
    OUT.normal = transNorm(IN.normal);
    OUT.binormal = transNorm(IN.binormal);
    OUT.tangent = transNorm(IN.tangent);

    return OUT;
}

outColoredPerVertex VS_ColoredAmbient(inColored IN, uniform float4 ambCol)
{
    outColoredPerVertex OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);

    OUT.finalColor = ambCol;

    return OUT;
}

outColoredPerVertex VS_ColoredPerVertexTwoDirLights(inColored IN,
  uniform bool firstPass, uniform float3 lightDir1, uniform float3 lightDir2,
  uniform float4 diffCol1, uniform float4 diffCol2, uniform float4 specCol1, uniform float4 specCol2,
  uniform float4 ambCol1, uniform float4 ambCol2)
{
    outColoredPerVertex OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);

    // Apply lighting
    lightComponents components = calcTwoDirLights(OUT.pos, transNorm(IN.normal), lightDir1, lightDir2, diffCol1, diffCol2, specCol1, specCol2, ambCol1, ambCol2);
    OUT.finalColor = components.diffuseAmbient + components.specular;
    if (firstPass) OUT.finalColor += emissiveColor; // Add emissive light only once (since it's technically not related to a specific light source)

    return OUT;
}

outColoredPerVertex VS_ColoredPerVertexOneDirLight(inColored IN,
  uniform bool firstPass, uniform float3 lightDir, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol)
{
    outColoredPerVertex OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);

    // Apply lighting
    lightComponents components = calcDirLight(OUT.pos, transNorm(IN.normal), lightDir, diffCol, specCol, ambCol);
    OUT.finalColor = components.diffuseAmbient + components.specular;
    if (firstPass) OUT.finalColor += emissiveColor; // Add emissive light only once (since it's technically not related to a specific light source)

    return OUT;
}

outColoredPerVertex VS_ColoredPerVertexOnePointLight(inColored IN,
  uniform bool firstPass, uniform float3 lightPos, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol, uniform float3 att)
{
    outColoredPerVertex OUT;

    // Apply transforms
    OUT.pos = transProj(IN.entityPos);

    // Apply lighting
    lightComponents components = calcPointLight(transWorld(IN.entityPos), OUT.pos, transNorm(IN.normal), lightPos, diffCol, specCol, ambCol, att);
    OUT.finalColor = components.diffuseAmbient + components.specular;
    if (firstPass) OUT.finalColor += emissiveColor; // Add emissive light only once (since it's technically not related to a specific light source)

    return OUT;
}


//---------------- Pixel shaders ----------------

float4 PS_TexturedPerVertex(outTexturedPerVertex IN, uniform bool useEmissiveMap, uniform bool firstPass) : COLOR
{
    float4 diffuseMap;
    diffuseMap = tex2D(diffuseSampler, IN.texCoord);
    diffuseMap.rgb += useEmissiveMap ? readEmissiveMap(IN.texCoord) : emissiveColor.rgb;

    float3 diffCol = IN.diffAmbColor.rgb * diffuseMap.rgb;

    // Bake in the specular color...
    if (firstPass) return float4(diffCol + IN.specCol.rgb, diffuseMap.a); // ... and pass the alpha-channel through on the first pass
    else return float4(diffCol * diffuseMap.a + IN.specCol.rgb, 1); // ... and bake in the alpha-channel for all additional passes since they use additive blending
}

float4 PS_TexturedTwoDirLights(outTextured IN,
  uniform bool useNormalMap, uniform bool useSpecularMap, uniform bool useEmissiveMap, uniform bool firstPass,
  uniform float3 lightDir1, uniform float3 lightDir2,
  uniform float4 diffCol1, uniform float4 diffCol2, uniform float4 specCol1, uniform float4 specCol2,
  uniform float4 ambCol1, uniform float4 ambCol2) : COLOR
{
    float4 diffuseMap = tex2D(diffuseSampler, IN.texCoord);
    float3 normal = useNormalMap ? applyNormalMap(IN.texCoord, IN.normal, IN.binormal, IN.tangent) : IN.normal;
    float4 specMap = useSpecularMap ? readSpecMap(IN.texCoord) : 1;

    // Apply lighting
    lightComponents components = calcTwoDirLights(IN.worldPos, normal, lightDir1, lightDir2, diffCol1, diffCol2, specCol1 * specMap, specCol2 * specMap, ambCol1, ambCol2);
    if (firstPass) // Add emissive light only once (since it's technically not related to a specific light source)
        components.diffuseAmbient.rgb += useEmissiveMap ? readEmissiveMap(IN.texCoord) : emissiveColor.rgb;
    float3 diffAmbCol = components.diffuseAmbient.rgb * diffuseMap.rgb;

    // Bake in the specular color...
    if (firstPass) return float4(diffAmbCol + components.specular.rgb, diffuseMap.a); // ... and pass the alpha-channel through on the first pass
    else return float4(diffAmbCol * diffuseMap.a + components.specular.rgb, 1); // ... and bake in the alpha-channel for all additional passes since they use additive blending
}

float4 PS_TexturedOneDirOrPointLight(outTextured IN,
  uniform bool useNormalMap, uniform bool useSpecularMap, uniform bool useEmissiveMap, uniform bool firstPass, uniform bool pointLight,
  uniform float3 lightDirPos, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol, uniform float3 att) : COLOR
{
    float4 diffuseMap = tex2D(diffuseSampler, IN.texCoord);
    float3 normal = useNormalMap ? applyNormalMap(IN.texCoord, IN.normal, IN.binormal, IN.tangent) : IN.normal;
    float4 specMap = useSpecularMap ? readSpecMap(IN.texCoord) : 1;

    // Apply lighting
    lightComponents components;
    if (pointLight) components = calcPointLight(IN.worldPos, IN.position, normal, lightDirPos, diffCol, specCol * specMap, ambCol, att);
    else components = calcDirLight(IN.worldPos, normal, lightDirPos, diffCol, specCol * specMap, ambCol);
    if (firstPass) // Add emissive light only once (since it's technically not related to a specific light source)
        components.diffuseAmbient.rgb += useEmissiveMap ? readEmissiveMap(IN.texCoord) : emissiveColor.rgb;
    float3 diffAmbCol = components.diffuseAmbient.rgb * diffuseMap.rgb;

    // Bake in the specular color...
    if (firstPass) return float4(diffAmbCol + components.specular.rgb, diffuseMap.a); // ... and pass the alpha-channel through on the first pass
    else return float4(diffAmbCol * diffuseMap.a + components.specular.rgb, 1); // ... and bake in the alpha-channel for all additional passes since they use additive blending
}

float4 PS_ColoredTwoDirLights(outColored IN,
  uniform bool firstPass,
  uniform float3 lightDir1, uniform float3 lightDir2,
  uniform float4 diffCol1, uniform float4 diffCol2, uniform float4 specCol1, uniform float4 specCol2,
  uniform float4 ambCol1, uniform float4 ambCol2) : COLOR
{
    lightComponents components = calcTwoDirLights(IN.worldPos, IN.normal, lightDir1, lightDir2, diffCol1, diffCol2, specCol1, specCol2, ambCol1, ambCol2);
    return components.diffuseAmbient + components.specular;
}

float4 PS_ColoredOneDirLight(outColored IN,
  uniform bool firstPass, uniform float3 lightDir, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol) : COLOR
{
    lightComponents components = calcDirLight(IN.worldPos, IN.normal, lightDir, diffCol, specCol, ambCol);
    return components.diffuseAmbient + components.specular;
}

float4 PS_ColoredOnePointLight(outColored IN,
  uniform bool firstPass, uniform float3 lightPos, uniform float4 diffCol, uniform float4 specCol, uniform float4 ambCol, uniform float3 att) : COLOR
{
    lightComponents components = calcPointLight(IN.worldPos, IN.position, IN.normal, lightPos, diffCol, specCol, ambCol, att);
    return components.diffuseAmbient + components.specular;
}


//---------------- Techniques ----------------

#define ADDITIVE_STATES ZWriteEnable = false; ZFunc = LessEqual; CullMode = None; AlphaBlendEnable = true; SrcBlend = One; DestBlend = One;

technique FXComposerTest {
  pass OnePointLight {
    VertexShader = compile vs_1_1 VS_Textured();
    PixelShader = compile ps_2_0 PS_TexturedOneDirOrPointLight(/*useNormalMap*/true, /*useSpecularMap*/true, /*useEmissiveMap*/false, /*firstPass*/true, /*pointLight*/true, lightPosition1, diffuseColor1, specularColor1, ambientColor1, attenuation1);
  }
}

technique ColoredPerVertex {
  pass AmbientLight {
    VertexShader = compile vs_1_1 VS_ColoredAmbient(ambientColor1);
    PixelShader = null;
  }
  pass TwoDirLights {
    VertexShader = compile vs_1_1 VS_ColoredPerVertexTwoDirLights(/*firstPass*/true, -lightDirection1, -lightDirection2, diffuseColor1, diffuseColor2, specularColor1, specularColor2, ambientColor1, ambientColor2);
    PixelShader = null;
  }
  pass TwoDirLightsAdd {
    ADDITIVE_STATES
    VertexShader = compile vs_1_1 VS_ColoredPerVertexTwoDirLights(/*firstPass*/false, -lightDirection1, -lightDirection2, diffuseColor1, diffuseColor2, specularColor1, specularColor2, ambientColor1, ambientColor2);
    PixelShader = null;
  }
  pass OneDirLight {
    VertexShader = compile vs_1_1 VS_ColoredPerVertexOneDirLight(/*firstPass*/true, -lightDirection1, diffuseColor1, specularColor1, ambientColor1);
    PixelShader = null;
  }
  pass OneDirLightAdd {
    ADDITIVE_STATES
    VertexShader = compile vs_1_1 VS_ColoredPerVertexOneDirLight(/*firstPass*/false, -lightDirection1, diffuseColor1, specularColor1, ambientColor1);
    PixelShader = null;
  }
  pass OnePointLight {
    VertexShader = compile vs_1_1 VS_ColoredPerVertexOnePointLight(/*firstPass*/true, lightPosition1, diffuseColor1, specularColor1, ambientColor1, attenuation1);
    PixelShader = null;
  }
  pass OnePointLightAdd {
    ADDITIVE_STATES
    VertexShader = compile vs_1_1 VS_ColoredPerVertexOnePointLight(/*firstPass*/false, lightPosition1, diffuseColor1, specularColor1, ambientColor1, attenuation1);
    PixelShader = null;
  }
}

technique Colored {
  pass AmbientLight {
    VertexShader = compile vs_1_1 VS_ColoredAmbient(ambientColor1);
    PixelShader = null;
  }
  pass TwoDirLights {
    VertexShader = compile vs_1_1 VS_Colored();
    PixelShader = compile ps_2_0 PS_ColoredTwoDirLights(/*firstPass*/true, -lightDirection1, -lightDirection2, diffuseColor1, diffuseColor2, specularColor1, specularColor2, ambientColor1, ambientColor2);
  }
  pass TwoDirLightsAdd {
    ADDITIVE_STATES
    VertexShader = compile vs_1_1 VS_Colored();
    PixelShader = compile ps_2_0 PS_ColoredTwoDirLights(/*firstPass*/false, -lightDirection1, -lightDirection2, diffuseColor1, diffuseColor2, specularColor1, specularColor2, ambientColor1, ambientColor2);
  }
  pass OneDirLight {
    VertexShader = compile vs_1_1 VS_Colored();
    PixelShader = compile ps_2_0 PS_ColoredOneDirLight(/*firstPass*/true, -lightDirection1, diffuseColor1, specularColor1, ambientColor1);
  }
  pass OneDirLightAdd {
    ADDITIVE_STATES
    VertexShader = compile vs_1_1 VS_Colored();
    PixelShader = compile ps_2_0 PS_ColoredOneDirLight(/*firstPass*/false, -lightDirection1, diffuseColor1, specularColor1, ambientColor1);
  }
  pass OnePointLight {
    VertexShader = compile vs_1_1 VS_Colored();
    PixelShader = compile ps_2_0 PS_ColoredOnePointLight(/*firstPass*/true, lightPosition1, diffuseColor1, specularColor1, ambientColor1, attenuation1);
  }
  pass OnePointLightAdd {
    ADDITIVE_STATES
    VertexShader = compile vs_1_1 VS_Colored();
    PixelShader = compile ps_2_0 PS_ColoredOnePointLight(/*firstPass*/false, lightPosition1, diffuseColor1, specularColor1, ambientColor1, attenuation1);
  }
}

technique ColoredEmissiveOnly {
  pass Emissive {
    VertexShader = compile vs_1_1 VS_ColoredAmbient(/*ambCol*/0);
    PixelShader = null;
  }
}

technique TexturedPerVertex {
  pass AmbientLight {
    VertexShader = compile vs_1_1 VS_TexturedAmbient(ambientColor1);
    PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/true);
  }
  pass TwoDirLights {
    VertexShader = compile vs_1_1 VS_TexturedPerVertexTwoDirLights(/*firstPass*/true, -lightDirection1, -lightDirection2, diffuseColor1, diffuseColor2, specularColor1, specularColor2, ambientColor1, ambientColor2);
    PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/true);
  }
  pass TwoDirLightsAdd {
    ADDITIVE_STATES
    VertexShader = compile vs_1_1 VS_TexturedPerVertexTwoDirLights(/*firstPass*/false, -lightDirection1, -lightDirection2, diffuseColor1, diffuseColor2, specularColor1, specularColor2, ambientColor1, ambientColor2);
    PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/false);
  }
  pass OneDirLight {
    VertexShader = compile vs_1_1 VS_TexturedPerVertexOneDirLight(/*firstPass*/true, -lightDirection1, diffuseColor1, specularColor1, ambientColor1);
    PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/true);
  }
  pass OneDirLightAdd {
    ADDITIVE_STATES
    VertexShader = compile vs_1_1 VS_TexturedPerVertexOneDirLight(/*firstPass*/false, -lightDirection1, diffuseColor1, specularColor1, ambientColor1);
    PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/false);
  }
  pass OnePointLight {
    VertexShader = compile vs_1_1 VS_TexturedPerVertexOnePointLight(/*firstPass*/true, lightPosition1, diffuseColor1, specularColor1, ambientColor1, attenuation1);
    PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/true);
  }
  pass OnePointLightAdd {
    ADDITIVE_STATES
    VertexShader = compile vs_1_1 VS_TexturedPerVertexOnePointLight(/*firstPass*/false, lightPosition1, diffuseColor1, specularColor1, ambientColor1, attenuation1);
    PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/false);
  }
}

#define TEXTURED(useNormalMap, useSpecularMap, useEmissiveMap) \
pass AmbientLight { \
    VertexShader = compile vs_1_1 VS_TexturedAmbient(ambientColor1); \
    PixelShader = compile ps_2_0 PS_TexturedPerVertex(useEmissiveMap, /*firstPass*/true); \
} \
pass TwoDirLights { \
    VertexShader = compile vs_1_1 VS_Textured(); \
    PixelShader = compile ps_2_0 PS_TexturedTwoDirLights(useNormalMap, useSpecularMap, useEmissiveMap, /*firstPass*/true, -lightDirection1, -lightDirection2, diffuseColor1, diffuseColor2, specularColor1, specularColor2, ambientColor1, ambientColor2); \
} \
pass TwoDirLightsAdd { \
    ADDITIVE_STATES \
    VertexShader = compile vs_1_1 VS_Textured(); \
    PixelShader = compile ps_2_0 PS_TexturedTwoDirLights(useNormalMap, useSpecularMap, /*useEmissiveMap*/false, /*firstPass*/false, -lightDirection1, -lightDirection2, diffuseColor1, diffuseColor2, specularColor1, specularColor2, ambientColor1, ambientColor2); \
} \
pass OneDirLight { \
    VertexShader = compile vs_1_1 VS_Textured(); \
    PixelShader = compile ps_2_0 PS_TexturedOneDirOrPointLight(useNormalMap, useSpecularMap, useEmissiveMap, /*firstPass*/true, /*pointLight*/false, -lightDirection1, diffuseColor1, specularColor1, ambientColor1, attenuation1); \
} \
pass OneDirLightAdd { \
    ADDITIVE_STATES \
    VertexShader = compile vs_1_1 VS_Textured(); \
    PixelShader = compile ps_2_0 PS_TexturedOneDirOrPointLight(useNormalMap, useSpecularMap, /*useEmissiveMap*/false, /*firstPass*/false, /*pointLight*/false, -lightDirection1, diffuseColor1, specularColor1, ambientColor1, attenuation1); \
} \
\
pass OnePointLight { \
    VertexShader = compile vs_1_1 VS_Textured(); \
    PixelShader = compile ps_2_0 PS_TexturedOneDirOrPointLight(useNormalMap, useSpecularMap, useEmissiveMap, /*firstPass*/true, /*pointLight*/true, lightPosition1, diffuseColor1, specularColor1, ambientColor1, attenuation1); \
} \
pass OnePointLightAdd { \
    ADDITIVE_STATES \
    VertexShader = compile vs_1_1 VS_Textured(); \
    PixelShader = compile ps_2_0 PS_TexturedOneDirOrPointLight(useNormalMap, useSpecularMap, /*useEmissiveMap*/false, /*firstPass*/false, /*pointLight*/true, lightPosition1, diffuseColor1, specularColor1, ambientColor1, attenuation1); \
}

technique Textured < string Script = " Pass=OnePointLight;"; > {
  TEXTURED(false, false, false)
}

technique TexturedNormalMap < string Script = " Pass=OnePointLight;"; > {
  TEXTURED(true, false, false)
}

technique TexturedSpecularMap < string Script = " Pass=OnePointLight;"; > {
  TEXTURED(false, true, false)
}

technique TexturedNormalSpecularMap < string Script = " Pass=OnePointLight;"; > {
  TEXTURED(true, true, false)
}

technique TexturedEmissiveMap < string Script = " Pass=OnePointLight;"; > {
  TEXTURED(false, false, true)
}

technique TexturedNormalEmissiveMap < string Script = " Pass=OnePointLight;"; > {
  TEXTURED(true, false, true)
}

technique TexturedNormalSpecularEmissiveMap < string Script = " Pass=OnePointLight;"; > {
  TEXTURED(true, true, true)
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
