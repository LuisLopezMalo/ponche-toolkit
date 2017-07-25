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
    /// Represents a single bone of a mesh. A bone has a name which allows it to be found in the frame
    /// hierarchy and by which it can be addressed by animations. In addition it has a number of
    /// influences on vertices.
    /// </summary>
    public sealed class PTBone : ModelPiece
    {
        private string name;
        private List<VertexWeight> weights;
        private Matrix offsetMatrix;

        /// <summary>
        /// Gets or sets the name of the bone.
        /// </summary>
        public string Name { get { return name; } set { SetProperty(ref name, value); } }

        /// <summary>
        /// Get the vertex weights owned by the bone.
        /// </summary>
        public List<VertexWeight> VertexWeights { get { return weights; } set { SetProperty(ref weights, value); } }

        /// <summary>
        /// Get or set the matrix that transforms from mesh space to bone space in bind pose.
        /// </summary>
        public Matrix OffsetMatrix { get { return offsetMatrix; } set { SetProperty(ref offsetMatrix, value); } }

        #region Initialization
        /// <summary>
        /// Constructs a new instance of the <see cref="PTBone"/> class.
        /// </summary>
        public PTBone()
        {
            name = null;
            offsetMatrix = Matrix.Identity;
            weights = new List<VertexWeight>();
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="PTBone"/> class.
        /// </summary>
        /// <param name="name">Name of the bone</param>
        /// <param name="offsetMatrix">Bone's offset matrix</param>
        /// <param name="weights">Vertex weights</param>
        public PTBone(String name, Matrix offsetMatrix, VertexWeight[] weights)
        {
            this.name = name;
            this.offsetMatrix = offsetMatrix;
            this.weights = new List<VertexWeight>();

            if (weights != null)
                this.weights.AddRange(weights);
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override bool UpdateState()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}