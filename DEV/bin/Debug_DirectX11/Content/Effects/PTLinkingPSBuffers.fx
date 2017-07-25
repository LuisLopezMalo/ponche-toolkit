#ifndef _PTLinkingPSBuffers
#define _PTLinkingPSBuffers

#ifndef _PTLinkingClasses
#include "PTLinkingClasses.fx"
#endif

cbuffer MaterialConstantBuffer : register(b0)
{
    MaterialClass Material;
};

cbuffer GlobalDataConstantBuffer : register(b1)
{
    GlobalDataClass GlobalData;
};

cbuffer LightsConstantBuffer : register(b2) // 64 kb per ConstantBuffer MAX
{
    LightClass Lights[MAX_LIGHTS];
};
#endif