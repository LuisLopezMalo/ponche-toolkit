#ifndef _PTClusteredHelper
#include "PTClusteredHelper.fx"
#endif

#ifndef _PTDeclarations
#include "PTDeclarations.fx"
#endif

#ifndef _PTLightingHelper
#include "PTLightingHelper.fx"
#endif

// ==== Vertex Shader Constant Buffers
//cbuffer MatrixBuffer : register(b0)
//{
//    matrix World;
//    //matrix WorldView;
//    matrix ViewProjection;
//    matrix WorldViewProjection;
//    //matrix InverseTransposeWorld;
//    float3 CameraPosition;
//};

cbuffer MatrixBuffer : register(b0)
{
    matrix World;
    //matrix WorldView;
    matrix ViewProjection;
    matrix WorldViewProjection;
    //matrix InverseTransposeWorld;
    float3 CameraPosition;
};

cbuffer ClippingBuffer : register(b1)
{
    float4 Clip;
};

cbuffer ReflectionBuffer : register(b2)
{
    matrix ReflectionMatrixProjectionWorld;
};

cbuffer ClusteredConstantsBuffer : register(b3)
{
    float2 bufferSize;
    float2 inverseBufferSize;
    float Near; // The near plane from the camera.
    float NearLog; // The near plane using the logarithmic function to compute cluster index.  -- 1 / log(sD + 1)
};
// ======== Vertex Shader Constant Buffers ===================

// ======== Pixel Shader Constant Buffers ====================
cbuffer MaterialBuffer : register(b0)
{
    _Material Material;
};

cbuffer GlobalLightingBuffer : register(b1)
{
    float4 GlobalAmbient; // Global color to be applied to all elements.
};

cbuffer LightBuffer : register(b2)
{
    _Light Lights[MAX_LIGHTS];
};
// ==== Pixel Shader Constant Buffers

//Texture2DArray<float4> shaderTextures : register(t0);
Texture2D<float4> shaderTextures[4] : register(t0);
Texture2D<float4> bumpMapTexture : register(t4);
Texture2D<float4> specularMapTexture : register(t5);
Texture2D<float4> reflectionTexture : register(t6);
//StructuredBuffer<_Light> Lights2 : register(t7); // Sending the lights in a StructuredBuffer will make gain more memory. A Constant buffer has a 512 KB restriction.
SamplerState textureSampler : register(s0);

_PixelShader_IN VertexShaderEntry(_AppData_IN input)
{
    _PixelShader_IN output = (_PixelShader_IN) 0;

    input.Position.w = 1.0f;

    // Calculate te wvp matrices here. (using the GPU).
    //output.pos = mul(input.pos, world);
    //output.pos = mul(output.pos, view);
    //output.pos = mul(output.pos, projection);

    // Matrices calculated using the CPU.
    output.Position = mul(input.Position, WorldViewProjection);
    output.WorldPos = mul(input.Position, World);
    output.ReflectionPos = mul(input.Position, ReflectionMatrixProjectionWorld);

    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    // Calculate the normal vector against the world matrix only.
    //output.normal = mul(input.normal, (float3x3) world);
	
    // Normalize the normal vector.
    //output.Normal = normalize(mul((float3x3) inverseTransposeWorld, input.normal));
    output.Normal = normalize(mul(input.Normal, (float3x3) World));

    // Calculate the tangent.
    output.Tangent = mul(input.Tangent, (float3x3) World);
    output.Tangent = normalize(output.Tangent);

    // Calculate the binormal.
    output.Binormal = mul(input.Binormal, (float3x3) World);
    output.Binormal = normalize(output.Binormal);

    // Determine the viewing direction based on the position of the camera and the position of the vertex in the world.
    output.ViewDirection = normalize(CameraPosition.xyz - output.Position.xyz);

    // Set the clipping plane.
    output.Clip = dot(mul(input.Position, World), Clip);

    return output;
}

[earlydepthstencil]
float4 PixelShaderEntry(_PixelShader_IN input) : SV_TARGET
{
    input.TexCoord += Material.TextureTranslation;

    float4 textureColor;
    float lightIntensity;
    
    float2 reflectionCoords;
    float4 reflectionTextureColor;

    float4 blendColor;
    float3 normal = normalize(input.Normal);
    float4 bumpMap;
    float3 bumpNormal = float3(0, 0, 0);
    float4 resultColor = float4(0, 0, 0, 0);

    // ==== Multi texture
    // Get the pixel color from the sent textures. === WITH COMMON TEXTURE ARRAY -- [4]
    for (int i = 0; i < Material.BlendTexturesCount; i++)
    {
        float4 col = shaderTextures[i].Sample(textureSampler, input.TexCoord);
        blendColor *= col;
    }

    if (Material.BlendTexturesCount > 0)
    {
        if (Material.BlendTexturesCount == 1)
            blendColor = blendColor * (Material.Gamma * 0.5f);
        else
            blendColor = blendColor * Material.Gamma;
    }
    
    // Do not process pixel.
    if (blendColor.a <= 0)
        discard;

    // Saturate the blend color.
    if (Material.BlendTexturesCount > 0)
        textureColor = saturate(blendColor);

    // ===== REFLECTION TEXTURE ====
    // Calculate the projected reflection texture coordinates.
    if (Material.IsReflective)
    {
        //reflectionCoords.x = input.reflectionPos.x / input.reflectionPos.w / 2.0f + 0.5f;
        //reflectionCoords.y = -input.reflectionPos.y / input.reflectionPos.w / 2.0f + 0.5f;

        //// Sample the texture pixel from the reflection texture using the projected texture coordinates.
        //reflectionTextureColor = reflectionTexture.Sample(textureSampler, reflectionCoords);
        //textureColor = lerp(textureColor, reflectionTextureColor, reflectivity);

        reflectionTextureColor = reflectionTexture.Sample(textureSampler, input.TexCoord);
        //reflectionTextureColor.a = reflectionTextureColor.a * reflectivity;
        //textureColor.xyz = saturate(textureColor * reflectionTextureColor).xyz;
        //textureColor.a = saturate(textureColor.a + reflectionTextureColor.a);
        textureColor = max(0, min(1, lerp(textureColor, reflectionTextureColor, Material.Reflectivity)));

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
    if (Material.IsBump)
    {
        bumpMap = bumpMapTexture.Sample(textureSampler, input.TexCoord);
        bumpMap = (bumpMap * 2.0f) - 1.0f;
        bumpNormal = (bumpMap.x * input.Tangent) + (bumpMap.y * input.Binormal) + (bumpMap.z * normal);
        bumpNormal = normalize(bumpNormal);
    }
    // ====  BUMP MAPPING

    _LightCalculationResult lightResult = CalculateLights(Material, Lights, input.WorldPos, input.ViewDirection,
                                                normal, bumpNormal, input.TexCoord, specularMapTexture, textureSampler);

    float4 finalEmissive = Material.EmissiveColor;
    float4 finalAmbient = Material.AmbientColor * GlobalAmbient;
    float4 finalDiffuse = Material.DiffuseColor * lightResult.Diffuse;
    float4 finalSpecular = Material.SpecularColor * lightResult.Specular;

    resultColor = (finalEmissive + finalAmbient + finalDiffuse);
    if (Material.IsSpecular)
    {
        float4 specularMapColor = float4(0, 0, 0, 0);
        if (Material.HasSpecularMap)
        {
            specularMapColor = specularMapTexture.Sample(textureSampler, input.TexCoord);
            resultColor += (finalSpecular * specularMapColor);
        }else
            resultColor += finalSpecular;
    }


    // Multiply the texture pixel and the final diffuse color to get the final pixel color result.
    if (Material.BlendTexturesCount > 0)
        resultColor = resultColor * textureColor;

    // To test alpha blending
    resultColor.a = Material.Opacity;

    return saturate(resultColor);
}

technique11 Render
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VertexShaderEntry()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PixelShaderEntry()));
    }
}