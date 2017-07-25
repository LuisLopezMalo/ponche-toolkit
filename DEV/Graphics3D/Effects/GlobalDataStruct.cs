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
    /// Struct that holds the basic lighting information to be sent to the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct GlobalDataStruct
    {
        /// <summary>
        /// The global ambient color.
        /// </summary>
        public Vector4 GlobalAmbient;

        /// <summary>
        /// The number of lights that will be sent in the current frame.
        /// This value will be changing dynamically.
        /// </summary>
        public int CurrentLights;

        /// <summary>
        /// Padding.
        /// </summary>
        public Vector3 Padding;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="globalAmbient">The global ambient color.</param>
        /// <param name="lights">The lights to be sento to the shader. There is a maximum of <see cref="PTLight.FORWARD_SHADING_MAX_LIGHTS"/> number of lights.</param>
        public GlobalDataStruct(Vector4 globalAmbient, int lights)
        {
            this.GlobalAmbient = globalAmbient;
            this.CurrentLights = lights;
            this.Padding = Vector3.Zero;
        }

        public static int SizeOf()
        {
            return Marshal.SizeOf<GlobalDataStruct>();
        }
    }
}
