using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D.Effects
{
    /// <summary>
    /// Struct that holds the material information to be sent to the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct MaterialStruct
    {
        /// <summary>
        /// The color that emits from the material even when there is no external lights.
        /// Default: <see cref="Vector4.Zero"/>
        /// </summary>
        public Vector4 EmissiveColor;
        /// <summary>
        /// The color of the ambient component
        /// Default: <see cref="Vector4.Zero"/>
        /// </summary>
        public Vector4 AmbientColor;
        /// <summary>
        /// The color of the diffuse component.
        /// Default: <see cref="Vector4.Zero"/>
        /// </summary>
        public Vector4 DiffuseColor;
        /// <summary>
        /// The color of the specular component.
        /// Default: <see cref="Vector4.Zero"/>
        /// </summary>
        public Vector4 SpecularColor;
        /// <summary>
        /// The color that of the reflection.
        /// Default: <see cref="Vector4.Zero"/>
        /// </summary>
        public Vector4 ReflectiveColor;
        /// <summary>
        /// The power of the specular component.
        /// Default: 15.
        /// </summary>
        public float SpecularPower;
        /// <summary>
        /// The power of the specular component.
        /// Default: 15.
        /// </summary>
        public float Reflectivity;
        /// <summary>
        /// The power of the specular component.
        /// Default: 1.
        /// </summary>
        public float Opacity;
        /// <summary>
        /// The property used to blend multiple textures.
        /// Default: 15.
        /// </summary>
        public float Gamma;
        /// <summary>
        /// Number of textures sent to the shader for blending.
        /// By default it is set to 1.
        /// </summary>
        public int BlendTexturesCount;
        /// <summary>
        /// Set to render specular lighting or not. 16 bit
        /// 0 = false, 1 = true
        /// </summary>
        public int IsSpecular;
        /// <summary>
        /// Set to render bump mapping. A Bump map texture must be set.
        /// 0 = false, 1 = true. Default: false
        /// </summary>
        public int IsBump;
        /// <summary>
        /// Set to render reflections or not. 16 bit
        /// 0 = false, 1 = true
        /// </summary>
        public int IsReflective;
        /// <summary>
        /// Set to render bump mapping. A Bump map texture must be set.
        /// 0 = false, 1 = true. Default: false
        /// </summary>
        public int HasSpecularMap;
        /// <summary>
        /// The translation of the texture (this translation applies to the result of the blended textures).
        /// </summary>
        public Vector2 TextureTranslation;
        /// <summary>
        /// Padding.
        /// </summary>
        public float Padding;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="emissiveColor">The color that the material emmits even without external lights.</param>
        /// <param name="ambientColor">The color of the ambient component.</param>
        /// <param name="diffuseColor">The color of the diffuse component.</param>
        /// <param name="specularColor">The color of the specular component.</param>
        /// <param name="reflectiveColor">The color of the reflection component.</param>
        /// <param name="specularPower">The specular power of the material.</param>
        /// <param name="reflectivity">The value of the power of reflectivity.</param>
        /// <param name="opacity">The alpha value for opacity.</param>
        /// <param name="gamma">The float value to multiply the textures (depends on the monitor).</param>
        /// <param name="isSpecular">Boolean value to set if the material draw specular lighting.</param>
        /// <param name="isBump">Boolean value to set if the material draw bump mapping.</param>
        /// <param name="isReflective">Boolean value to set if the material draw reflection.</param>
        /// <param name="hasSpecularMap">Boolean value to set if the material has a specular map.</param>
        /// <param name="blendTexturesCount">Number of textures sent to be blended.</param>
        /// <param name="textureTranslation">The translation of the UV coordinates of the textures.</param>
        public MaterialStruct(Vector4 emissiveColor, Vector4 ambientColor, Vector4 diffuseColor, Vector4 specularColor, Vector4 reflectiveColor, 
            float specularPower, float reflectivity, float opacity, float gamma, 
            bool isSpecular, bool isBump, bool isReflective, bool hasSpecularMap, int blendTexturesCount, Vector2 textureTranslation)
        {
            this.EmissiveColor = emissiveColor;
            this.AmbientColor = ambientColor;
            this.DiffuseColor = diffuseColor;
            this.SpecularColor = specularColor;
            this.ReflectiveColor = reflectiveColor;
            this.SpecularPower = specularPower;
            this.Reflectivity = reflectivity;
            this.Opacity = opacity;
            this.Gamma = gamma;

            this.BlendTexturesCount = blendTexturesCount;
            this.IsSpecular = isSpecular == true ? 1 : 0;
            this.IsBump = isBump == true ? 1 : 0;
            this.IsReflective = isReflective == true ? 1 : 0;
            this.HasSpecularMap = hasSpecularMap == true ? 1 : 0;
            this.TextureTranslation = textureTranslation;

            this.Padding = 0;
        }

        /// <summary>
        /// Constructor. Without reflection.
        /// </summary>
        /// <param name="emissiveColor">The color that the material emmits even without external lights.</param>
        /// <param name="ambientColor">The color of the ambient component.</param>
        /// <param name="diffuseColor">The color of the diffuse component.</param>
        /// <param name="specularColor">The color of the specular component.</param>
        /// <param name="specularPower">The specular power of the material.</param>
        /// <param name="opacity">The alpha value for opacity.</param>
        /// <param name="gamma">The float value to multiply the textures (depends on the monitor).</param>
        /// <param name="isSpecular">Boolean value to set if the material draw specular lighting.</param>
        /// <param name="isBump">Boolean value to set if the material draw bump mapping.</param>
        /// <param name="hasSpecularMap">Boolean value to set if the material has a specular map.</param>
        /// <param name="blendTexturesCount">Number of textures sent to be blended.</param>
        /// <param name="textureTranslation">The translation of the UV coordinates of the textures.</param>
        public MaterialStruct(Vector4 emissiveColor, Vector4 ambientColor, Vector4 diffuseColor, Vector4 specularColor,
            float specularPower, float opacity, float gamma, bool isSpecular, bool isBump, bool hasSpecularMap, int blendTexturesCount, Vector2 textureTranslation)
            : this(emissiveColor, ambientColor, diffuseColor, specularColor, Vector4.Zero, specularPower, 0, opacity, gamma,
                  isSpecular, isBump, false, hasSpecularMap, blendTexturesCount, textureTranslation)
        {
        }

        /// <summary>
        /// Constructor. Without reflection and Specular.
        /// </summary>
        /// <param name="emissiveColor">The color that the material emmits even without external lights.</param>
        /// <param name="ambientColor">The color of the ambient component.</param>
        /// <param name="diffuseColor">The color of the diffuse component.</param>
        /// <param name="opacity">The alpha value for opacity.</param>
        /// <param name="gamma">The float value to multiply the textures (depends on the monitor).</param>
        /// <param name="isBump">Boolean value to set if the material draw bump mapping.</param>
        /// <param name="blendTexturesCount">Number of textures sent to be blended.</param>
        /// <param name="textureTranslation">The translation of the UV coordinates of the textures.</param>
        public MaterialStruct(Vector4 emissiveColor, Vector4 ambientColor, Vector4 diffuseColor,
            float opacity, float gamma, bool isBump, int blendTexturesCount, Vector2 textureTranslation)
            : this(emissiveColor, ambientColor, diffuseColor, Vector4.Zero, Vector4.Zero, 15, 0, opacity, gamma,
                  false, isBump, false, false, blendTexturesCount, textureTranslation)
        {
        }

        public static int SizeOf()
        {
            return Utilities.SizeOf<MaterialStruct>();
        }

        #region Operators
        ///// <summary>
        ///// Convert <see cref="LightStruct"/> from a <see cref="PTLight"/>.
        ///// </summary>
        ///// <param name="mat">The <see cref="SpotLightGPU"/> instance to be mapped as a struct that will be sent to the shader.</param>
        //public static implicit operator MaterialStruct2(PTMaterial2 mat)
        //{
        //    if (mat != null)
        //        return new LightStruct(mat.Position, mat.Direction, mat.Color, mat.SpotAngle, mat.ConstantAttenuation,
        //            mat.LinearAttenuation, mat.QuadraticAttenuation, mat.Type, mat.IsEnabled);

        //    return new LightStruct();
        //}
        #endregion
    }
}
