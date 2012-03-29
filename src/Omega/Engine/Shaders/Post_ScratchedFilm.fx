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


% Create a scratched-movie-film look using animated noise

keywords: image_processing animation virtual_machine
date: 070401


To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

******************************************************************************/

// shared-surface access supported in Cg version
#include <include\\Quad.fxh>

#define NOISE2D_SCALE 1
//#define NOISE2D_FORMAT "A16B16G16R16F"
#include <include\\noise_2d.fxh>

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
> = {0.1, 0.1, 0.1, 0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

float Timer : TIME <string UIWidget="none";>;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float Speed <
    string UIName = "Speed (Slower=Longer Scratches)";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 0.2f;
    float UIStep = 0.0001f;
> = 0.03f;

float Speed2 <
    string UIName = "Side Scroll Speed";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 0.01f;
    float UIStep = 0.0001f;
> = 0.02f;

float ScratchIntensity <
    string UIName = "Scratch Threshhold";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 1.0f;
    float UIStep = 0.0001f;
> = 0.65f;

float IS <
    string UIName = "Scratch Width";
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 0.1f;
    float UIStep = 0.0001f;
> = 0.0075f;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"A8R8G8B8")

//////////////////////////////////////////////////////
/////////////////////////////////// pixel shader /////
//////////////////////////////////////////////////////

static float ScanLine = (Timer*Speed);
static float Side = (Timer*Speed2);

float4 scratchPS(QuadVertexOutput IN) : COLOR {
    float4 img = tex2D(SceneSampler,IN.UV);
    float2 s = float2(IN.UV.x+Side,ScanLine);
    float scratch = tex2D(Noise2DSamp,s).x;
    scratch = 2.0f*(scratch - ScratchIntensity)/IS;
    scratch = 1.0-abs(1.0f-scratch);
    //scratch = scratch * 100.0f;
    scratch = max(0,scratch);
    //scratch = min(scratch,1.0f);
    return img + float4(scratch.xxx,0);
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
        PixelShader = compile ps_2_a scratchPS();
    }
}
