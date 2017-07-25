using PoncheToolkit.Core;
using PoncheToolkit.Graphics3D;
using System;

namespace PoncheToolkit
{
    #region Effects
    /// <summary>
    /// Type of shaders supported.
    /// </summary>
    public enum ShaderType
    {
        /// <summary>
        /// Pixel shader.
        /// </summary>
        Pixel,
        /// <summary>
        /// Vertex shader.
        /// </summary>
        Vertex,
        /// <summary>
        /// Tesselation shader.
        /// </summary>
        Tessel,
        /// <summary>
        /// Hull shader.
        /// </summary>
        Hull
    }

    /// <summary>
    /// The blending type used by the <see cref="Game11"/>.
    /// Change it by calling <see cref="Game.ToggleBlending(BlendingState)"/> 
    /// </summary>
    public enum BlendingState
    {
        /// <summary>
        /// No Blending used.
        /// </summary>
        Disabled,
        /// <summary>
        /// Alpha blending will be used.
        /// </summary>
        AlphaBlending,
        /// <summary>
        /// Mainly used when using multipasses.
        /// </summary>
        AdditiveBlending
    }

    /// <summary>
    /// The type of content accepted by the engine.
    /// </summary>
    public enum ContentType
    {
        /// <summary>
        /// Represent a .fx effect file.
        /// </summary>
        Effect,
        /// <summary>
        /// A Texture.
        /// </summary>
        Texture2D,
        /// <summary>
        /// A 3D Model.
        /// </summary>
        Model3D
    }
    #endregion

    /// <summary>
    /// The types of existing cameras.
    /// </summary>
    public enum CameraType
    {
        /// <summary>
        /// The main camera used.
        /// </summary>
        Main,
        /// <summary>
        /// Type of camera used for rendering the scene for a reflective material over a specific model.
        /// </summary>
        Reflective
    }

    /// <summary>
    /// Set the type of rendering used.
    /// By default the <see cref="Immediate"/> rendering will be used.
    /// The <see cref="MultiThread"/> rendering can be used - if the gpu driver supports it - so
    /// the Engine group the meshes to be rendered and use different threads and 
    /// <see cref="SharpDX.Direct3D11.CommandList"/> or <see cref="SharpDX.Direct3D12.CommandList"/> to send info to the gpu.
    /// </summary>
    public enum ProcessRenderingMode
    {
        /// <summary>
        /// Use only one thread and one context for rendering.
        /// </summary>
        Immediate,
        /// <summary>
        /// Use multiple threads and multiple contexts for rendering.
        /// </summary>
        MultiThread
    }

    /// <summary>
    /// The rendering technique for the entire Engine.
    /// Define the algorithm used to render dynamic lights, shadowing, etc.
    /// </summary>
    public enum ShadingRenderingMode
    {
        /// <summary>
        /// Basic implementation for rendering dynamic lights.
        /// It just loop through all the lights to be applied to all materials.
        /// <para>
        /// This is the slowest method. In modern hardware, more less 15 dynamic lights can be rendered at the same time.
        /// 15 Lights is the MAX settings for this type of rendering.
        /// </para>
        /// </summary>
        ForwardShading,
        /// <summary>
        /// This method is an evolution of the TiledShading, so this converts the tiles (2D) to clusters (3D) so the depth is
        /// inherently incorporated and the depth discontinuities are solved in a better way. Also when using many lights (thousands)
        /// it performs better.
        /// <para>
        /// The only con it has, is the creation of the clusters, but even this calculation is faster that the "Z-prepass" needed for tiledShading.
        /// It supports like 1024 dynamic lights and it can be optimized more in the future.
        /// </para>
        /// </summary>
        ClusteredForwardShading,
        /// <summary>
        /// This will be used if a custom propietary algorithm wants to be used.
        /// </summary>
        Custom1,
        /// <summary>
        /// This will be used if a custom propietary algorithm wants to be used.
        /// </summary>
        Custom2,
        /// <summary>
        /// This will be used if a custom propietary algorithm wants to be used.
        /// </summary>
        Custom3
    }

    #region Assimp
    /// <summary>
    /// Enumerates geometric primitive types.
    /// </summary>
    [Flags]
    public enum AssimpPrimitiveType : int
    {
        /// <summary>
        /// Point primitive. This is just a single vertex
        /// in the virtual world. A face has one index for such a primitive.
        /// </summary>
        Point = 0x1,

        /// <summary>
        /// Line primitive. This is  a line defined through a start and an
        /// end position. A face contains exactly two indices for such a primitive.
        /// </summary>
        Line = 0x2,

        /// <summary>
        /// Triangle primitive, consisting of three indices.
        /// </summary>
        Triangle = 0x4,

        /// <summary>
        /// A n-Gon that has more than three edges (thus is not a triangle).
        /// </summary>
        Polygon = 0x8
    }

    /// <summary>
    /// Defines how UV coordinates should be transformed.
    /// </summary>
    [Flags]
    internal enum AssimpUVTransformFlags
    {
        /// <summary>
        /// Scaling is evaluated.
        /// </summary>
        Scaling = 0x1,

        /// <summary>
        /// Rotation is evaluated.
        /// </summary>
        Rotation = 0x2,

        /// <summary>
        /// Translation is evaluated.
        /// </summary>
        Translation = 0x4
    }

    /// <summary>
    /// Defines some mixed flags for a particular texture. This corresponds
    /// to the AI_MAT_KEY_TEXFLAGS property.
    /// </summary>
    [Flags]
    internal enum AssimpTextureFlags
    {
        /// <summary>
        /// The texture's color values have to be inverted (componentwise 1-n).
        /// </summary>
        Invert = 0x1,

        /// <summary>
        /// Explicit request to the application to process the alpha channel of the texture. This is mutually
        /// exclusive with <see cref="AssimpTextureFlags.IgnoreAlpha"/>. These flags are
        /// set if the library can say for sure that the alpha channel is used/is not used.
        /// If the model format does not define this, iti s left to the application to decide
        /// whether the texture alpha channel - if any - is evaluated or not.
        /// </summary>
        UseAlpha = 0x2,

        /// <summary>
        /// Explicit request to the application to ignore the alpha channel of the texture. This is mutually
        /// exclusive with <see cref="AssimpTextureFlags.UseAlpha"/>.
        /// </summary>
        IgnoreAlpha = 0x4
    }

    /// <summary>
    /// Defines how UV coordinates outside the [0..1] range are handled. Commonly
    /// referred to as the 'wrapping mode'
    /// </summary>
    internal enum AssimpTextureWrapMode
    {
        /// <summary>
        /// A texture coordinate u|v is translated to u % 1| v % 1.
        /// </summary>
        Wrap = 0x0,

        /// <summary>
        /// Texture coordinates outside [0...1] are clamped to the nearest valid value.
        /// </summary>
        Clamp = 0x1,

        /// <summary>
        /// A texture coordinate u|v becomes u1|v1 if (u - (u % 1)) % 2 is zero
        /// and 1 - (u % 1) | 1 - (v % 1) otherwise.
        /// </summary>
        Mirror = 0x2,

        /// <summary>
        /// If the texture coordinates for a pixel are outside [0...1] the texture is not
        /// applied to that pixel.
        /// </summary>
        Decal = 0x3,
    }

    /// <summary>
    /// Defines how texture coordinates are generated
    /// <para>
    /// Real-time applications typically require full UV coordinates. So the use
    /// of <see cref="Assimp.PostProcessSteps.GenerateUVCoords"/> step is highly recommended.
    /// It generates proper UV channels for non-UV mapped objects, as long as an accurate
    /// description of how the mapping should look like is given.
    /// </para>
    /// </summary>
    internal enum AssimpTextureMapping
    {
        /// <summary>
        /// Coordinates are taken from the an existing UV channel.
        /// <para>
        /// The AI_MATKEY_UVWSRC key specifies from the UV channel the texture coordinates
        /// are to be taken from since meshes can have more than one UV channel.
        /// </para>
        /// </summary>
        FromUV = 0x0,

        /// <summary>
        /// Spherical mapping
        /// </summary>
        Sphere = 0x1,

        /// <summary>
        /// Cylinder mapping
        /// </summary>
        Cylinder = 0x2,

        /// <summary>
        /// Cubic mapping
        /// </summary>
        Box = 0x3,

        /// <summary>
        /// Planar mapping
        /// </summary>
        Plane = 0x4,

        /// <summary>
        /// Unknown mapping that is not recognied.
        /// </summary>
        Unknown = 0x5
    }

    /// <summary>
    /// Defines the purpose of a texture.
    /// </summary>
    internal enum AssimpTextureType : int
    {
        /// <summary>
        /// No texture, but the value can be used as a 'texture semantic'.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// A diffuse texture that is combined with the result of the diffuse lighting equation.
        /// </summary>
        Diffuse = 0x1,

        /// <summary>
        /// A specular texture that is combined with the result of the specular lighting equation.
        /// </summary>
        Specular = 0x2,

        /// <summary>
        /// An ambient texture that is combined with the ambient lighting equation.
        /// </summary>
        Ambient = 0x3,

        /// <summary>
        /// An emissive texture that is added to the result of the lighting calculation. It is not influenced
        /// by incoming light, instead it represents the light that the object is naturally emitting.
        /// </summary>
        Emissive = 0x4,

        /// <summary>
        /// A height map texture. by convention, higher gray-scale values stand for
        /// higher elevations from some base height.
        /// </summary>
        Height = 0x5,

        /// <summary>
        /// A tangent-space normal map. There are several conventions for normal maps
        /// and Assimp does (intentionally) not distinguish here.
        /// </summary>
        Normals = 0x6,

        /// <summary>
        /// A texture that defines the glossiness of the material. This is the exponent of the specular (phong)
        /// lighting equation. Usually there is a conversion function defined to map the linear color values
        /// in the texture to a suitable exponent.
        /// </summary>
        Shininess = 0x7,

        /// <summary>
        /// The texture defines per-pixel opacity. usually 'white' means opaque and 'black' means 'transparency. Or quite
        /// the opposite.
        /// </summary>
        Opacity = 0x8,

        /// <summary>
        /// A displacement texture. The exact purpose and format is application-dependent. Higher color values stand for higher vertex displacements.
        /// </summary>
        Displacement = 0x9,

        /// <summary>
        /// A lightmap texture (aka Ambient occlusion). Both 'lightmaps' and dedicated 'ambient occlusion maps' are covered by this material property. The
        /// texture contains a scaling value for the final color value of a pixel. Its intensity is not affected by incoming light.
        /// </summary>
        Lightmap = 0xA,

        /// <summary>
        /// A reflection texture. Contains the color of a perfect mirror reflection. This is rarely used, almost never for real-time applications.
        /// </summary>
        Reflection = 0xB,

        /// <summary>
        /// An unknown texture that does not mention any of the defined texture type definitions. It is still imported, but is excluded from any
        /// further postprocessing.
        /// </summary>
        Unknown = 0xC
    }

    /// <summary>
    /// Defines how the Nth texture of a specific type is combined
    /// with the result of all previous layers.
    /// <para>
    /// Example (left: key, right: value):
    /// <code>
    /// DiffColor0     - gray
    /// DiffTextureOp0 - TextureOperation.Multiply
    /// DiffTexture0   - tex1.png
    /// DiffTextureOp0 - TextureOperation.Add
    /// DiffTexture1   - tex2.png
    /// </code>
    /// <para>
    /// Written as an equation, the final diffuse term for a specific
    /// pixel would be:
    /// </para>
    /// <code>
    /// diffFinal = DiffColor0 * sampleTex(DiffTexture0, UV0) + sampleTex(DiffTexture1, UV0) * diffContrib;
    /// </code>
    /// </para>
    /// </summary>
    internal enum assimpTextureOperation
    {
        /// <summary>
        /// T = T1 * T2
        /// </summary>
        Multiply = 0x0,

        /// <summary>
        /// T = T1 + T2
        /// </summary>
        Add = 0x1,

        /// <summary>
        /// T = T1 - T2
        /// </summary>
        Subtract = 0x2,

        /// <summary>
        /// T = T1 / T2
        /// </summary>
        Divide = 0x3,

        /// <summary>
        /// T = (T1 + T2) - (T1 * T2)
        /// </summary>
        SmoothAdd = 0x4,

        /// <summary>
        /// T = T1 + (T2 - 0.5)
        /// </summary>
        SignedAdd = 0x5
    }

    /// <summary>
    /// Defines material property types.
    /// </summary>
    internal enum AssimpPropertyType
    {
        /// <summary>
        /// Array of single-precision (32 bit) floats.
        /// </summary>
        Float = 0x1,

        /// <summary>
        /// Property is a string.
        /// </summary>
        String = 0x3,

        /// <summary>
        /// Array of 32 bit integers.
        /// </summary>
        Integer = 0x4,

        /// <summary>
        /// Byte buffer where the content is undefined.
        /// </summary>
        Buffer = 0x5
    }

    /// <summary>
    /// Defines all shading models supported by the library.
    /// <para>
    /// The list of shading modes has been taken from Blender. See Blender
    /// documentation for more information.
    /// </para>
    /// </summary>
    internal enum AssimpShadingMode
    {
        /// <summary>
        /// No shading mode defined.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Flat shading. Shading is done on a per-face basis and is diffuse only. Also known
        /// as 'faceted shading'.
        /// </summary>
        Flat = 0x1,

        /// <summary>
        /// Simple Gouraud shading.
        /// </summary>
        Gouraud = 0x2,

        /// <summary>
        /// Phong Shading.
        /// </summary>
        Phong = 0x3,

        /// <summary>
        /// Phong-Blinn Shading.
        /// </summary>
        Blinn = 0x4,

        /// <summary>
        /// Toon-shading, also known as a 'comic' shader.
        /// </summary>
        Toon = 0x5,

        /// <summary>
        /// OrenNayer shading model. Extension to standard Lambertian shading, taking the roughness
        /// of the material into account.
        /// </summary>
        OrenNayar = 0x6,

        /// <summary>
        /// Minnaert shading model. Extension to standard Lambertian shading, taking the "darkness" of
        /// the material into account.
        /// </summary>
        Minnaert = 0x7,

        /// <summary>
        /// CookTorrance shading model. Special shader for metallic surfaces.
        /// </summary>
        CookTorrance = 0x8,

        /// <summary>
        /// No shading at all. Constant light influence of 1.0.
        /// </summary>
        NoShading = 0x9,

        /// <summary>
        /// Fresnel shading.
        /// </summary>
        Fresnel = 0xa
    }

    /// <summary>
    /// Defines alpha blending flags, how the final
    /// color value of a pixel is computed, based on the following equation:
    /// <para>
    /// sourceColor * sourceBlend + destColor * destBlend
    /// </para>
    /// <para>
    /// Where the destColor is the previous color in the frame buffer
    /// and sourceColor is the material color before the
    /// transparency calculation. This corresponds to the AI_MATKEY_BLEND_FUNC property.</para>
    /// </summary>
    internal enum AssimpBlendMode
    {
        /// <summary>
        /// Default blending: sourceColor * sourceAlpha + destColor * (1 - sourceAlpha)
        /// </summary>
        Default = 0x0,

        /// <summary>
        /// Additive blending: sourcecolor * 1 + destColor * 1.
        /// </summary>
        Additive = 0x1,
    }

    /// <summary>
    /// Constant values.
    /// </summary>
    internal class AssimpConstants
    {
        #region Config Defaults

        /// <summary>
        /// Default value.
        /// </summary>
        public const int AI_SLM_DEFAULT_MAX_TRIANGLES = 1000000;

        /// <summary>
        /// Default value.
        /// </summary>
        public const int AI_SLM_DEFAULT_MAX_VERTICES = 1000000;

        /// <summary>
        /// Default value.
        /// </summary>
        public const int AI_LBW_MAX_WEIGHTS = 0x4;

        /// <summary>
        /// Default value.
        /// </summary>
        public const int PP_ICL_PTCACHE_SIZE = 12;

        /// <summary>
        /// Default value.
        /// </summary>
        public const int AI_UVTRAFO_ALL = (int)(AssimpUVTransformFlags.Rotation | AssimpUVTransformFlags.Scaling | AssimpUVTransformFlags.Translation);

        #endregion

        #region Mesh Limits

        /// <summary>
        /// Defines the maximum number of indices per face (polygon).
        /// </summary>
        public const int AI_MAX_FACE_INDICES = 0x7fff;

        /// <summary>
        /// Defines the maximum number of bone weights.
        /// </summary>
        public const int AI_MAX_BONE_WEIGHTS = 0x7fffffff;

        /// <summary>
        /// Defines the maximum number of vertices per mesh.
        /// </summary>
        public const int AI_MAX_VERTICES = 0x7fffffff;

        /// <summary>
        /// Defines the maximum number of faces per mesh.
        /// </summary>
        public const int AI_MAX_FACES = 0x7fffffff;

        /// <summary>
        /// Defines the maximum number of vertex color sets per mesh.
        /// </summary>
        public const int AI_MAX_NUMBER_OF_COLOR_SETS = 0x8;

        /// <summary>
        /// Defines the maximum number of texture coordinate sets (UV(W) channels) per mesh.
        /// </summary>
        public const int AI_MAX_NUMBER_OF_TEXTURECOORDS = 0x8;

        /// <summary>
        /// Defines the default bone count limit.
        /// </summary>
        public const int AI_SBBC_DEFAULT_MAX_BONES = 60;

        /// <summary>
        /// Defines the deboning threshold.
        /// </summary>
        public const float AI_DEBONE_THRESHOLD = 1.0f;

        #endregion

        #region Types limits
        /// <summary>
        /// Defines the maximum length of a string used in AiString.
        /// </summary>
        public const int MAX_LENGTH = 1024;
        #endregion

        #region Material limits
        /// <summary>
        /// Defines the default color material.
        /// </summary>
        public const string AI_DEFAULT_MATERIAL_NAME = "DefaultMaterial";

        /// <summary>
        /// Defines the default textured material (if the meshes have UV coords).
        /// </summary>
        public const string AI_DEFAULT_TEXTURED_MATERIAL_NAME = "TexturedDefaultMaterial";
        #endregion
    }

    /// <summary>
    /// Static class containing material key constants. A fully qualified mat key
    /// name here means that it's a string that combines the mat key (base) name, its
    /// texture type semantic, and its texture index into a single string delimited by
    /// commas. For non-texture material properties, the texture type semantic and texture
    /// index are always zero.
    /// </summary>
    internal static class AssimpMaterialKeys
    {
        /// <summary>
        /// Material name (string)
        /// </summary>
        public const string NAME_BASE = "?mat.name";

        /// <summary>
        /// Material name (string)
        /// </summary>
        public const string NAME = "?mat.name,0,0";

        /// <summary>
        /// Two sided property (boolean)
        /// </summary>
        public const string TWOSIDED_BASE = "$mat.twosided";

        /// <summary>
        /// Two sided property (boolean)
        /// </summary>
        public const string TWOSIDED = "$mat.twosided,0,0";

        /// <summary>
        /// Shading mode property (ShadingMode)
        /// </summary>
        public const string SHADING_MODEL_BASE = "$mat.shadingm";

        /// <summary>
        /// Shading mode property (ShadingMode)
        /// </summary>
        public const string SHADING_MODEL = "$mat.shadingm,0,0";

        /// <summary>
        /// Enable wireframe property (boolean)
        /// </summary>
        public const string ENABLE_WIREFRAME_BASE = "$mat.wireframe";

        /// <summary>
        /// Enable wireframe property (boolean)
        /// </summary>
        public const string ENABLE_WIREFRAME = "$mat.wireframe,0,0";

        /// <summary>
        /// Blending function (BlendMode)
        /// </summary>
        public const string BLEND_FUNC_BASE = "$mat.blend";

        /// <summary>
        /// Blending function (BlendMode)
        /// </summary>
        public const string BLEND_FUNC = "$mat.blend,0,0";

        /// <summary>
        /// Opacity (float)
        /// </summary>
        public const string OPACITY_BASE = "$mat.opacity";

        /// <summary>
        /// Opacity (float)
        /// </summary>
        public const string OPACITY = "$mat.opacity,0,0";

        /// <summary>
        /// Bumpscaling (float)
        /// </summary>
        public const string BUMPSCALING_BASE = "$mat.bumpscaling";

        /// <summary>
        /// Bumpscaling (float)
        /// </summary>
        public const string BUMPSCALING = "$mat.bumpscaling,0,0";

        /// <summary>
        /// Shininess (float)
        /// </summary>
        public const string SHININESS_BASE = "$mat.shininess";

        /// <summary>
        /// Shininess (float)
        /// </summary>
        public const string SHININESS = "$mat.shininess,0,0";

        /// <summary>
        /// Reflectivity (float)
        /// </summary>
        public const string REFLECTIVITY_BASE = "$mat.reflectivity";

        /// <summary>
        /// Reflectivity (float)
        /// </summary>
        public const string REFLECTIVITY = "$mat.reflectivity,0,0";

        /// <summary>
        /// Shininess strength (float)
        /// </summary>
        public const string SHININESS_STRENGTH_BASE = "$mat.shinpercent";

        /// <summary>
        /// Shininess strength (float)
        /// </summary>
        public const string SHININESS_STRENGTH = "$mat.shinpercent,0,0";

        /// <summary>
        /// Refracti (float)
        /// </summary>
        public const string REFRACTI_BASE = "$mat.refracti";

        /// <summary>
        /// Refracti (float)
        /// </summary>
        public const string REFRACTI = "$mat.refracti,0,0";

        /// <summary>
        /// Diffuse color (Color4D)
        /// </summary>
        public const string COLOR_DIFFUSE_BASE = "$clr.diffuse";

        /// <summary>
        /// Diffuse color (Color4D)
        /// </summary>
        public const string COLOR_DIFFUSE = "$clr.diffuse,0,0";

        /// <summary>
        /// Ambient color (Color4D)
        /// </summary>
        public const string COLOR_AMBIENT_BASE = "$clr.ambient";

        /// <summary>
        /// Ambient color (Color4D)
        /// </summary>
        public const string COLOR_AMBIENT = "$clr.ambient,0,0";

        /// <summary>
        /// Specular color (Color4D)
        /// </summary>
        public const string COLOR_SPECULAR_BASE = "$clr.specular";

        /// <summary>
        /// Specular color (Color4D)
        /// </summary>
        public const string COLOR_SPECULAR = "$clr.specular,0,0";

        /// <summary>
        /// Emissive color (Color4D)
        /// </summary>
        public const string COLOR_EMISSIVE_BASE = "$clr.emissive";

        /// <summary>
        /// Emissive color (Color4D)
        /// </summary>
        public const string COLOR_EMISSIVE = "$clr.emissive,0,0";

        /// <summary>
        /// Transparent color (Color4D)
        /// </summary>
        public const string COLOR_TRANSPARENT_BASE = "$clr.transparent";

        /// <summary>
        /// Transparent color (Color4D)
        /// </summary>
        public const string COLOR_TRANSPARENT = "$clr.transparent,0,0";

        /// <summary>
        /// Reflective color (Color4D)
        /// </summary>
        public const string COLOR_REFLECTIVE_BASE = "$clr.reflective";

        /// <summary>
        /// Reflective color (Color4D)
        /// </summary>
        public const string COLOR_REFLECTIVE = "$clr.reflective,0,0";

        /// <summary>
        /// Background image (string)
        /// </summary>
        public const string GLOBAL_BACKGROUND_IMAGE_BASE = "?bg.global";

        /// <summary>
        /// Background image (string)
        /// </summary>
        public const string GLOBAL_BACKGROUND_IMAGE = "?bg.global,0,0";

        /// <summary>
        /// Texture base name
        /// </summary>
        public const string TEXTURE_BASE = "$tex.file";

        /// <summary>
        /// UVWSRC base name
        /// </summary>
        public const string UVWSRC_BASE = "$tex.uvwsrc";

        /// <summary>
        /// Texture op base name
        /// </summary>
        public const string TEXOP_BASE = "$tex.op";

        /// <summary>
        /// Mapping base name
        /// </summary>
        public const string MAPPING_BASE = "$tex.mapping";

        /// <summary>
        /// Texture blend base name.
        /// </summary>
        public const string TEXBLEND_BASE = "$tex.blend";

        /// <summary>
        /// Mapping mode U base name
        /// </summary>
        public const string MAPPINGMODE_U_BASE = "$tex.mapmodeu";

        /// <summary>
        /// Mapping mode V base name
        /// </summary>
        public const string MAPPINGMODE_V_BASE = "$tex.mapmodev";

        /// <summary>
        /// Texture map axis base name
        /// </summary>
        public const string TEXMAP_AXIS_BASE = "$tex.mapaxis";

        /// <summary>
        /// UV transform base name
        /// </summary>
        public const string UVTRANSFORM_BASE = "$tex.uvtrafo";

        /// <summary>
        /// Texture flags base name
        /// </summary>
        public const string TEXFLAGS_BASE = "$tex.flags";

        /// <summary>
        /// Helper function to get the fully qualified name of a texture property type name. Takes
        /// in a base name constant, a texture type, and a texture index and outputs the name in the format:
        /// <para>"baseName,TextureType,texIndex"</para>
        /// </summary>
        /// <param name="baseName">Base name</param>
        /// <param name="texType">Texture type</param>
        /// <param name="texIndex">Texture index</param>
        /// <returns>Fully qualified texture name</returns>
        public static string GetFullTextureName(string baseName, AssimpTextureType texType, int texIndex)
        {
            return string.Format("{0},{1},{2}", baseName, (int)texType, texIndex);
        }

        /// <summary>
        /// Helper function to get the base name from a fully qualified name of a material property type name. The format
        /// of such a string is:
        /// <para>"baseName,TextureType,texIndex"</para>
        /// </summary>
        /// <param name="fullyQualifiedName">Fully qualified material property name.</param>
        /// <returns>Base name of the property type.</returns>
        public static string GetBaseName(string fullyQualifiedName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedName))
                return string.Empty;

            string[] substrings = fullyQualifiedName.Split(',');
            if (substrings != null && substrings.Length == 3)
                return substrings[0];

            return string.Empty;
        }
    }

    #endregion
}
