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

% Convert the current scene to monochrome with "sepia" toning

keywords: image_processing color_conversion


To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

******************************************************************************/

// shared-surface access supported in Cg version
#include <include\\Quad.fxh>

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

float Desat <
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 1.0f;
    float UIStep = 0.01f;
    string UIName = "Desaturation";
> = 0.5f;

float Toned <
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 1.0f;
    float UIStep = 0.01f;
    string UIName = "Toning";
> = 1.0f;

float3 LightColor <
    string UIWidget = "color";
    string UIName = "Paper Tone";
> = {1,0.9,0.5};

float3 DarkColor <
    string UIWidget = "color";
    string UIName = "Stain Tone";
> = {0.2,0.05,0};

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"A8R8G8B8")

//////////////////////////////////////////////////////
/////////////////////////////////// pixel shader /////
//////////////////////////////////////////////////////

QUAD_REAL4 sepiaPS(QuadVertexOutput IN) : COLOR
{   
    QUAD_REAL3 scnColor = tex2D(SceneSampler, IN.UV).xyz;
    QUAD_REAL3 grayXfer = QUAD_REAL3(0.3,0.59,0.11);
    QUAD_REAL gray = dot(grayXfer,scnColor);
    QUAD_REAL3 muted = lerp(scnColor,gray.xxx,Desat);
    QUAD_REAL3 sepia = lerp(DarkColor,LightColor,gray);
    QUAD_REAL3 result = lerp(muted,sepia,Toned);
    return QUAD_REAL4(result,1);
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
        VertexShader = compile vs_2_0 ScreenQuadVS();
        ZEnable = false;
        ZWriteEnable = false;
        CullMode = None;
        PixelShader = compile ps_2_a sepiaPS();
    }
}
