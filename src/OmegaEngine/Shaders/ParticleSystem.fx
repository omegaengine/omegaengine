// Description: Shader for creating a particle system
//
// Techniques:
// - ParticleSystem (ps_2_0, 1 texture, no lighting)

//---------------- Parameters ----------------

// Camera
float4x4 worldViewProjection : WorldViewProjection;
float4x4 worldProjection : ViewProjection;
float4x4 view : View;

// Particle system
float SpawnRadius
<
   string UIWidget = "slider";
   float UIMin = 0.00;
   float UIMax = 2.00;
> = 1.00;
float SystemHeight
<
   string UIWidget = "slider";
   bool UIVisible =  true;
   float UIMin = 0.00;
   float UIMax = 160.00;
> = 80.00;
float ParticleSpeed
<
   string UIWidget = "slider";
   float UIMin = 0.00;
   float UIMax = 2.00;
> = 0.48;
float ParticleSpread
<
   string UIWidget = "slider";
   float UIMin = 0.00;
   float UIMax = 50.00;
> = 20.00;
float ParticleSize
<
   string UIWidget = "slider";
   float UIMin = 0.00;
   float UIMax = 20.00;
> = 7.80;
float ParticleShape
<
   string UIWidget = "slider";
   float UIMin = 0.00;
   float UIMax = 1.00;
> = 0.37;
float time : Time;


//---------------- Texture samplers ----------------

texture ParticleTexture < string ResourceName = "Flame.tga"; >;
sampler ParticleSampler = sampler_state
{
  Texture = <ParticleTexture>;
  AddressU = Mirror; AddressV = Mirror;
  MinFilter = Linear; MagFilter = Linear; MipFilter = Linear;
};


//---------------- Structs ----------------

struct VS_OUTPUT
{
   float4 Pos: POSITION;
   float2 texCoord: TEXCOORD0;
   float color: TEXCOORD1;
};


//---------------- Vertex shaders ----------------

// The model for the particle system consists of a hundred quads.
// These quads are simple (-1,-1) to (1,1) quads where each quad
// has a z ranging from 0 to 1. The z will be used to differenciate
// between different particles

VS_OUTPUT VS_Particle(float4 Pos: POSITION)
{
   // Loop particles
   float t = frac(Pos.z + ParticleSpeed * time);

   // Determine the shape of the system
   float s = pow(t, SpawnRadius);

   // Spread particles in a semi-random fashion
   float3 pos;
   pos.x = ParticleSpread * s * cos(62 * Pos.z);
   pos.z = ParticleSpread * s * sin(163 * Pos.z);

   // Particles goes up
   pos.y = SystemHeight * t;

   VS_OUTPUT OUT;
   // Billboard the quads.
   // The view matrix gives us our right and up vectors.
   pos += ParticleSize * (Pos.x * view[0] + Pos.y * view[1]);

   OUT.Pos = mul(worldProjection, float4(pos, 1));
   OUT.texCoord = Pos.xy;
   OUT.color = 1 - t;

   return OUT; 
}


//---------------- Pixel shaders ----------------

float4 PS_Particle(VS_OUTPUT IN) : COLOR
{
   // Fade the particle to a circular shape
   float fade = pow(dot(IN.texCoord, IN.texCoord), ParticleShape);
   return (1 - fade) * tex2D(ParticleSampler, float2(IN.color,0.5f));
}


//---------------- Techniques ----------------

technique ParticleSystem
{
  pass p0
  {
    VertexShader = compile vs_1_1 VS_Particle();
    CullMode = None;
    PixelShader = compile ps_2_0 PS_Particle();
  }
}