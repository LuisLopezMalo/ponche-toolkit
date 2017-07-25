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
    //[StructLayout(LayoutKind.Explicit, Pack = 16, Size = 80 * PTLight.FORWARD_SHADING_MAX_LIGHTS)]
    [StructLayout(LayoutKind.Explicit, Pack = 16)]
    internal struct LightBufferStruct
    {
        /// <summary>
        /// The lights to be sent to the shader.
        /// </summary>
        [FieldOffset(0)]
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = PTLight.FORWARD_SHADING_MAX_LIGHTS)]
        //[MarshalAs(UnmanagedType.ByValArray)]
        public LightStruct[] Lights; // 80 per light.

        //[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(ArrayMarshaler<LightStruct>))]
        //public LightStruct Lights;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lights">The lights to be sento to the shader. There is a maximum of <see cref="PTLight.FORWARD_SHADING_MAX_LIGHTS"/> number of lights.</param>
        public LightBufferStruct(List<PTLight> lights)
        {
            if (lights != null)
            {
                this.Lights = new LightStruct[Math.Min(PTLight.FORWARD_SHADING_MAX_LIGHTS, lights.Count)];
                for (int i = 0; i < Math.Min(PTLight.FORWARD_SHADING_MAX_LIGHTS, lights.Count); i++)
                {
                    Lights[i] = lights[i].LightBuffer;
                }
            }else
                this.Lights = new LightStruct[PTLight.FORWARD_SHADING_MAX_LIGHTS];
        }

        //public void AddLight(PTLight light)
        //{
        //    this.Lights[light.Index] = light.LightBuffer;
        //}

        public static int SizeOf()
        {
            return Utilities.SizeOf<LightStruct>() * PTLight.FORWARD_SHADING_MAX_LIGHTS;
        }
    }
}
