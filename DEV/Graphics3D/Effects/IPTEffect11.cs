using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics3D.Effects;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace PoncheToolkit.Core
{
#if DX11
    /// <summary>
    /// Main interface to implement to create an effect.
    /// This interface has properties that DirectX11 and DirectX12 share.
    /// </summary>
    public interface IPTEffect11 : IPTEffect
    {
        #region Methods
        /// <summary>
        /// Initialize the buffers with default behavior.
        /// Create the Matrcies, Clip, Reflection, Material, Lights buffers among others.
        /// This method is called withing the <see cref="PTEffect.LoadContent(ContentManager11)"/> method.
        /// </summary>
        void LoadBuffers();

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        Buffer MatricesConstantBuffer { get; set; }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        Buffer ClipConstantBuffer { get; set; }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        Buffer ReflectionConstantBuffer { get; set; }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        Buffer MaterialConstantBuffer { get; set; }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        Buffer GlobalDataConstantBuffer { get; set; }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        Buffer LightsConstantBuffer { get; set; }

        /// <summary>
        /// The buffer used to draw instanced meshes.
        /// </summary>
        Buffer InstanceBuffer { get; set; }
        #endregion
    }
#endif
}