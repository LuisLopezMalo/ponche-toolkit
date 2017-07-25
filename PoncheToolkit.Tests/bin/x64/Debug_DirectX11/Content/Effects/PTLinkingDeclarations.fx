#ifndef _PTLinkingDeclarations
#define _PTLinkingDeclarations

#ifndef _PTLinkingInterfaces
#include "PTLinkingInterfaces.fx"
#endif

IGlobalData GlobalDataAbstract;
IMaterial MaterialAbstract;
ILight LightsAbstract[MAX_LIGHTS];

Texture2D<float4> shaderTextures[4] : register(t0);
Texture2D<float4> bumpMapTexture : register(t4);
Texture2D<float4> specularMapTexture : register(t5);
Texture2D<float4> reflectionTexture : register(t6);
SamplerState textureSampler : register(s0);
//StructuredBuffer<LightClass> Lights : register(t7);  // Sending the lights in a StructuredBuffer will make gain more memory. A Constant buffer has a 64 KB restriction.

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

#endif