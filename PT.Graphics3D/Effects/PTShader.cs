using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Util;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Graphics3D.Effects
{
    /// <summary>
    /// Class that wraps shader functionality like type (Vertex, Pixel, Hull, etc)
    /// And contain other properties like ShaderSignature.
    /// </summary>
    public class PTShader : UpdatableStateObject
    {
        /// <summary>
        /// The shader signature.
        /// </summary>
        public ShaderSignature VertexShaderSignature;
        /// <summary>
        /// The pixel shader compiled object.
        /// </summary>
        public PixelShader PixelShader;
        /// <summary>
        /// The vertex shader compiled object.
        /// </summary>
        public VertexShader VertexShader;
        /// <summary>
        /// The content to send to the gpu in its first stage: Input Layout.
        /// </summary>
        public InputLayout InputLayout;
        /// <summary>
        /// The physical path where the .fx file is.
        /// </summary>
        public string Path;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTShader()
        {
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Utilities.Dispose(ref VertexShaderSignature);
            Utilities.Dispose(ref PixelShader);
            Utilities.Dispose(ref VertexShader);
            Utilities.Dispose(ref InputLayout);
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            throw new NotImplementedException();
        }
    }
}
