static const float PI = 3.14159265f;

// alpha threshold for our occlusion map
static const float THRESHOLD = 0.75;

cbuffer TextureBuffer
{
    Texture2D shaderTexture : register(t0);
    SamplerState textureSampler : register(s0);
};

cbuffer RenderingBuffer
{
    float4 color;
    // Resolution of the map texture used.
    float2 resolution;
    // The linear interpolation for the gaussian result
    float softShadows;
    float padding;
};

struct PixelMainInput
{
    float4 pos : SV_Position;
    float4 posScene : SCENE_POSITION;
    float4 tex : TEXCOORD0;
};

float sample(float2 coord, float r)
{
    return step(r, shaderTexture.Sample(textureSampler, coord).r);
}

float4 PixelShaderEntry(PixelMainInput input) : SV_TARGET
{
    //return float4(0.9, 0.4, 0.4, 0.5);
    //return color * float4(0.9, 0.4, 0.4, 0.5);

    //float4 col = shaderTexture.Sample(textureSampler, float2(input.tex.x, input.tex.y));
    //return col;

    //rectangular to polar
    float2 norm = input.tex * 2.0 - 1.0;
    //float theta = atan(norm);
    float theta = atan2(norm.y, norm.x);
    float r = length(norm);
    float coord = (theta + PI) / (2.0 * PI);
	
	// the tex coord to sample our 1D lookup texture	
	// always 0.0 on y axis
    float2 tc = float2(coord, 0.0);
	
	//the center tex coord, which gives us hard shadows
    float center = sample(float2(tc.x, tc.y), r);
	
	//we multiply the blur amount by our distance from center
	//this leads to more blurriness as the shadow "fades away"
    float blur = (1.0 / resolution.x) * smoothstep(0.0, 1.0, r);
	
	// now we use a simple gaussian blur
    float sum = 0.0;

    sum += sample(float2(tc.x - 4.0 * blur, tc.y), r) * 0.05;
    sum += sample(float2(tc.x - 3.0 * blur, tc.y), r) * 0.09;
    sum += sample(float2(tc.x - 2.0 * blur, tc.y), r) * 0.12;
    sum += sample(float2(tc.x - 1.0 * blur, tc.y), r) * 0.15;
	
    sum += center * 0.16;
	
    sum += sample(float2(tc.x + 1.0 * blur, tc.y), r) * 0.15;
    sum += sample(float2(tc.x + 2.0 * blur, tc.y), r) * 0.12;
    sum += sample(float2(tc.x + 3.0 * blur, tc.y), r) * 0.09;
    sum += sample(float2(tc.x + 4.0 * blur, tc.y), r) * 0.05;
	
	// 1.0 -> in light, 0.0 -> in shadow
    float lit = lerp(center, sum, softShadows);

    //return color;
     //float4(0.5, 1, 0.5, 0.2);

 	// Multiply the summed amount by our distance, which gives us a radial falloff
 	// then multiply by vertex (light) color  
    return color * float4(1, 1, 1, lit * smoothstep(1.0, 0.0, r));
}


technique11 Render
{
    pass P0
    {
        SetPixelShader(CompileShader(ps_5_0, PixelShaderEntry()));
    }
}