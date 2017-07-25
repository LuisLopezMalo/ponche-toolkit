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
using PoncheToolkit.Graphics2D.Effects;

namespace PoncheToolkit.Graphics3D.Effects
{
    /// <summary>
    /// Class that represent the very basic implementation for rendering.
    /// This effect supports at most 16 dynamic lights.
    /// </summary>
    public class PTForwardRenderEffect : PTEffect
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
            set { SetProperty(ref cameraPosition, value); }
        }

        /// <summary>
        /// The <see cref="PTMaterial"/> material to be sent to the gpu.
        /// </summary>
        public PTMaterial MaterialBuffer
        {
            get { return materialBuffer; }
            set { SetPropertyAsDirty(ref materialBuffer, value); }
        }

        /// <summary>
        /// Get or set the <see cref="Vector4"/> vector for clipping.
        /// By default it is initialized to clip a distance of 1000 in the three axis. (1000, 1000, 1000, 1)
        /// </summary>
        public Vector4 ClipPlainVector
        {
            get { return clipPlain.Clip; }
            set { SetProperty(ref clipPlain.Clip, value); }
        }

        /// <summary>
        /// Get or set the <see cref="ClipPlainStruct"/> resource to be updated to the gpu.
        /// By default it is initialized to clip a distance of 100 in the three axis. (100, 100, 100)
        /// </summary>
        internal ClipPlainStruct ClipPlain
        {
            get { return clipPlain; }
            set { SetPropertyAsDirty(ref clipPlain, value); }
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
        /// <param name="game">The game instance.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="physicalShader">The path to use for the .fx file.</param>
#if DX11
        public PTForwardRenderEffect(Game11 game, List<string> includePaths, string physicalShader = PTEffect.FORWARD_RENDER_EFFECT_PATH) 
#elif DX12
        public PTForwardRenderEffect(Game12 game, List<string> includePaths, string physicalShader = PTEffect.FORWARD_RENDER_EFFECT_PATH)
#endif
            : base(game, physicalShader, includePaths)
        {
            //this.ClipPlainVector = new Vector4(1000, 1000, 1000, 1);
            this.ClipPlainVector = new Vector4(-1, 1, 1, 100);
            this.globalAmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
            this.LightsBuffer = new LightBufferStruct(null);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="vertexShaderPath">The path to use for the .fx file.</param>
        /// <param name="pixelShaderPath">The path to use for the .fx file.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
#if DX11
        public PTForwardRenderEffect(Game11 game, string vertexShaderPath, string pixelShaderPath, List<string> includePaths)
#elif DX12
        public PTForwardRenderEffect(Game12 game, string vertexShaderPath, string pixelShaderPath, List<string> includePaths)
#endif
            : this(game, vertexShaderPath, pixelShaderPath, includePaths, -1)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
        /// <param name="vertexShaderPath">The path to use for the .fx file.</param>
        /// <param name="pixelShaderPath">The path to use for the .fx file.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="customPixelShaderSlot">The slot to be used if a custom shader want to be used. If no custom shader is going to be used, set it o -1.
#if DX11
        public PTForwardRenderEffect(Game11 game, string vertexShaderPath, string pixelShaderPath, List<string> includePaths, int customPixelShaderSlot = -1)
#elif DX12
        public PTForwardRenderEffect(Game12 game, string vertexShaderPath, string pixelShaderPath, List<string> includePaths, int customPixelShaderSlot = -1)
#endif
            : base(game, vertexShaderPath, pixelShaderPath, includePaths, customPixelShaderSlot)
        {
            //this.ClipPlainVector = new Vector4(1000, 1000, 1000, 1);
            this.ClipPlainVector = new Vector4(-1, 1, 1, 100);
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

            // Compile Vertex and Pixel shaders
            try
            {
                Shader.InputLayout = new InputLayout(Game.Renderer.Device, Shader.VertexShaderSignature, new[]
                {
                    new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                    new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float, InputElement.AppendAligned, 0),
                    new InputElement("NORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0),
                    new InputElement("TANGENT", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0),
                    new InputElement("BINORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0),
                });
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
        public override void LoadBuffers()
        {
            base.LoadBuffers();
        }

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
            //CurrentLights = Math.Min(Lights.Count, PTLight.FORWARD_SHADING_MAX_LIGHTS); // Set the number of current lights.
            var globalData = GlobalDataBuffer;
            var globalDataDataBox = context.MapSubresource(GlobalDataConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            Utilities.Write(globalDataDataBox.DataPointer, ref globalData);
            context.UnmapSubresource(GlobalDataConstantBuffer, 0);

            var lightBuffer = LightsBuffer.Lights;
            var dataBoxLightBuffer = context.MapSubresource(LightsConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            //Utilities.Write(dataBoxLightBuffer.DataPointer, lightBuffer, 0, PTLight.FORWARD_SHADING_MAX_LIGHTS);
            Utilities.Write(dataBoxLightBuffer.DataPointer, lightBuffer, 0, CurrentLights);
            context.UnmapSubresource(LightsConstantBuffer, 0);

#if DX11
            if (Game.Renderer.ProcessRenderMode == ProcessRenderingMode.MultiThread)
            {
                context.Rasterizer.SetViewport(Game.Viewports[0]);
                context.OutputMerger.SetTargets(Game.DepthStencilView, Game.BackBufferRenderTarget.RenderTargetView);
            }
#endif
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
