using PoncheToolkit.Core.Management.Content;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D.Effects
{
    /// <summary>
    /// Class that wraps shader functionality like type (Vertex, Pixel, Hull, etc)
    /// And contain other properties like ShaderSignature.
    /// </summary>
    public class Shader : GameContent
    {
        /// <summary>
        /// The shader signature.
        /// </summary>
        internal ShaderSignature VertexShaderSignature;
        /// <summary>
        /// The pixel shader compiled object.
        /// </summary>
        internal PixelShader PixelShader;
        /// <summary>
        /// The vertex shader compiled object.
        /// </summary>
        internal VertexShader VertexShader;
        /// <summary>
        /// The parameters that will be passed to the shader.
        /// </summary>
        public ShaderParameters Parameters;
        /// <summary>
        /// The content to send to the gpu in its first stage: Input Layout.
        /// </summary>
        internal InputLayout InputLayout;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Shader()
        {
            Parameters = new ShaderParameters();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Utilities.Dispose(ref VertexShaderSignature);
            Utilities.Dispose(ref PixelShader);
            Utilities.Dispose(ref VertexShader);
            Utilities.Dispose(ref InputLayout);
        }
    }
}
