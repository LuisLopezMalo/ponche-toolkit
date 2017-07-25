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
    [StructLayout(LayoutKind.Explicit, Pack = 16, Size = 80 * PTLight.MAX_LIGHTS)]
    internal struct LightBufferStruct
    {
        /// <summary>
        /// The lights to be sent to the shader.
        /// </summary>
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = PTLight.MAX_LIGHTS)]
        public LightStruct[] Lights; // 80 per light. 16 lights

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lights">The lights to be sento to the shader. There is a maximum of <see cref="PTLight.MAX_LIGHTS"/> number of lights.</param>
        public LightBufferStruct(List<PTLight> lights)
        {
            this.Lights = new LightStruct[PTLight.MAX_LIGHTS];

            if (lights != null)
            {
                for (int i = 0; i < Math.Min(PTLight.MAX_LIGHTS, lights.Count); i++)
                {
                    Lights[i] = lights[i].LightBuffer;
                }
            }
        }

        public void AddLight(PTLight light)
        {
            this.Lights[light.Index] = light.LightBuffer;
        }

        public static int SizeOf()
        {
            return Marshal.SizeOf<LightBufferStruct>();
        }
    }
}
