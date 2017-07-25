using System;
using PoncheToolkit.Core;
using PoncheToolkit.Util;
using SharpDX.Direct3D11;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.DXGI;
using PoncheToolkit.Core.Management.Content;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace PoncheToolkit.Graphics3D.Effects
{
    /// <summary>
    /// Class that represent the very basic implementation for rendering.
    /// This effect supports at most 16 dynamic lights.
    /// </summary>
#if DX11
    public class PTDualParaboloidReflectionEffect : PTEffect11
#elif DX12
    public class PTDualParaboloidReflectionEffect : PTEffect12
#endif
    {
        #region Fields
        //private List<PTLight> lights;
    private ClipPlainStruct clipPlain;
        private PTMaterial materialBuffer;
        private Vector4 globalAmbientColor;
        private Vector4 cameraPosition;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override int MaxLightsCount { get { return PTLight.FORWARD_SHADING_MAX_LIGHTS; } }

        /// <summary>
        /// The camera position to apply the effect.
        /// </summary>
        public Vector4 CameraPosition
        {
            get { return cameraPosition; }
            set
            {
                SetProperty(ref cameraPosition, value);
            }
        }

        /// <summary>
        /// Get or set the <see cref="ClipPlainStruct"/> resource to be updated to the gpu.
        /// By default it is initialized to clip a distance of 100 in the three axis. (100, 100, 100)
        /// </summary>
        public ClipPlainStruct ClipPlain
        {
            get { return clipPlain; }
            set { SetPropertyAsDirty(ref clipPlain, value); }
        }

        /// <summary>
        /// The <see cref="PTMaterial"/> material to be sent to the gpu.
        /// </summary>
        public PTMaterial MaterialBuffer
        {
            get { return materialBuffer; }
            set { SetPropertyAsDirty(ref materialBuffer, value); }
        }
        #endregion

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
#if DX11
        public PTDualParaboloidReflectionEffect(Game11 game)
#elif DX12
        public PTDualParaboloidReflectionEffect(Game12 game)
#endif
            : base(game, PTEffect.DUAL_PARABOLOID_REFLECTION_EFFECT_PATH, includePaths: null)
        {
            this.ClipPlain = new ClipPlainStruct(new Vector4(-1, -1, -1, 1000f));
            this.globalAmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
            this.LightsBuffer = new LightBufferStruct(null);
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <inheritdoc/>
        public override void LoadContent(IContentManager contentManager)
        {
            base.LoadContent(contentManager);

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

#if DX11
        /// <inheritdoc/>
        public override void Apply(DeviceContext1 context)
        {
            base.Apply(context);

            // Set the matrices and lights constant buffers.
            context.VertexShader.SetConstantBuffer(0, MatricesConstantBuffer);
            context.VertexShader.SetConstantBuffer(1, ClipConstantBuffer);
            context.VertexShader.SetConstantBuffer(2, ReflectionConstantBuffer);
            //context.VertexShader.SetConstantBuffers(0, MatricesConstantBuffer, ClipConstantBuffer, ReflectionConstantBuffer);
            context.PixelShader.SetConstantBuffer(0, MaterialConstantBuffer);
            context.PixelShader.SetConstantBuffer(1, GlobalDataConstantBuffer);
            context.PixelShader.SetConstantBuffer(2, LightsConstantBuffer);
            //context.PixelShader.SetConstantBuffers(0, MaterialConstantBuffer, GlobalLightingConstantBuffer, LightsConstantBuffer);

            // ===== Update common Effect properties
            //context.UpdateSubresource(ref light, effect.LightningConstantBuffer);
            var clipPlain = ClipPlain;
            var dataBoxClip = context.MapSubresource(ClipConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            Utilities.Write(dataBoxClip.DataPointer, ref clipPlain);
            context.UnmapSubresource(ClipConstantBuffer, 0);

            // === Global Data
            CurrentLights = Math.Min(Lights.Count, PTLight.FORWARD_SHADING_MAX_LIGHTS); // Set the number of current lights.
            var globalData = GlobalDataBuffer;
            var globalDataDataBox = context.MapSubresource(GlobalDataConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            Utilities.Write(globalDataDataBox.DataPointer, ref globalData);
            context.UnmapSubresource(GlobalDataConstantBuffer, 0);

            //// Global Lighting
            //var globalLightingBuffer = GlobalLightingBuffer;
            //context.UpdateSubresource(ref globalLightingBuffer, GlobalLightingConstantBuffer);


            var lightBuffer = LightsBuffer.Lights;
            var dataBoxLightBuffer = context.MapSubresource(LightsConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            //Utilities.Write(dataBoxLightBuffer.DataPointer, lightBuffer, 0, PTLight.FORWARD_SHADING_MAX_LIGHTS);
            Utilities.Write(dataBoxLightBuffer.DataPointer, lightBuffer, 0, CurrentLights);
            context.UnmapSubresource(LightsConstantBuffer, 0);


            if (Game.Renderer.ProcessRenderMode == ProcessRenderingMode.MultiThread)
            {
                context.Rasterizer.SetViewport(Game.Viewports[0]);
                context.OutputMerger.SetTargets(Game.DepthStencilView, Game.BackBufferRenderTarget.RenderTargetView);
            }
        }
#endif

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
