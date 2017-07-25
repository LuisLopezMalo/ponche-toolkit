using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management.Content
{
    /// <summary>
    /// Class that represent a piece of content that can be loaded into the game.
    /// It includes textures, shaders, models, etc.
    /// </summary>
    public abstract class GameResourceContent : GameContent
    {
        /// <summary>
        /// Get the shader resource view to be sent to the shader.
        /// </summary>
        public abstract ShaderResourceView ShaderResourceView { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameResourceContent()
        {

        }
    }
}
