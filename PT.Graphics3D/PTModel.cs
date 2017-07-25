using PoncheToolkit.Core.Components;
using System;
using System.Collections.Generic;
using PoncheToolkit.Core;
using SharpDX.Direct3D11;
using SharpDX;
using Assimp;

namespace PT.Graphics3D
{
    using Resources;
    using Util;
    using Util.Exceptions;
    using Effects;
    using Core.Management.Content;
    using Graphics2D;
    using Core.Management.Input;

    /// <summary>
    /// Base Model class that has properties to draw the vertices of an imported model
    /// or create a model from primitives.
    /// </summary>
    public class PTModel : GameRenderableComponent, ICloneable, IContentItem, IInputReceivable
    {
        #region Fields
        private Vector3 position;
        private Vector3 rotation;
        private Vector3 size;
        private Vector3 up;
        private bool acceptInput;
        private string path;
        private List<PTMesh> meshes;
        private List<PTMaterial> importedMaterials;
        private Dictionary<PTMesh, int[]> indicesPerMesh;

        private Matrix world;
        private BoundingBox boundingBox;
        private bool hasCalculatedTangentBinormals;
        private float uvRepeatFactor;
        #endregion

        #region Properties
        /// <summary>
        /// The size of the object. Vector3.One is the original size.
        /// </summary>
        public Vector3 Size
        {
            get { return size; }
            set { SetPropertyAsDirty(ref size, value); }
        }

        /// <summary>
        /// The Position in 3D space.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { SetPropertyAsDirty(ref position, value); }
        }

        /// <summary>
        /// The Rotation in 3D space.
        /// </summary>
        public Vector3 Rotation
        {
            get { return rotation; }
            set { SetPropertyAsDirty(ref rotation, value); }
        }

        /// <summary>
        /// The Up Vector.
        /// </summary>
        public Vector3 Up
        {
            get { return up; }
            set { SetPropertyAsDirty(ref up, value); }
        }

        /// <summary>
        /// Get if the model has any meshes.
        /// </summary>
        public bool HasMeshes
        {
            get { return meshes.Count > 0; }
        }

        /// <summary>
        /// Value to tell if this model react to user input or no.
        /// </summary>
        public bool AcceptInput
        {
            get { return acceptInput; }
            set { SetPropertyAsDirty(ref acceptInput, value); }
        }

        /// <summary>
        /// Value that indicates if the tangent and binormals has been calculated.
        /// This is set to true when importing a model that was made with a 3D modelling program.
        /// If the model is a primitive or something else, it is calculated once when the <see cref="LoadContent(ContentManager)"/>
        /// method is called.
        /// </summary>
        public bool HasCalculatedTangentBinormals
        {
            get { return hasCalculatedTangentBinormals; }
            internal set { SetPropertyAsDirty(ref hasCalculatedTangentBinormals, value); }
        }

        /// <summary>
        /// The materials that comes from assimp when importing a model.
        /// This materials will be passed to the <see cref="Effect"/>.
        /// </summary>
        public List<PTMaterial> ImportedMaterials
        {
            get { return importedMaterials; }
            internal set { SetPropertyAsDirty(ref importedMaterials, value); }
        }

        /// <summary>
        /// The path where the model was loaded.
        /// </summary>
        public string Path
        {
            get { return path; }
            internal set { SetPropertyAsDirty(ref path, value); }
        }

        /// <summary>
        /// Dictionary containing the indices per mesh.
        /// </summary>
        public Dictionary<PTMesh, int[]> IndicesPerMesh
        {
            get { return indicesPerMesh; }
            internal set { SetPropertyAsDirty(ref indicesPerMesh, value); }
        }

        /// <summary>
        /// List with the meshes of the model.
        /// </summary>
        public List<PTMesh> Meshes
        {
            get { return meshes; }
            set { SetPropertyAsDirty(ref meshes, value); }
        }

        /// <summary>
        /// Get or set the bounding box surrounding this model.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
            set { SetProperty(ref boundingBox, value); }
        }

        /// <summary>
        /// Get or set the World matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }
            set { SetProperty(ref world, value); }
        }

        /// <summary>
        /// Get or set the factor for the texture uv repeat coordinates.
        /// Default: 1. (The texture is mapped to the 100% of the model).
        /// </summary>
        public float UVRepeatFactor
        {
            get { return uvRepeatFactor; }
            set { SetPropertyAsDirty(ref uvRepeatFactor, value); }
        }
        #endregion

        #region Events
        /// <inheritdoc/>
        public override event EventHandlers.OnInitializedHandler OnInitialized;
        /// <inheritdoc/>
        public override event EventHandlers.OnFinishLoadContentHandler OnFinishLoadContent;
        #endregion
        
        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="game">The game instance.</param>
        public PTModel(Game11 game)
            : base(game)
        {
            indicesPerMesh = new Dictionary<PTMesh, int[]>();
            meshes = new List<PTMesh>();
            importedMaterials = new List<PTMaterial>();
            Size = Vector3.One;
            Rotation = Vector3.Zero;
            Position = Vector3.Zero;
            Up = Vector3.Up;
            acceptInput = true;
            uvRepeatFactor = 1;
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();

            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        /// <summary>
        /// Set the material to be used for the model.
        /// The material is applied to all the meshes of the model.
        /// This method must be called when the <see cref="PTModel"/> has already finished loading so it has its meshes assigned.
        /// This can be typically be called in the <see cref="OnFinishLoadContent"/> event.
        /// </summary>
        /// <param name="materialName">The name of the material to be applied.</param>
        public void SetMaterial(string materialName)
        {
            foreach (PTMesh m in meshes)
                m.MaterialName = materialName;
        }

        /// <summary>
        /// Set the material to be used for the model.
        /// The material is applied to all the meshes of the model.
        /// This method must be called when the <see cref="PTModel"/> has already finished loading so it has its meshes assigned.
        /// This can be typically be called in the <see cref="OnFinishLoadContent"/> event.
        /// </summary>
        /// <param name="material">The instance of the material to be applied.</param>
        public void SetMaterial(PTMaterial material)
        {
            if (string.IsNullOrEmpty(material.Name))
                throw new ArgumentNullException("The material to be applied has no name.");
            SetMaterial(material.Name);
        }

        /// <summary>
        /// Set the material to be used for the model.
        /// If a mesh is not sent, the material will be applied to all the meshes.
        /// This method must be called when the <see cref="PTModel"/> has already finished loading so it has its meshes assigned.
        /// This can be typically be called in the <see cref="OnFinishLoadContent"/> event.
        /// </summary>
        /// <param name="materialName">The name of the material to be applied.</param>
        /// <param name="mesh">The optional mesh to which the material will be set. If this is set to null, the material applies to all the meshes in the model.</param>
        public void SetMaterial(string materialName, PTMesh mesh)
        {
            if (mesh != null)
            {
                mesh.MaterialName = materialName;
                return;
            }

            foreach (PTMesh m in meshes)
                m.MaterialName = materialName;
        }

        /// <summary>
        /// Set the material to be used for the model.
        /// If a mesh is not sent, the material will be applied to all the meshes.
        /// This method must be called when the <see cref="PTModel"/> has already finished loading so it has its meshes assigned.
        /// This can be typically be called in the <see cref="OnFinishLoadContent"/> event.
        /// </summary>
        /// <param name="material">The instance of the material to be applied.</param>
        /// <param name="mesh">The optional mesh to which the material will be set. If this is set to null, the material applies to all the meshes in the model.</param>
        public void SetMaterial(PTMaterial material, PTMesh mesh)
        {
            if (string.IsNullOrEmpty(material.Name))
                throw new ArgumentNullException("The material to be applied has no name.");
            SetMaterial(material.Name, mesh);
        }

        /// <summary>
        /// Set the material to be used for the model.
        /// If a mesh is not sent, the material will be applied to all the meshes.
        /// This method must be called when the <see cref="PTModel"/> has already finished loading so it has its meshes assigned.
        /// This can be typically be called in the <see cref="OnFinishLoadContent"/> event.
        /// </summary>
        /// <param name="materialName">The name of the material to be applied.</param>
        /// <param name="meshIndex">The optional index of the mesh to which the material will be set.</param>
        public void SetMaterial(string materialName, int meshIndex)
        {
            PTMesh mesh = meshes[meshIndex];
            if (mesh != null)
            {
                mesh.MaterialName = materialName;
                return;
            }

            foreach (PTMesh m in meshes)
                m.MaterialName = materialName;
        }

        /// <summary>
        /// Set the material to be used for the model.
        /// If a mesh is not sent, the material will be applied to all the meshes.
        /// This method must be called when the <see cref="PTModel"/> has already finished loading so it has its meshes assigned.
        /// This can be typically be called in the <see cref="OnFinishLoadContent"/> event.
        /// </summary>
        /// <param name="material">The instance of the material to be applied.</param>
        /// <param name="meshIndex">The optional index of the mesh to which the material will be set.</param>
        public void SetMaterial(Material material, int meshIndex)
        {
            if (string.IsNullOrEmpty(material.Name))
                throw new ArgumentNullException("The material to be applied has no name.");
            SetMaterial(material, meshIndex);
        }

        /// <summary>
        /// Set the effect and applies it for any <see cref="PTMesh"/> objects.
        /// </summary>
        /// <param name="effect"></param>
        public void SetEffect(PTForwardRenderEffect effect)
        {
            this.Effect = effect;
            foreach (PTMesh mesh in meshes)
            {
                mesh.Effect = effect;
            }
        }

        /// <summary>
        /// The Model base LoadContent method load the default basic shaders to draw with color, texture, etc.
        /// Also creates the Buffers to be sent to the shader.
        /// </summary>
        public override void LoadContent(ContentManager contentManager)
        {
            base.LoadContent(contentManager);

            for (int i = 0; i < meshes.Count; i++)
            {
                PTMesh mesh = meshes[i];
                mesh.LoadContent(contentManager);
            }

            if (!hasCalculatedTangentBinormals)
            {
                CalculateRenderingVectors();
                hasCalculatedTangentBinormals = true;
            }

            IsContentLoaded = true;
            OnFinishLoadContent?.Invoke();
        }

        /// <summary>
        /// Calculate the Tangent, Binormal and Normal vectors by triangle (polygon).
        /// </summary>
        public void CalculateRenderingVectors()
        {
            Log.Debug("Calculating Tangent and Binormals for -{0}-", this.Name);

            foreach (PTMesh mesh in meshes)
            {
                int index = 0;
                int[] indices = mesh.GetIndices();
                for (int i = 0; i < indices.Length / 3; i++)
                {
                    Vector3 tangent;
                    Vector3 binormal;
                    Vector3 normal;

                    VertexMainStruct vertex1 = mesh.Vertices[indices[i]];
                    index++;
                    VertexMainStruct vertex2 = mesh.Vertices[indices[i + 1]];
                    index++;
                    VertexMainStruct vertex3 = mesh.Vertices[indices[i + 2]];
                    index++;
                    CalculateTangentBinormal(vertex1, vertex2, vertex3, out tangent, out binormal);
                    CalculateNormal(tangent, binormal, out normal);

                    vertex1.Tangent = tangent;
                    vertex1.BiNormal = binormal;
                    vertex1.Normal = normal;
                    vertex2.Tangent = tangent;
                    vertex2.BiNormal = binormal;
                    vertex2.Normal = normal;
                    vertex3.Tangent = tangent;
                    vertex3.BiNormal = binormal;
                    vertex3.Normal = normal;

                    mesh.Vertices[indices[i]] = vertex1;
                    mesh.Vertices[indices[i + 1]] = vertex2;
                    mesh.Vertices[indices[i + 2]] = vertex3;

                    // Load again the Vertex buffer with the new vertices.
                    mesh.LoadContent(Game.ContentManager);
                }
            }
        }

        /// <summary>
        /// Calculate the Tangent and Binormal for 3 vertices (triangle).
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="vertex3"></param>
        /// <param name="tangent"></param>
        /// <param name="binormal"></param>
        public virtual void CalculateTangentBinormal(VertexMainStruct vertex1, VertexMainStruct vertex2, VertexMainStruct vertex3,
            out Vector3 tangent, out Vector3 binormal)
        {
            float[] vector1 = new float[3];
            float[] vector2 = new float[3];
            float[] tuVector = new float[2];
            float[] tvVector = new float[2];
            float den;
            float length;

            // Calculate the two vectors for this face.
            vector1[0] = vertex2.Position.X - vertex1.Position.X;
            vector1[1] = vertex2.Position.Y - vertex1.Position.Y;
            vector1[2] = vertex2.Position.Z - vertex1.Position.Z;

            vector2[0] = vertex3.Position.X - vertex1.Position.X;
            vector2[1] = vertex3.Position.Y - vertex1.Position.Y;
            vector2[2] = vertex3.Position.Z - vertex1.Position.Z;

            // Calculate the tu and tv texture space vectors.
            tuVector[0] = vertex2.TexCoord.X - vertex1.TexCoord.X;
            tvVector[0] = vertex2.TexCoord.Y - vertex1.TexCoord.Y;

            tuVector[1] = vertex3.TexCoord.X - vertex1.TexCoord.X;
            tvVector[1] = vertex3.TexCoord.Y - vertex1.TexCoord.Y;

            // Calculate the denominator of the tangent/binormal equation.
            den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

            // Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
            tangent.X = (tvVector[1] * vector1[0] - tvVector[0] * vector2[0]) * den;
            tangent.Y = (tvVector[1] * vector1[1] - tvVector[0] * vector2[1]) * den;
            tangent.Z = (tvVector[1] * vector1[2] - tvVector[0] * vector2[2]) * den;

            binormal.X = (tuVector[0] * vector2[0] - tuVector[1] * vector1[0]) * den;
            binormal.Y = (tuVector[0] * vector2[1] - tuVector[1] * vector1[1]) * den;
            binormal.Z = (tuVector[0] * vector2[2] - tuVector[1] * vector1[2]) * den;

            // Calculate the length of this normal.
            length = (float)Math.Sqrt((tangent.X * tangent.X) + (tangent.Y * tangent.Y) + (tangent.Z * tangent.Z));

            // Normalize the normal and then store it
            tangent.Z = tangent.X / length;
            tangent.Y = tangent.Y / length;
            tangent.Z = tangent.Z / length;

            // Calculate the length of this normal.
            length = (float)Math.Sqrt((binormal.X * binormal.X) + (binormal.Y * binormal.Y) + (binormal.Z * binormal.Z));

            // Normalize the normal and then store it
            binormal.X = binormal.X / length;
            binormal.Y = binormal.Y / length;
            binormal.Z = binormal.Z / length;
        }

        /// <summary>
        /// Calculate the Normal from the Tangent and Binormal.
        /// </summary>
        /// <param name="tangent"></param>
        /// <param name="binormal"></param>
        /// <param name="normal"></param>
        public virtual void CalculateNormal(Vector3 tangent, Vector3 binormal, out Vector3 normal)
        {
            float length;
            
            // Calculate the cross product of the tangent and binormal which will give the normal vector.
            normal.X = (tangent.Y * binormal.Z) - (tangent.Z * binormal.Y);
            normal.Y = (tangent.Z * binormal.X) - (tangent.X * binormal.Z);
            normal.Z = (tangent.X * binormal.Y) - (tangent.Y * binormal.X);

            // Calculate the length of the normal.
            length = (float)Math.Sqrt((normal.X * normal.X) + (normal.Y * normal.Y) + (normal.Z * normal.Z));

            // Normalize the normal.
            normal.X = normal.X / length;
            normal.Y = normal.Y / length;
            normal.Z = normal.Z / length;
        }

        /// <inheritdoc/>
        public override void UnloadContent()
        {
            
        }

        /// <summary>
        /// By default updates the Bounding Box with a general calculation algorithm.
        /// Override the <see cref="UpdateBoundingBox"/> method for more specific calculations.
        /// </summary>
        public override void UpdateLogic()
        {
            UpdateBoundingBox();
        }

        /// <summary>
        /// Update the model's BoundingBox taken from its position and size.
        /// </summary>
        public virtual void UpdateBoundingBox()
        {
            boundingBox = new BoundingBox(new Vector3(Position.X - Size.X / 2, Position.Y - Size.Y / 2, Position.Z + Size.Z / 2),
                    new Vector3(Position.X + Size.X / 2, Position.Y + Size.Y / 2, Position.Z - Size.Z / 2));
        }

        /// <summary>
        /// The base Model Render method by default send the transposed World-View-Projection matrices
        /// using the <see cref="DeviceContext.UpdateSubresource{T}(ref T, SharpDX.Direct3D11.Resource, int, int, int, ResourceRegion?)"/> method.
        /// The matrices multiplication must be made inside the shader.
        /// <para>It call last the <see cref="DeviceContext.Draw(int, int)"/> method.</para>
        /// </summary>
        public override void Render(SpriteBatch spriteBatch, SharpDX.Direct3D11.DeviceContext context = null)
        {
            foreach (PTMesh mesh in meshes)
                mesh.Render(spriteBatch, context);
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (IsStateUpdated)
                return IsStateUpdated;

            IsStateUpdated = true;
            OnStateUpdated();

            return IsStateUpdated;
        }

        /// <summary>
        /// Add a new <see cref="PTMesh"/> to the model.
        /// </summary>
        /// <param name="mesh">The mesh to be added.</param>
        public void AddMesh(PTMesh mesh)
        {
            if (Meshes.Contains(mesh))
                throw new DuplicateComponentException(ExceptionTexts.AddElement_AddedTwice);

            Meshes.Add(mesh);

            OnPropertyChanged(mesh, nameof(Meshes));
        }

        /// <summary>
        /// Get the vertices of all the meshes in the model.
        /// </summary>
        /// <returns></returns>
        public int[] GetIndices()
        {
            List<int> result = new List<int>();
            foreach (int[] indices in indicesPerMesh.Values)
                result.AddRange(indices);

            return result.ToArray();
        }

        /// <summary>
        /// Get the vertices of all the meshes in the model.
        /// </summary>
        /// <returns></returns>
        public List<VertexMainStruct> GetAllVertices()
        {
            List<VertexMainStruct> result = new List<VertexMainStruct>();
            for (int i = 0; i < meshes.Count; i++)
                result.AddRange(meshes[i].Vertices);

            return result;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            UnloadContent();
            
            foreach (PTMesh mesh in meshes)
                mesh.Dispose();
            foreach (PTMaterial mat in importedMaterials)
                mat.Dispose();

            importedMaterials.Clear();
            indicesPerMesh.Clear();
            indicesPerMesh = null;
            importedMaterials = null;
        }
        #endregion

        /// <summary>
        /// Convert a <see cref="Assimp.Scene"/> object to local <see cref="PoncheToolkit.Graphics3D.PTModel"/>
        /// TODO: It supports FOR NOW only one UV texture channel.
        /// </summary>
        /// <param name="scene">The object of the assimp scene.</param>
        /// <param name="model">The <see cref="PTModel"/> object passed as reference.</param>
        /// <param name="modelPath">The path where the model has been loaded.</param>
        /// <returns></returns>
        public static PTModel FromAssimpScene(ref Scene scene, ref PTModel model, string modelPath)
        {
            // Create relative path always.
            Uri uri = new Uri(System.IO.Path.GetDirectoryName(modelPath));
            Uri currentUri = new Uri(model.Game.ContentDirectoryFullPath);

            model.Path = currentUri.MakeRelativeUri(uri).ToString();
            scene.Meshes.ReadMeshes(ref model, ref scene);
            model.HasCalculatedTangentBinormals = true;
            //model.textures = scene.Textures.ConvertToLocal(ref model);

            return model;
        }

        /// <summary>
        /// Just return the result of the <see cref="object.MemberwiseClone"/> method.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
