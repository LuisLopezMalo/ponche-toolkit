cbuffer MatrixBuffer
{
    float4x4 world;
    float4x4 view;
    float4x4 ortho;
};

Texture2D shaderTexture : register(t0);
SamplerState textureSampler : register(s0);

struct VertexShader_IN
{
    float4 position : POSITION;
    //float4 color : COLOR0;
    float2 texCoord : TEXCOORD0;
};

struct PixelShader_IN
{
    float4 position : SV_POSITION;
    //float4 color : COLOR0;
    float2 texCoord : TEXCOORD0;
};


PixelShader_IN VertexShaderEntry(VertexShader_IN input)
{
    PixelShader_IN output;
    
	// Change the position vector to be 4 units for proper matrix calculations.
    input.position.w = 1.0f;

	// Calculate the position of the vertex against the world, view, and ortho matrices.
    output.position = mul(input.position, world);
    output.position = mul(output.position, view);
    output.position = mul(output.position, ortho);
    
	// Store the texture coordinates for the pixel shader.
    output.texCoord = input.texCoord;

    //output.color = input.color;
    
    return output;
}

float4 PixelShaderEntry(PixelShader_IN input) : SV_TARGET
{
    float4 textureColor;

    // Sample the pixel color from the texture using the sampler at this texture coordinate location.
    textureColor = shaderTexture.Sample(textureSampler, input.texCoord);

    return textureColor;
    //return textureColor * input.color;
}

technique11 Render
{
	pass P0
	{
		SetGeometryShader(0);
        SetVertexShader(CompileShader(vs_5_0, VertexShaderEntry()));
        SetPixelShader(CompileShader(ps_5_0, PixelShaderEntry()));
    }
}