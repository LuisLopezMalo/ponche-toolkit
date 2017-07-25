#ifndef _PTImplementationPS
#define _PTImplementationPS

#ifndef _PTStructs
#include "PTStructs.fx"
#endif

//Texture2D<float4> shaderTextures[4] : register(t0);
//Texture2D<float4> bumpMapTexture : register(t4);
//Texture2D<float4> specularMapTexture : register(t5);
//Texture2D<float4> reflectionTexture : register(t6);
//SamplerState textureSampler : register(s0);

// ================================ IMaterial methods ================================
_BumpProperties CalculateBump(MaterialStruct material, _PixelShader_IN input, Texture2D<float4> bumpMapTexture, SamplerState textureSampler)
{
    _BumpProperties result = { { 0, 0, 0 }, { 0, 0, 0, 0 } };
    if (!material.IsBump)
        return result;

    result.BumpMap = bumpMapTexture.Sample(textureSampler, input.TexCoord);
    result.BumpMap = (result.BumpMap * 2.0f) - 1.0f;
    result.BumpNormal = (result.BumpMap.x * input.Tangent) + (result.BumpMap.y * input.Binormal) + (result.BumpMap.z * input.Normal);
    result.BumpNormal = normalize(result.BumpNormal);

    return result;
}

float4 CalculateMaterialDiffuse(MaterialStruct material, float4 globalAmbient, float4 lightDiffuse, float4 lightSpecular)
{
    float4 finalEmissive = material.EmissiveColor;
    float4 finalAmbient = material.AmbientColor * globalAmbient;
    float4 finalDiffuse = material.DiffuseColor * lightDiffuse;
    float4 finalSpecular = material.SpecularColor * lightSpecular;

    float4 resultColor = (finalEmissive + finalAmbient + finalDiffuse);
    return resultColor;
}

float4 CalculateMaterialSpecular(MaterialStruct material, Texture2D specularMapTexture, SamplerState samplerState, float4 currentSpecular, float2 texCoord)
{
    float4 resultColor = float4(0, 0, 0, 0);
    if (material.IsSpecular)
    {
        float4 specularMapColor = float4(0, 0, 0, 0);
        if (material.HasSpecularMap)
        {
            specularMapColor = specularMapTexture.Sample(samplerState, texCoord);
            resultColor = (currentSpecular * specularMapColor);
        }
        else
            resultColor = currentSpecular;
    }

    return resultColor;
}
// ================================ IMaterial methods ================================


// Calculate the attenuation of a light using its Constant, Linear and Quadratic attenuation values.
float CalculateAtt(LightStruct light, float distance)
{
    //float d = max(distance - GetRange(), 0);
    //float denom = (d / GetRange()) + 1;
    //float attenuation = 1 / (denom * denom);
     
    //// scale and bias attenuation such that:
    ////   attenuation == 0 at extent of max influence
    ////   attenuation == 1 when d == 0
    //float cutoff = 0.005;
    //attenuation = (attenuation - cutoff) / (1 - cutoff);
    //attenuation = max(attenuation, 0);
    //return attenuation;

    //float b = (1 / (GetRange() * GetRange() * 0.01));
    //return 1.0f / (GetConstantAttenuation() + (GetLinearAttenuation() * distance) + (b * distance * distance));
    return 1.0f / (light.ConstAtt + (light.LineAtt * distance) + (light.QuadAtt * distance * distance));
    //return 1.0f - smoothstep(GetRange() * 0.6, GetRange(), distance);
}

// ================================ ILight methods ================================
// Calulcate the diffuse lighting.
float4 CalculateDiffuse(LightStruct light, MaterialStruct material, _PixelShader_IN input, _BumpProperties bumpProps, float3 lightVector, out float diffuseIntensity)
{
    float diffuse;
    if (material.IsBump)
        diffuse = max(0, dot(bumpProps.BumpNormal, lightVector));
    else
        diffuse = saturate(dot(input.Normal, lightVector));
    
    diffuseIntensity = max(0, diffuse);

    return light.Color * light.Intensity * diffuseIntensity;
}

// Calulcate the specular lighting.
float4 CalculateSpecular(LightStruct light, MaterialStruct material, _PixelShader_IN input, _BumpProperties bumpProps, float3 lightVector,
                            Texture2D<float4> specularMap, SamplerState textureSampler)
{
    float3 reflection;
    float dotProduct;

    //// Phong lighting.
    //if (mat.GetIsBump())
    //    reflection = normalize(reflect(-lightDirection, bumpNormal));
    //else
    //    reflection = normalize(reflect(-lightDirection, normal));
    //dotProduct = max(0, dot(reflection, viewDirection));

    // Blinn-Phong lighting
    reflection = normalize(lightVector + input.ViewDirection);
    if (material.IsBump)
        dotProduct = max(0, dot(bumpProps.BumpNormal, reflection));
    else
        dotProduct = max(0, dot(input.Normal, reflection));

    float4 result = light.Color * pow(dotProduct, material.SpecularPower);
    if (material.HasSpecularMap)
        result *= specularMap.Sample(textureSampler, input.TexCoord);

    return result * light.Intensity;
}

float CalculateSpotCone(LightStruct light, float3 lightVector)
{
    float minCos = cos(light.SpotAngle);
    float maxCos = (minCos + 1.0f) / 2.0f;
    float cosAngle = dot(light.Direction.xyz, -lightVector);
    return smoothstep(minCos, maxCos, cosAngle);
}

_LightCalculationResult CalculateDirectionalLight(LightStruct light, MaterialStruct material, _PixelShader_IN input, _BumpProperties bumpProps,
                            Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0, false };
 
    float3 lightVector = -light.Direction.xyz;
 
    result.Diffuse = CalculateDiffuse(light, material, input, bumpProps, lightVector, result.DiffuseIntensity);
    if (material.IsSpecular)
        result.Specular = CalculateSpecular(light, material, input, bumpProps, lightVector, specularMap, textureSampler);
 
    return result;
}

_LightCalculationResult CalculatePointLight(LightStruct light, MaterialStruct material, _PixelShader_IN input, _BumpProperties bumpProps, 
                            Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0, false };

    float3 lightVector = normalize(light.Position - input.WorldPos).xyz;
    float distance = length(lightVector);
    lightVector = lightVector / distance;
 
    float attenuation = CalculateAtt(light, distance);
 
    result.Diffuse = CalculateDiffuse(light, material, input, bumpProps, lightVector, result.DiffuseIntensity) * attenuation;
    if (material.IsSpecular)
        result.Specular = CalculateSpecular(light, material, input, bumpProps, lightVector, specularMap, textureSampler) * attenuation;
 
    return result;
}

_LightCalculationResult CalculateSpotLight(LightStruct light, MaterialStruct material, _PixelShader_IN input, _BumpProperties bumpProps, 
                            Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0, false };

    float3 lightVector = normalize(light.Position - input.WorldPos).xyz;
    //float3 lightVector = (Position - position).xyz;
    float distance = length(lightVector);
    lightVector = lightVector / distance;
 
    float attenuation = CalculateAtt(light, distance);
    float spotIntensity = CalculateSpotCone(light, lightVector);
 
    result.Diffuse = CalculateDiffuse(light, material, input, bumpProps, lightVector, result.DiffuseIntensity) * attenuation * spotIntensity;
    if (material.IsSpecular)
        result.Specular = CalculateSpecular(light, material, input, bumpProps, lightVector, specularMap, textureSampler) * attenuation * spotIntensity;

    return result;
}

_LightCalculationResult CalculateLight(LightStruct light, MaterialStruct material, _PixelShader_IN input, _BumpProperties bumpProps, Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0, false };
    float dist = 0;

    if (light.IsEnabled)
    {
        switch (light.Type)
        {
            case DIRECTIONAL_LIGHT:
                result = CalculateDirectionalLight(light, material, input, bumpProps, specularMap, textureSampler);
                break;
            case POINT_LIGHT:
                dist = distance(light.Position, input.WorldPos);
                if (dist > light.Range)
                {
                    result.Discarded = true;
                    return result;
                }
                result = CalculatePointLight(light, material, input, bumpProps, specularMap, textureSampler);
                break;
            case SPOT_LIGHT:
                dist = distance(light.Position, input.WorldPos);
                if (dist > light.Range)
                {
                    result.Discarded = true;
                    return result;
                }
                result = CalculateSpotLight(light, material, input, bumpProps, specularMap, textureSampler);
                break;
        }
    }

    //result.Diffuse = float4(0.6, 0.6, 0.6, 0.4f);

    return result;
};

float4 CalculateReflection(MaterialStruct material, _PixelShader_IN input, Texture2D<float4> reflectionTexture, SamplerState textureSampler, float4 textureColor)
{
    if (!material.IsReflective)
        return textureColor;
    
    float4 reflectionTextureColor = reflectionTexture.Sample(textureSampler, input.TexCoord);
    textureColor = max(0, min(1, lerp(textureColor, reflectionTextureColor, material.Reflectivity)));

    return textureColor;
}

// ========================== GLOBAL DATA ==========================
float4 ProcessFinalColor(float4 finalColor)
{
    //float gray = ((0.2126 * finalColor.r) + (0.7152 * finalColor.g) + (0.0722 * finalColor.b));
    //return float4(gray, gray, gray, 1);

    return finalColor;
}
#endif