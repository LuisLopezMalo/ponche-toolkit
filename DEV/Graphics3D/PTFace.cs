using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// A single face in a mesh.
    /// It can represent a triangle or polygon depending of how many vertex the face has.
    /// </summary>
    public sealed class PTFace : ModelPiece
    {
        private List<int> m_indices;

        /// <summary>
        /// Gets the number of indices defined in the face.
        /// </summary>
        public int IndexCount { get { return m_indices.Count; } }

        /// <summary>
        /// Gets or sets the indices that refer to positions of vertex data in the mesh's vertex 
        /// arrays.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public List<int> Indices
        {
            get { return m_indices; }
            internal set
            {
                m_indices = new List<int>();
                if (value != null)
                    m_indices.AddRange(value);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PTFace()
        {
            m_indices = new List<int>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="indices">Face indices</param>
        public PTFace(int[] indices)
        {
            m_indices = new List<int>();

            if (indices != null)
                m_indices.AddRange(indices);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="indices">Face indices</param>
        public PTFace(List<int> indices)
        {
            m_indices = new List<int>();

            if (indices != null)
                m_indices.AddRange(indices);
        }

        /// <inheritdoc/>
        public override bool UpdateState()
        {
            throw new NotImplementedException();
        }
    }
}
