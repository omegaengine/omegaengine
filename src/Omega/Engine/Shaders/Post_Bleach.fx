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

% An image effect that's intended to look like the movie-film printing
% effect called "bleach bypass," where a normal step of processing is
% skipped to cause unique color-shift and contrast effects.
% The "Blend Opacity" slider lets you dial-in the strength of this effect.

keywords: image_processing


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
> = {0.3, 0.3, 0.3, 1.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

/////////////////////////////////////////////////////
//// Tweakables /////////////////////////////////////
/////////////////////////////////////////////////////

float Opacity <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 1.0;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Textures //////////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"A8R8G8B8")
// DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

//////////////////////////////////////////////////////
/////////////////////////////////// pixel shader /////
//////////////////////////////////////////////////////

QUAD_REAL4 bypassPS(QuadVertexOutput IN) : COLOR {
    QUAD_REAL4 base = tex2D(SceneSampler, IN.UV);
    QUAD_REAL3 lumCoeff = QUAD_REAL3(0.25,0.65,0.1);
    QUAD_REAL lum = dot(lumCoeff,base.rgb);
    QUAD_REAL3 blend = lum.rrr;
    QUAD_REAL L = min(1,max(0,10*(lum- 0.45)));
    QUAD_REAL3 result1 = 2.0f * base.rgb * blend;
    QUAD_REAL3 result2 = 1.0f - 2.0f*(1.0f-blend)*(1.0f-base.rgb);
    QUAD_REAL3 newColor = lerp(result1,result2,L);
    QUAD_REAL A2 = Opacity * base.a;
    QUAD_REAL3 mixRGB = A2 * newColor.rgb;
    mixRGB += ((1.0f-A2) * base.rgb);
    return QUAD_REAL4(mixRGB,base.a);
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
        PixelShader = compile ps_2_a bypassPS();
    }
}
