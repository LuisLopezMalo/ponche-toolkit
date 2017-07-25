#ifndef _PTDeclarations
#include "PTDeclarations.fx"
#endif

//cbuffer PerObject : register(b0)
//{
//    matrix World;
//    matrix InverseTransposeWorld;
//    matrix WorldViewProjection;
//}

_PixelShader_IN SimpleVertexShader(_AppDataPerObject_IN input)
{
    _PixelShader_IN output;
 
    input.Position.w = 1.0f;

    // TODO: the multiplications are made 'a la inversa' as how I had them previously.
    // all the future multiplications must be this way.
    output.Position = mul(WorldViewProjection, input.Position);
    output.WorldPos = mul(World, input.Position);
    output.Normal = normalize((mul((float3x3)InverseTransposeWorld, input.Normal)));
    output.TexCoord = input.TexCoord;
 
    output.Color = input.Color;

    // Calculate the tangent.
    output.Tangent = mul((float3x3) World, input.Tangent);
    output.Tangent = normalize(output.Tangent);

    // Calculate the binormal.
    output.Binormal = mul((float3x3) World, input.Binormal);
    output.Binormal = normalize(output.Binormal);

    // Determine the viewing direction based on the position of the camera and the position of the vertex in the world.
    output.ViewDirection = normalize(CameraPosition.xyz - output.Position.xyz);

    // Set the clipping plane.
    output.Clip = dot(mul(input.Position, World), Clip);

    return output;
}