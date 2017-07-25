cbuffer MatrixBuffer
{
    float4x4 world;
    float4x4 view;
    float4x4 projection;
};

cbuffer TextureBuffer
{
    Texture2D shaderTexture : register(t0);
    SamplerState textureSampler : register(s0);
};

cbuffer LightBuffer
{
    float4 diffuseColor;
    float3 lightDirection;
    float padding;
};

struct VertexShader_IN
{
    float4 pos : POSITION0;
    float4 col : COLOR0;
    float2 tex : TEXCOORD0;
    float3 norm : NORMAL;
};

struct PixelShader_IN
{
	float4 pos : SV_POSITION0;
    float4 col : COLOR0;
    float2 tex : TEXCOORD0;
    float3 norm : NORMAL;
};

PixelShader_IN VertexShaderEntry(VertexShader_IN input)
{
    PixelShader_IN output = (PixelShader_IN) 0;

    input.pos.w = 1.0f;

    // Calculate te wvp matrices here. (using the GPU).
    output.pos = mul(input.pos, world);
    output.pos = mul(output.pos, view);
    output.pos = mul(output.pos, projection);

    output.col = float4(1, 1, 1, 1);
    output.tex = input.tex;

    // Calculate the normal vector against the world matrix only.
    output.norm = mul(input.norm, (float3x3)world);
	
    // Normalize the normal vector.
    output.norm = normalize(output.norm);

	return output;
}

float4 PixelShaderEntry(PixelShader_IN input) : SV_TARGET
{
    float4 textureColor;
    float3 lightDir;
    float lightIntensity;
    float4 color;

    textureColor = shaderTexture.Sample(textureSampler, input.tex);
    //return textureColor;
    
    // Invert the light direction for calculations.
    lightDir = -lightDirection;

    // Calculate the amount of light on this pixel.
    lightIntensity = saturate(dot(input.norm, lightDir));

    // Determine the final amount of diffuse color based on the diffuse color combined with the light intensity.
    color = saturate(diffuseColor * lightIntensity);

    // Multiply the texture pixel and the final diffuse color to get the final pixel color result.
    color = color * textureColor;
    
    return color;
}

technique11 Render
{
	pass P0
	{
        SetVertexShader(CompileShader(vs_5_0, VertexShaderEntry()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PixelShaderEntry()));
    }
}