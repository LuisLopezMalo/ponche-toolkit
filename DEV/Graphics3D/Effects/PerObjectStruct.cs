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
    /// Struct that holds the View-Projection matrix information to be sent to the shader.
    /// This struct will be used when rendering only one instance of a model in one draw-call.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct PerObjectStruct
    {
        /// <summary>
        /// The World matrix per object.
        /// </summary>
        public Matrix World;
        /// <summary>
        /// The InverseTransposeWorld matrix per object.
        /// </summary>
        public Matrix InverseTransposeWorld;
        /// <summary>
        /// The WorldViewProjection matrix per object.
        /// </summary>
        public Matrix WorldViewProjection;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="world">The world matrix.</param>
        /// <param name="inverseTransposeWorld">The inverseTransposeWorld matrix.</param>
        /// <param name="worldViewProjection">The worldViewProjection matrix.</param>
        public PerObjectStruct(Matrix world, Matrix inverseTransposeWorld, Matrix worldViewProjection)
        {
            this.World = world;
            this.InverseTransposeWorld = inverseTransposeWorld;
            this.WorldViewProjection = worldViewProjection;
        }
    }
}
