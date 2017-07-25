using PoncheToolkit.Core.Management.Content;
using PoncheToolkit.Graphics3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core
{
    /// <summary>
    /// Interface to set the LoadShaders method.
    /// </summary>
    public interface IShaderLoadable
    {
        /// <summary>
        /// Method to load the shaders of the component.
        /// Is called before the <see cref="PTModel.LoadContent"/>, so the LoadContent method has the
        /// shaders objects already initialized.
        /// </summary>
        /// <param name="contentManager">The content manager instance to load the shaders.</param>
        void LoadShadersAndMaterials(IContentManager contentManager);
    }
}
