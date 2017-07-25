#ifndef _PTToonImplementation
#include "PTToonImplementation.fx"
#endif

cbuffer MaterialConstantBuffer : register(b0)
{
    MaterialClass Material;
};

cbuffer GlobalLightingConstantBuffer : register(b1)
{
    float4 GlobalAmbient; // Global color to be applied to all elements.
    int CurrentLights; // Number of current lights.
};

cbuffer LightsConstantBuffer : register(b2) // 64 kb per ConstantBuffer MAX
{
    LightClass Lights[MAX_LIGHTS];
};

Texture2D<float4> shaderTextures[4] : register(t0);
Texture2D<float4> bumpMapTexture : register(t4);
Texture2D<float4> specularMapTexture : register(t5);
Texture2D<float4> reflectionTexture : register(t6);
//StructuredBuffer<_Light> Lights2 : register(t7); // Sending the lights in a StructuredBuffer will make gain more memory. A Constant buffer has a 64 KB restriction.
//StructuredBuffer<LightClass> Lights : register(t7);
SamplerState textureSampler : register(s0);

[earlydepthstencil]
float4 PixelShaderEntry(_PixelShader_IN input) : SV_TARGET
{
    input.TexCoord += MaterialAbstract.GetTextureTranslation();

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
    for (int i = 0; i < MaterialAbstract.GetBlendTexturesCount(); i++)
    {
        float4 col = shaderTextures[i].Sample(textureSampler, input.TexCoord);
        blendColor *= col;
    }

    if (MaterialAbstract.GetBlendTexturesCount() > 0)
    {
        if (MaterialAbstract.GetBlendTexturesCount() == 1)
            blendColor = blendColor * (MaterialAbstract.GetGamma() * 0.5f);
        else
            blendColor = blendColor * MaterialAbstract.GetGamma();
    }
    
    // Do not process pixel if it is not seen.
    if (blendColor.a <= 0.01)
        discard;

    // Saturate the blend color.
    if (MaterialAbstract.GetBlendTexturesCount() > 0)
        textureColor = saturate(blendColor);

    // ===== REFLECTION TEXTURE ====
    // Calculate the projected reflection texture coordinates.
    if (MaterialAbstract.GetIsReflective())
    {
        //reflectionCoords.x = input.ReflectionPos.x / input.ReflectionPos.w / 2.0f + 0.5f;
        //reflectionCoords.y = -input.ReflectionPos.y / input.ReflectionPos.w / 2.0f + 0.5f;

        //// Sample the texture pixel from the reflection texture using the projected texture coordinates.
        //reflectionTextureColor = reflectionTexture.Sample(textureSampler, reflectionCoords);
        ////textureColor = lerp(textureColor, reflectionTextureColor, reflectivity);

        //// Do a linear interpolation between the two textures for a blend effect.
        //resultColor = lerp(textureColor, reflectionTextureColor, 0.15f);



        reflectionTextureColor = reflectionTexture.Sample(textureSampler, input.TexCoord);
        //reflectionTextureColor.a = reflectionTextureColor.a * reflectivity;
        //textureColor.xyz = saturate(textureColor * reflectionTextureColor).xyz;
        //textureColor.a = saturate(textureColor.a + reflectionTextureColor.a);
        textureColor = max(0, min(1, lerp(textureColor, reflectionTextureColor, MaterialAbstract.GetReflectivity())));


        //clip(input.ReflectionClipZ);

    }
    // ===== REFLECTION TEXTURE ====

    // ==== ALPHA BLENDING: TODO
    //// Get the alpha value from the alpha map texture.
    //alphaValue = shaderTextures[2].Sample(SampleType, input.tex);
    //// Combine the two textures based on the alpha value.
    //blendColor = (alphaValue * color1) + ((1.0 - alphaValue) * color2);
    // ==== ALPHA BLENDING: TODO
     
    // ==== End multi texture blending.

    // ==== BUMP MAPPING
    bumpProps = MaterialAbstract.CalculateBump(bumpMapTexture, textureSampler, input.Tangent, input.Binormal, normal, input.TexCoord);
    // ====  BUMP MAPPING

    //_LightCalculationResult lightResult = LightsAbstract[0].CalculateLight(MaterialAbstract, input.WorldPos, input.ViewDirection,
    //                                            normal, bumpProps.BumpNormal, input.TexCoord, specularMapTexture, textureSampler);

    _LightCalculationResult lightResult = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0, false };
    //[unroll]
    for (i = 0; i < CurrentLights; i++)
    {
        _LightCalculationResult tempLight = LightsAbstract[i].CalculateLight(MaterialAbstract, input.WorldPos, input.ViewDirection,
                                                normal, bumpProps.BumpNormal, input.TexCoord, specularMapTexture, textureSampler);

        lightResult.Diffuse += tempLight.Diffuse;
        lightResult.Specular += tempLight.Specular;
    }

    float4 finalEmissive = MaterialAbstract.GetEmissiveColor();
    float4 finalAmbient = MaterialAbstract.GetAmbientColor() * GlobalAmbient;
    float4 finalDiffuse = MaterialAbstract.GetDiffuseColor() * lightResult.Diffuse;
    float4 finalSpecular = MaterialAbstract.GetSpecularColor() * lightResult.Specular;

    resultColor = (finalEmissive + finalAmbient + finalDiffuse);
    resultColor += MaterialAbstract.CalculateSpecular(specularMapTexture, textureSampler, finalSpecular, input.TexCoord);

    // Multiply the texture pixel and the final diffuse color to get the final pixel color result.
    if (MaterialAbstract.GetBlendTexturesCount() > 0)
        resultColor = resultColor * textureColor;

    // Set alpha blending property.
    resultColor.a = MaterialAbstract.GetOpacity();

    return saturate(resultColor);
}