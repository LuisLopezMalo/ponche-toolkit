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
    public class PTLinkingForwardRenderEffect : PTEffect
    {
        #region Fields
        private ClipPlainStruct clipPlain;
        private PTMaterial materialBuffer;
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
            set
            {
                SetProperty(ref clipPlain.Clip, value);
            }
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
        /// <param name="customPixelShaderSlot">The slot to be used if a custom shader want to be used. If no custom shader is going to be used, set it to -1.
        /// For more information see <see cref="ContentManager11.LoadPixelShaderInto(string, ref PTShader, int, List{string}, string)"/> </param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        public PTLinkingForwardRenderEffect(Game11 game, int customPixelShaderSlot, List<string> includePaths) 
            : base(game, "Effects/PTForwardRenderingVS.fx", "Effects/PTForwardRenderingPS.fx", includePaths, customPixelShaderSlot)
        {
            //this.ClipPlainVector = new Vector4(1000, 1000, 1000, 1);
            this.ClipPlainVector = new Vector4(-1, 1, 1, 100);
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

            //BufferDescription buffDesc = new BufferDescription();
            //buffDesc.StructureByteStride = Utilities.SizeOf<LightStruct>();
            //buffDesc.SizeInBytes = LightBufferStruct.SizeOf();
            //buffDesc.BindFlags = BindFlags.ShaderResource;
            //buffDesc.Usage = ResourceUsage.Dynamic;
            //buffDesc.CpuAccessFlags = CpuAccessFlags.Write;
            //buffDesc.OptionFlags = ResourceOptionFlags.BufferStructured;

            //LightsStructuredBuffer = new Buffer(Game.Renderer.Device, buffDesc);
            ////LightsStructuredBuffer = new Buffer(Game.Renderer.Device, LightBufferStruct.SizeOf(), ResourceUsage.Default, BindFlags.ShaderResource | BindFlags.UnorderedAccess, CpuAccessFlags.Write, ResourceOptionFlags.BufferStructured, Utilities.SizeOf<LightStruct>());

            //ShaderResourceViewDescription desc = new ShaderResourceViewDescription();
            //desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.ExtendedBuffer;
            ////desc.Buffer.ElementCount = PTLight.FORWARD_SHADING_MAX_LIGHTS;
            ////desc.Buffer.ElementWidth = Utilities.SizeOf<LightStruct>();
            //desc.BufferEx.ElementCount = PTLight.FORWARD_SHADING_MAX_LIGHTS;
            //desc.BufferEx.Flags = ShaderResourceViewExtendedBufferFlags.None;
            //desc.Format = Format.Unknown;
            //structuredResourceView = new ShaderResourceView(Game.Renderer.Device, LightsStructuredBuffer, desc);
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


            //// With data stream
            //var lightBuffer = LightsBuffer.Lights;
            //DataStream stream;
            //var dataBoxLightBuffer = context.MapSubresource(LightsConstantBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
            //stream.WriteRange(lightBuffer, 0, PTLight.FORWARD_SHADING_MAX_LIGHTS);
            ////stream.WriteRange(lightBuffer);
            //context.UnmapSubresource(LightsConstantBuffer, 0);
            //stream.Dispose();


            // ============= Structured Buffer tests
            //// With data stream
            //var lightBuffer = LightsBuffer.Lights;
            //DataStream stream;
            //var dataBoxLightBuffer2 = context.MapSubresource(LightsStructuredBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
            //stream.WriteRange(lightBuffer, 0, PTLight.FORWARD_SHADING_MAX_LIGHTS);
            //context.UnmapSubresource(LightsStructuredBuffer, 0);
            //stream.Dispose();

            ////context.PixelShader.SetShaderResource(7, structuredResourceView);
            //var lightBuffer = LightsBuffer.Lights;
            //DataStream stream;
            //var dataBoxLightBuffer2 = context.MapSubresource(LightsStructuredBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
            //stream.WriteRange(lightBuffer, 0, PTLight.FORWARD_SHADING_MAX_LIGHTS);
            //context.UnmapSubresource(LightsStructuredBuffer, 0);
            //stream.Dispose();


            // ============= Single light tests
            //var lightBuffer = LightsBuffer.Lights[3];
            //var dataBoxLightBuffer2 = context.MapSubresource(LightsConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            //Utilities.Write(dataBoxLightBuffer2.DataPointer, ref lightBuffer);
            //context.UnmapSubresource(LightsConstantBuffer, 0);


            if (Game.Renderer.ProcessRenderMode == ProcessRenderingMode.MultiThread)
            {
                context.Rasterizer.SetViewport(Game.Viewports[0]);
                context.OutputMerger.SetTargets(Game.DepthStencilView, Game.BackBufferRenderTarget.RenderTargetView);
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
