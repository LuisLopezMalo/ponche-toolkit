using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Graphics3D
{
    using PoncheToolkit.Util;
    using SharpDX;
    /// <summary>
    /// A material contains all the information that describes how to render a mesh. E.g. textures, colors, and render states. Internally
    /// all this information is stored as key-value pair properties. The class contains many convienence methods and properties for
    /// accessing non-texture/texture properties without having to know the Assimp material key names. Not all properties may be present,
    /// and if they aren't a default value will be returned.
    /// </summary>
    internal sealed class AssimpMaterial : ModelPiece
    {
        private Dictionary<string, MaterialProperty> m_properties;

        /// <summary>
        /// Gets the number of properties contained in the material.
        /// </summary>
        public int PropertyCount { get { return m_properties.Count; } }

        #region Convienent non-texture properties
        /// <summary>
        /// Checks if the material has a name property.
        /// </summary>
        public bool HasName { get { return HasProperty(AssimpMaterialKeys.NAME); } }

        /// <summary>
        /// Gets the material name value, if any. Default value is an empty string.
        /// </summary>
        public string Name
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.NAME);
                if (prop != null)
                    return prop.GetStringValue();

                return string.Empty;
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.NAME);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.NAME_BASE, value);
                    AddProperty(prop);
                }

                prop.SetStringValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a two-sided property.
        /// </summary>
        public bool HasTwoSided { get { return HasProperty(AssimpMaterialKeys.TWOSIDED); } }

        /// <summary>
        /// Gets if the material should be rendered as two-sided. Default value is false.
        /// </summary>
        public bool IsTwoSided
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.TWOSIDED);
                if (prop != null)
                    return prop.GetBooleanValue();

                return false;
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.TWOSIDED);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.TWOSIDED_BASE, value);
                    AddProperty(prop);
                }

                prop.SetBooleanValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a shading-mode property.
        /// </summary>
        public bool HasShadingMode { get { return HasProperty(AssimpMaterialKeys.SHADING_MODEL); } }

        /// <summary>
        /// Gets the shading mode. Default value is <see cref="PoncheToolkit.AssimpShadingMode.None"/>, meaning it is not defined.
        /// </summary>
        public AssimpShadingMode ShadingMode
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.SHADING_MODEL);
                if (prop != null)
                    return (AssimpShadingMode)prop.GetIntegerValue();

                return AssimpShadingMode.None;
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.SHADING_MODEL);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.SHADING_MODEL_BASE, (int)value);
                    AddProperty(prop);
                }

                prop.SetIntegerValue((int)value);
            }
        }

        /// <summary>
        /// Checks if the material has a wireframe property.
        /// </summary>
        public bool HasWireFrame
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.ENABLE_WIREFRAME);
            }
        }

        /// <summary>
        /// Gets if wireframe should be enabled. Default value is false.
        /// </summary>
        public bool IsWireFrameEnabled
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.ENABLE_WIREFRAME);
                if (prop != null)
                    return prop.GetBooleanValue();

                return false;
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.ENABLE_WIREFRAME);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.ENABLE_WIREFRAME_BASE, value);
                    AddProperty(prop);
                }

                prop.SetBooleanValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a blend mode property.
        /// </summary>
        public bool HasBlendMode
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.BLEND_FUNC);
            }
        }

        /// <summary>
        /// Gets the blending mode. Default value is <see cref="PoncheToolkit.AssimpBlendMode.Default"/>.
        /// </summary>
        public AssimpBlendMode BlendMode
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.BLEND_FUNC);
                if (prop != null)
                    return (AssimpBlendMode)prop.GetIntegerValue();

                return AssimpBlendMode.Default;
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.BLEND_FUNC);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.BLEND_FUNC_BASE, (int)value);
                    AddProperty(prop);
                }

                prop.SetIntegerValue((int)value);
            }
        }

        /// <summary>
        /// Checks if the material has an opacity property.
        /// </summary>
        public bool HasOpacity
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.OPACITY);
            }
        }

        /// <summary>
        /// Gets the opacity. Default value is 1.0f.
        /// </summary>
        public float Opacity
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.OPACITY);
                if (prop != null)
                    return prop.GetFloatValue();

                return 1.0f;
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.OPACITY);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.OPACITY_BASE, value);
                    AddProperty(prop);
                }

                prop.SetFloatValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a bump scaling property.
        /// </summary>
        public bool HasBumpScaling
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.BUMPSCALING);
            }
        }

        /// <summary>
        /// Gets the bump scaling. Default value is 0.0f;
        /// </summary>
        public float BumpScaling
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.BUMPSCALING);
                if (prop != null)
                    return prop.GetFloatValue();

                return 0.0f;
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.BUMPSCALING);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.BUMPSCALING_BASE, value);
                    AddProperty(prop);
                }

                prop.SetFloatValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a shininess property.
        /// </summary>
        public bool HasShininess
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.SHININESS);
            }
        }

        /// <summary>
        /// Gets the shininess. Default value is 0.0f;
        /// </summary>
        public float Shininess
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.SHININESS);
                if (prop != null)
                    return prop.GetFloatValue();

                return 0.0f;
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.SHININESS);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.SHININESS_BASE, value);
                    AddProperty(prop);
                }

                prop.SetFloatValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a shininess strength property.
        /// </summary>
        public bool HasShininessStrength
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.SHININESS_STRENGTH);
            }
        }

        /// <summary>
        /// Gets the shininess strength. Default vaulue is 1.0f.
        /// </summary>
        public float ShininessStrength
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.SHININESS_STRENGTH);
                if (prop != null)
                    return prop.GetFloatValue();

                return 1.0f;
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.SHININESS_STRENGTH);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.SHININESS_STRENGTH_BASE, value);
                    AddProperty(prop);
                }

                prop.SetFloatValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a reflectivty property.
        /// </summary>
        public bool HasReflectivity
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.REFLECTIVITY);
            }
        }


        /// <summary>
        /// Gets the reflectivity. Default value is 0.0f;
        /// </summary>
        public float Reflectivity
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.REFLECTIVITY);
                if (prop != null)
                    return prop.GetFloatValue();

                return 0.0f;
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.REFLECTIVITY);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.REFLECTIVITY_BASE, value);
                    AddProperty(prop);
                }

                prop.SetFloatValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a color diffuse property.
        /// </summary>
        public bool HasColorDiffuse
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.COLOR_DIFFUSE);
            }
        }

        /// <summary>
        /// Gets the color diffuse. Default value is white.
        /// </summary>
        public Color ColorDiffuse
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_DIFFUSE);
                if (prop != null)
                    return prop.GetColor4DValue();

                return new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_DIFFUSE);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.COLOR_DIFFUSE_BASE, value);
                    AddProperty(prop);
                }

                prop.SetColor4DValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a color ambient property.
        /// </summary>
        public bool HasColorAmbient { get { return HasProperty(AssimpMaterialKeys.COLOR_AMBIENT); } }

        /// <summary>
        /// Gets the color ambient. Default value is (.2f, .2f, .2f, 1.0f).
        /// </summary>
        public Color ColorAmbient
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_AMBIENT);
                if (prop != null)
                    return prop.GetColor4DValue();

                return new Color(.2f, .2f, .2f, 1.0f);
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_AMBIENT);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.COLOR_AMBIENT_BASE, value);
                    AddProperty(prop);
                }

                prop.SetColor4DValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a color specular property.
        /// </summary>
        public bool HasColorSpecular { get { return HasProperty(AssimpMaterialKeys.COLOR_SPECULAR); } }

        /// <summary>
        /// Gets the color specular. Default value is black.
        /// </summary>
        public Color ColorSpecular
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_SPECULAR);
                if (prop != null)
                    return prop.GetColor4DValue();

                return new Color(0, 0, 0, 1.0f);
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_SPECULAR);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.COLOR_SPECULAR_BASE, value);
                    AddProperty(prop);
                }

                prop.SetColor4DValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a color emissive property.
        /// </summary>
        public bool HasColorEmissive
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.COLOR_EMISSIVE);
            }
        }

        /// <summary>
        /// Gets the color emissive. Default value is black.
        /// </summary>
        public Color ColorEmissive
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_EMISSIVE);
                if (prop != null)
                    return prop.GetColor4DValue();

                return new Color(0, 0, 0, 1.0f);
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_EMISSIVE);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.COLOR_EMISSIVE_BASE, value);
                    AddProperty(prop);
                }

                prop.SetColor4DValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a color transparent property.
        /// </summary>
        public bool HasColorTransparent
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.COLOR_TRANSPARENT);
            }
        }

        /// <summary>
        /// Gets the color transparent. Default value is black.
        /// </summary>
        public Color ColorTransparent
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_TRANSPARENT);
                if (prop != null)
                    return prop.GetColor4DValue();

                return new Color(0, 0, 0, 1.0f);
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_TRANSPARENT);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.COLOR_TRANSPARENT_BASE, value);
                    AddProperty(prop);
                }

                prop.SetColor4DValue(value);
            }
        }

        /// <summary>
        /// Checks if the material has a color reflective property.
        /// </summary>
        public bool HasColorReflective
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.COLOR_REFLECTIVE);
            }
        }

        /// <summary>
        /// Gets the color reflective. Default value is black.
        /// </summary>
        public Color ColorReflective
        {
            get
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_REFLECTIVE);
                if (prop != null)
                    return prop.GetColor4DValue();

                return new Color(0, 0, 0, 1.0f);
            }
            set
            {
                MaterialProperty prop = GetProperty(AssimpMaterialKeys.COLOR_REFLECTIVE);

                if (prop == null)
                {
                    prop = new MaterialProperty(AssimpMaterialKeys.COLOR_REFLECTIVE_BASE, value);
                    AddProperty(prop);
                }

                prop.SetColor4DValue(value);
            }
        }

        #endregion

        #region Convienent texture properties

        /// <summary>
        /// Gets if the material has a diffuse texture in the first texture index.
        /// </summary>
        public bool HasTextureDiffuse
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.TEXTURE_BASE, AssimpTextureType.Diffuse, 0);
            }
        }

        /// <summary>
        /// Gets or sets diffuse texture properties in the first texture index.
        /// </summary>
        public MaterialTexture TextureDiffuse
        {
            get
            {
                MaterialTexture tex;
                GetMaterialTexture(AssimpTextureType.Diffuse, 0, out tex);

                return tex;
            }
            set
            {
                if (value.TextureIndex == 0 && value.TextureType == AssimpTextureType.Diffuse)
                    AddMaterialTexture(ref value);
            }
        }

        /// <summary>
        /// Gets if the material has a specular texture in the first texture index.
        /// </summary>
        public bool HasTextureSpecular
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.TEXTURE_BASE, AssimpTextureType.Specular, 0);
            }
        }

        /// <summary>
        /// Gets or sets specular texture properties in the first texture index.
        /// </summary>
        public MaterialTexture TextureSpecular
        {
            get
            {
                MaterialTexture tex;
                GetMaterialTexture(AssimpTextureType.Specular, 0, out tex);

                return tex;
            }
            set
            {
                if (value.TextureIndex == 0 && value.TextureType == AssimpTextureType.Specular)
                    AddMaterialTexture(ref value);
            }
        }

        /// <summary>
        /// Gets if the material has a ambient texture in the first texture index.
        /// </summary>
        public bool HasTextureAmbient
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.TEXTURE_BASE, AssimpTextureType.Ambient, 0);
            }
        }

        /// <summary>
        /// Gets or sets ambient texture properties in the first texture index.
        /// </summary>
        public MaterialTexture TextureAmbient
        {
            get
            {
                MaterialTexture tex;
                GetMaterialTexture(AssimpTextureType.Ambient, 0, out tex);

                return tex;
            }
            set
            {
                if (value.TextureIndex == 0 && value.TextureType == AssimpTextureType.Ambient)
                    AddMaterialTexture(ref value);
            }
        }

        /// <summary>
        /// Gets if the material has a emissive texture in the first texture index.
        /// </summary>
        public bool HasTextureEmissive
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.TEXTURE_BASE, AssimpTextureType.Emissive, 0);
            }
        }

        /// <summary>
        /// Gets or sets emissive texture properties in the first texture index.
        /// </summary>
        public MaterialTexture TextureEmissive
        {
            get
            {
                MaterialTexture tex;
                GetMaterialTexture(AssimpTextureType.Emissive, 0, out tex);

                return tex;
            }
            set
            {
                if (value.TextureIndex == 0 && value.TextureType == AssimpTextureType.Emissive)
                    AddMaterialTexture(ref value);
            }
        }

        /// <summary>
        /// Gets if the material has a height texture in the first texture index.
        /// </summary>
        public bool HasTextureHeight
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.TEXTURE_BASE, AssimpTextureType.Height, 0);
            }
        }

        /// <summary>
        /// Gets or sets height texture properties in the first texture index.
        /// </summary>
        public MaterialTexture TextureHeight
        {
            get
            {
                MaterialTexture tex;
                GetMaterialTexture(AssimpTextureType.Height, 0, out tex);

                return tex;
            }
            set
            {
                if (value.TextureIndex == 0 && value.TextureType == AssimpTextureType.Height)
                    AddMaterialTexture(ref value);
            }
        }

        /// <summary>
        /// Gets if the material has a normal texture in the first texture index.
        /// </summary>
        public bool HasTextureNormal
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.TEXTURE_BASE, AssimpTextureType.Normals, 0);
            }
        }

        /// <summary>
        /// Gets or sets normal texture properties in the first texture index.
        /// </summary>
        public MaterialTexture TextureNormal
        {
            get
            {
                MaterialTexture tex;
                GetMaterialTexture(AssimpTextureType.Normals, 0, out tex);

                return tex;
            }
            set
            {
                if (value.TextureIndex == 0 && value.TextureType == AssimpTextureType.Normals)
                    AddMaterialTexture(ref value);
            }
        }

        /// <summary>
        /// Gets if the material has an opacity texture in the first texture index.
        /// </summary>
        public bool HasTextureOpacity
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.TEXTURE_BASE, AssimpTextureType.Opacity, 0);
            }
        }

        /// <summary>
        /// Gets or sets opacity texture properties in the first texture index.
        /// </summary>
        public MaterialTexture TextureOpacity
        {
            get
            {
                MaterialTexture tex;
                GetMaterialTexture(AssimpTextureType.Opacity, 0, out tex);

                return tex;
            }
            set
            {
                if (value.TextureIndex == 0 && value.TextureType == AssimpTextureType.Opacity)
                    AddMaterialTexture(ref value);
            }
        }

        /// <summary>
        /// Gets if the material has a displacement texture in the first texture index.
        /// </summary>
        public bool HasTextureDisplacement
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.TEXTURE_BASE, AssimpTextureType.Displacement, 0);
            }
        }

        /// <summary>
        /// Gets or sets displacement texture properties in the first texture index.
        /// </summary>
        public MaterialTexture TextureDisplacement
        {
            get
            {
                MaterialTexture tex;
                GetMaterialTexture(AssimpTextureType.Displacement, 0, out tex);

                return tex;
            }
            set
            {
                if (value.TextureIndex == 0 && value.TextureType == AssimpTextureType.Displacement)
                    AddMaterialTexture(ref value);
            }
        }

        /// <summary>
        /// Gets if the material has a light map texture in the first texture index.
        /// </summary>
        public bool HasTextureLightMap
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.TEXTURE_BASE, AssimpTextureType.Lightmap, 0);
            }
        }

        /// <summary>
        /// Gets or sets light map texture properties in the first texture index.
        /// </summary>
        public MaterialTexture TextureLightMap
        {
            get
            {
                MaterialTexture tex;
                GetMaterialTexture(AssimpTextureType.Lightmap, 0, out tex);

                return tex;
            }
            set
            {
                if (value.TextureIndex == 0 && value.TextureType == AssimpTextureType.Lightmap)
                    AddMaterialTexture(ref value);
            }
        }

        /// <summary>
        /// Gets if the material has a reflection texture in the first texture index.
        /// </summary>
        public bool HasTextureReflection
        {
            get
            {
                return HasProperty(AssimpMaterialKeys.TEXTURE_BASE, AssimpTextureType.Reflection, 0);
            }
        }

        /// <summary>
        /// Gets or sets reflection texture properties in the first texture index.
        /// </summary>
        public MaterialTexture TextureReflection
        {
            get
            {
                MaterialTexture tex;
                GetMaterialTexture(AssimpTextureType.Reflection, 0, out tex);

                return tex;
            }
            set
            {
                if (value.TextureIndex == 0 && value.TextureType == AssimpTextureType.Reflection)
                    AddMaterialTexture(ref value);
            }
        }

        #endregion

        /// <summary>
        /// Constructs a new instance of the <see cref="AssimpMaterial"/> class.
        /// </summary>
        public AssimpMaterial()
        {
            m_properties = new Dictionary<string, MaterialProperty>();
        }

        /// <summary>
        /// Helper method to construct a fully qualified name from the input parameters. All the input parameters are combined into the fully qualified name: {baseName},{texType},{texIndex}. E.g.
        /// "$clr.diffuse,0,0" or "$tex.file,1,0". This is the name that is used as the material dictionary key.
        /// </summary>
        /// <param name="baseName">Key basename, this must not be null or empty</param>
        /// <param name="texType">Texture type; non-texture properties should leave this <see cref="AssimpTextureType.None"/></param>
        /// <param name="texIndex">Texture index; non-texture properties should leave this zero.</param>
        /// <returns>The fully qualified name</returns>
        public static string CreateFullyQualifiedName(string baseName, AssimpTextureType texType, int texIndex)
        {
            if (string.IsNullOrEmpty(baseName))
                return string.Empty;

            return string.Format("{0},{1},{2}", baseName, (int)texType, texIndex);
        }

        /// <summary>
        /// Gets the non-texture properties contained in this Material. The name should be
        /// the "base name", as in it should not contain texture type/texture index information. E.g. "$clr.diffuse" rather than "$clr.diffuse,0,0". The extra
        /// data will be filled in automatically.
        /// </summary>
        /// <param name="baseName">Key basename</param>
        /// <returns>The material property, if it exists</returns>
        public MaterialProperty GetNonTextureProperty(string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                return null;
            }
            string fullyQualifiedName = CreateFullyQualifiedName(baseName, AssimpTextureType.None, 0);
            return GetProperty(fullyQualifiedName);
        }

        /// <summary>
        /// Gets the material property. All the input parameters are combined into the fully qualified name: {baseName},{texType},{texIndex}. E.g.
        /// "$clr.diffuse,0,0" or "$tex.file,1,0".
        /// </summary>
        /// <param name="baseName">Key basename</param>
        /// <param name="texType">Texture type; non-texture properties should leave this <see cref="AssimpTextureType.None"/></param>
        /// <param name="texIndex">Texture index; non-texture properties should leave this zero.</param>
        /// <returns>The material property, if it exists</returns>
        public MaterialProperty GetProperty(string baseName, AssimpTextureType texType, int texIndex)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                return null;
            }
            string fullyQualifiedName = CreateFullyQualifiedName(baseName, texType, texIndex);
            return GetProperty(fullyQualifiedName);
        }

        /// <summary>
        /// Gets the material property by its fully qualified name. The format is: {baseName},{texType},{texIndex}. E.g.
        /// "$clr.diffuse,0,0" or "$tex.file,1,0".
        /// </summary>
        /// <param name="fullyQualifiedName">Fully qualified name of the property</param>
        /// <returns>The material property, if it exists</returns>
        public MaterialProperty GetProperty(string fullyQualifiedName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedName))
            {
                return null;
            }
            MaterialProperty prop;
            if (!m_properties.TryGetValue(fullyQualifiedName, out prop))
            {
                return null;
            }
            return prop;
        }

        /// <summary>
        /// Checks if the material has the specified non-texture property. The name should be
        /// the "base name", as in it should not contain texture type/texture index information. E.g. "$clr.diffuse" rather than "$clr.diffuse,0,0". The extra
        /// data will be filled in automatically.
        /// </summary>
        /// <param name="baseName">Key basename</param>
        /// <returns>True if the property exists, false otherwise.</returns>
        public bool HasNonTextureProperty(string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                return false;
            }
            string fullyQualifiedName = CreateFullyQualifiedName(baseName, AssimpTextureType.None, 0);
            return HasProperty(fullyQualifiedName);
        }

        /// <summary>
        /// Checks if the material has the specified property. All the input parameters are combined into the fully qualified name: {baseName},{texType},{texIndex}. E.g.
        /// "$clr.diffuse,0,0" or "$tex.file,1,0".
        /// </summary>
        /// <param name="baseName">Key basename</param>
        /// <param name="texType">Texture type; non-texture properties should leave this <see cref="AssimpTextureType.None"/></param>
        /// <param name="texIndex">Texture index; non-texture properties should leave this zero.</param>
        /// <returns>True if the property exists, false otherwise.</returns>
        public bool HasProperty(string baseName, AssimpTextureType texType, int texIndex)
        {
            if (string.IsNullOrEmpty(baseName))
            {
                return false;
            }

            string fullyQualifiedName = CreateFullyQualifiedName(baseName, texType, texIndex);
            return HasProperty(fullyQualifiedName);
        }

        /// <summary>
        /// Checks if the material has the specified property by looking up its fully qualified name. The format is: {baseName},{texType},{texIndex}. E.g.
        /// "$clr.diffuse,0,0" or "$tex.file,1,0".
        /// </summary>
        /// <param name="fullyQualifiedName">Fully qualified name of the property</param>
        /// <returns>True if the property exists, false otherwise.</returns>
        public bool HasProperty(string fullyQualifiedName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedName))
            {
                return false;
            }
            return m_properties.ContainsKey(fullyQualifiedName);
        }

        /// <summary>
        /// Adds a property to this material.
        /// </summary>
        /// <param name="matProp">Material property</param>
        /// <returns>True if the property was successfully added, false otherwise (e.g. null or key already present).</returns>
        public bool AddProperty(MaterialProperty matProp)
        {
            if (matProp == null)
                return false;

            if (m_properties.ContainsKey(matProp.FullyQualifiedName))
                return false;

            m_properties.Add(matProp.FullyQualifiedName, matProp);

            return true;
        }

        /// <summary>
        /// Removes a non-texture property from the material.
        /// </summary>
        /// <param name="baseName">Property name</param>
        /// <returns>True if the property was removed, false otherwise</returns>
        public bool RemoveNonTextureProperty(string baseName)
        {
            if (string.IsNullOrEmpty(baseName))
                return false;

            return RemoveProperty(CreateFullyQualifiedName(baseName, AssimpTextureType.None, 0));
        }

        /// <summary>
        /// Removes a property from the material.
        /// </summary>
        /// <param name="baseName">Name of the property</param>
        /// <param name="texType">Property texture type</param>
        /// <param name="texIndex">Property texture index</param>
        /// <returns>True if the property was removed, false otherwise</returns>
        public bool RemoveProperty(string baseName, AssimpTextureType texType, int texIndex)
        {
            if (string.IsNullOrEmpty(baseName))
                return false;

            return RemoveProperty(CreateFullyQualifiedName(baseName, texType, texIndex));
        }

        /// <summary>
        /// Removes a property from the material.
        /// </summary>
        /// <param name="fullyQualifiedName">Fully qualified name of the property ({basename},{texType},{texIndex})</param>
        /// <returns>True if the property was removed, false otherwise</returns>
        public bool RemoveProperty(string fullyQualifiedName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedName))
                return false;

            return m_properties.Remove(fullyQualifiedName);
        }

        /// <summary>
        /// Removes all properties from the material;
        /// </summary>
        public void Clear()
        {
            m_properties.Clear();
        }

        /// <summary>
        /// Gets -all- properties contained in the Material.
        /// </summary>
        /// <returns>All properties in the material property map.</returns>
        public MaterialProperty[] GetAllProperties()
        {
            MaterialProperty[] matProps = new MaterialProperty[m_properties.Values.Count];
            m_properties.Values.CopyTo(matProps, 0);

            return matProps;
        }

        /// <summary>
        /// Gets all the number of textures that are of the specified texture type.
        /// </summary>
        /// <param name="texType">Texture type</param>
        /// <returns>Texture count</returns>
        public int GetMaterialTextureCount(AssimpTextureType texType)
        {
            int count = 0;
            foreach (KeyValuePair<string, MaterialProperty> kv in m_properties)
            {
                MaterialProperty matProp = kv.Value;

                if (matProp.Name.StartsWith(AssimpMaterialKeys.TEXTURE_BASE) && matProp.TextureType == texType)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Adds a texture to the material - this bulk creates a property for each field. This will
        /// either create properties or overwrite existing properties. If the texture has no
        /// file path, nothing is added.
        /// </summary>
        /// <param name="texture">Texture to add</param>
        /// <returns>True if the texture properties were added or modified</returns>
        public bool AddMaterialTexture(ref MaterialTexture texture)
        {
            return AddMaterialTexture(ref texture, false);
        }

        /// <summary>
        /// Adds a texture to the material - this bulk creates a property for each field. This will
        /// either create properties or overwrite existing properties. If the texture has no
        /// file path, nothing is added.
        /// </summary>
        /// <param name="texture">Texture to add</param>
        /// <param name="onlySetFilePath">True to only set the texture's file path, false otherwise</param>
        /// <returns>True if the texture properties were added or modified</returns>
        public bool AddMaterialTexture(ref MaterialTexture texture, bool onlySetFilePath)
        {
            if (string.IsNullOrEmpty(texture.FilePath))
                return false;

            AssimpTextureType texType = texture.TextureType;
            int texIndex = texture.TextureIndex;

            string texName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXTURE_BASE, texType, texIndex);

            MaterialProperty texNameProp = GetProperty(texName);

            if (texNameProp == null)
                AddProperty(new MaterialProperty(AssimpMaterialKeys.TEXTURE_BASE, texture.FilePath, texType, texIndex));
            else
                texNameProp.SetStringValue(texture.FilePath);

            if (onlySetFilePath)
                return true;

            string mappingName = CreateFullyQualifiedName(AssimpMaterialKeys.MAPPING_BASE, texType, texIndex);
            string uvIndexName = CreateFullyQualifiedName(AssimpMaterialKeys.UVWSRC_BASE, texType, texIndex);
            string blendFactorName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXBLEND_BASE, texType, texIndex);
            string texOpName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXOP_BASE, texType, texIndex);
            string uMapModeName = CreateFullyQualifiedName(AssimpMaterialKeys.MAPPINGMODE_U_BASE, texType, texIndex);
            string vMapModeName = CreateFullyQualifiedName(AssimpMaterialKeys.MAPPINGMODE_V_BASE, texType, texIndex);
            string texFlagsName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXFLAGS_BASE, texType, texIndex);

            MaterialProperty mappingNameProp = GetProperty(mappingName);
            MaterialProperty uvIndexNameProp = GetProperty(uvIndexName);
            MaterialProperty blendFactorNameProp = GetProperty(blendFactorName);
            MaterialProperty texOpNameProp = GetProperty(texOpName);
            MaterialProperty uMapModeNameProp = GetProperty(uMapModeName);
            MaterialProperty vMapModeNameProp = GetProperty(vMapModeName);
            MaterialProperty texFlagsNameProp = GetProperty(texFlagsName);

            if (mappingNameProp == null)
                AddProperty(new MaterialProperty(AssimpMaterialKeys.MAPPING_BASE, (int)texture.Mapping));
            else
                mappingNameProp.SetIntegerValue((int)texture.Mapping);

            if (uvIndexNameProp == null)
                AddProperty(new MaterialProperty(AssimpMaterialKeys.MAPPING_BASE, (int)texture.Mapping));
            else
                uvIndexNameProp.SetIntegerValue(texture.UVIndex);

            if (blendFactorNameProp == null)
                AddProperty(new MaterialProperty(AssimpMaterialKeys.TEXBLEND_BASE, texture.BlendFactor));
            else
                blendFactorNameProp.SetFloatValue(texture.BlendFactor);

            if (texOpNameProp == null)
                AddProperty(new MaterialProperty(AssimpMaterialKeys.TEXOP_BASE, (int)texture.Operation));
            else
                texOpNameProp.SetIntegerValue((int)texture.Operation);

            if (uMapModeNameProp == null)
                AddProperty(new MaterialProperty(AssimpMaterialKeys.MAPPINGMODE_U_BASE, (int)texture.WrapModeU));
            else
                uMapModeNameProp.SetIntegerValue((int)texture.WrapModeU);

            if (vMapModeNameProp == null)
                AddProperty(new MaterialProperty(AssimpMaterialKeys.MAPPINGMODE_V_BASE, (int)texture.WrapModeV));
            else
                vMapModeNameProp.SetIntegerValue((int)texture.WrapModeV);

            if (texFlagsNameProp == null)
                AddProperty(new MaterialProperty(AssimpMaterialKeys.TEXFLAGS_BASE, texture.Flags));
            else
                texFlagsNameProp.SetIntegerValue(texture.Flags);

            return true;
        }

        /// <summary>
        /// Removes a texture from the material - this bulk removes a property for each field.
        /// If the texture has no file path, nothing is removed
        /// </summary>
        /// <param name="texture">Texture to remove</param>
        /// <returns>True if the texture was removed, false otherwise.</returns>
        public bool RemoveMaterialTexture(ref MaterialTexture texture)
        {
            if (string.IsNullOrEmpty(texture.FilePath))
                return false;

            AssimpTextureType texType = texture.TextureType;
            int texIndex = texture.TextureIndex;

            string texName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXTURE_BASE, texType, texIndex);
            string mappingName = CreateFullyQualifiedName(AssimpMaterialKeys.MAPPING_BASE, texType, texIndex);
            string uvIndexName = CreateFullyQualifiedName(AssimpMaterialKeys.UVWSRC_BASE, texType, texIndex);
            string blendFactorName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXBLEND_BASE, texType, texIndex);
            string texOpName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXOP_BASE, texType, texIndex);
            string uMapModeName = CreateFullyQualifiedName(AssimpMaterialKeys.MAPPINGMODE_U_BASE, texType, texIndex);
            string vMapModeName = CreateFullyQualifiedName(AssimpMaterialKeys.MAPPINGMODE_V_BASE, texType, texIndex);
            string texFlagsName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXFLAGS_BASE, texType, texIndex);

            RemoveProperty(texName);
            RemoveProperty(mappingName);
            RemoveProperty(uvIndexName);
            RemoveProperty(blendFactorName);
            RemoveProperty(texOpName);
            RemoveProperty(uMapModeName);
            RemoveProperty(vMapModeName);
            RemoveProperty(texFlagsName);

            return true;
        }

        /// <summary>
        /// Gets a texture that corresponds to the type/index.
        /// </summary>
        /// <param name="texType">Texture type</param>
        /// <param name="texIndex">Texture index</param>
        /// <param name="texture">Texture description</param>
        /// <returns>True if the texture was found in the material</returns>
        public bool GetMaterialTexture(AssimpTextureType texType, int texIndex, out MaterialTexture texture)
        {
            texture = new MaterialTexture();

            string texName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXTURE_BASE, texType, texIndex);

            MaterialProperty texNameProp = GetProperty(texName);

            //This one is necessary, the rest are optional
            if (texNameProp == null)
                return false;

            string mappingName = CreateFullyQualifiedName(AssimpMaterialKeys.MAPPING_BASE, texType, texIndex);
            string uvIndexName = CreateFullyQualifiedName(AssimpMaterialKeys.UVWSRC_BASE, texType, texIndex);
            string blendFactorName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXBLEND_BASE, texType, texIndex);
            string texOpName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXOP_BASE, texType, texIndex);
            string uMapModeName = CreateFullyQualifiedName(AssimpMaterialKeys.MAPPINGMODE_U_BASE, texType, texIndex);
            string vMapModeName = CreateFullyQualifiedName(AssimpMaterialKeys.MAPPINGMODE_V_BASE, texType, texIndex);
            string texFlagsName = CreateFullyQualifiedName(AssimpMaterialKeys.TEXFLAGS_BASE, texType, texIndex);

            MaterialProperty mappingNameProp = GetProperty(mappingName);
            MaterialProperty uvIndexNameProp = GetProperty(uvIndexName);
            MaterialProperty blendFactorNameProp = GetProperty(blendFactorName);
            MaterialProperty texOpNameProp = GetProperty(texOpName);
            MaterialProperty uMapModeNameProp = GetProperty(uMapModeName);
            MaterialProperty vMapModeNameProp = GetProperty(vMapModeName);
            MaterialProperty texFlagsNameProp = GetProperty(texFlagsName);

            texture.FilePath = texNameProp.GetStringValue();
            texture.TextureType = texType;
            texture.TextureIndex = texIndex;
            texture.Mapping = (mappingNameProp != null) ? (AssimpTextureMapping)mappingNameProp.GetIntegerValue() : AssimpTextureMapping.FromUV;
            texture.UVIndex = (uvIndexNameProp != null) ? uvIndexNameProp.GetIntegerValue() : 0;
            texture.BlendFactor = (blendFactorNameProp != null) ? blendFactorNameProp.GetFloatValue() : 0.0f;
            texture.Operation = (texOpNameProp != null) ? (assimpTextureOperation)texOpNameProp.GetIntegerValue() : 0;
            texture.WrapModeU = (uMapModeNameProp != null) ? (AssimpTextureWrapMode)uMapModeNameProp.GetIntegerValue() : AssimpTextureWrapMode.Wrap;
            texture.WrapModeV = (vMapModeNameProp != null) ? (AssimpTextureWrapMode)vMapModeNameProp.GetIntegerValue() : AssimpTextureWrapMode.Wrap;
            texture.Flags = (texFlagsNameProp != null) ? texFlagsNameProp.GetIntegerValue() : 0;

            return true;
        }

        /// <summary>
        /// Gets all textures that correspond to the type.
        /// </summary>
        /// <param name="type">Texture type</param>
        /// <returns>The array of textures</returns>
        public MaterialTexture[] GetMaterialTextures(AssimpTextureType type)
        {
            int count = GetMaterialTextureCount(type);

            if (count == 0)
                return new MaterialTexture[0];

            MaterialTexture[] textures = new MaterialTexture[count];

            for (int i = 0; i < count; i++)
            {
                MaterialTexture tex;
                GetMaterialTexture(type, i, out tex);
                textures[i] = tex;
            }

            return textures;
        }

        /// <summary>
        /// Gets all textures in the material.
        /// </summary>
        /// <returns>The array of textures</returns>
        public MaterialTexture[] GetAllMaterialTextures()
        {
            List<MaterialTexture> textures = new List<MaterialTexture>();
            AssimpTextureType[] types = Enum.GetValues(typeof(AssimpTextureType)) as AssimpTextureType[];

            foreach (AssimpTextureType texType in types)
            {
                textures.AddRange(GetMaterialTextures(texType));
            }

            return textures.ToArray();
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Clear();
        }
    }
}