using PoncheToolkit.Core.Management.Content;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Class that replace the Direct3D texture and add IConvertible implementation.
    /// </summary>
    public class Texture2D : GameResourceContent
    {
        private ShaderResourceView shaderResourceView;

        /// <summary>
        /// Width of the texture.
        /// </summary>
        public int Width { get { return Texture.Description.Width; } }
        
        /// <summary>
        /// Height of the texture.
        /// </summary>
        public int Height { get { return Texture.Description.Height; } }

        /// <inheritdoc/>
        public override ShaderResourceView ShaderResourceView
        {
            get
            {
                if (shaderResourceView == null)
                    shaderResourceView = new ShaderResourceView(Texture.Device, Texture);
                return shaderResourceView;
            }
        }

        /// <summary>
        /// The DirectX Texture.
        /// </summary>
        internal SharpDX.Direct3D11.Texture2D Texture;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="texture"></param>
        public Texture2D(SharpDX.Direct3D11.Texture2D texture)
        {
            this.Texture = texture;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Utilities.Dispose(ref shaderResourceView);
            Utilities.Dispose(ref Texture);
        }
    }
}
