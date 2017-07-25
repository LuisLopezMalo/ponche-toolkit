#ifndef _Custom_ShaderPS0_Classes
#define _Custom_ShaderPS0_Classes

#ifndef _PTLinkingInterfaces
#include "../../../DEV/Content/Effects/PTLinkingInterfaces.fx"
#endif

//#ifndef _PTLinkingInterfaces
//#include "PTLinkingInterfaces.fx"
//#endif

class MaterialClass0 : IMaterial
{
    float4 EmissiveColor;
    float4 AmbientColor;
    float4 DiffuseColor;
    float4 SpecularColor;
    float4 ReflectionColor;
    
    float SpecularPower;
    float Reflectivity;
    float Opacity;
    float Gamma; // For multiple texture blending.

    int BlendTexturesCount;
    bool IsSpecular;
    bool IsBump;
    bool IsReflective;

    bool HasSpecularMap;
    float2 TextureTranslation;
    float Padding;

    float4 GetEmissiveColor()
    {
        return EmissiveColor;
    }
    float4 GetAmbientColor()
    {
        return AmbientColor;
    }
    float4 GetDiffuseColor()
    {
        return AmbientColor;
    }
    float4 GetSpecularColor()
    {
        return SpecularColor;
    }
    float4 GetReflectionColor()
    {
        return ReflectionColor;
    }
    float GetSpecularPower()
    {
        return SpecularPower;
    }
    float GetReflectivity()
    {
        return Reflectivity;
    }
    float GetOpacity()
    {
        return Opacity;
    }
    float GetGamma()
    {
        return Gamma;
    }
    int GetBlendTexturesCount()
    {
        return BlendTexturesCount;
    }
    bool GetIsSpecular()
    {
        return IsSpecular;
    }
    bool GetIsBump()
    {
        return IsBump;
    }
    bool GetIsReflective()
    {
        return IsReflective;
    }
    bool GetHasSpecularMap()
    {
        return HasSpecularMap;
    }
    float2 GetTextureTranslation()
    {
        return TextureTranslation;
    }

    // ==== Methods
    _BumpProperties CalculateBump(Texture2D<float4> bumpMapTexture, SamplerState textureSampler, float3 tangent, float3 binormal, float3 normal, float2 texCoord);
    float4 CalculateDiffuse(float4 globalAmbient, float4 lightDiffuse, float4 lightSpecular);
    float4 CalculateSpecular(Texture2D specularMapTexture, SamplerState samplerState, float4 currentSpecular, float2 texCoord);
};

class LightClass0 : ILight
{
    float4 Col;
    float4 Pos;
    float4 Dir;
    
    float SpotAngle;
    float ConstAtt;
    float LineAtt;
    float QuadAtt;

    float Intensity;
    float Range;
    int Type;
    bool IsEnabled;

    // ======= Methods
    float4 CalculateDiffuse(IMaterial mat, float3 lightDirection, float3 normal, float3 bumpNormal, out float diffuseIntensity);
    float4 CalculateSpecular(IMaterial mat, float3 viewDirection, float3 lightDirection, float3 normal, float3 bumpNormal, float2 texCoord,
                            Texture2D<float4> specularMap, SamplerState textureSampler);
    float CalculateSpotCone(float3 lightVector);
    float CalculateAtt(float distance);

    // Type of lights
    _LightCalculationResult CalculateDirectionalLight(IMaterial mat, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler);
    _LightCalculationResult CalculatePointLight(IMaterial mat, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler);
    _LightCalculationResult CalculateSpotLight(IMaterial mat, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler);

    _LightCalculationResult CalculateLight(IMaterial mat, float4 position, float3 viewDirection,
                            float3 normal, float3 bumpNormal, float2 texCoord, Texture2D<float4> specularMap, SamplerState textureSampler);
};

class GlobalDataClass0 : IGlobalData
{
    float4 GlobalAmbient;
    int CurrentLights;

    int GetCurrentLights();
    float4 GetGlobalAmbient();
    float4 ProcessFinalColor(float4 finalColor);
};
#endif