#ifndef _PTPS
#define _PTPS

#if defined(CUSTOM_IMPL_PS0)
    #define _CustomImplPs
    #include "Custom_ShaderPS0.fx"
#endif
#if defined(CUSTOM_IMPL_PS1)
    #define _CustomImplPs
    #include "Custom_ShaderPS1.fx"
#endif
#if defined(CUSTOM_IMPL_PS2)
    #define _CustomImplPs
    #include "Custom_ShaderPS2.fx"
#endif
#if defined(CUSTOM_IMPL_PS3)
    #define _CustomImplPs
    #include "Custom_ShaderPS3.fx"
#endif
#if defined(CUSTOM_IMPL_PS4)
    #define _CustomImplPs
    #include "Custom_ShaderPS4.fx"
#endif
#if defined(CUSTOM_IMPL_PS5)
    #define _CustomImplPs
    #include "Custom_ShaderPS5.fx"
#endif
#if defined(CUSTOM_IMPL_PS6)
    #define _CustomImplPs
    #include "Custom_ShaderPS6.fx"
#endif
#if defined(CUSTOM_IMPL_PS7)
    #define _CustomImplPs
    #include "Custom_ShaderPS7.fx"
#endif
#if defined(CUSTOM_IMPL_PS8)
    #define _CustomImplPs
    #include "Custom_ShaderPS8.fx"
#endif
#if defined(CUSTOM_IMPL_PS9)
    #define _CustomImplPs
    #include "Custom_ShaderPS9.fx"
#endif

#if !defined(_CustomImplPs) && !defined(_PTImplementation)
    #include "PTImplementationPS.fx"
#endif

cbuffer MaterialConstantBuffer : register(b0)
{
    MaterialStruct Material;
};

cbuffer GlobalDataConstantBuffer : register(b1)
{
    GlobalDataStruct GlobalData;
};

cbuffer LightsConstantBuffer : register(b2) // 64 kb per ConstantBuffer MAX
{
    LightStruct Lights[MAX_LIGHTS];
};

// Main Pixel Shader implementation.
// Here all the abstract interfaces are called, using the specific classes implementations.
[earlydepthstencil]
float4 PixelShaderEntry(_PixelShader_IN input) : SV_TARGET
{
    input.TexCoord += Material.TextureTranslation;

    float4 textureColor;
    float lightIntensity;
    
    float2 reflectionCoords;
    float4 reflectionTextureColor;

    float4 blendColor = float4(1, 1, 1, 1);
    float3 normal = normalize(input.Normal);
    float4 resultColor = float4(0, 0, 0, 0);
    _BumpProperties bumpProps;

    // ==== Multi texture
    // Get the pixel color from the sent textures. === WITH COMMON TEXTURE ARRAY -- [4]
    for (int i = 0; i < Material.BlendTexturesCount; i++)
    {
        float4 col = shaderTextures[i].Sample(textureSampler, input.TexCoord);
        blendColor *= col;
    }
    
    if (Material.BlendTexturesCount > 0)
    {
        //if (Material.BlendTexturesCount == 1)
        //    blendColor = blendColor * (Material.Gamma * 0.5f);
        //else
        //    blendColor = blendColor * Material.Gamma;

        blendColor = blendColor * Material.Gamma;
    }
    
    // Do not process pixel if it is not seen.
    if (blendColor.a < 0)
        discard;

    // Saturate the blend color.
    if (Material.BlendTexturesCount > 0)
        textureColor = saturate(blendColor);

    // ===== REFLECTION TEXTURE ====
    // Calculate the projected reflection texture coordinates.
    //if (Material.IsReflective)
    //{
    //    //reflectionCoords.x = input.ReflectionPos.x / input.ReflectionPos.w / 2.0f + 0.5f;
    //    //reflectionCoords.y = -input.ReflectionPos.y / input.ReflectionPos.w / 2.0f + 0.5f;

    //    //// Sample the texture pixel from the reflection texture using the projected texture coordinates.
    //    //reflectionTextureColor = reflectionTexture.Sample(textureSampler, reflectionCoords);
    //    ////textureColor = lerp(textureColor, reflectionTextureColor, reflectivity);

    //    //// Do a linear interpolation between the two textures for a blend effect.
    //    //resultColor = lerp(textureColor, reflectionTextureColor, 0.15f);


    //    reflectionTextureColor = reflectionTexture.Sample(textureSampler, input.TexCoord);
    //    //reflectionTextureColor.a = reflectionTextureColor.a * reflectivity;
    //    //textureColor.xyz = saturate(textureColor * reflectionTextureColor).xyz;
    //    //textureColor.a = saturate(textureColor.a + reflectionTextureColor.a);
    //    textureColor = max(0, min(1, lerp(textureColor, reflectionTextureColor, Material.Reflectivity)));

    //    //clip(input.ReflectionClipZ);
    //}

    // Calculate the projected reflection texture coordinates.
    textureColor = CalculateReflection(Material, input, reflectionTexture, textureSampler, textureColor);
    // ===== REFLECTION TEXTURE ====

    // ==== ALPHA BLENDING: TODO
    //// Get the alpha value from the alpha map texture.
    //alphaValue = shaderTextures[2].Sample(SampleType, input.tex);
    //// Combine the two textures based on the alpha value.
    //blendColor = (alphaValue * color1) + ((1.0 - alphaValue) * color2);
    // ==== ALPHA BLENDING: TODO
     
    // ==== End multi texture blending.

    // ==== BUMP MAPPING
    bumpProps = CalculateBump(Material, input, bumpMapTexture, textureSampler);
    // ====  BUMP MAPPING

    _LightCalculationResult lightResult = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0, false };
    //[unroll]
    for (i = 0; i <= min(GlobalData.CurrentLights, MAX_LIGHTS); i++)
    {
        _LightCalculationResult tempLight = CalculateLight(Lights[i], Material, input, bumpProps, specularMapTexture, textureSampler);

        lightResult.Diffuse += tempLight.Diffuse;
        lightResult.Specular += tempLight.Specular;
    }

    resultColor = CalculateMaterialDiffuse(Material, GlobalData.GlobalAmbient, lightResult.Diffuse, lightResult.Specular);
    resultColor += CalculateMaterialSpecular(Material, specularMapTexture, textureSampler, Material.SpecularColor * lightResult.Specular, input.TexCoord);

    // Multiply the texture pixel and the final diffuse color to get the final pixel color result.
    if (Material.BlendTexturesCount > 0)
        resultColor = resultColor * textureColor;

    // Set alpha blending property.
    resultColor.a = Material.Opacity;

    // Make a processing of the result color.
    resultColor = ProcessFinalColor(resultColor);

    return saturate(resultColor);
}

#endif