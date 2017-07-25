using PoncheToolkit.Core.Services;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management.Content
{
    /// <summary>
    /// Main interface to implement for a content manager in DirectX11.
    /// </summary>
    public interface IContentManager11 : IContentManager
    {
        /// <summary>
        /// Load in memory a texture2D.
        /// This method is used when creating a custom Texture2D not taken from a physical file.
        /// </summary>
        /// <param name="textureKey">The string to save this texture in the <see cref="ContentsPool.Textures"/> dictionary.</param>
        /// <param name="fromTexture">The texture.</param>
        /// <returns></returns>
        Graphics2D.PTTexture2D LoadTexture2D(string textureKey, Texture2D fromTexture);
    }
}
