#ifndef _PTVS
#define _PTVS

#ifndef _PTStructs
#include "PTStructs.fx"
#endif

// ======== Vertex Shader Constant Buffers ========
cbuffer MatrixBuffer : register(b0)
{
    matrix World;
    matrix ViewProjection;
    matrix WorldViewProjection;
    float3 CameraPosition;
};

cbuffer ClippingBuffer : register(b1)
{
    float4 ClipPlane;
};

cbuffer ReflectionBuffer : register(b2)
{
    matrix ReflectionMatrixProjectionWorld;
    matrix ParaboloidView;
    float Direction;
    float NearPlane;
    float FarPlane;
    float padding;
};
// ======== Vertex Shader Constant Buffers ===================

[clipplanes(ClipPlane)]
_PixelShader_IN VertexShaderEntry(_AppData_IN input)
{
    _PixelShader_IN output = (_PixelShader_IN) 0;

    input.Position.w = 1.0f;

    // Calculate te wvp matrices here. (using the GPU).
    //output.pos = mul(input.pos, world);
    //output.pos = mul(output.pos, view);
    //output.pos = mul(output.pos, projection);

    // Matrices calculated using the CPU.
    output.Position = mul(input.Position, WorldViewProjection);

    // Reflection
    output.ReflectionClipZ = 0;
    //if (MaterialAbstract.GetIsReflective())
    if (false)
    {
        output.Position.z = output.Position.z * Direction;
	
        float L = length(output.Position.xyz); // determine the distance between (0,0,0) and the vertex
        output.Position = output.Position / L; // divide the vertex position by the distance 
	
        output.ReflectionClipZ = output.Position.z; // remember which hemisphere the vertex is in
        output.Position.z = output.Position.z + 1; // add the reflected vector to find the normal vector

        output.Position.x = output.Position.x / output.Position.z; // divide x coord by the new z-value
        output.Position.y = output.Position.y / output.Position.z; // divide y coord by the new z-value

        output.Position.z = (L - NearPlane) / (FarPlane - NearPlane); // scale the depth to [0, 1]
        output.Position.w = 1;
    }

    output.WorldPos = mul(input.Position, World);
    output.ReflectionPos = mul(input.Position, ReflectionMatrixProjectionWorld);

    output.Color = input.Color;
    output.TexCoord = input.TexCoord;

    // Calculate the normal vector against the world matrix only.
    //output.normal = mul(input.normal, (float3x3) world);
	
    // Normalize the normal vector.
    //output.Normal = normalize(mul((float3x3) inverseTransposeWorld, input.normal));
    output.Normal = normalize(mul(input.Normal, (float3x3) World));

    // Calculate the tangent.
    output.Tangent = mul(input.Tangent, (float3x3) World);
    output.Tangent = normalize(output.Tangent);

    // Calculate the binormal.
    output.Binormal = mul(input.Binormal, (float3x3) World);
    output.Binormal = normalize(output.Binormal);

    // Determine the viewing direction based on the position of the camera and the position of the vertex in the world.
    output.ViewDirection = normalize(CameraPosition.xyz - output.Position.xyz);

    return output;
}
#endif