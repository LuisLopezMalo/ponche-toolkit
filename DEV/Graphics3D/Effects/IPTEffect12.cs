using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics3D.Effects;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Main interface to implement to create an effect.
    /// This interface has properties that DirectX11 and DirectX12 share.
    /// </summary>
    public interface IPTEffect12 : IInitializable, IContentLoadable, IContentItem
    {
#if DX12
        #region Methods
        
        #endregion
#endif
    }
}