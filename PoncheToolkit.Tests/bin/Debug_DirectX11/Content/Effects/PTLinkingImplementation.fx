#ifndef _PTLinkingImplementation
#define _PTLinkingImplementation

#ifndef _PTLinkingDeclarations
#include "PTLinkingDeclarations.fx"
#endif

#ifndef _PTLinkingClasses
#include "PTLinkingClasses.fx"
#endif

#ifndef _PTLinkingBuffersPS
#include "PTLinkingBuffersPS.fx"
#endif

// ================================ IMaterial methods ================================
_BumpProperties MaterialClass::CalculateBump(Texture2D<float4> bumpMapTexture, SamplerState textureSampler, float3 tangent, float3 binormal, float3 normal, float2 texCoord)
{
    _BumpProperties result = { { 0, 0, 0 }, { 0, 0, 0, 0 } };
    if (!GetIsBump())
        return result;

    result.BumpMap = bumpMapTexture.Sample(textureSampler, texCoord);
    result.BumpMap = (result.BumpMap * 2.0f) - 1.0f;
    result.BumpNormal = (result.BumpMap.x * tangent) + (result.BumpMap.y * binormal) + (result.BumpMap.z * normal);
    result.BumpNormal = normalize(result.BumpNormal);

    return result;
}

float4 MaterialClass::CalculateSpecular(Texture2D specularMapTexture, SamplerState samplerState, float4 currentSpecular, float2 texCoord)
{
    float4 resultColor = float4(0, 0, 0, 0);
    if (GetIsSpecular())
    {
        float4 specularMapColor = float4(0, 0, 0, 0);
        if (GetHasSpecularMap())
        {
            specularMapColor = specularMapTexture.Sample(samplerState, texCoord);
            resultColor = (currentSpecular * specularMapColor);
        }
        else
            resultColor = currentSpecular;
    }

    return resultColor;
}

float4 MaterialClass::CalculateDiffuse(float4 globalAmbient, float4 lightDiffuse, float4 lightSpecular)
{
    float4 finalEmissive = GetEmissiveColor();
    float4 finalAmbient = GetAmbientColor() * globalAmbient;
    float4 finalDiffuse = GetDiffuseColor() * lightDiffuse;
    float4 finalSpecular = GetSpecularColor() * lightSpecular;

    float4 resultColor = (finalEmissive + finalAmbient + finalDiffuse);
    return resultColor;
}
// ================================ IMaterial methods ================================


// ================================ ILight methods ================================
_LightCalculationResult LightClass::CalculateLight(IMaterial mat, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0, false };
    float dist = 0;

    if (IsEnabled)
    {
        switch (Type)
        {
            case DIRECTIONAL_LIGHT:
                result = CalculateDirectionalLight(mat, position, viewDirection, normal, bumpNormal, texCoord, specularMap, textureSampler);
                break;
            case POINT_LIGHT:
                dist = distance(Pos, position);
                if (dist > Range)
                {
                    result.Discarded = true;
                    return result;
                }
                result = CalculatePointLight(mat, position, viewDirection, normal, bumpNormal, texCoord, specularMap, textureSampler);
                break;
            case SPOT_LIGHT:
                dist = distance(Pos, position);
                if (dist > Range)
                {
                    result.Discarded = true;
                    return result;
                }
                result = CalculateSpotLight(mat, position, viewDirection, normal, bumpNormal, texCoord, specularMap, textureSampler);
                break;
        }
    }

    return result;
};

// Calulcate the diffuse lighting.
float4 LightClass::CalculateDiffuse(IMaterial mat, float3 lightDirection, float3 normal, float3 bumpNormal, out float diffuseIntensity)
{
    float diffuse;

    if (mat.GetIsBump())
        diffuse = max(0, dot(bumpNormal, lightDirection));
    else
        diffuse = saturate(dot(normal, lightDirection));
    
    diffuseIntensity = max(0, diffuse);

    return Col * Intensity * diffuseIntensity;
}

// Calulcate the specular lighting.
float4 LightClass::CalculateSpecular(IMaterial mat, float3 viewDirection, float3 lightDirection, float3 normal, float3 bumpNormal, float2 texCoord,
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
    reflection = normalize(lightDirection + viewDirection);
    if (mat.GetIsBump())
        dotProduct = max(0, dot(bumpNormal, reflection));
    else
        dotProduct = max(0, dot(normal, reflection));

    float4 result = Col * pow(dotProduct, mat.GetSpecularPower());
    if (mat.GetHasSpecularMap())
        result *= specularMap.Sample(textureSampler, texCoord);

    return result * Intensity;
}

float LightClass::CalculateSpotCone(float3 lightVector)
{
    float minCos = cos(SpotAngle);
    float maxCos = (minCos + 1.0f) / 2.0f;
    float cosAngle = dot(Dir.xyz, -lightVector);
    return smoothstep(minCos, maxCos, cosAngle);
}

_LightCalculationResult LightClass::CalculateDirectionalLight(IMaterial mat, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0, false };
 
    float3 lightVector = -Dir.xyz;
 
    result.Diffuse = CalculateDiffuse(mat, lightVector, normal, bumpNormal, result.DiffuseIntensity);
    if (mat.GetIsSpecular())
        result.Specular = CalculateSpecular(mat, viewDirection, lightVector, normal, bumpNormal, texCoord, specularMap, textureSampler);
 
    return result;
}

_LightCalculationResult LightClass::CalculatePointLight(IMaterial mat, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0, false };

    float3 lightVector = normalize(Pos - position).xyz;
    float distance = length(lightVector);
    lightVector = lightVector / distance;
 
    float attenuation = CalculateAtt(distance);
 
    result.Diffuse = CalculateDiffuse(mat, lightVector, normal, bumpNormal, result.DiffuseIntensity) * attenuation;
    if (mat.GetIsSpecular())
        result.Specular = CalculateSpecular(mat, viewDirection, lightVector, normal, bumpNormal, texCoord, specularMap, textureSampler) * attenuation;
 
    return result;
}

_LightCalculationResult LightClass::CalculateSpotLight(IMaterial mat, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler)
{
    _LightCalculationResult result = { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, 0, false };

    float3 lightVector = normalize(Pos - position).xyz;
    //float3 lightVector = (Position - position).xyz;
    float distance = length(lightVector);
    lightVector = lightVector / distance;
 
    float attenuation = CalculateAtt(distance);
    float spotIntensity = CalculateSpotCone(lightVector);
 
    result.Diffuse = CalculateDiffuse(mat, lightVector, normal, bumpNormal, result.DiffuseIntensity) * attenuation * spotIntensity;
    if (mat.GetIsSpecular())
        result.Specular = CalculateSpecular(mat, viewDirection, lightVector, normal, bumpNormal, texCoord, specularMap, textureSampler) * attenuation * spotIntensity;

    return result;
}

// Calculate the attenuation of a light using its Constant, Linear and Quadratic attenuation values.
float LightClass::CalculateAtt(float distance)
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
    return 1.0f / (ConstAtt + (LineAtt * distance) + (QuadAtt * distance * distance));
    //return 1.0f - smoothstep(GetRange() * 0.6, GetRange(), distance);
}


// ========================== GLOBAL DATA ===============
float4 GlobalDataClass::ProcessFinalColor(float4 finalColor)
{
    return finalColor;
}

int GlobalDataClass::GetCurrentLights()
{
    return CurrentLights;
}

float4 GlobalDataClass::GetGlobalAmbient()
{
    return GlobalAmbient;
}
#endif