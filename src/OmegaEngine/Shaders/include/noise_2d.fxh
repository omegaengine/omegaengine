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


    This file, to be #included in HLSL FX effects, will create
	a 2D noise texture using the DirectX Virtual machine.


To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

******************************************************************************/

#ifndef _H_NOISE2D
#define _H_NOISE2D

texture Noise2DTex  <
    string TextureType = "2D";
    string ResourceName = "Noise128.dds";
    string UIName = "2D Noise Texture";
>;

// samplers
sampler2D Noise2DSamp = sampler_state 
{
    texture = <Noise2DTex>;
    AddressU  = Wrap;        
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
};

#define NOISE2D(p) tex2D(Noise2DSamp,(p))
#define SNOISE2D(p) (NOISE2D(p)-0.5)

#endif /* _H_NOISE2D */

///////////////////////////// eof ///
