#ifndef _PTLinkingInterfaces
#define _PTLinkingInterfaces

#define DIRECTIONAL_LIGHT 0
#define POINT_LIGHT 1
#define SPOT_LIGHT 2

#if !defined(MAX_LIGHTS)
#define MAX_LIGHTS 15
#endif

struct _LightCalculationResult
{
    float4 Diffuse;
    float4 Specular;
    float DiffuseIntensity;
    bool Discarded;
};

struct _BumpProperties
{
    float4 BumpMap;
    float3 BumpNormal;
};

interface IMaterial
{
    float4 GetEmissiveColor();
    float4 GetAmbientColor();
    float4 GetDiffuseColor();
    float4 GetSpecularColor();
    float4 GetReflectionColor();
    
    float GetSpecularPower();
    float GetReflectivity();
    float GetOpacity();
    float GetGamma(); // For multiple texture blending.

    int GetBlendTexturesCount();
    bool GetIsSpecular();
    bool GetIsBump();
    bool GetIsReflective();

    bool GetHasSpecularMap();
    float2 GetTextureTranslation();

    // ======= Methods
    //float4 CalculateBlendTexturesColor(Texture2D<float4> bumpMapTexture, SamplerState textureSampler, float3 tangent, float3 binormal, float3 normal, float2 texCoord);
    _BumpProperties CalculateBump(Texture2D<float4> bumpMapTexture, SamplerState textureSampler, float3 tangent, float3 binormal, float3 normal, float2 texCoord);
    float4 CalculateDiffuse(float4 globalAmbient, float4 lightDiffuse, float4 lightSpecular);
    float4 CalculateSpecular(Texture2D specularMapTexture, SamplerState samplerState, float4 currentSpecular, float2 texCoord);
};

interface ILight
{
    // ======= Methods
    float4 CalculateDiffuse(IMaterial mat, float3 lightDirection, float3 normal, float3 bumpNormal, out float diffuseIntensity);
    float4 CalculateSpecular(IMaterial mat, float3 viewDirection, float3 lightDirection, float3 normal, float3 bumpNormal, float2 texCoord,
                            Texture2D<float4> specularMap, SamplerState textureSampler);

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

interface IGlobalData
{
    int GetCurrentLights();
    float4 GetGlobalAmbient();
    float4 ProcessFinalColor(float4 finalColor);
};
#endif