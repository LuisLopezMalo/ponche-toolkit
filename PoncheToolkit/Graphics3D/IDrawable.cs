using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Interface that represent an object that can be rendered on screen.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// The render method.
        /// </summary>
        void Render();
    }
}
