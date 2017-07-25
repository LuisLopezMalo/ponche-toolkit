using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Main interface to implement for a Graphics renderer to work on DirectX 11.
    /// </summary>
    public interface IGraphicsRenderer11 : IGraphicsRenderer
    {
        /// <summary>
        /// The <see cref="SharpDX.Direct3D11.DeviceContext1"/> immediate context used to render the 3D content.
        /// </summary>
        SharpDX.Direct3D11.DeviceContext1 ImmediateContext { get; set; }

        /// <summary>
        /// The Swap Chain to render.
        /// </summary>
        SwapChain1 SwapChain { get; set; }

        /// <summary>
        /// Instance of the device used.
        /// </summary>
        SharpDX.Direct3D11.Device1 Device { get; set; }
    }
}
