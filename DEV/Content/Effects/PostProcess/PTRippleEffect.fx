Texture2D InputTexture : register(t0);
SamplerState InputSampler : register(s0);

cbuffer constants : register(b0)
{
    float frequency : packoffset(c0.x);
    float phase : packoffset(c0.y);
    float amplitude : packoffset(c0.z);
    float spread : packoffset(c0.w);
    float2 center : packoffset(c1);
};

struct PixelMainInput
{
    float4 pos : SV_Position;
    float4 posScene : SCENE_POSITION;
    float4 texCoord : TEXCOORD0;
};

float4 PixelShaderEntry(PixelMainInput input) : SV_Target
{
    float2 wave;

    float2 toPixel = input.posScene.xy - center;

    float distance = length(toPixel) * input.texCoord.z;
    float2 direction = normalize(toPixel);

    sincos(frequency * distance + phase, wave.x, wave.y);

    // Clamps the distance between 0 and 1 and squares the value.
    float falloff = saturate(1 - distance);
    falloff = pow(falloff, 1.0f / spread);

    // Calculates new mapping coordinates based on the frequency, center, and amplitude.
    float2 uv2 = input.texCoord.xy + (wave.x * falloff * amplitude) * direction * input.texCoord.zw;

    float lighting = lerp(1.0f, 1.0f + wave.x * falloff * 0.2f, saturate(amplitude / 20.0f));
            
    // Resamples the image based on the new coordinates.
    float4 color = InputTexture.Sample(InputSampler, uv2);
    color.rgb *= lighting;
    
    return color * float4(0.7f, 0.75f, 0.8f, 0.6f);
}