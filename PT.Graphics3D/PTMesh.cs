using PoncheToolkit.Core;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Collections.Generic;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace PT.Graphics3D
{
    using Resources;
    using Util;
    using Util.Exceptions;
    using Effects;
    using Core.Components;
    using Graphics2D;
    using Core.Management.Content;
    using Cameras;
    /// <summary>
    /// Class that represent a Mesh of a model.
    /// </summary>
    public sealed class PTMesh : GameRenderableComponent, System.ICloneable
    {
        #region Fields
        private string name;
        private AssimpPrimitiveType primitiveType;
        private int materialIndex;
        private List<Vector3> normals;
        private List<Vector3> tangents;
        private List<Vector3> bitangents;
        private List<PTFace> faces;
        private List<Color>[] m_colors;
        private List<Vector3>[] m_texCoords;
        private int[] texComponentCount;
        private List<PTBone> bones;
        private List<MeshAnimationAttachment> meshAttachments;
        private Game11 game;
        
        private Buffer vertexBuffer;
        private Buffer indexBuffer;
        private int indicesCount;
        private int instanceIndex;

        private PTModel model;
        #endregion

        #region Properties
        /// <summary>
        /// Vertices that represent this mesh.
        /// </summary>
        public List<VertexMainStruct> Vertices;

        /// <summary>
        /// The Vertex Buffer.
        /// </summary>
        public Buffer VertexBuffer
        {
            get { return vertexBuffer; }
            set { SetProperty(ref vertexBuffer, value); }
        }

        /// <summary>
        /// Get or set the Model to which this mesh belongs.
        /// </summary>
        public PTModel Model
        {
            get { return model; }
            set { SetProperty(ref model, value); }
        }

        /// <summary>
        /// The Index Buffer.
        /// </summary>
        public Buffer IndexBuffer
        {
            get { return indexBuffer; }
            set { SetProperty(ref indexBuffer, value); }
        }

        /// <summary>
        /// Get the number of indices.
        /// </summary>
        public int IndicesCount
        {
            get { return indicesCount; }
            internal set { SetProperty(ref indicesCount, value); }
        }

        /// <summary>
        /// Gets or sets the mesh name. This tends to be used
        /// when formats name nodes and meshes independently,
        /// vertex animations refer to meshes by their names,
        /// or importers split meshes up, each mesh will reference
        /// the same (dummy) name.
        /// </summary>
        public new string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        /// <summary>
        /// Gets or sets the primitive type. This may contain more than one
        /// type unless if <see cref="Assimp.PostProcessSteps.SortByPrimitiveType"/>
        /// option is not set.
        /// </summary>
        public AssimpPrimitiveType PrimitiveType
        {
            get { return primitiveType; }
            set { SetProperty(ref primitiveType, value); }
        }

        /// <summary>
        /// Gets or sets the index of the material associated with this mesh.
        /// </summary>
        public int MaterialIndex
        {
            get { return materialIndex; }
            set { SetProperty(ref materialIndex, value); }
        }

        /// <summary>
        /// Gets if the mesh as normals. If it does exist, the count should be the same as the vertex count.
        /// </summary>
        public bool HasNormals
        {
            get { return normals.Count > 0; }
        }

        /// <summary>
        /// Gets the vertex normal list.
        /// </summary>
        public List<Vector3> Normals
        {
            get { return normals; }
            internal set { SetProperty(ref normals, value); }
        }

        /// <summary>
        /// Gets if the mesh has tangents and bitangents. It is not
        /// possible for one to be without the other. If it does exist, the count should be the same as the vertex count.
        /// </summary>
        public bool HasTangentBasis
        {
            get { return tangents.Count > 0 && bitangents.Count > 0; }
        }

        /// <summary>
        /// Gets the vertex tangent list.
        /// </summary>
        public List<Vector3> Tangents
        {
            get { return tangents; }
            internal set { SetProperty(ref tangents, value); }
        }

        /// <summary>
        /// Gets the vertex bitangent list.
        /// </summary>
        public List<Vector3> BiTangents
        {
            get { return bitangents; }
            internal set { SetProperty(ref bitangents, value); }
        }

        /// <summary>
        /// Gets the number of faces contained in the mesh.
        /// </summary>
        public int FaceCount { get { return faces.Count; } }

        /// <summary>
        /// Gets if the mesh contains faces. If no special
        /// scene flags are set, this should always return true.
        /// </summary>
        public bool HasFaces { get { return faces.Count > 0; } }

        /// <summary>
        /// Gets the mesh's faces. Each face will contain indices
        /// to the vertices.
        /// </summary>
        public List<PTFace> Faces { get { return faces; } internal set { SetProperty(ref faces, value); } }

        /// <summary>
        /// Gets the number of valid vertex color channels contained in the
        /// mesh (list is not empty/not null). This can be a value between zero and the maximum vertex color count. Each individual channel
        /// should be the size of <see cref="Vertices"/>.
        /// </summary>
        public int VertexColorChannelCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < m_colors.Length; i++)
                {
                    if (HasVertexColors(i))
                        count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the number of valid texture coordinate channels contained
        /// in the mesh (list is not empty/not null). This can be a value between zero and the maximum texture coordinate count.
        /// Each individual channel should be the size of <see cref="Vertices"/>.
        /// </summary>
        public int TextureCoordinateChannelCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < m_texCoords.Length; i++)
                {
                    if (HasTextureCoords(i))
                        count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Gets the array that contains each vertex color channels, by default all are lists of zero (but can be set to null). Each index
        /// in the array corresponds to the texture coordinate channel. The length of the array corresponds to Assimp's maximum vertex color channel limit.
        /// </summary>
        public List<Color>[] VertexColorChannels { get { return m_colors; } internal set { SetProperty(ref m_colors, value); } }

        /// <summary>
        /// Gets the array that contains each texture coordinate channel, by default all are lists of zero (but can be set to null). Each index
        /// in the array corresponds to the texture coordinate channel. The length of the array corresponds to Assimp's maximum UV channel limit.
        /// </summary>
        public List<Vector3>[] TextureCoordinateChannels { get { return m_texCoords; } internal set { SetProperty(ref m_texCoords, value); } }

        /// <summary>
        /// Gets the array that contains the count of UV(W) components for each texture coordinate channel, usually 2 (UV) or 3 (UVW). A component
        /// value of zero means the texture coordinate channel does not exist. The channel index (index in the array) corresponds
        /// to the texture coordinate channel index.
        /// </summary>
        public int[] UVComponentCount { get { return texComponentCount; } internal set { SetProperty(ref texComponentCount, value); } }

        /// <summary>
        /// Gets the number of bones that influence this mesh.
        /// </summary>
        public int BoneCount { get { return bones.Count; } }

        /// <summary>
        /// Gets if this mesh has bones.
        /// </summary>
        public bool HasBones { get { return bones.Count > 0; } }

        /// <summary>
        /// Gets the bones that influence this mesh.
        /// </summary>
        public List<PTBone> Bones { get { return bones; } internal set { SetProperty(ref bones, value); } }

        /// <summary>
        /// Gets the number of mesh animation attachments that influence this mesh.
        /// </summary>
        public int MeshAnimationAttachmentCount { get { return meshAttachments.Count; } }

        /// <summary>
        /// Gets if this mesh has mesh animation attachments.
        /// </summary>
        public bool HasMeshAnimationAttachments { get { return meshAttachments.Count > 0; } }

        /// <summary>
        /// Gets the mesh animation attachments that influence this mesh.
        /// </summary>
        public List<MeshAnimationAttachment> MeshAnimationAttachments { get { return meshAttachments; } internal set { SetProperty(ref meshAttachments, value); } }

        /// <summary>
        /// The material name to use for this Mesh.
        /// This name will work to retrieve the Material from the <see cref="PTEffect"/> Materials dictionary.
        /// </summary>
        public string MaterialName { get; internal set; }

        /// <summary>
        /// Get or set the instance index for this mesh only if the mesh is set as Instanced. (reuse of vertices).
        /// If the value is -1 it means the mesh is not instanced and will be tretaed as a single object.
        /// Default: -1
        /// </summary>
        public int InstanceIndex { get { return instanceIndex; } internal set { SetProperty(ref instanceIndex, value); } }
        #endregion

        #region Events
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructs a new instance of the <see cref="PTMesh"/> class.
        /// </summary>
        /// <param name="model">The <see cref="Model"/> to which this mesh belongs.</param>
        public PTMesh(PTModel model)
            : this(model, string.Empty, AssimpPrimitiveType.Triangle)
        {
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="PTMesh"/> class.
        /// </summary>
        /// <param name="model">The <see cref="Model"/> to which this mesh belongs.</param>
        /// <param name="name">Name of the mesh.</param>
        public PTMesh(PTModel model, string name)
            : this(model, name, AssimpPrimitiveType.Triangle)
        {
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="PTMesh"/> class.
        /// </summary>
        /// <param name="model">The <see cref="Model"/> to which this mesh belongs.</param>
        /// <param name="primType">Primitive types contained in the mesh.</param>
        public PTMesh(PTModel model, AssimpPrimitiveType primType)
            : this(model, string.Empty, primType)
        {
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="PTMesh"/> class.
        /// </summary>
        /// <param name="model">The <see cref="Model"/> to which this mesh belongs.</param>
        /// <param name="name">Name of the mesh</param>
        /// <param name="primType">Primitive types contained in the mesh.</param>
        public PTMesh(PTModel model, string name, AssimpPrimitiveType primType)
            : base(model.Game)
        {
            this.model = model;
            this.name = name;
            this.game = model.Game;
            this.primitiveType = primType;
            this.materialIndex = 0;
            this.instanceIndex = -1;

            Vertices = new List<VertexMainStruct>();
            normals = new List<Vector3>();
            tangents = new List<Vector3>();
            bitangents = new List<Vector3>();
            bones = new List<PTBone>();
            faces = new List<PTFace>();
            texComponentCount = new int[AssimpConstants.AI_MAX_NUMBER_OF_TEXTURECOORDS];
            m_colors = new List<Color>[AssimpConstants.AI_MAX_NUMBER_OF_COLOR_SETS];
            m_texCoords = new List<Vector3>[AssimpConstants.AI_MAX_NUMBER_OF_TEXTURECOORDS];
            meshAttachments = new List<MeshAnimationAttachment>();

            for (int i = 0; i < m_colors.Length; i++)
                m_colors[i] = new List<Color>();
            
            for (int i = 0; i < m_texCoords.Length; i++)
                m_texCoords[i] = new List<Vector3>();
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            IsInitialized = true;
            base.Initialize();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a new vertex.
        /// </summary>
        /// <param name="vertex"></param>
        public void AddVertex(VertexMainStruct vertex)
        {
            Vertices.Add(vertex);
            OnPropertyChanged(vertex, "Vertex");
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (!IsStateUpdated)
            {
                IsStateUpdated = true;
                OnStateUpdated();
            }

            return IsStateUpdated;
        }

        /// <inheritdoc/>
        public override void LoadContent(ContentManager contentManager)
        {
            if (Vertices.Count <= 0)
                throw new LoadContentException(ExceptionTexts.LoadContent_VerticesEmpty + " - {0}", Name);

            // Load the buffers
            VertexBuffer = Buffer.Create(game.Renderer.Device, BindFlags.VertexBuffer, Vertices.ToArray(), 0, ResourceUsage.Default, CpuAccessFlags.None, ResourceOptionFlags.None);
            IndexBuffer = ((indicesCount > 0) ? (Buffer.Create(game.Renderer.Device, BindFlags.IndexBuffer, GetIndices())) : null);

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();

            //base.LoadContent(contentManager);
        }

        /// <inheritdoc/>
        public override void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null)
        {
            // Set the vertices and indices buffers into the InputAssembler phase.
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<VertexMainStruct>(), 0));
            context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);

            // Draw
            context.DrawIndexed(indicesCount, 0, 0);
        }

        /// <summary>
        /// Get the properties of the model and create the World Matrix.
        /// </summary>
        /// <param name="camera"></param>
        public void UpdateMatrices(Camera camera)
        {
            this.Model.World = Matrix.Scaling(model.Size) * Matrix.RotationYawPitchRoll(model.Rotation.Y, model.Rotation.X, model.Rotation.Z) * Matrix.Translation(model.Position);
        }

        /// <summary>
        /// Apply its state to the GPU. Calculate the mesh matrices.
        /// </summary>
        /// <param name="effect">The effect used for this material.</param>
        /// <param name="context">Context to apply the rendering.</param>
        public void Apply(PTForwardRenderEffect effect, DeviceContext1 context)
        {
            UpdateMatrices(Game.CurrentCamera);
            
            // Set the matrices constant buffer.
            MatricesStruct matrices = game.CurrentCamera.GetMatrices(this.Model.World);
            //context.UpdateSubresource(ref matrices, effect.MatricesConstantBuffer);
            var dataBox = context.MapSubresource(effect.MatricesConstantBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            Utilities.Write(dataBox.DataPointer, ref matrices);
            context.UnmapSubresource(effect.MatricesConstantBuffer, 0);

            //this.Model.World.Transpose();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();

            ClearBuffers();

            Utilities.Dispose(ref vertexBuffer);
            Utilities.Dispose(ref indexBuffer);
            Effect.Dispose();
        }

        #region Assimp methods
        /// <summary>
        /// Checks if the mesh has vertex colors for the specified channel. This returns false if the list
        /// is null or empty. The channel, if it exists, should contain the same number of entries as <see cref="Vertices"/>.
        /// </summary>
        /// <param name="channelIndex">Channel index</param>
        /// <returns>True if vertex colors are present in the channel.</returns>
        public bool HasVertexColors(int channelIndex)
        {
            if (channelIndex >= this.m_colors.Length || channelIndex < 0)
                return false;

            List<Color> colors = this.m_colors[channelIndex];

            if (colors != null)
                return colors.Count > 0;

            return false;
        }

        /// <summary>
        /// Checks if the mesh has texture coordinates for the specified channel. This returns false if the list
        /// is null or empty. The channel, if it exists, should contain the same number of entries as <see cref="Vertices"/>.
        /// </summary>
        /// <param name="channelIndex">Channel index</param>
        /// <returns>True if texture coordinates are present in the channel.</returns>
        public bool HasTextureCoords(int channelIndex)
        {
            if (channelIndex >= this.m_texCoords.Length || channelIndex < 0)
                return false;

            List<Vector3> texCoords = this.m_texCoords[channelIndex];

            if (texCoords != null)
                return texCoords.Count > 0;

            return false;
        }

        /// <summary>
        /// Convienence method for setting this meshe's face list from an index buffer.
        /// </summary>
        /// <param name="indices">Index buffer</param>
        /// <param name="indicesPerFace">Indices per face</param>
        /// <returns>True if the operation succeeded, false otherwise (e.g. not enough data)</returns>
        public bool SetIndices(int[] indices, int indicesPerFace)
        {
            if (indices == null || indices.Length == 0 || ((indices.Length % indicesPerFace) != 0))
                return false;

            faces.Clear();

            indicesCount = indices.Length;
            int numFaces = indices.Length / indicesPerFace;
            int index = 0;

            for (int i = 0; i < numFaces; i++)
            {
                PTFace face = new PTFace();
                for (int j = 0; j < indicesPerFace; j++)
                {
                    face.Indices.Add(indices[index]);
                    index++;
                }
                faces.Add(face);
            }

            return true;
        }

        ///// <summary>
        ///// Convienence method for setting this meshe's face list from an index buffer.
        ///// </summary>
        ///// <param name="indices">Index buffer</param>
        ///// <param name="face">The indices for a given face.</param>
        ///// <returns>True if the operation succeeded, false otherwise (e.g. not enough data)</returns>
        //public bool UpdateIndices(ref Mesh mesh)
        //{
        //    indicesCount = 0;

        //    for (int i = 0; i < faces.Count; i++)
        //    {
        //        Face face = faces[i];
        //        for (int j = 0; j < face.Indices.Count; j++)
        //        {
        //            mesh.Indices.Add(indices[indicesCount]);
        //            indicesCount++;
        //        }
        //        faces.Add(face);
        //    }

        //    return true;
        //}

        /// <summary>
        /// Convienence method for setting this meshe's face list from an index buffer.
        /// </summary>
        /// <param name="indices">Index buffer</param>
        /// <param name="indicesPerFace">Indices per face</param>
        /// <returns>True if the operation succeeded, false otherwise (e.g. not enough data)</returns>
        public bool SetIndices(List<int> indices, int indicesPerFace)
        {
            return SetIndices(indices.ToArray(), indicesPerFace);
        }

        /// <summary>
        /// Convienence method for accumulating all face indices into a single
        /// index array.
        /// </summary>
        /// <returns>int index array</returns>
        public int[] GetIndices()
        {
            if (HasFaces)
            {
                List<int> indices = new List<int>();
                foreach (PTFace face in faces)
                {
                    if (face.IndexCount > 0 && face.Indices != null)
                    {
                        indices.AddRange(face.Indices);
                    }
                }
                return indices.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Convienence method for accumulating all face indices into a single index
        /// array as unsigned integers (the default from Assimp, if you need them).
        /// </summary>
        /// <returns>uint index array</returns>
        public uint[] GetUnsignedIndices()
        {
            if (HasFaces)
            {
                List<uint> indices = new List<uint>();
                foreach (PTFace face in faces)
                {
                    if (face.IndexCount > 0 && face.Indices != null)
                    {
                        foreach (uint index in face.Indices)
                        {
                            indices.Add((uint)index);
                        }
                    }
                }

                return indices.ToArray();
            }

            return null;
        }

        /// <summary>
        /// Convienence method for accumulating all face indices into a single
        /// index array.
        /// </summary>
        /// <returns>short index array</returns>
        public short[] GetShortIndices()
        {
            if (HasFaces)
            {
                List<short> indices = new List<short>();
                foreach (PTFace face in faces)
                {
                    if (face.IndexCount > 0 && face.Indices != null)
                    {
                        foreach (uint index in face.Indices)
                        {
                            indices.Add((short)index);
                        }
                    }
                }

                return indices.ToArray();
            }

            return null;
        }
        #endregion

        #endregion

        #region Private Methods
        private void ClearBuffers()
        {
            Vertices.Clear();
            normals.Clear();
            tangents.Clear();
            bitangents.Clear();

            for (int i = 0; i < m_colors.Length; i++)
            {
                List<Color> colors = m_colors[i];

                if (colors == null)
                    m_colors[i] = new List<Color>();
                else
                    colors.Clear();
            }

            for (int i = 0; i < m_texCoords.Length; i++)
            {
                List<Vector3> texCoords = m_texCoords[i];

                if (texCoords == null)
                    m_texCoords[i] = new List<Vector3>();
                else
                    texCoords.Clear();
            }

            for (int i = 0; i < texComponentCount.Length; i++)
            {
                texComponentCount[i] = 0;
            }

            bones.Clear();
            faces.Clear();
            meshAttachments.Clear();
        }

        private Vector3[] CopyTo(List<Vector3> list, Vector3[] copy)
        {
            list.CopyTo(copy);
            return copy;
        }

        /// <summary>
        /// Clone object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        #endregion
    }
}
