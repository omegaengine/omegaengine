// Description: Helpers to generate techniques for General.fx
//
// Macros:
// - ColoredPerVertexMacro (ps_1_1, plain color, per-vertex lighting)
// - ColoredMacro (ps_2_0, plain color, per-pixel lighting)
// - TexturedPerVertexMacro (ps_1_1, textured, per-vertex lighting)
// - TexturedMacro (ps_2_0, textured, per-pixel lighting, optional normal map + specular map + emissive map)

//---------------- Shortcuts ----------------

#define AdditiveBlending ZWriteEnable = false; ZFunc = LessEqual; CullMode = None; AlphaBlendEnable = true; SrcBlend = One; DestBlend = One
#define OneLightColor diffuseColor1, specularColor1, ambientColor1
#define TwoLightColors diffuseColor1, diffuseColor2, specularColor1, specularColor2, ambientColor1, ambientColor2


//---------------- Techniques ----------------

#define ColoredPerVertexMacro \
pass AmbientLight { \
	VertexShader = compile vs_1_1 VS_ColoredAmbient(ambientColor1); \
	PixelShader = null; \
} \
\
pass TwoDirLights { \
	VertexShader = compile vs_1_1 VS_ColoredPerVertex(/*firstPass*/true, -lightDirection1, -lightDirection2, TwoLightColors); \
	PixelShader = null; \
} \
\
pass TwoDirLightsAdd { \
	VertexShader = compile vs_1_1 VS_ColoredPerVertex(/*firstPass*/false, -lightDirection1, -lightDirection2, TwoLightColors); \
	AdditiveBlending; \
	PixelShader = null; \
} \
\
pass OneDirLight { \
	VertexShader = compile vs_1_1 VS_ColoredPerVertex(/*firstPass*/true, -lightDirection1, OneLightColor); \
	PixelShader = null; \
} \
\
pass OneDirLightAdd { \
	VertexShader = compile vs_1_1 VS_ColoredPerVertex(/*firstPass*/false, -lightDirection1, OneLightColor, attenuation1); \
	AdditiveBlending; \
	PixelShader = null; \
} \
\
pass OnePointLight { \
	VertexShader = compile vs_1_1 VS_ColoredPerVertex(/*firstPass*/true, lightPosition1, OneLightColor, attenuation1); \
	PixelShader = null; \
} \
\
pass OnePointLightAdd { \
	VertexShader = compile vs_1_1 VS_ColoredPerVertex(/*firstPass*/false, lightPosition1, OneLightColor, attenuation1); \
	AdditiveBlending; \
	PixelShader = null; \
}


#define ColoredMacro \
pass AmbientLight { \
	VertexShader = compile vs_1_1 VS_ColoredAmbient(ambientColor1); \
	PixelShader = null; \
} \
\
pass TwoDirLights { \
	VertexShader = compile vs_1_1 VS_Colored(); \
	PixelShader = compile ps_2_0 PS_Colored(/*firstPass*/true, -lightDirection1, -lightDirection2, TwoLightColors); \
} \
\
pass TwoDirLightsAdd { \
	VertexShader = compile vs_1_1 VS_Colored(); \
	AdditiveBlending; \
	PixelShader = compile ps_2_0 PS_Colored(/*firstPass*/false, -lightDirection1, -lightDirection2, TwoLightColors); \
} \
\
pass OneDirLight { \
	VertexShader = compile vs_1_1 VS_Colored(); \
	PixelShader = compile ps_2_0 PS_Colored(/*firstPass*/true, -lightDirection1, OneLightColor); \
} \
\
pass OneDirLightAdd { \
	VertexShader = compile vs_1_1 VS_Colored(); \
	AdditiveBlending; \
	PixelShader = compile ps_2_0 PS_Colored(/*firstPass*/false, -lightDirection1, OneLightColor); \
} \
\
pass OnePointLight { \
	VertexShader = compile vs_1_1 VS_Colored(); \
	PixelShader = compile ps_2_0 PS_Colored(/*firstPass*/true, lightPosition1, OneLightColor, attenuation1); \
} \
\
pass OnePointLightAdd { \
	VertexShader = compile vs_1_1 VS_Colored(); \
	AdditiveBlending; \
	PixelShader = compile ps_2_0 PS_Colored(/*firstPass*/false, lightPosition1, OneLightColor, attenuation1); \
}


#define TexturedPerVertexMacro \
pass AmbientLight { \
	VertexShader = compile vs_1_1 VS_TexturedAmbient(ambientColor1); \
	PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/true); \
} \
\
pass TwoDirLights { \
	VertexShader = compile vs_1_1 VS_TexturedPerVertex(/*firstPass*/true, -lightDirection1, -lightDirection2, TwoLightColors); \
	PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/true); \
} \
\
pass TwoDirLightsAdd { \
	VertexShader = compile vs_1_1 VS_TexturedPerVertex(/*firstPass*/false, -lightDirection1, -lightDirection2, TwoLightColors); \
	AdditiveBlending; \
	PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/false); \
} \
\
pass OneDirLight { \
	VertexShader = compile vs_1_1 VS_TexturedPerVertex(/*firstPass*/true, -lightDirection1, OneLightColor); \
	PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/true); \
} \
\
pass OneDirLightAdd { \
	VertexShader = compile vs_1_1 VS_TexturedPerVertex(/*firstPass*/false, -lightDirection1, OneLightColor); \
	AdditiveBlending; \
	PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/false); \
} \
\
pass OnePointLight { \
	VertexShader = compile vs_1_1 VS_TexturedPerVertex(/*firstPass*/true, lightPosition1, OneLightColor, attenuation1); \
	PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/true); \
} \
\
pass OnePointLightAdd { \
	VertexShader = compile vs_1_1 VS_TexturedPerVertex(/*firstPass*/false, lightPosition1, OneLightColor, attenuation1); \
	AdditiveBlending; \
	PixelShader = compile ps_1_1 PS_TexturedPerVertex(/*useEmissive*/false, /*firstPass*/false); \
}


#define TexturedMacro(useNormalMap, useSpecularMap, useEmissiveMap) \
pass AmbientLight { \
	VertexShader = compile vs_1_1 VS_TexturedAmbient(ambientColor1); \
	PixelShader = compile ps_2_0 PS_TexturedPerVertex(useEmissiveMap, /*firstPass*/true); \
} \
\
pass TwoDirLights { \
	VertexShader = compile vs_1_1 VS_Textured(); \
	PixelShader = compile ps_2_0 PS_Textured(useNormalMap, useSpecularMap, useEmissiveMap, /*firstPass*/true, \
	-lightDirection1, -lightDirection2, TwoLightColors); \
} \
\
pass TwoDirLightsAdd { \
	VertexShader = compile vs_1_1 VS_Textured(); \
	AdditiveBlending; \
	PixelShader = compile ps_2_0 PS_Textured(useNormalMap, useSpecularMap, /*useEmissiveMap*/false, /*firstPass*/false, \
	-lightDirection1, -lightDirection2, TwoLightColors); \
} \
\
pass OneDirLight { \
	VertexShader = compile vs_1_1 VS_Textured(); \
	PixelShader = compile ps_2_0 PS_Textured(useNormalMap, useSpecularMap, useEmissiveMap, /*firstPass*/true, \
	false/*pointLight*/, -lightDirection1, OneLightColor, attenuation1); \
} \
\
pass OneDirLightAdd { \
	VertexShader = compile vs_1_1 VS_Textured(); \
	AdditiveBlending; \
	PixelShader = compile ps_2_0 PS_Textured(useNormalMap, useSpecularMap, /*useEmissiveMap*/false, /*firstPass*/false, \
	false/*pointLight*/, -lightDirection1, OneLightColor, attenuation1); \
} \
\
pass OnePointLight { \
	VertexShader = compile vs_1_1 VS_Textured(); \
	PixelShader = compile ps_2_0 PS_Textured(useNormalMap, useSpecularMap, useEmissiveMap, /*firstPass*/true, \
	true/*pointLight*/, lightPosition1, OneLightColor, attenuation1); \
} \
\
pass OnePointLightAdd { \
	VertexShader = compile vs_1_1 VS_Textured(); \
	AdditiveBlending; \
	PixelShader = compile ps_2_0 PS_Textured(useNormalMap, useSpecularMap, /*useEmissiveMap*/false, /*firstPass*/false, \
	true/*pointLight*/, lightPosition1, OneLightColor, attenuation1); \
}
