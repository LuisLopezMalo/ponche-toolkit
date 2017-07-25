#ifndef _PTDeclarations
#include "PTDeclarations.fx"
#endif

#ifndef _PTLightingHelper
#include "PTLightingHelper.fx"
#endif

#ifndef _PTBumpMapped
#include "PTBumpMapped.fx"
#endif

// ======== Vertex Shader Constant Buffers ========
cbuffer MatrixBuffer : register(c0)
{
    matrix World;
    matrix ViewProjection;
    matrix WorldViewProjection;
    float3 CameraPosition;
};

cbuffer ClippingBuffer : register(c1)
{
    float4 ClipPlane;
};

cbuffer ReflectionBuffer : register(c2)
{
    matrix ReflectionMatrixProjectionWorld;
    matrix ParaboloidView;
    float Direction;
    float NearPlane;
    float FarPlane;
    float padding;
};
// ======== Vertex Shader Constant Buffers ===================

// ======== Pixel Shader Constant Buffers ====================
cbuffer MaterialBuffer : register(c0)
{
    _Material Material;
};

cbuffer GlobalLightingBuffer : register(c1)
{
    float4 GlobalAmbient; // Global color to be applied to all elements.
};

cbuffer LightBuffer : register(c2)
{
    _Light Lights[MAX_LIGHTS];
};
// ======== Pixel Shader Constant Buffers ========

//Texture2DArray<float4> shaderTextures : register(t0);
Texture2D<float4> shaderTextures[4] : register(t0);
Texture2D<float4> specularMapTexture : register(t5);
Texture2D<float4> reflectionTexture : register(t6);
//StructuredBuffer<_Light> Lights2 : register(t7); // Sending the lights in a StructuredBuffer will make gain more memory. A Constant buffer has a 512 KB restriction.
SamplerState textureSampler : register(s0);

[clipplanes(ClipPlane)]
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

    // Reflection
    output.ReflectionClipZ = 0;
    if (Material.IsReflective)
    {
        output.Position.z = output.Position.z * Direction;
	
        float L = length(output.Position.xyz); // determine the distance between (0,0,0) and the vertex
        output.Position = output.Position / L; // divide the vertex position by the distance 
	
        output.ReflectionClipZ = output.Position.z; // remember which hemisphere the vertex is in
        output.Position.z = output.Position.z + 1; // add the reflected vector to find the normal vector

        output.Position.x = output.Position.x / output.Position.z; // divide x coord by the new z-value
        output.Position.y = output.Position.y / output.Position.z; // divide y coord by the new z-value

        output.Position.z = (L - NearPlane) / (FarPlane - NearPlane); // scale the depth to [0, 1]
        output.Position.w = 1;
    }

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
    //output.Clip = dot(mul(input.Position, World), Clip);

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

    float4 blendColor = float4(1, 1, 1, 1);
    float3 normal = normalize(input.Normal);
    _BumpProperties bumpProps;
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
    
    // Do not process pixel if it is not seen.
    if (blendColor.a <= 0)
        discard;

    // Saturate the blend color.
    if (Material.BlendTexturesCount > 0)
        textureColor = saturate(blendColor);

    // ===== REFLECTION TEXTURE ====
    // Calculate the projected reflection texture coordinates.
    if (Material.IsReflective)
    {
        //reflectionCoords.x = input.ReflectionPos.x / input.ReflectionPos.w / 2.0f + 0.5f;
        //reflectionCoords.y = -input.ReflectionPos.y / input.ReflectionPos.w / 2.0f + 0.5f;

        //// Sample the texture pixel from the reflection texture using the projected texture coordinates.
        //reflectionTextureColor = reflectionTexture.Sample(textureSampler, reflectionCoords);
        ////textureColor = lerp(textureColor, reflectionTextureColor, reflectivity);

        //// Do a linear interpolation between the two textures for a blend effect.
        //resultColor = lerp(textureColor, reflectionTextureColor, 0.15f);



        //reflectionTextureColor = reflectionTexture.Sample(textureSampler, input.TexCoord);
        ////reflectionTextureColor.a = reflectionTextureColor.a * reflectivity;
        ////textureColor.xyz = saturate(textureColor * reflectionTextureColor).xyz;
        ////textureColor.a = saturate(textureColor.a + reflectionTextureColor.a);
        //textureColor = max(0, min(1, lerp(textureColor, reflectionTextureColor, Material.Reflectivity)));


        clip(input.ReflectionClipZ);

    }
    // ===== REFLECTION TEXTURE ====

    // ==== BUMP MAPPING
    if (Material.IsBump)
        bumpProps = CalculateBump(bumpMapTexture, textureSampler, input.Tangent, input.Binormal, normal, input.TexCoord);
    // ====  BUMP MAPPING

    _LightCalculationResult lightResult = CalculateLights(Material, Lights, input.WorldPos, input.ViewDirection,
                                                normal, bumpProps.BumpNormal, input.TexCoord, specularMapTexture, textureSampler);

    float4 finalEmissive = Material.EmissiveColor;
    float4 finalAmbient = Material.AmbientColor * GlobalAmbient;
    float4 finalSpecular = Material.SpecularColor * lightResult.Specular;
    //float4 finalDiffuse = Material.DiffuseColor * lightResult.Diffuse;

    float4 diff = float4(0,0,0,0);
    if (lightResult.DiffuseIntensity > 0.9)
        diff = lightResult.Diffuse * 1;
    if (lightResult.DiffuseIntensity > 0.5)
        diff = lightResult.Diffuse * 0.5;
    if (lightResult.DiffuseIntensity > 0.1)
        diff = lightResult.Diffuse * 0.2;
    else
        diff = lightResult.Diffuse * 0.1;
    float4 finalDiffuse = Material.DiffuseColor * diff;

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

    // Set alpha blending property.
    resultColor.a = Material.Opacity;
    
    resultColor.r = 1;
    resultColor.g = 1;
    resultColor.b = 1;
    resultColor.a = 0.5;

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