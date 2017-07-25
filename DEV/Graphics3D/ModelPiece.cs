using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Represent a part of a model, like a face, a mesh, etc.
    /// </summary>
    public abstract class ModelPiece : UpdatableStateObject
    {
        private PTModel model;

        /// <summary>
        /// The model to which this piece belongs to.
        /// </summary>
        public PTModel Model
        {
            get { return model; }
            set { SetProperty(ref model, value); }
        }
    }
}
