using PoncheToolkit.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Core;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX;
using PoncheToolkit.Graphics3D.Effects;
using SharpDX.DXGI;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Base Model class that has properties to draw the vertices of an imported model
    /// or create a model from primitives.
    /// </summary>
    public abstract class Model : GameDrawableComponent
    {
        #region Fields
        private List<int> indices;
        private Shader shader;
        private Buffer vertexBuffer;
        private Buffer indexBuffer;
        private bool hasTexture;
        private bool acceptInput;
        #endregion

        #region Properties
        /// <summary>
        /// SamplerState to send to the Shader to sample the texture.
        /// </summary>
        public SamplerState Sampler;

        /// <summary>
        /// Vertices that represent this primitive.
        /// </summary>
        public List<VertexMainStruct> Vertices;

        /// <summary>
        /// The Vertex shader signature.
        /// </summary>
        public Vector3 Size;

        /// <summary>
        /// The Position in 3D space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Indices that represent this primitive.
        /// </summary>
        public List<int> Indices
        {
            get { return indices; }
        }

        /// <summary>
        /// The Shader object.
        /// </summary>
        public Shader Shader
        {
            get { return shader; }
            set { shader = value; }
        }

        /// <summary>
        /// The Vertex Buffer.
        /// </summary>
        public Buffer VertexBuffer
        {
            get { return vertexBuffer; }
            set { vertexBuffer = value; }
        }

        /// <summary>
        /// The Index Buffer.
        /// </summary>
        public Buffer IndexBuffer
        {
            get { return indexBuffer; }
            set { indexBuffer = value; }
        }

        /// <summary>
        /// Value to indicate if the model has a texture, so it can be sent to the shader correctly.
        /// </summary>
        public bool HasTexture
        {
            get { return hasTexture; }
            set { hasTexture = value; }
        }

        /// <summary>
        /// Value to tell if this model react to user input or no.
        /// </summary>
        public bool AcceptInput
        {
            get { return acceptInput; }
            set { acceptInput = value; }
        }

        /// <summary>
        /// Get or Set the buffer to be sent to the shader.
        /// </summary>
        public Buffer MatricesConstantBuffer;

        /// <summary>
        /// Get or Set the buffer to be sent to the shader.
        /// </summary>
        public Buffer LightningConstantBuffer;

        /// <summary>
        /// List of lights sent to the shader.
        /// It has 1 default light.
        /// </summary>
        public List<LightningStruct> Lights;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
        public Model(Game11 game)
            : base(game)
        {
            Vertices = new List<VertexMainStruct>();
            indices = new List<int>();
            Lights = new List<LightningStruct>();
            Size = Vector3.One;
            Position = Vector3.Zero;
            acceptInput = true;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void LoadShaders()
        {
            // Compile Vertex and Pixel shaders
            try
            {
                // TODO: Create a single shader that can manage textures or not.
                if (hasTexture)
                {
                    Shader = Game.ContentManager.Load<Shader>("PTTextureEffect.fx");
                    Shader.InputLayout = new InputLayout(Game.Renderer.Device, shader.VertexShaderSignature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, InputElement.AppendAligned, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, InputElement.AppendAligned, 0)
                    });

                    // Create the Texture sampler.
                    SamplerStateDescription samplerDescription = new SamplerStateDescription();
                    samplerDescription.Filter = Filter.MinMagMipLinear;
                    samplerDescription.AddressU = TextureAddressMode.Wrap;
                    samplerDescription.AddressV = TextureAddressMode.Wrap;
                    samplerDescription.AddressW = TextureAddressMode.Wrap;
                    samplerDescription.MipLodBias = 0.0f;
                    samplerDescription.MaximumAnisotropy = 16;
                    samplerDescription.ComparisonFunction = Comparison.Never;
                    samplerDescription.BorderColor = Color.Black;
                    samplerDescription.MinimumLod = 0;
                    samplerDescription.MaximumLod = float.MaxValue;

                    Sampler = new SamplerState(Game.Renderer.Device, samplerDescription);
                }
                else
                {
                    Shader = Game.ContentManager.Load<Shader>("PTColorEffect.fx");
                    Shader.InputLayout = new InputLayout(Game.Renderer.Device, Shader.VertexShaderSignature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                        new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error when loading effect.", ex);
            }


            // Add a simple light
            Lights.Add(new LightningStruct()
            {
                LightDirection = new Vector3(0f, 0f, 1f),
                //DiffuseColor = new Vector4(0.7f, 0.4f, 1f, 1.0f)
                DiffuseColor = Vector4.One
            });

        }

        /// <summary>
        /// The Model base LoadContent method load the default basic shaders to draw with color, texture, etc.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();

            // Load the buffers
            VertexBuffer = Buffer.Create(Game.Renderer.Device, BindFlags.VertexBuffer, Vertices.ToArray(), 0, ResourceUsage.Default, CpuAccessFlags.None, ResourceOptionFlags.None);
            IndexBuffer = ((Indices.Count > 0) ? (Buffer.Create(Game.Renderer.Device, BindFlags.IndexBuffer, Indices.ToArray())) : null);
            MatricesConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<MatricesStruct>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            LightningConstantBuffer = new Buffer(Game.Renderer.Device, Utilities.SizeOf<LightningStruct>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        /// <inheritdoc/>
        public override void UnloadContent()
        {
            
        }

        /// <summary>
        /// The base Model Render method by default send the transposed World-View-Projection matrices
        /// using the <see cref="DeviceContext.UpdateSubresource{T}(ref T, SharpDX.Direct3D11.Resource, int, int, int, ResourceRegion?)"/> method.
        /// The matrices multiplication must be made inside the shader.
        /// <para>It call last the <see cref="DeviceContext.Draw(int, int)"/> method.</para>
        /// </summary>
        public override void Render()
        {
            // Set the value of the World-View-Projection matrices.
            MatricesStruct matrices = new MatricesStruct()
            {
                World = Matrix.Identity,
                View = Game.Cameras[0].View,
                Projection = Game.Cameras[0].Projection
            };
            matrices.World *= Matrix.Scaling(Size);
            matrices.World *= Matrix.Translation(Position);
            matrices.World.Transpose();
            matrices.View.Transpose();
            matrices.Projection.Transpose();

            // Send the worldViewProjection matrix to the shader
            Game.Renderer.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexMainStruct>(), 0));

            // Assing the subresources (constant buffers) values.
            LightningStruct light = Lights[0];
            Game.Renderer.DeviceContext.UpdateSubresource(ref matrices, MatricesConstantBuffer);
            Game.Renderer.DeviceContext.UpdateSubresource(ref light, LightningConstantBuffer);

            // Set the input layout
            Game.Renderer.DeviceContext.InputAssembler.InputLayout = Shader.InputLayout;
            Game.Renderer.DeviceContext.Draw(Vertices.Count, 0);

            // TODO: Tests to draw with indices.
            //for (int i = 0; i < technique.Description.PassCount; ++i)
            //{
            //    pass.Apply(Game.Renderer.DeviceContext);
            //    Game.Renderer.DeviceContext.Draw(3, 0);
            //    //Game.Renderer.DeviceContext.DrawIndexed(Indices.Count, 0, 0);
            //}
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            UnloadContent();

            shader.Dispose();
            Utilities.Dispose(ref MatricesConstantBuffer);
            Utilities.Dispose(ref vertexBuffer);
            Utilities.Dispose(ref indexBuffer);
            Vertices.Clear();
            indices.Clear();
            Vertices = null;
            indices = null;
        }
        #endregion
    }
}
