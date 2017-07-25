using System;
using PoncheToolkit.Core;
using PoncheToolkit.Util;
using SharpDX.Direct3D11;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.DXGI;
using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics3D.Effects;

namespace PoncheToolkit.Graphics2D.Effects
{
    /// <summary>
    /// Class that represent the effect that at least all the models will have.
    /// Has basic lights just for a minimum "good looking" rendering.
    /// </summary>
    public class PTShadowRenderEffect2D : PTEffect
    {
        /// <summary>
        /// 
        /// </summary>
        public static string NAME_PATH = "Effects/PT2DShadowRenderEffect.fx";

        #region Events
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;

        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game"></param>
        public PTShadowRenderEffect2D(Game11 game) 
            : base(game, NAME_PATH)
        {
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void LoadContent(ContentManager contentManager)
        {
            base.LoadContent(contentManager);

            // Compile Vertex and Pixel shaders
            try
            {
                Shader.InputLayout = new InputLayout(Game.Renderer.Device, Shader.VertexShaderSignature, new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float, InputElement.AppendAligned, 0),
                });
                
                MatricesConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<MatricesStruct2D>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
                LightningConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<MaterialStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
                ToDispose(MatricesConstantBuffer);
                ToDispose(LightningConstantBuffer);
            }
            catch (Exception ex)
            {
                Log.Error("Error when loading effect.", ex);
                throw;
            }

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
