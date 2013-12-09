// Description: Shader for simulating a water surface
//
// Lights:
// - Light 1+2: directional
// - Light 3+4: point or directional
//
// Techniques:
// - RefractionReflection (ps_2_0, 1, refraction and refrection map)
// - Refraction (ps_2_0, refraction map and scrolling texture)
// - Simple (ps_1_1, scrolling texture)
//---------------- Parameters ----------------

// Camera
float3 cameraPosition          : CameraPosition;
float4x4 world                 : World;
float4x4 worldViewProjection   : WorldViewProjection;
float4x4 ReflectionViewProjection;

// Water
float DullBlendFactor <
    string UIWidget = "slider";
    float UIMin = 0.0f; float UIMax = 1.0f; float UIStep = 0.05f;
> = 0.15f;
float4 DullColor = {0.3f, 0.3f, 0.5f, 1.0f};
float WaveLength <
    string UIWidget = "slider";
    float UIMin = 0.0f; float UIMax = 1.0f; float UIStep = 0.05f;
> = 0.1;
float WaveHeight <
    string UIWidget = "slider";
    float UIMin = 0.0f; float UIMax = 0.2f; float UIStep = 0.01f;
> = 0.01;
float WindForce <
    string UIWidget = "slider";
    float UIMin = 0.0f; float UIMax = 1.0f; float UIStep = 0.05f;
> = 0.2;
float4x4 WindDirection;
float time : Time;


//---------------- Texture samplers ----------------

Texture NormalTexture < string ResourceName = "default_bump_normal.dds"; >;
sampler normalSampler = sampler_state
{
  texture = <NormalTexture>;
  AddressU = Mirror; AddressV = Mirror;
  MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
};

Texture RefractionMap < string ResourceName = "default_color.dds"; >;
sampler RefractionSampler = sampler_state
{
  texture = <RefractionMap>;
  AddressU = Mirror; AddressV = Mirror;
  MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
};

Texture ReflectionMap < string ResourceName = "default_reflection.dds"; >;
sampler ReflectionSampler = sampler_state
{
  texture = <ReflectionMap>;
  AddressU = Mirror; AddressV = Mirror;
  MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
};


//---------------- Structs ----------------

struct inWater
{
    float4 position : POSITION;
    float3 normal   : NORMAL;
    float2 texCoord : TEXCOORD;
};

struct outRefractionReflection
{
    float4 pos                      : POSITION;
    float3 worldPos                 : TEXCOORD0;
    float3 normal                   : TEXCOORD1;
    float2 bumpMapSamplingPos       : TEXCOORD2;
    float3 refractionMapSamplingPos : TEXCOORD3;
    float3 reflectionMapSamplingPos : TEXCOORD4;
};

struct outRefraction
{
    float4 pos                      : POSITION;
    float3 worldPos                 : TEXCOORD0;
    float3 normal                   : TEXCOORD1;
    float2 texCoord                 : TEXCOORD2;
    float2 bumpMapSamplingPos       : TEXCOORD3;
    float3 refractionMapSamplingPos : TEXCOORD4;
};

struct outSimple
{
    float4 pos      : POSITION;
    float2 texCoord : TEXCOORD0;
};


//---------------- Helper functions ----------------

float2 calcSamplingCoord(float2 texCoord)
{
    float4 rotatedTexCoords = mul(float4(texCoord, 0, 1), WindDirection);
    float2 moveVector = float2(0, 1);
    return rotatedTexCoords.xy/WaveLength + time*WindForce*moveVector.xy;
}


//---------------- Vertex shaders ----------------

outRefractionReflection VS_RefractionReflection(inWater IN)
{
    outRefractionReflection OUT;

    // Transform data into world space
    float4x4 worldReflectionViewProjection = mul(world, ReflectionViewProjection);
    OUT.pos = mul(float4(IN.position.xyz, 1.0), worldViewProjection); OUT.worldPos = mul(float4(IN.position.xyz, 1.0), world);
    OUT.normal = normalize(mul(normalize(IN.normal), world));
    OUT.reflectionMapSamplingPos = mul(IN.position, worldReflectionViewProjection).xyw;
    OUT.refractionMapSamplingPos = mul(IN.position, worldViewProjection).xyw;
    OUT.bumpMapSamplingPos = calcSamplingCoord(IN.texCoord);

    return OUT;    
}

outRefraction VS_Refraction(inWater IN)
{
    outRefraction OUT;

    // Transform data into world space
    OUT.pos = mul(float4(IN.position.xyz, 1.0), worldViewProjection); OUT.worldPos = mul(float4(IN.position.xyz, 1.0), world);
    OUT.normal = normalize(mul(normalize(IN.normal), world));
    OUT.texCoord = calcSamplingCoord(IN.texCoord);
    OUT.refractionMapSamplingPos = mul(IN.position, worldViewProjection).xyw;
    OUT.bumpMapSamplingPos = calcSamplingCoord(IN.texCoord);

    return OUT;    
}

outSimple VS_Simple(inWater IN)
{
    outSimple OUT;

    // Transform data into world space
    OUT.pos = mul(float4(IN.position.xyz, 1.0), worldViewProjection);
    OUT.texCoord = calcSamplingCoord(IN.texCoord);
    OUT.texCoord = calcSamplingCoord(IN.texCoord);

    return OUT;    
}


//---------------- Pixel shaders ----------------

float4 PS_RefractionReflection(outRefractionReflection IN) : COLOR
{
    // Read normal map
    float4 bumpColor = tex2D(normalSampler, IN.bumpMapSamplingPos);
    float2 perturbation = WaveHeight*(bumpColor.rg - 0.5f);

    // Calculate wave offset for reflection
    float2 ProjectedTexCoords;
    ProjectedTexCoords.x = IN.reflectionMapSamplingPos.x/IN.reflectionMapSamplingPos.z/2.0f + 0.5f;
    ProjectedTexCoords.y = -IN.reflectionMapSamplingPos.y/IN.reflectionMapSamplingPos.z/2.0f + 0.5f;
    float2 perturbatedTexCoords = ProjectedTexCoords + perturbation;
    float4 reflectiveColor = tex2D(ReflectionSampler, perturbatedTexCoords);

    // Calculate wave offset for refraction
    float2 ProjectedRefrTexCoords;
    ProjectedRefrTexCoords.x = IN.refractionMapSamplingPos.x/IN.refractionMapSamplingPos.z/2.0f + 0.5f;
    ProjectedRefrTexCoords.y = -IN.refractionMapSamplingPos.y/IN.refractionMapSamplingPos.z/2.0f + 0.5f;
    float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords + perturbation;
    float4 refractiveColor = tex2D(RefractionSampler, perturbatedRefrTexCoords);

    // Calculate fresnel term
    float3 eyeVector = normalize(cameraPosition - IN.worldPos);
    float fresnelTerm = dot(eyeVector, IN.normal);
    float4 combinedColor = refractiveColor*fresnelTerm + reflectiveColor*(1-fresnelTerm);

    return DullBlendFactor*DullColor + (1-DullBlendFactor)*combinedColor;
}

float4 PS_Refraction(outRefraction IN) : COLOR
{
    float4 reflectiveColor = tex2D(ReflectionSampler, IN.texCoord);

    // Read normal map
    float4 bumpColor = tex2D(normalSampler, IN.bumpMapSamplingPos);
    float2 perturbation = WaveHeight*(bumpColor.rg - 0.5f);

    // Calculate wave offset for refraction
    float2 ProjectedRefrTexCoords;
    ProjectedRefrTexCoords.x = IN.refractionMapSamplingPos.x/IN.refractionMapSamplingPos.z/2.0f + 0.5f;
    ProjectedRefrTexCoords.y = -IN.refractionMapSamplingPos.y/IN.refractionMapSamplingPos.z/2.0f + 0.5f;
    float2 perturbatedRefrTexCoords = ProjectedRefrTexCoords + perturbation;
    float4 refractiveColor = tex2D(RefractionSampler, perturbatedRefrTexCoords);

    // Calculate fresnel term
    float3 eyeVector = normalize(cameraPosition - IN.worldPos);
    float fresnelTerm = dot(eyeVector, IN.normal);
    float4 combinedColor = refractiveColor*fresnelTerm + reflectiveColor*(1-fresnelTerm);

    return DullBlendFactor*DullColor + (1-DullBlendFactor)*combinedColor;
}

float4 PS_Simple(outSimple IN) : COLOR
{
    // Calculate wave offset for reflection
    return tex2D(ReflectionSampler, IN.texCoord);
}


//---------------- Techniques ----------------

technique RefractionReflection
{
  pass p0
  {
    VertexShader = compile vs_1_1 VS_RefractionReflection();
    PixelShader = compile ps_2_0 PS_RefractionReflection();
  }
}

technique Refraction
{
  pass p0
  {
    VertexShader = compile vs_1_1 VS_Refraction();
    PixelShader = compile ps_2_0 PS_Refraction();
  }
}

technique Simple
{
  pass p0
  {
    VertexShader = compile vs_1_1 VS_Simple();
    PixelShader = compile ps_1_1 PS_Simple();
  }
}
