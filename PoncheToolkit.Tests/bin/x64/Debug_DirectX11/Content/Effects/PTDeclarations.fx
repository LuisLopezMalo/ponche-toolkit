#ifndef _PTDeclarations
#define _PTDeclarations

#define DIRECTIONAL_LIGHT 0
#define POINT_LIGHT 1
#define SPOT_LIGHT 2

//#ifdef _PTClusteredHelper
//#define MAX_LIGHTS 2048
////#define MAX_LIGHTS 8192
//#else
//#define MAX_LIGHTS 16
//#endif

#define MAX_LIGHTS 20

// ============== INITIAL STRUCTS =================
//struct _AppData_IN  // MINIMUN FLOATING PRECISION
//{
//    // Per-vertex data
//    min16float4 Position : POSITION0;
//    min16float4 Color : COLOR0;
//    min16float2 TexCoord : TEXCOORD0;
//    min16float3 Normal : NORMAL;
//    min16float3 Tangent : TANGENT;
//    min16float3 Binormal : BINORMAL;
//};

struct _AppData_IN
{
    // Per-vertex data
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
    float3 Binormal : BINORMAL;
};

// For instancing. -Per object
struct _AppDataPerObject_IN
{
    // Per-vertex data
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord: TEXCOORD0;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
    float3 Binormal : BINORMAL;
};

// For hardware instancing. -Per instance
struct _AppDataPerInstance_IN
{
    // Per-vertex data
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
    float3 Binormal : BINORMAL;
    // Per-instance data
    matrix World: WORLDMATRIX;
    matrix InverseTransposeWorld : INVERSETRANSPOSEWORLDMATRIX;
    uint InstanceId : SV_InstanceID;
};

struct _PixelShader_IN
{
    float4 Position : SV_POSITION0;
    float4 Color : COLOR0;
    float4 WorldPos : POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT;
    float3 Binormal : BINORMAL;
    float4 ReflectionPos : TEXCOORD1;
    float3 ViewDirection : TEXCOORD2;
    float ReflectionClipZ : TEXCOORD3;
};
// ============== INITIAL STRUCTS =================


struct _Material  // 128 bytes
{
    float4 EmissiveColor;
    float4 AmbientColor;
    float4 DiffuseColor;
    float4 SpecularColor;
    float4 ReflectionColor;
    
    float SpecularPower;
    float Reflectivity;
    float Opacity;
    float Gamma; // For multiple texture blending.

    int BlendTexturesCount;
    bool IsSpecular;
    bool IsBump;
    bool IsReflective;

    bool HasSpecularMap;
    float2 TextureTranslation;
    float Padding;
};

// ============ Lighting =============
struct _Light  // 80 bytes  -- MAX 819 possible lights with a Constant Buffer (64 Kb)
{
    float4 Color;
    float4 Position;
    float4 Direction;
    
    float SpotAngle;
    float ConstantAttenuation;
    float LinearAttenuation;
    float QuadraticAttenuation;

    float Intensity;
    float Range;
    int Type;
    bool IsEnabled;
};

struct _DirectionalLight
{
    float4 AmbientColor;
    float4 DiffuseColor;
    float4 SpecularColor;
    float4 ReflectionColor;
    float3 Direction;
    float SpecularPower;
};

struct _PointLight
{
    float4 AmbientColor;
    float4 DiffuseColor;
    float4 SpecularColor;
    float4 ReflectionColor;
    
    float SpecularPower;
    float3 Position;
    
    float Range;
    float3 Attenuation;
};

struct _SpotLight
{
    float4 DiffuseColor;
    float4 AmbientColor;
    float4 SpecularColor;
    float4 ReflectionColor;
    float3 Direction;
    float SpecularPower;

    float3 Position;
    float Range;

    float Spot;
    float3 Attenuation;
};

struct _LightCalculationResult
{
    float4 Diffuse;
    float4 Specular;
    float DiffuseIntensity;
};
#endif