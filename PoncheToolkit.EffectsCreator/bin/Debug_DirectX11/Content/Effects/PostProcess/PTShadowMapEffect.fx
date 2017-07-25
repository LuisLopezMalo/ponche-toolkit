static const float PI = 3.14159265f;

// Alpha threshold for our occlusion map
static const float THRESHOLD = 0.75;

cbuffer TextureBuffer
{
    Texture2D shaderTexture : register(t0);
    SamplerState textureSampler : register(s0);
};

cbuffer RenderingBuffer : register(b0)
{
    // Resolution of the map texture used.
    //float UpScale : packoffset(c0.x);
    //float2 Resolution : packoffset(c1);
    
    float2 Resolution;
    float UpScale;
    float Padding;
};

struct PixelMainInput
{
    float4 pos : SV_Position;
    float4 posScene : SCENE_POSITION;
    float4 tex : TEXCOORD0;
};

float4 PixelShaderEntry(PixelMainInput input) : SV_TARGET
{
    float distance = 1.0;

    for (float y = 0.0; y < Resolution.y; y += 1.0)
    {
        // Rectangular to polar filter
        float2 norm = float2(input.tex.x, y / Resolution.y) * 2.0 - 1.0;
        float theta = PI * 1.5 + norm.x * PI;
        float r = (1.0 + norm.y) * 0.5;

        // Coord which we will sample from occlude map
        float2 coord = float2(-r * sin(theta), -r * cos(theta)) / 2.0 + 0.5;

        // Sample the occlusion map
        float4 data = shaderTexture.Sample(textureSampler, coord);

        // The current distance is how far from the top we've come
        float dst = y / Resolution.y;

        // If we've hit an opaque fragment (occluder), then get new distance
        // If the new distance is below the current, then we'll use that for our ray
        float alpha = data.a;
        if (alpha > THRESHOLD)
        {
            distance = min(distance, dst);
            //NOTE: we could probably use "break" or "return" here
        }
    }

    return float4(distance, distance, distance, 1.0);
}

technique11 Render
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_5_0, PixelShaderEntry()));
    }
}