#ifndef _PTStructs
#define _PTStructs

#define DIRECTIONAL_LIGHT 0
#define POINT_LIGHT 1
#define SPOT_LIGHT 2

#if !defined(MAX_LIGHTS)
#define MAX_LIGHTS 15
#endif

Texture2D<float4> shaderTextures[4] : register(t0);
Texture2D<float4> bumpMapTexture : register(t4);
Texture2D<float4> specularMapTexture : register(t5);
Texture2D<float4> reflectionTexture : register(t6);
SamplerState textureSampler : register(s0);

struct _AppData_IN
{
    // Per-vertex data
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
    float3 Binormal : BINORMAL;
};

struct _PixelShader_IN
{
    float4 Position : SV_POSITION0;
    float4 Color : COLOR0;
    float4 WorldPos : POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
    float3 Binormal : BINORMAL;
    float4 ReflectionPos : TEXCOORD1;
    float3 ViewDirection : TEXCOORD2;
    float ReflectionClipZ : TEXCOORD3;
};

struct MaterialStruct
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
};

struct LightStruct
{
    float4 Color;
    float4 Position;
    float4 Direction;
    
    float SpotAngle;
    float ConstAtt;
    float LineAtt;
    float QuadAtt;

    float Intensity;
    float Range;
    int Type;
    bool IsEnabled;
};

struct GlobalDataStruct
{
    int CurrentLights;
    float4 GlobalAmbient;
};

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
#endif