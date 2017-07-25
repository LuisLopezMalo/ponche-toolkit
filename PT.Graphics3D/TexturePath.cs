using PoncheToolkit.Graphics2D;
using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Graphics3D
{
    /// <summary>
    /// Class that represent a texture path.
    /// This path will be managed from the <see cref="PoncheToolkit.Core.Game11"/> class
    /// when its UpdateState method is called.
    /// </summary>
    public class TexturePath : UpdatableStateObject
    {
        private string path;
        //private int slot;
        private PTTexture2D.TextureType type;

        /// <summary>
        /// Path of the texture to load.
        /// </summary>
        public string Path
        {
            get { return path; }
            set { SetPropertyAsDirty(ref path, value); }
        }

        /// <summary>
        /// Type of texture this path represents.
        /// Default: <see cref="PTTexture2D.TextureType.Render"/>
        /// </summary>
        public PTTexture2D.TextureType Type
        {
            get { return type; }
            set { SetPropertyAsDirty(ref type, value); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">The path of the texture</param>
        /// <param name="type">The type of texture.</param>
        ///// <param name="slot">Index of the texture to add this path.</param>
        public TexturePath(string path, PTTexture2D.TextureType type)
        {
            this.path = path;
            this.type = type;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">The path of the texture</param>
        public TexturePath(string path)
            : this(path, PTTexture2D.TextureType.Render)
        {
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
    }
}
