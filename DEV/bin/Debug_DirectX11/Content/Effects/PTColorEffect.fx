cbuffer MatrixBuffer
{
    float4x4 world;
    float4x4 view;
    float4x4 projection;
};

//cbuffer MatrixBuffer
//{
//    float4x4 wvp;
//};

struct VertexShader_IN
{
    float4 pos : POSITION0;
	float4 col : COLOR0;
};

struct PixelShader_IN
{
	float4 pos : SV_POSITION0;
	float4 col : COLOR0;
};

PixelShader_IN VertexShaderEntry(VertexShader_IN input)
{
    PixelShader_IN output = (PixelShader_IN) 0;

    input.pos.w = 1.0f;
	// Calculate the wvp multiplied matrices.
    // The world-view-projection matrix is calculated in code. (using the CPU)
    //output.pos = mul(input.pos, wvp);

    // Calculate te wvp matrices here. (using the GPU).
    output.pos = mul(input.pos, world);
    output.pos = mul(output.pos, view);
    output.pos = mul(output.pos, projection);

	output.col = input.col;

	return output;
}

float4 PixelShaderEntry(PixelShader_IN input) : SV_TARGET
{
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