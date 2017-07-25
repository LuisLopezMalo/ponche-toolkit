using PoncheToolkit.Util;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Graphics3D
{
    /// <summary>
    /// A mesh attachment store per-vertex animations for a particular frame. You may
    /// think of this as a 'patch' for the host mesh, since the mesh attachment replaces only certain
    /// vertex data streams at a particular time. Each mesh stores 'n' attached meshes. The actual
    /// relationship between the time line and mesh attachments is established by the mesh animation channel,
    /// which references singular mesh attachments by their ID and binds them to a time offset.
    /// </summary>
    public sealed class MeshAnimationAttachment : UpdatableStateObject
    {
        private List<Vector3> vertices;
        private List<Vector3> normals;
        private List<Vector3> tangents;
        private List<Vector3> bitangents;
        private List<Color>[] m_colors;
        private List<Vector3>[] m_texCoords;

        /// <summary>
        /// Gets the number of vertices in this mesh. This is a replacement
        /// for the host mesh's vertex count. Likewise, a mesh attachment
        /// cannot add or remove per-vertex attributes, therefore the existance
        /// of vertex data will match the existance of data in the mesh.
        /// </summary>
        public int VertexCount { get { return vertices.Count; } }

        /// <summary>
        /// Checks whether the attachment mesh overrides the vertex positions
        /// of its host mesh.
        /// </summary>
        public bool HasVertices { get { return vertices.Count > 0; } }

        /// <summary>
        /// Gets the vertex position list.
        /// </summary>
        public List<Vector3> Vertices { get { return vertices; } internal set { SetProperty(ref vertices, value); } }

        /// <summary>
        /// Checks whether the attachment mesh overrides the vertex normals of
        /// its host mesh.
        /// </summary>
        public bool HasNormals { get { return normals.Count > 0; } }

        /// <summary>
        /// Gets the vertex normal list.
        /// </summary>
        public List<Vector3> Normals { get { return normals; } internal set { SetProperty(ref normals, value); } }

        /// <summary>
        /// Checks whether the attachment mesh overrides the vertex
        /// tangents and bitangents of its host mesh.
        /// </summary>
        public bool HasTangentBasis { get { return tangents.Count > 0 && bitangents.Count > 0; } }

        /// <summary>
        /// Get the vertex tangent list.
        /// </summary>
        public List<Vector3> Tangents { get { return tangents; } internal set { SetProperty(ref tangents, value); } }

        /// <summary>
        /// Gets the vertex bitangent list.
        /// </summary>
        public List<Vector3> BiTangents { get { return bitangents; } internal set { SetProperty(ref bitangents, value); } }

        /// <summary>
        /// Gets the number of valid vertex color channels contained in the
        /// mesh (list is not empty/not null). This can be a value between zero and the maximum vertex color count. Each individual channel
        /// should be the size of <see cref="VertexCount"/>.
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
        /// Each individual channel should be the size of <see cref="VertexCount"/>.
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
        /// Gets the array that contains each vertex color channels that override a specific channel in the host mesh, by default all are lists of zero (but can be set to null). 
        /// Each index in the array corresponds to the texture coordinate channel. The length of the array corresponds to Assimp's maximum vertex color channel limit.
        /// </summary>
        public List<Color>[] VertexColorChannels { get { return m_colors; } internal set { SetProperty(ref m_colors, value); } }

        /// <summary>
        /// Gets the array that contains each texture coordinate channel that override a specific channel in the host mesh, by default all are lists of zero (but can be set to null).
        /// Each index in the array corresponds to the texture coordinate channel. The length of the array corresponds to Assimp's maximum UV channel limit.
        /// </summary>
        public List<Vector3>[] TextureCoordinateChannels { get { return m_texCoords; } internal set { SetProperty(ref m_texCoords, value); } }

        #region Initialization
        /// <summary>
        /// Constructs a new instance of the <see cref="MeshAnimationAttachment"/> class.
        /// </summary>
        public MeshAnimationAttachment()
        {
            vertices = new List<Vector3>();
            normals = new List<Vector3>();
            tangents = new List<Vector3>();
            bitangents = new List<Vector3>();
            m_colors = new List<Color>[AssimpConstants.AI_MAX_NUMBER_OF_COLOR_SETS];

            for (int i = 0; i < m_colors.Length; i++)
            {
                m_colors[i] = new List<Color>();
            }

            m_texCoords = new List<Vector3>[AssimpConstants.AI_MAX_NUMBER_OF_TEXTURECOORDS];

            for (int i = 0; i < m_texCoords.Length; i++)
            {
                m_texCoords[i] = new List<Vector3>();
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks if the mesh attachment overrides a particular set of vertex colors on
        /// the host mesh. This returns false if the list is null or empty. The index is between 
        /// zero and the maximumb number of vertex color channels.
        /// </summary>
        /// <param name="channelIndex">Channel index</param>
        /// <returns>True if vertex colors are present in the channel.</returns>
        public bool HasVertexColors(int channelIndex)
        {
            if (channelIndex >= m_colors.Length || channelIndex < 0)
                return false;

            List<Color> colors = m_colors[channelIndex];

            if (colors != null)
                return colors.Count > 0;

            return false;
        }

        /// <summary>
        /// Checks if the mesh attachment overrides a particular set of texture coordinates on
        /// the host mesh. This returns false if the list is null or empty. The index is 
        /// between zero and the maximum number of texture coordinate channels.
        /// </summary>
        /// <param name="channelIndex">Channel index</param>
        /// <returns>True if texture coordinates are present in the channel.</returns>
        public bool HasTextureCoords(int channelIndex)
        {
            if (channelIndex >= m_texCoords.Length || channelIndex < 0)
                return false;

            List<Vector3> texCoords = m_texCoords[channelIndex];

            if (texCoords != null)
                return texCoords.Count > 0;

            return false;
        }
        
        private Vector3[] CopyTo(List<Vector3> list, Vector3[] copy)
        {
            list.CopyTo(copy);
            return copy;
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        private void ClearBuffers()
        {
            vertices.Clear();
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
        }
        #endregion
    }
}