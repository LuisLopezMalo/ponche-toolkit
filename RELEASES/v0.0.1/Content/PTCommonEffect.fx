//cbuffer MatrixBuffer
//{
//    float4x4 wvp;
//};

//cbuffer TextureBuffer
//{
//    Texture2D shaderTexture : register(t0);
//    SamplerState textureSampler : register(s0);
//};

cbuffer CommonBuffer : register(b0)
{
    float4x4 wvp;
    int HasTexture;
}

Texture2D shaderTexture : register(t0);
SamplerState textureSampler : register(s0);

struct VertexShader_IN
{
    float4 pos : POSITION0;
	float4 col : COLOR0;
    float2 tex : TEXCOORD0;
};

struct PixelShader_IN
{
	float4 pos : SV_POSITION0;
	float4 col : COLOR0;
    float2 tex : TEXCOORD0;
};

PixelShader_IN VertexShaderEntry(VertexShader_IN input)
{
    PixelShader_IN output = (PixelShader_IN) 0;

    input.pos.w = 1.0f;

	// Calculate the wvp multiplied matrices.
    // The world-view-projection matrix is calculated in code. (using the CPU)
    output.pos = mul(input.pos, wvp);
    output.tex = input.col;
    if (HasTexture == 1)
        output.tex = input.tex;

    return output;
}

float4 PixelShaderEntry(PixelShader_IN input) : SV_TARGET
{
    if (HasTexture == 1)
    {
        float4 textureColor = shaderTexture.Sample(textureSampler, input.tex);
        return textureColor;
    }
    else
        return input.col;
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