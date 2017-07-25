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
    /// The types of lights the engine can render.
    /// </summary>
    public enum LightType
    {
        /// <summary>
        /// A directional light. This is a light that has no position, it only
        /// has direction, infinite.
        /// </summary>
        Directional = 0,
        /// <summary>
        /// A light that points everywhere from its position.
        /// </summary>
        Point = 1,
        /// <summary>
        /// A light in form of a cone within a certain angle.
        /// </summary>
        Spot = 2
    }

    /// <summary>
    /// Struct that holds the basic lighting information to be sent to the shader.
    /// </summary>
    //[StructLayout(LayoutKind.Sequential, Pack = 16)]
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    [Serializable]
    internal struct LightStruct
    {
        /// <summary>
        /// The light color.
        /// </summary>
        [FieldOffset(0)]
        public Vector4 Color;
        /// <summary>
        /// The position of the emition of light.
        /// </summary>
        [FieldOffset(16)]
        public Vector4 Position;
        /// <summary>
        /// The light direction.
        /// </summary>
        [FieldOffset(32)]
        public Vector4 Direction;
        /// <summary>
        /// The angle of the light if it is a SpotLight.
        /// </summary>
        [FieldOffset(48)]
        public float SpotAngle;
        /// <summary>
        /// The attenuation (light intensity) when going far from the emitting position.
        /// </summary>
        [FieldOffset(52)]
        public float ConstantAttenuation;
        /// <summary>
        /// The attenuation (light intensity) when going far from the emitting position.
        /// </summary>
        [FieldOffset(56)]
        public float LinearAttenuation;
        /// <summary>
        /// The attenuation (light intensity) when going far from the emitting position.
        /// </summary>
        [FieldOffset(60)]
        public float QuadraticAttenuation;
        /// <summary>
        /// The intensity of the light. Default: 1
        /// </summary>
        [FieldOffset(64)]
        public float Intensity;
        /// <summary>
        /// The range in units before the light become unseeable.
        /// </summary>
        [FieldOffset(68)]
        public float Range;
        /// <summary>
        /// The type of light obtained from <see cref="LightType"/>.
        /// </summary>
        [FieldOffset(72)]
        public int Type;
        /// <summary>
        /// If the light is enabled.
        /// </summary>
        [FieldOffset(76)]
        public int IsEnabled;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The diffuse color.</param>
        /// <param name="direction">The ambient color.</param>
        /// <param name="color">The specular color.</param>
        /// <param name="spotAngle">The angle for the spot lights.</param>
        /// <param name="constantAtt">The constant light attenuation for the <see cref="LightType.Point"/> and <see cref="LightType.Spot"/> types.</param>
        /// <param name="linearAtt">The linear light attenuation for the <see cref="LightType.Point"/> and <see cref="LightType.Spot"/> types.</param>
        /// <param name="quadraticAtt">The quadratic light attenuation for the <see cref="LightType.Point"/> and <see cref="LightType.Spot"/> types.</param>
        /// <param name="intensity">The intensity of the light. Default: 1</param>
        /// <param name="range">The range in units before the light become unseeable.</param>
        /// <param name="type">The type of light to be rendered, taken from <see cref="LightType"/>.</param>
        /// <param name="isEnabled">If the light is enabled.</param>
        public LightStruct(Vector4 color, Vector4 position, Vector4 direction, float spotAngle, float constantAtt, 
            float linearAtt, float quadraticAtt, float intensity, float range, LightType type, bool isEnabled)
        {
            this.Position = position;
            this.Direction = direction;
            this.Color = color;
            this.SpotAngle = spotAngle;
            this.ConstantAttenuation = constantAtt;
            this.LinearAttenuation = linearAtt;
            this.QuadraticAttenuation = quadraticAtt;
            this.Intensity = intensity;
            this.Range = range;
            this.Type = (int)type;
            this.IsEnabled = isEnabled == true ? 1 : 0;
        }

        #region Operators
        ///// <summary>
        ///// Convert <see cref="LightStruct"/> from a <see cref="PTLight"/>.
        ///// </summary>
        ///// <param name="light">The <see cref="SpotLightGPU"/> instance to be mapped as a struct that will be sent to the shader.</param>
        //public static implicit operator LightStruct(PTLight light)
        //{
        //    if (light != null)
        //        return new LightStruct(light.Position, light.Direction, light.Color, light.SpotAngle, light.ConstantAttenuation,
        //            light.LinearAttenuation, light.QuadraticAttenuation, light.Type, light.IsEnabled);

        //    return new LightStruct();
        //}
        #endregion
    }
}
