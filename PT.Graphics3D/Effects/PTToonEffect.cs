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

namespace PT.Graphics3D.Effects
{
    /// <summary>
    /// Class that represent the effect that at least all the models will have.
    /// Has basic lights just for a minimum "good looking" rendering.
    /// </summary>
    public class PTToonEffect : PTEffect
    {
        #region Fields
        private List<PTLight> lights;
        private ClipPlainStruct clipPlain;
        private PTMaterial materialBuffer;
        private Vector4 globalAmbientColor;
        private Vector4 cameraPosition;

        internal GlobalLightingStruct GlobalLightingBuffer;
        internal LightBufferStruct LightsBuffer;
        #endregion

        #region Properties
        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        internal Buffer MatricesConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        internal Buffer ClipConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. VertexShader
        /// </summary>
        internal Buffer ReflectionConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        internal Buffer MaterialConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader. PixelShader
        /// </summary>
        internal Buffer GlobalLightingConstantBuffer;

        internal Buffer LightsConstantBuffer;

        /// <summary>
        /// The global ambient color of the scene.
        /// <para>
        /// Default: Vector4(0.5f, 0.5f, 0.5f, 1)
        /// </para>
        /// </summary>
        public Vector4 GlobalAmbientColor
        {
            get { return globalAmbientColor; }
            set
            {
                SetProperty(ref globalAmbientColor, value);
                GlobalLightingBuffer.GlobalAmbient = globalAmbientColor;
            }
        }

        /// <summary>
        /// All the lights used by this effect.
        /// The lights can be added or removed using the <see cref="AddLight(PTLight)"/> or <see cref="RemoveLight(PTLight)"/> methods.
        /// <para>
        /// </para>
        /// </summary>
        public IReadOnlyList<PTLight> Lights
        {
            get { return lights; }
        }

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
        public PTToonEffect(Game11 game) 
            : base(game, PTEffect.FORWARD_RENDER_EFFECT_PATH)
        {
            this.ClipPlain = new ClipPlainStruct(new Vector4(-1, -1, -1, 1000f));
            this.globalAmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
            this.lights = new List<PTLight>();
            GlobalLightingBuffer = new GlobalLightingStruct(this.globalAmbientColor);
            this.LightsBuffer = new LightBufferStruct(null);
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

            // Create the constant buffers.
            MatricesConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<MatricesStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            ClipConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<ClipPlainStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            ReflectionConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<ReflectionStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            MaterialConstantBuffer = new Buffer(Game.Renderer.Device, MaterialStruct.SizeOf(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            //LightConstantBuffer = new Buffer(Game.Renderer.Device, LightBufferStruct.SizeOf(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            GlobalLightingConstantBuffer = new Buffer(Game.Renderer.Device, GlobalLightingStruct.SizeOf(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            LightsConstantBuffer = new Buffer(Game.Renderer.Device, LightBufferStruct.SizeOf(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            ToDispose(MatricesConstantBuffer);
            ToDispose(ClipConstantBuffer);
            ToDispose(ReflectionConstantBuffer);
            ToDispose(GlobalLightingConstantBuffer);
            ToDispose(MaterialConstantBuffer);
            ToDispose(LightsConstantBuffer);

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

                // Add the default materials.
                PTMaterial woodMaterial = new PTMaterial(Game, new TexturePath("crate1.jpg"));
                woodMaterial.Name = "Common wood material";
                //woodMaterial.DirectionalLight1 = new DirectionalLightGPU();
                //woodMaterial.DirectionalLight1.DiffuseColor = Vector4.One;
                //woodMaterial.DirectionalLight1.AmbientColor = new Vector4(0.2f, 0.2f, 0.2f, 1f);
                //woodMaterial.DirectionalLight1.LightDirection = new Vector3(0.2f, -0.2f, 0.9f);
                woodMaterial.SpecularPower = 20f;
                woodMaterial.IsSpecularEnabled = true;
                woodMaterial.LoadContent(contentManager);
                AddMaterial(PTMaterial.COMMON_WOOD_MATERIAL_KEY, woodMaterial);

                PTMaterial metalMaterial = new PTMaterial(Game, new TexturePath("metal1.jpg"));
                metalMaterial.Name = "Common metal material";
                //metalMaterial.DirectionalLight1 = new DirectionalLightGPU();
                //metalMaterial.DirectionalLight1.DiffuseColor = Vector4.One;
                //metalMaterial.DirectionalLight1.AmbientColor = new Vector4(0.2f, 0.2f, 0.2f, 1f);
                //metalMaterial.DirectionalLight1.LightDirection = new Vector3(-0.8f, 0f, 0.8f);
                //metalMaterial.DirectionalLight1.SpecularColor = new Vector4(0.85f, 0.85f, 0.85f, 1);
                metalMaterial.SpecularPower = 15f;
                metalMaterial.IsSpecularEnabled = true;
                metalMaterial.LoadContent(contentManager);
                AddMaterial(PTMaterial.COMMON_METAL_MATERIAL_KEY, metalMaterial);
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
        public override void Apply(DeviceContext1 context)
        {
            base.Apply(context);

            // Set the matrices and lights constant buffers.
            context.VertexShader.SetConstantBuffer(0, MatricesConstantBuffer);
            context.VertexShader.SetConstantBuffer(1, ClipConstantBuffer);
            context.VertexShader.SetConstantBuffer(2, ReflectionConstantBuffer);
            context.PixelShader.SetConstantBuffer(0, MaterialConstantBuffer);
            context.PixelShader.SetConstantBuffer(1, GlobalLightingConstantBuffer);
            context.PixelShader.SetConstantBuffer(2, LightsConstantBuffer);

            // ===== Update common Effect properties
            //context.UpdateSubresource(ref light, effect.LightningConstantBuffer);
            var clipPlain = ClipPlain;
            var dataBoxClip = context.MapSubresource(ClipConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            Utilities.Write(dataBoxClip.DataPointer, ref clipPlain);
            context.UnmapSubresource(ClipConstantBuffer, 0);

            var globalLightingBuffer = GlobalLightingBuffer;
            context.UpdateSubresource(ref globalLightingBuffer, GlobalLightingConstantBuffer);

            //var lightBuffer2 = LightsBuffer.Lights;
            //var dataBoxLightBuffer2 = context.MapSubresource(LightsConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            //Utilities.Write(dataBoxLightBuffer2.DataPointer, lightBuffer2, 0, PTLight.MAX_LIGHTS);
            //context.UnmapSubresource(LightsConstantBuffer, 0);


            // With data stream
            var lightBuffer = LightsBuffer.Lights;
            DataStream stream;
            var dataBoxLightBuffer2 = context.MapSubresource(LightsConstantBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
            stream.WriteRange(lightBuffer, 0, PTLight.MAX_LIGHTS);
            context.UnmapSubresource(LightsConstantBuffer, 0);
            stream.Dispose();


            if (Game.Renderer.ProcessRenderMode == GraphicsRenderer.ProcessRenderingMode.MultiThread)
            {
                context.Rasterizer.SetViewport(Game.Viewports[0]);
                context.OutputMerger.SetTargets(Game.DepthStencilView, Game.RenderTarget.RenderTarget);
            }
        }

        #region Lights
        /// <summary>
        /// Add a new light to the <see cref="Lights"/> list.
        /// </summary>
        /// <param name="light">The Light.</param>
        public void AddLight(PTLight light)
        {
            if (lights.Count >= PTLight.MAX_LIGHTS)
            {
                Log.Warning("The maximum number of lights reached. Light not added - Max Lights: " + PTLight.MAX_LIGHTS);
                return;
            }

            LightsBuffer.AddLight(light);
            light.OnStateUpdatedEvent += Light_OnStateUpdatedEvent;
            lights.Add(light);
        }

        /// <summary>
        /// Remove a light from the <see cref="Lights"/> list.
        /// </summary>
        /// <param name="light">The Light to be removed.</param>
        public void RemoveLight(PTLight light)
        {
            if (lights.Contains(light))
            {
                LightsBuffer.Lights[light.Index].IsEnabled = 0;
                light.OnStateUpdatedEvent -= Light_OnStateUpdatedEvent;
                lights.Remove(light);
            }
        }

        private void Light_OnStateUpdatedEvent(object sender, EventArgs e)
        {
            PTLight light = (sender as PTLight);
            LightsBuffer.Lights[light.Index] = light.LightBuffer;
        }
        #endregion

        #region Materials
        

        
        #endregion

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
