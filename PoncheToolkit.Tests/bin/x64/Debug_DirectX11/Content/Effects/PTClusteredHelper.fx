#ifndef _PTClusteredHelper
#define _PTClusteredHelper

#define ClusterSizeX 64
#define ClusterSizeY 64

cbuffer ClusteredRenderBuffer : register(b3)
{
    float2 clusterFactor;
}

float4 CalculateClustered(float4 diffuse, float4 specular, float3 position, float3 normal, float3 viewDirection, float2 texCoord, float shininess, 
                SamplerState textureSampler, Texture2D<float4> clusterGrid)
{
    float4 offset = clusterGrid.Sample(textureSampler, texCoord);
    
    int lightOffset = int(offset.x);
    int lightCount = int(offset.y);
}

/**
 * Linearlizes the 3D cluster location into an offset, which can be used as an
 * address into a linear buffer.
 */
int CalculateClusterOffset(float3 clusterPosition)
{
    return (clusterPosition.z * clusterFactor.y + clusterPosition.y) * clusterFactor.x + clusterPosition.x;
}

/**
 * Computes the {i,j,k} integer index into the cluster grid. For details see our paper:
 * 'Clustered Deferred and Forward Shading'
 * http://www.cse.chalmers.se/~olaolss/main_frame.php?contents=publication&id=clustered_shading
 */
float3 CalculateClusterPosition(float3 position, float viewSpaceZ)
{
	// i and j coordinates are just the same as tiled shading, and based on screen space position.
    float2 l = float2(int(position.x) / ClusterSizeX, int(position.y) / ClusterSizeY);

	// k is based on the log of the view space Z coordinate.
    float gridLocZ = log(-viewSpaceZ * recNear) * recLogSD1;

    return float3(l, int(gridLocZ));
}

/**
 * Convenience function.
 */
int CalculateClusterOffset(float3 position, float viewSpaceZ)
{
    return CalculateClusterOffset(CalculateClusterPosition(position, viewSpaceZ));
}

/**
 * Computes clustered shading for the current fragment, using the built in 
 * gl_FragCoord and the view space Z coordinate to determine the correct 
 * cluster. The position must be in view space (and thus the normal also).
 */
float3 evalClusteredShading(float3 diffuse, float3 specular, float shininess, float3 position, float3 normal, float3 viewDirection, float3 clusterPosition,
                                SamplerState textureSampler, Texture2D<float4> clusterGridTexture)
{
	// fetch cluster data (i.e. offset to light indices, and numer of lights) from grid buffer.
    float2 offsetCount = clusterGridTexture.sample(textureSampler, CalculateClusterOffset(clusterPosition, position.z)).xy;

    int lightOffset = int(offsetCount.x);
    int lightCount = int(offsetCount.y);

    float3 result = float3(0.0, 0.0, 0.0);
	
    for (int i = 0; i < lightCount; ++i)
    {
		// fetch light index from list of lights for the cluster.
        int lightIndex = texelFetch(clusterLightIndexListsTex, lightOffset + i).x;
		// compute and accumulate shading.
        result += doLight(position, normal, diffuse, specular, shininess, viewDirection, lightIndex);
    }

    return result;
}
#endif