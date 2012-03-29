/******************************************************************************

Copyright NVIDIA Corporation 2002-2004
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.


Comments:
    Render-to-Texture (RTT) glow example.
    Blurs are done in two separable passes.

Modified: Copyright 2008-2010 Bastian Eicher
Now used as a gaussian blur filter
    
******************************************************************************/

#include <include\\Quad.fxh>

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Blur;";
> = 0.8; // version #

float4 ClearColor <
    string UIWidget = "color";
    string UIName = "background";
> = {0,0,0,0};

float ClearDepth <string UIWidget = "none";> = 1.0;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float BlurStrength <
    string UIName = "Blur Strength";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 10.0f;
    float UIStep = 0.02f;
> = 1.0f;

float GlowStrength <
    string UIName = "Glow Strength";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 100.0f;
    float UIStep = 0.02f;
> = 1.0f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneMap,SceneSamp,"X8R8G8B8")
DECLARE_QUAD_TEX(TempMap,TempSamp,"A8R8G8B8")

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct VS_OUTPUT_BLUR
{
    float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float4 TexCoord0   : TEXCOORD0;
    float4 TexCoord1   : TEXCOORD1;
    float4 TexCoord2   : TEXCOORD2;
    float4 TexCoord3   : TEXCOORD3;
    float4 TexCoord4   : TEXCOORD4;
    float4 TexCoord5   : TEXCOORD5;
    float4 TexCoord6   : TEXCOORD6;
    float4 TexCoord7   : TEXCOORD7;
    float4 TexCoord8   : COLOR1;   
};

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

VS_OUTPUT_BLUR VS_Quad_Horizontal_9tap(float3 Position : POSITION, 
            float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    float TexelIncrement = BlurStrength/QuadScreenSize.x;
    OUT.TexCoord0 = float4(TexCoord.x + TexelIncrement, TexCoord.y, TexCoord.z, 1);
    OUT.TexCoord1 = float4(TexCoord.x + TexelIncrement * 2, TexCoord.y, TexCoord.z, 1);
    OUT.TexCoord2 = float4(TexCoord.x + TexelIncrement * 3, TexCoord.y, TexCoord.z, 1);
    OUT.TexCoord3 = float4(TexCoord.x + TexelIncrement * 4, TexCoord.y, TexCoord.z, 1);
    OUT.TexCoord4 = float4(TexCoord.x, TexCoord.y, TexCoord.z, 1);
    OUT.TexCoord5 = float4(TexCoord.x - TexelIncrement, TexCoord.y, TexCoord.z, 1);
    OUT.TexCoord6 = float4(TexCoord.x - TexelIncrement * 2, TexCoord.y, TexCoord.z, 1);
    OUT.TexCoord7 = float4(TexCoord.x - TexelIncrement * 3, TexCoord.y, TexCoord.z, 1);
    OUT.TexCoord8 = float4(TexCoord.x - TexelIncrement * 4, TexCoord.y, TexCoord.z, 1);
    return OUT;
}

VS_OUTPUT_BLUR VS_Quad_Vertical_9tap(float3 Position : POSITION, 
            float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = float4(Position, 1);
    float TexelIncrement = BlurStrength/QuadScreenSize.y;
    OUT.TexCoord0 = float4(TexCoord.x, TexCoord.y + TexelIncrement, TexCoord.z, 1);
    OUT.TexCoord1 = float4(TexCoord.x, TexCoord.y + TexelIncrement * 2, TexCoord.z, 1);
    OUT.TexCoord2 = float4(TexCoord.x, TexCoord.y + TexelIncrement * 3, TexCoord.z, 1);
    OUT.TexCoord3 = float4(TexCoord.x, TexCoord.y + TexelIncrement * 4, TexCoord.z, 1);
    OUT.TexCoord4 = float4(TexCoord.x, TexCoord.y, TexCoord.z, 1);
    OUT.TexCoord5 = float4(TexCoord.x, TexCoord.y - TexelIncrement, TexCoord.z, 1);
    OUT.TexCoord6 = float4(TexCoord.x, TexCoord.y - TexelIncrement * 2, TexCoord.z, 1);
    OUT.TexCoord7 = float4(TexCoord.x, TexCoord.y - TexelIncrement * 3, TexCoord.z, 1);
    OUT.TexCoord8 = float4(TexCoord.x, TexCoord.y - TexelIncrement * 4, TexCoord.z, 1);
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

// For two-pass blur, we have chosen to do  the horizontal blur FIRST. The
//    vertical pass includes a post-blur scale factor.

// Relative filter weights indexed by distance from "home" texel
//    This set for 9-texel sampling
#define WT9_0 1.0
#define WT9_1 0.8
#define WT9_2 0.6
#define WT9_3 0.4
#define WT9_4 0.2

// Alt pattern -- try your own!
// #define WT9_0 0.1
// #define WT9_1 0.2
// #define WT9_2 3.0
// #define WT9_3 1.0
// #define WT9_4 0.4

#define WT9_NORMALIZE (WT9_0+2.0*(WT9_1+WT9_2+WT9_3+WT9_4))

float4 PS_Blur_Horizontal_9tap(VS_OUTPUT_BLUR IN) : COLOR
{
    float4 OutCol = tex2D(SceneSamp, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SceneSamp, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SceneSamp, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SceneSamp, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(SceneSamp, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(SceneSamp, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SceneSamp, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SceneSamp, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SceneSamp, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    return OutCol;
}

float4 PS_Blur_Vertical_9tap(VS_OUTPUT_BLUR IN, uniform float factor) : COLOR
{
    float4 OutCol = tex2D(TempSamp, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(TempSamp, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(TempSamp, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(TempSamp, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(TempSamp, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(TempSamp, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(TempSamp, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(TempSamp, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(TempSamp, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
	return OutCol * factor;
}

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Blur <
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script =
    "RenderColorTarget0=SceneMap;"
                "ClearSetColor=ClearColor; ClearSetDepth=ClearDepth;"
                "Clear=Color; Clear=Depth;"
                "ScriptExternal=color;"
            "Pass=BlurGlowBuffer_Horz;"
            "Pass=BlurGlowBuffer_Vert;";
> {
    pass BlurGlowBuffer_Horz <
        string Script ="RenderColorTarget0=TempMap; Draw=Buffer;";
    > {
        VertexShader = compile vs_2_0 VS_Quad_Horizontal_9tap();
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        CullMode = None;
        PixelShader  = compile ps_2_0 PS_Blur_Horizontal_9tap();
    }

    pass BlurGlowBuffer_Vert <
        string Script = "RenderColorTarget0=; Draw=Buffer;";
    > {
        VertexShader = compile vs_2_0 VS_Quad_Vertical_9tap();
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        CullMode = None;
        PixelShader  = compile ps_2_0 PS_Blur_Vertical_9tap(1);
    }
}

technique Glow < string Script =
    "RenderColorTarget0=SceneMap;"
    "ClearSetColor=ClearColor; ClearSetDepth=ClearDepth; Clear=Color; Clear=Depth;"
    "ScriptExternal=color;"
    "Pass=BlurGlowBuffer_Horz;"
    "Pass=BlurGlowBuffer_Vert;";
> {
    pass BlurGlowBuffer_Horz < string Script ="RenderColorTarget0=TempMap; Draw=Buffer;"; >
    {
        VertexShader = compile vs_2_0 VS_Quad_Horizontal_9tap();
        ZEnable = false;
        ZWriteEnable = false;
        CullMode = None;
        PixelShader  = compile ps_2_0 PS_Blur_Horizontal_9tap();
    }

    pass BlurGlowBuffer_Vert < string Script = "RenderColorTarget0=; Draw=Buffer;"; >
    {
        VertexShader = compile vs_2_0 VS_Quad_Vertical_9tap();
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = true;
        SrcBlend = One;
        DestBlend = One;
        CullMode = None;
        PixelShader  = compile ps_2_0 PS_Blur_Vertical_9tap(GlowStrength);
    }
}
