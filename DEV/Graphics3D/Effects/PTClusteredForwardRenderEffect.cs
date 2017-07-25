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
using PoncheToolkit.Graphics3D.Cameras;

namespace PoncheToolkit.Graphics3D.Effects
{
    /// <summary>
    /// Class that represent the effect that at least all the models will have.
    /// Has basic lights just for a minimum "good looking" rendering.
    /// </summary>
    public class PTClusteredForwardRenderEffect : PTEffect
    {
        public static int CLUSTER_SIZE_X = 64;
        public static int CLUSTER_SIZE_Y = 64;
        public static int CLUSTER_MAX_SIZE_X = 256;
        public static int CLUSTER_MAX_SIZE_Y = 256;
        public static int CLUSTER_MAX_SIZE_Z = 256;

        #region Fields
        private Dictionary<string, PTMaterial> materials;
        private List<PTLight> lights;
        private ClipPlainStruct clipPlain;
        private PTMaterial materialBuffer;
        private Vector4 globalAmbientColor;
        private Vector4 cameraPosition;
        private Camera currentCamera;

        internal ClusteredGlobalStruct ClusteredBuffer;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override int MaxLightsCount { get { return PTLight.CLUSTERED_FORWARD_SHADING_MAX_LIGHTS; } }

        /// <summary>
        /// The camera position to apply the effect.
        /// </summary>
        public Vector4 CameraPosition
        {
            get { return cameraPosition; }
            set { SetProperty(ref cameraPosition, value); }
        }

        /// <summary>
        /// The current camera used for transformations.
        /// </summary>
        public Camera CurrentCamera
        {
            get { return currentCamera; }
            set { SetProperty(ref currentCamera, value); }
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

        /// <summary>
        /// The game instance.
        /// </summary>
        public int MaterialCount
        {
            get { return materials.Count; }
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
        public PTClusteredForwardRenderEffect(Game11 game) 
            : base(game, PTEffect.CLUSTERED_FORWARD_RENDER_EFFECT_PATH, includePaths: null)
        {
            this.materials = new Dictionary<string, PTMaterial>();
            this.ClipPlain = new ClipPlainStruct(new Vector4(-1, -1, -1, 1000f));
            this.globalAmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1);
            this.lights = new List<PTLight>();
            this.LightsBuffer = new LightBufferStruct(null);

            CLUSTER_MAX_SIZE_X = ((Game.Settings.Resolution.Width + CLUSTER_SIZE_X - 1) / CLUSTER_SIZE_X);
            CLUSTER_MAX_SIZE_Y = ((Game.Settings.Resolution.Height + CLUSTER_MAX_SIZE_Y - 1) / CLUSTER_MAX_SIZE_Y);
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
            
            // Create the constant buffers.
            MatricesConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<MatricesStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            ClipConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<ClipPlainStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            ReflectionConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<ReflectionStruct>(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            MaterialConstantBuffer = new Buffer(Game.Renderer.Device, MaterialStruct.SizeOf(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            //LightConstantBuffer = new Buffer(Game.Renderer.Device, LightBufferStruct.SizeOf(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            LightsConstantBuffer = new Buffer(Game.Renderer.Device, LightBufferStruct.SizeOf(), ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            ToDispose(MatricesConstantBuffer);
            ToDispose(ClipConstantBuffer);
            ToDispose(ReflectionConstantBuffer);
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
            context.PixelShader.SetConstantBuffer(2, LightsConstantBuffer);

            // ===== Update common Effect properties
            //context.UpdateSubresource(ref light, effect.LightningConstantBuffer);
            ClipPlainStruct clipPlain = ClipPlain;
            var dataBoxClip = context.MapSubresource(ClipConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            Utilities.Write(dataBoxClip.DataPointer, ref clipPlain);
            context.UnmapSubresource(ClipConstantBuffer, 0);
            

            // With data stream
            //LightStruct[] lights = LightsBuffer.Lights;
            //DataStream stream;
            //var dataBoxLightBuffer2 = context.MapSubresource(LightsConstantBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
            //stream.WriteRange(lights, 0, PTLight.MAX_LIGHTS);
            //context.UnmapSubresource(LightsConstantBuffer, 0);
            //stream.Dispose();


            // ==== Implementation of Clustered Forward
            LightStruct[] lights = LightsBuffer.Lights;
            for (int i = 0; i<lights.Length; i++)
            {
                lights[i].Position = Vector4.Transform(lights[i].Position, currentCamera.View);
            }

            //ClusteredGlobalStruct clusteredBuffer = ClusteredBuffer;
            //clusteredBuffer.Near = 1.0f / Game.CurrentCamera.NearPlane;
            //int gridY = (Game.Settings.Resolution.Height + LIGHT_GRID_TILE_DIM_Y - 1) / LIGHT_GRID_TILE_DIM_Y;
            //float sD = (float)(2.0f * Math.Tan(MathUtil.DegreesToRadians(Game.CurrentCamera.FOV) * 0.5f) / gridY);
            //clusteredBuffer.NearLog = (float)(1.0f / Math.Log(sD + 1.0f));



            if (Game.Renderer.ProcessRenderMode == ProcessRenderingMode.MultiThread)
            {
                context.Rasterizer.SetViewport(Game.Viewports[0]);
                context.OutputMerger.SetTargets(Game.DepthStencilView, Game.BackBufferRenderTarget.RenderTargetView);
            }
        }

        private void renderZOpaquePass()
        {

        }

        #region Lights
        /// <summary>
        /// Add a new light to the <see cref="Lights"/> list.
        /// </summary>
        /// <param name="light">The Light.</param>
        public void AddLight(PTLight light)
        {
            if (lights.Count >= PTLight.FORWARD_SHADING_MAX_LIGHTS)
            {
                Log.Warning("The maximum number of lights reached. Light not added - Max Lights: " + PTLight.FORWARD_SHADING_MAX_LIGHTS);
                return;
            }

            //LightsBuffer.AddLight(light);
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
        /// <summary>
        /// Add a new material to the <see cref="Materials"/> dictionary.
        /// The material must have a name assigned.
        /// </summary>
        /// <param name="material">The material.</param>
        public void AddMaterial(PTMaterial material)
        {
            if (string.IsNullOrEmpty(material.Name))
                throw new ArgumentNullException("The name property is null or empty.");

            PTMaterial mat;
            materials.TryGetValue(material.Name.ToLower(), out mat);
            if (mat == null)
                materials.Add(material.Name.ToLower(), material);
            else
                Log.Warning("The material trying to be added with name -{0}- already exists.", material.Name);
        }

        ///// <summary>
        ///// Add a new material to the <see cref="materials"/> dictionary.
        ///// </summary>
        ///// <param name="name">Name of the material</param>
        ///// <param name="material">The material.</param>
        //public void AddMaterial(string name, PTMaterial material)
        //{
        //    if (string.IsNullOrEmpty(name))
        //        throw new ArgumentNullException("The name property is null or empty.");

        //    material.Name = name;
        //    PTMaterial mat;
        //    materials.TryGetValue(name.ToLower(), out mat);
        //    if (mat == null)
        //        materials.Add(name.ToLower(), material);
        //    else
        //        Log.Warning("The material trying to be added with name -{0}- already exists.", name);
        //}

        ///// <summary>
        ///// Retrieve a material by its name.
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public PTMaterial GetMaterial(string name)
        //{
        //    if (string.IsNullOrEmpty(name))
        //        throw new ArgumentNullException("The name of the material to retrieve is null or empty for effect -" + System.IO.Path.GetFileName(ShaderPath) + "-");

        //    PTMaterial mat;
        //    materials.TryGetValue(name.ToLower(), out mat);

        //    if (mat == null)
        //        Log.Warning("Material -{0}- does not exist in effect: -{1}-", name, System.IO.Path.GetFileName(ShaderPath));

        //    return mat;
        //}
        #endregion

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();

            ContentsPool.ClearShader(Shader);

            foreach (PTMaterial material in materials.Values)
                material.Dispose();
            materials.Clear();
        }
    }
}
