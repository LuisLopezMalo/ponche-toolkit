Texture2D InputTexture : register(t0);
SamplerState InputSampler : register(s0);

cbuffer ParametersConstantBuffer : register(b0)
{
    float2 ScreenSize : packoffset(c0.x);
    float Thickness : packoffset(c0.z);
    float Threshold : packoffset(c0.w);
};

struct PixelMainInput
{
    float4 Position : SV_Position;
    float4 PosScene : SCENE_POSITION;
    float2 TexCoord : TEXCOORD0;
};

float getGray(float4 color)
{
    return (dot(color.rgb, ((0.33333).xxx)));
}

float4 PixelShaderEntry(PixelMainInput input) : SV_Target
{
    // Get the source pixel color
    float4 Color = InputTexture.Sample(InputSampler, input.TexCoord);

	/* ox is the X offset vector where the offest is based
	 * on the scaled edge thickness
	 */
    float2 ox = float2(Thickness / ScreenSize.x, 0.0);

	/* oy is the Y offset vector where the offest is based
	 * on the scaled edge thickness
	 */
    float2 oy = float2(0.0, Thickness / ScreenSize.y);

	// Current xy (uv) texture coordinate
    float2 uv = input.TexCoord.xy;

	/* Our kernel filter is a 3x3 matrix in order to process
	 * it we need to get the 8 neighbor pixles (top left, top, top right,
	 * left, right, bottom left, bottom, and bottom right) and the
	 * current pixel.  For each of these pixels we then need to get
	 * its grey scale value using getGray.  We will store the gray scale
	 * values in a 3x3 matrix g:
	 * g00 g01 g02
	 * g10 g11 g12
	 * g20 g21 g22
	 */

	/* First the bottom row pixels
	 * bottom left uv - oy - ox, bottom uv - oy and
	 * bottom right uv - oy + ox
	 */
    float2 PP = uv - oy;
    float4 CC = InputTexture.Sample(InputSampler, PP - ox);
    float g00 = getGray(CC);
    CC = InputTexture.Sample(InputSampler, PP);
    float g01 = getGray(CC);
    CC = InputTexture.Sample(InputSampler, PP + ox);
    float g02 = getGray(CC);

	/* Next get the middle row pixels
	 * left uv - ox, current uv and right uv + ox
	 */
    PP = uv;
    CC = InputTexture.Sample(InputSampler, PP - ox);
    float g10 = getGray(CC);
    CC = InputTexture.Sample(InputSampler, PP);
    float g11 = getGray(CC);
    CC = InputTexture.Sample(InputSampler, PP + ox);
    float g12 = getGray(CC);

	/* Finally get the top row pixels
	 * top left uv + oy - ox, top uv + oy and
	 * top right uv + oy + ox
	 */
    PP = uv + oy;
    CC = InputTexture.Sample(InputSampler, PP - ox);
    float g20 = getGray(CC);
    CC = InputTexture.Sample(InputSampler, PP);
    float g21 = getGray(CC);
    CC = InputTexture.Sample(InputSampler, PP + ox);
    float g22 = getGray(CC);

	/* We will use a Sobell convolution filter
	 * -1 -2 -1
	 *  0  0  0
	 *  1  2  1
	 */
    float K00 = -1;
    float K01 = -2;
    float K02 = -1;
    float K10 = 0;
    float K11 = 0;
    float K12 = 0;
    float K20 = 1;
    float K21 = 2;
    float K22 = 1;

	/* Calculate sx as the summation
	 * of g.ij * K.ij
	 * This will give us horizantal edge detection
	 */
    float sx = 0;
    sx += g00 * K00;
    sx += g01 * K01;
    sx += g02 * K02;
    sx += g10 * K10;
    sx += g11 * K11;
    sx += g12 * K12;
    sx += g20 * K20;
    sx += g21 * K21;
    sx += g22 * K22;

	/* Calculate sy as the summation
	 * of g.ij * K.ji
	 * K.ji effectively rotates the kernel filter
	 * this will give us vertical edge detection
	 */
    float sy = 0;
    sy += g00 * K00;
    sy += g01 * K10;
    sy += g02 * K20;
    sy += g10 * K01;
    sy += g11 * K11;
    sy += g12 * K21;
    sy += g20 * K02;
    sy += g21 * K12;
    sy += g22 * K22;
	
	/* Now merge the results of the horizantal
	 * and veritcal edge detection calculations
	 * together by calculating the distance of the
	 * vector they form.
	 */
    float contrast = sqrt(sx * sx + sy * sy);
	
	/* assume no edge (result = 1)
	 */
    float result = 1;

	/* If the length of s.xy has a value
	 * greater than the threshold then the color change (contrast)
	 * accoss that pixel is enough that we want to consider
	 * it an edge.  Set result to 0 to black out that pixel.
	 */
    if (contrast > Threshold)
    {
        result = 0;
    }

	/* finally return the original color multiplied
	 * by the result.  For with contrast values over the
	 * threshold result will be 0 giving us a black edge.
	 * Make sure we do not clear out the alpha value though
	 * otherwise our edges will disappear if we use alpha
	 * blending.
	 */ 
    return Color * float4(result.xxx, 1);
}