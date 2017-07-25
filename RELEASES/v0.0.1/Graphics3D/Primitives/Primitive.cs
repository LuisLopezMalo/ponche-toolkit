using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buffer = SharpDX.Direct3D11.Buffer;
using PoncheToolkit.Core.Components;
using SharpDX;
using PoncheToolkit.Core;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using System.Collections;
using PoncheToolkit.Util;

namespace PoncheToolkit.Graphics3D.Primitives
{
    /// <summary>
    /// Abstract class that represent a generic primitive.
    /// </summary>
    public abstract class Primitive : Model
    {
        #region Fields
        private string texturePath;
        private Texture2D texture;
        #endregion

        #region Events
        /// <inheritdoc/>
        public event DelegateHandlers.IUpdatableStateOnValueChangedHandler OnValueChanged;
        /// <inheritdoc/>
        public event EventHandler OnUpdated;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public bool IsUpdated { get; set; }

        /// <summary>
        /// The path to the texture applied.
        /// </summary>
        public string TexturePath
        {
            get { return texturePath; }
            set
            {
                texturePath = value;
                OnValueChanged?.Invoke("texture", texturePath);
            }
        }

        /// <summary>
        /// The texture to be applied to this primitive.
        /// In case the HasTexture value is true.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
        public Primitive(Game11 game)
            : base(game)
        {
            texturePath = "crate1.jpg";
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void LoadContent()
        {
            base.LoadContent();

            if (HasTexture)
                texture = Game.ContentManager.Load<Texture2D>(texturePath);

            // Set the first graphic.
            //Game.Renderer.DeviceContext.InputAssembler.InputLayout = Shader.InputLayout;
            Game.Renderer.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            
            //Game.Renderer.DeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            Game.Renderer.DeviceContext.VertexShader.SetConstantBuffer(0, MatricesConstantBuffer);
            Game.Renderer.DeviceContext.VertexShader.Set(Shader.VertexShader);
            Game.Renderer.DeviceContext.PixelShader.Set(Shader.PixelShader);
            Game.Renderer.DeviceContext.PixelShader.SetConstantBuffer(0, LightningConstantBuffer);
            Game.Renderer.DeviceContext.OutputMerger.SetTargets(Game.RenderTargetView);
        }

        /// <summary>
        /// Add a new vertex.
        /// </summary>
        /// <param name="vertex"></param>
        public void AddVertex(VertexMainStruct vertex)
        {
            Vertices.Add(vertex);
            //OnValueChanged?.Invoke(vertex, null);
        }

        /// <summary>
        /// Add an index.
        /// </summary>
        /// <param name="index"></param>
        public void AddIndex(int index)
        {
            Indices.Add(index);
            //OnValueChanged?.Invoke(index, null);
        }

        /// <summary>
        /// Update the state of the primitive values.
        /// This will update the texture and other properties.
        /// May consume some processing power.
        /// </summary>
        /// <returns></returns>
        public bool UpdateState()
        {
            if (!IsUpdated)
            {
                if (HasTexture)
                {
                    texture.Dispose();
                    texture = Game.ContentManager.Load<Texture2D>(texturePath);
                }
            }

            OnUpdated?.Invoke(this, null);
            return IsUpdated = true;
        }

        /// <summary>
        /// Update the Vertex and Pixel stages.
        /// </summary>
        public override void Render()
        {
            if (HasTexture)
            {
                Game.Renderer.DeviceContext.PixelShader.SetSampler(0, Sampler);
                Game.Renderer.DeviceContext.PixelShader.SetShaderResource(0, Texture.ShaderResourceView);
            }

            Game.Renderer.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            //Game.Renderer.DeviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            Game.Renderer.DeviceContext.VertexShader.SetConstantBuffer(0, MatricesConstantBuffer);
            Game.Renderer.DeviceContext.VertexShader.Set(Shader.VertexShader);
            Game.Renderer.DeviceContext.PixelShader.Set(Shader.PixelShader);
            Game.Renderer.DeviceContext.PixelShader.SetConstantBuffer(0, LightningConstantBuffer);
            Game.Renderer.DeviceContext.OutputMerger.SetTargets(Game.RenderTargetView);

            base.Render();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            Utilities.Dispose(ref MatricesConstantBuffer);
            Utilities.Dispose(ref Sampler);
        }
        #endregion
    }
}
