using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PT.Graphics3D.Effects
{
    /// <summary>
    /// Struct that holds the basic lighting information to be sent to the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct GlobalLightingStruct
    {
        /// <summary>
        /// The global ambient color.
        /// </summary>
        public Vector4 GlobalAmbient;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="globalAmbient">The global ambient color.</param>
        /// <param name="lights">The lights to be sento to the shader. There is a maximum of <see cref="PTLight.MAX_LIGHTS"/> number of lights.</param>
        public GlobalLightingStruct(Vector4 globalAmbient)
        {
            this.GlobalAmbient = globalAmbient;
        }

        public static int SizeOf()
        {
            return Marshal.SizeOf<GlobalLightingStruct>();
        }
    }
}
