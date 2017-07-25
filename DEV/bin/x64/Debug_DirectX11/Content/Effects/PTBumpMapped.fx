#ifndef _PTBumpMapped
#define _PTBumpMapped

Texture2D<float4> bumpMapTexture : register(t4);

struct _BumpProperties
{
    float4 BumpMap;
    float3 BumpNormal;
};

_BumpProperties CalculateBump(Texture2D<float4> bumpTexture, SamplerState textureSampler,
                                float3 tangent, float3 binormal, float3 normal, float2 texCoord)
{
    _BumpProperties result;
    result.BumpMap = bumpMapTexture.Sample(textureSampler, texCoord);
    result.BumpMap = (result.BumpMap * 2.0f) - 1.0f;
    result.BumpNormal = (result.BumpMap.x * tangent) + (result.BumpMap.y * binormal) + (result.BumpMap.z * normal);
    result.BumpNormal = normalize(result.BumpNormal);

    return result;
}
#endif