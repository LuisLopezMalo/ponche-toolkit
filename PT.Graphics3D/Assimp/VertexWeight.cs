using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PT.Graphics3D
{
    /// <summary>
    /// Represents a single influence of a bone on a vertex.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexWeight
    {
        /// <summary>
        /// Index of the vertex which is influenced by the bone.
        /// </summary>
        public int VertexID;

        /// <summary>
        /// Strength of the influence in range of (0...1). All influences
        /// from all bones at one vertex amounts to 1.
        /// </summary>
        public float Weight;

        /// <summary>
        /// Constructs a new VertexWeight.
        /// </summary>
        /// <param name="vertID">Index of the vertex.</param>
        /// <param name="weight">Weight of the influence.</param>
        public VertexWeight(int vertID, float weight)
        {
            VertexID = vertID;
            Weight = weight;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            CultureInfo info = CultureInfo.CurrentCulture;
            return string.Format(info, "{{VertexID:{0} Weight:{1}}}",
                new object[] { VertexID.ToString(info), Weight.ToString(info) });
        }

        #region Operators
        /// <summary>
        /// Implicitly convert a <see cref="Assimp.VertexWeight"/> object to local <see cref="PoncheToolkit.Graphics3D.VertexWeight"/>
        /// </summary>
        /// <param name="weight"></param>
        public static implicit operator VertexWeight(Assimp.VertexWeight weight)
        {
            VertexWeight result = weight.ConvertToLocal();
            return result;
        }
        #endregion
    }
}
