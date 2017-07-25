#ifndef _PTLinkingBuffersPS
#define _PTLinkingBuffersPS

#ifndef _PTLinkingDeclarations
#include "PTLinkingDeclarations.fx"
#endif

#ifndef _PTLinkingClasses
#include "PTLinkingClasses.fx"
#endif

cbuffer MaterialConstantBuffer : register(b0)
{
    MaterialClass Material;
};

cbuffer GlobalDataConstantBuffer : register(b1)
{
    //float4 GlobalAmbient; // Global color to be applied to all elements.
    //int CurrentLights; // Number of current lights.
    GlobalDataClass GlobalData;
};

cbuffer LightsConstantBuffer : register(b2) // 64 kb per ConstantBuffer MAX
{
    LightClass Lights[MAX_LIGHTS];
};

#endif