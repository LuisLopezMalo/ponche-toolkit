#ifndef _PTDeclarations
#include "PTDeclarations.fx"
#endif

#ifndef _PTLightingHelper
#include "PTLightingHelper.fx"
#endif

// ==== Vertex Shader Constant Buffers
cbuffer MatrixBuffer : register(b0)
{
    matrix World;
    matrix ViewProjection;
    matrix WorldViewProjection;
    matrix InverseTransposeWorld;
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

cbuffer LightBuffer2 : register(b2)
{
    _Light Lights[MAX_LIGHTS];
};
// ==== Pixel Shader Constant Buffers

//Texture2DArray<float4> shaderTextures : register(t0);
Texture2D<float4> shaderTextures[4] : register(t0);
Texture2D<float4> bumpMapTexture : register(t4);
Texture2D<float4> specularMapTexture : register(t5);
Texture2D<float4> reflectionTexture : register(t6);
StructuredBuffer<_Light> Lights2 : register(t7); // Sending the lights in a StructuredBuffer will make gain more memory. A Constant buffer has a 512 KB restriction.
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
    //output.normal = normalize(output.normal);
    //output.normal = normalize((mul((float3x3) inverseTransposeWorld, input.normal)));
    output.Normal = normalize((mul(input.Normal, (float3x3) InverseTransposeWorld)));

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
    if (Material.BlendTexturesCount == 4)
    {
        float4 color1 = shaderTextures[0].Sample(textureSampler, input.TexCoord);
        float4 color2 = shaderTextures[1].Sample(textureSampler, input.TexCoord);
        float4 color3 = shaderTextures[2].Sample(textureSampler, input.TexCoord);
        float4 color4 = shaderTextures[3].Sample(textureSampler, input.TexCoord);
        blendColor = color1 * color2 * color3 * color4 * Material.Gamma;
    }
    else if (Material.BlendTexturesCount == 3)
    {
        float4 color1 = shaderTextures[0].Sample(textureSampler, input.TexCoord);
        float4 color2 = shaderTextures[1].Sample(textureSampler, input.TexCoord);
        float4 color3 = shaderTextures[2].Sample(textureSampler, input.TexCoord);
        blendColor = color1 * color2 * color3 * Material.Gamma;
    }
    else if (Material.BlendTexturesCount == 2)
    {
        float4 color1 = shaderTextures[0].Sample(textureSampler, input.TexCoord);
        float4 color2 = shaderTextures[1].Sample(textureSampler, input.TexCoord);
        blendColor = color1 * color2 * Material.Gamma;
    }
    else if (Material.BlendTexturesCount == 1)
    {
        float4 color1 = shaderTextures[0].Sample(textureSampler, input.TexCoord);
        blendColor = color1 * (Material.Gamma * 0.5f);
    }

    //// Get the pixel color from the sent textures. === WITH TEXTURE2DARRAY - the z value is the texture index.
    //if (Material.BlendTexturesCount == 4)
    //{
    //    float4 color1 = shaderTextures.Sample(textureSampler, float3(input.TexCoord, 0));
    //    float4 color2 = shaderTextures.Sample(textureSampler, float3(input.TexCoord, 1));
    //    float4 color3 = shaderTextures.Sample(textureSampler, float3(input.TexCoord, 2));
    //    float4 color4 = shaderTextures.Sample(textureSampler, float3(input.TexCoord, 3));
    //    blendColor = color1 * color2 * color3 * color4 * gamma;
    //}
    //else if (Material.BlendTexturesCount == 3)
    //{
    //    float4 color1 = shaderTextures.Sample(textureSampler, float3(input.TexCoord, 0));
    //    float4 color2 = shaderTextures.Sample(textureSampler, float3(input.TexCoord, 1));
    //    float4 color3 = shaderTextures.Sample(textureSampler, float3(input.TexCoord, 2));
    //    blendColor = color1 * color2 * color3 * gamma;
    //}
    //else if (Material.BlendTexturesCount == 2)
    //{
    //    float4 color1 = shaderTextures.Sample(textureSampler, float3(input.TexCoord, 0));
    //    float4 color2 = shaderTextures.Sample(textureSampler, float3(input.TexCoord, 1));
    //    blendColor = color1 * color2 * gamma;
    //}
    //else if (Material.BlendTexturesCount == 1)
    //{
    //    float4 color1 = shaderTextures.Sample(textureSampler, float3(input.TexCoord, 0));
    //    blendColor = color1 * (gamma * 0.5f);
    //}

    
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

    float4 specularMapColor = float4(0, 0, 0, 0);
    if (Material.HasSpecularMap)
        specularMapColor = specularMapTexture.Sample(textureSampler, input.TexCoord);

    //float diffuseIntensity;
    //// Calculate all the lights.
    //for (int i = 0; i < directionalLightsCount; i++)
    //{
    //    _DirectionalLight light;
    //    float4 specularTemp = float4(0.0f, 0.0f, 0.0f, 0.0f);
    //    diffuseIntensity = 0;

    //    if (i == 0)
    //        light = directionalLight1;
    //    else if (i == 1)
    //        light = directionalLight2;
    //    else if (i == 2)
    //        light = directionalLight3;
    //    else if (i == 3)
    //        light = directionalLight4;

    //    float4 res = CalculateDirectionalLight(light, input.Normal, bumpNormal, input.ViewDirection, 
    //        specularPower, isBump, isSpecular, hasSpecularMap,
    //        specularMapColor, specularTemp, diffuseIntensity);

    //    resultColor += res + specularTemp;
    //}

    //// Calculate the point lights.
    //for (int j = 0; j < pointLightsCount; j++)
    //{
    //    _PointLight light;
    //    float4 ambient = float4(0, 0, 0, 0);
    //    float4 diffuse = float4(0, 0, 0, 0);
    //    float4 spec = float4(0, 0, 0, 0);

    //    if (j == 0)
    //        light = pointLight1;
    //    else if (j == 1)
    //        light = pointLight2;
    //    else if (j == 2)
    //        light = pointLight3;
    //    else if (j == 3)
    //        light = pointLight4;

    //    CalculatePointLight(light, input.Position, input.Normal, input.ViewDirection, isBump, bumpNormal, isSpecular, hasSpecularMap,
    //        specularMapColor, ambient, diffuse, spec);

    //    //resultColor += res + specularTemp;

    //    //resultColor += saturate(spec + ambient + diffuse);
    //    //resultColor += spec + ambient + diffuse;
    //    resultColor += diffuse;
    //}



    _LightCalculationResult lightResult = CalculateLights(Material, Lights, input.WorldPos, input.ViewDirection,
                                                normal, bumpNormal);

    //_Light Light = Lights[0];
    //_LightCalculationResult lightResult;
    //if (Light.Type == 0)
    //{
    //    lightResult = CalculateNewDirectionalLight(Material, Light, input.WorldPos, input.ViewDirection,
    //                                            normal, bumpNormal);
    //}
    //else if (Light.Type == 1)
    //{
    //    lightResult = CalculateNewPointLight(Material, Light, input.WorldPos, input.ViewDirection,
    //                                            normal, bumpNormal);
    //}
    //else if (Light.Type == 2)
    //{
    //    lightResult = CalculateNewSpotLight(Material, Light, input.WorldPos, input.ViewDirection,
    //                                            normal, bumpNormal);
    //}

    float4 finalEmissive = Material.EmissiveColor;
    float4 finalAmbient = Material.AmbientColor * GlobalAmbient;
    float4 finalDiffuse = Material.DiffuseColor * lightResult.Diffuse;
    float4 finalSpecular = Material.SpecularColor * lightResult.Specular;

    resultColor = (finalEmissive + finalAmbient + finalDiffuse);
    if (Material.IsSpecular)
        resultColor += finalSpecular;


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