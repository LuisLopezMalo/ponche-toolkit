#ifndef _PTLightingHelper
#define _PTLightingHelper

#ifndef _PTDeclarations
#include "PTDeclarations.fx"
#endif

// Calculate the attenuation of a light using its Constant, Linear and Quadratic attenuation values.
float CalculateAttenuation(_Light light, float distance)
{
    return 1.0f / (light.ConstantAttenuation) + (light.LinearAttenuation * distance) + (light.QuadraticAttenuation * distance * distance);
    //return 1.0f - smoothstep(light.Range * 0.6, light.Range, distance);
}

// Calulcate the diffuse lighting.
float4 CalculateDiffuseLighting(_Material mat, _Light light, float3 lightDirection, float3 normal, float3 bumpNormal, out float diffuseIntensity)
{
    float diffuse;

    if (mat.IsBump)
        diffuse = max(0, dot(bumpNormal, lightDirection));
    else
        diffuse = saturate(dot(normal, lightDirection));
    
    diffuseIntensity = diffuse;

    return light.Color * diffuse * light.Intensity;
}

// Calulcate the specular lighting.
float4 CalculateSpecularLighting(_Material mat, _Light light, float3 viewDirection, float3 lightDirection, float3 normal, float3 bumpNormal, float2 texCoord,
                            Texture2D<float4> specularMap, SamplerState textureSampler)
{
    float3 reflection;
    float dotProduct;

    // Phong lighting.
    if (mat.IsBump)
        reflection = normalize(reflect(-lightDirection, bumpNormal));
    else
        reflection = normalize(reflect(-lightDirection, normal));

    dotProduct = max(0, dot(reflection, viewDirection));

    //// Blinn-Phong lighting
    ////float3 H = normalize(lightDirection + viewDirection);
    ////float NdotH = max(0, dot(normal, H));
    //reflection = normalize(lightDirection + viewDirection);
    //if (mat.IsBump)
    //    dotProduct = max(0, dot(bumpNormal, reflection));
    //else
    //    dotProduct = max(0, dot(normal, reflection));

    float4 result = light.Color * pow(dotProduct, mat.SpecularPower);

    if (mat.HasSpecularMap)
        result *= specularMap.Sample(textureSampler, texCoord);

    return result * light.Intensity;
}

float CalculateSpotCone(_Light light, float3 lightVector)
{
    float minCos = cos(light.SpotAngle);
    float maxCos = (minCos + 1.0f) / 2.0f;
    float cosAngle = dot(light.Direction.xyz, -lightVector);
    return smoothstep(minCos, maxCos, cosAngle);
}

_LightCalculationResult CalculateNewDirectionalLight(_Material mat, _Light light, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result;
    result.Diffuse = float4(0, 0, 0, 0);
    result.Specular = float4(0, 0, 0, 0);
 
    float3 lightVector = -light.Direction.xyz;
 
    result.Diffuse = CalculateDiffuseLighting(mat, light, lightVector, normal, bumpNormal, result.DiffuseIntensity);
    if (mat.IsSpecular)
        result.Specular = CalculateSpecularLighting(mat, light, viewDirection, lightVector, normal, bumpNormal, texCoord, specularMap, textureSampler);
 
    return result;
}

_LightCalculationResult CalculateNewPointLight(_Material mat, _Light light, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result;
    result.Diffuse = float4(0, 0, 0, 0);
    result.Specular = float4(0, 0, 0, 0);
        
    float3 lightVector = normalize(light.Position - position).xyz;
    float distance = length(lightVector);
    lightVector = lightVector / distance;
 
    float attenuation = CalculateAttenuation(light, distance);
 
    result.Diffuse = CalculateDiffuseLighting(mat, light, lightVector, normal, bumpNormal, result.DiffuseIntensity) * attenuation;
    if (mat.IsSpecular)
        result.Specular = CalculateSpecularLighting(mat, light, viewDirection, lightVector, normal, bumpNormal, texCoord, specularMap, textureSampler) * attenuation;
 
    return result;
}

_LightCalculationResult CalculateNewSpotLight(_Material mat, _Light light, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result;
    result.Diffuse = float4(0, 0, 0, 0);
    result.Specular = float4(0, 0, 0, 0);
 
    float3 lightVector = (light.Position - position).xyz;
    float distance = length(lightVector);
    lightVector = lightVector / distance;
 
    float attenuation = CalculateAttenuation(light, distance);
    float spotIntensity = CalculateSpotCone(light, lightVector);
 
    result.Diffuse = CalculateDiffuseLighting(mat, light, lightVector, normal, bumpNormal, result.DiffuseIntensity) * attenuation * spotIntensity;
    if (mat.IsSpecular)
        result.Specular = CalculateSpecularLighting(mat, light, viewDirection, lightVector, normal, bumpNormal, texCoord, specularMap, textureSampler) * attenuation * spotIntensity;
 
    return result;
}

_LightCalculationResult CalculateLights(_Material mat, _Light lights[MAX_LIGHTS], float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult finalResult = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0 };

    [unroll] // With unroll the shader uses more instructions slots (aprox 755) so the performance increase but use more memory.
    for (int i = 0; i < MAX_LIGHTS; ++i)
    {
        _Light light = lights[i];
        if (light.IsEnabled)
        {
            float dist = distance(light.Position, position);
            if (dist > light.Range)
                return finalResult;

            _LightCalculationResult lightResult = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0 };
         
            switch (light.Type)
            {
                case DIRECTIONAL_LIGHT:
                    lightResult = CalculateNewDirectionalLight(mat, light, position, viewDirection, normal, bumpNormal, texCoord, specularMap, textureSampler);
                    break;
                case POINT_LIGHT:
                    lightResult = CalculateNewPointLight(mat, light, position, viewDirection, normal, bumpNormal, texCoord, specularMap, textureSampler);
                    break;
                case SPOT_LIGHT:
                    lightResult = CalculateNewSpotLight(mat, light, position, viewDirection, normal, bumpNormal, texCoord, specularMap, textureSampler);
                    break;
            }
            finalResult.Diffuse += lightResult.Diffuse;
            finalResult.Specular += lightResult.Specular;
        }
    }
 
    finalResult.Diffuse = saturate(finalResult.Diffuse);
    finalResult.Specular = saturate(finalResult.Specular);
 
    return finalResult;
}
#endif