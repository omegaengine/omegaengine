/*********************************************************************NVMH3****
$Revision: #3 $

Copyright NVIDIA Corporation 2007
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY
LOSS) ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF
NVIDIA HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

% Emulates a vibrating camera entirely through image-processing.

keywords: image_processing animation vertex
date: 070225


To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

******************************************************************************/

float Time : TIME <string UIWidget="None";>;

#include <include\\Quad.fxh>
#include <include\\vertex_noise.fxh>

#ifdef _3DSMAX_
int ParamID = 0x0003; // Defined by Autodesk
#endif /* _3DSMAX_ */

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

float4 ClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float ClearDepth <string UIWidget = "none";> = 1.0;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float Speed <
    string UIWidget = "slider";
    string UIName = "Overall Speed";
    float UIMin = -1.0f;
    float UIMax = 100.0f;
    float UIStep = 0.01f;
> = 20.f;

float Shake <
    string UIWidget = "slider";
    string UIName = "Shakiness";
    float UIMin = 0.0f;
    float UIMax = 1.0f;
    float UIStep = 0.01f;
> = 0.25f;

float Sharpness <
    string UIWidget = "slider";
    string UIName = "Snappiness";
    float UIMin = 0.0f;
    float UIMax = 10.0f;
    float UIStep = 0.1f;
> = 2.2f;

float2 TimeDelta <
    string UIName = "Time Deltas for X, Y";
> = {1,.2};

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"A8R8G8B8")
// DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

//////////////////////////////////////////////////////
/////////////////////////////////// vertex shader ////
//////////////////////////////////////////////////////


static float2 animDelta = Speed*Time*TimeDelta;

QuadVertexOutput ShakerVS(
    float3 Position : POSITION, 
    float2 TexCoord : TEXCOORD0
) {
    QuadVertexOutput OUT;
    OUT.Position = float4(Position.xyz, 1);
    float2 noisePos = (float2)(0.5)+animDelta;
    float2 i = Shake*float2(vertex_noise(noisePos, NTab),
                vertex_noise(noisePos.yx, NTab));
    i = sign(i) * pow(abs(i),Sharpness);
   OUT.UV = TexCoord.xy+i+QuadTexelOffsets;
    return OUT;
}

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Main < string Script =
    "RenderColorTarget0=SceneTexture;"
    "ClearSetColor=ClearColor; ClearSetDepth=ClearDepth; Clear=Color; Clear=Depth;"
    "ScriptExternal=color;"
    "Pass=PostP0;";
> {
    pass PostP0 < string Script = "RenderColorTarget0=; Draw=Buffer;"; >
    {
        VertexShader = compile vs_2_0 ShakerVS();
        ZEnable = false;
        ZWriteEnable = false;
        CullMode = None;
        PixelShader = compile ps_2_a TexQuadPS(SceneSampler);
    }
}
