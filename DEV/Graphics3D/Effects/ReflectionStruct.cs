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
    /// Struct that holds the World-View-Projection matrices information to be sent to the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    internal struct ReflectionStruct
    {
        /// <summary>
        /// The ReflectionMatrixProjectionWorld matrix.
        /// </summary>
        public Matrix ReflectionMatrixProjectionWorld;

        /// <summary>
        /// The view matrix of the parabaoloid camera position.
        /// </summary>
        public Matrix ParaboloidView;

        /// <summary>
        /// The Z direction to set the camera when building the dual paraboloids.
        /// </summary>
        public float Direction;

        public float NearPlane;

        public float FarPlane;

        public float Padding;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reflectionMatrixProjectionWorld">The reflectionMatrixProjectionWorld matrix.</param>
        /// <param name="paraboloidView">The view matrix to build the paraboloids.</param>
        /// <param name="direction">The Z direction to build the paraboloids.</param>
        public ReflectionStruct(Matrix reflectionMatrixProjectionWorld, Matrix paraboloidView, float direction, float nearPlane, float farPlane)
        {
            this.ReflectionMatrixProjectionWorld = reflectionMatrixProjectionWorld;
            this.ParaboloidView = paraboloidView;
            this.Direction = direction;
            this.NearPlane = nearPlane;
            this.FarPlane = farPlane;
            this.Padding = 0;
        }
    }
}
