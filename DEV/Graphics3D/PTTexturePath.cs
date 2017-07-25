using PoncheToolkit.Graphics2D;
using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Class that represent a texture path.
    /// This path will be managed from the <see cref="PoncheToolkit.Core.Game"/> class
    /// when its UpdateState method is called.
    /// </summary>
    public class PTTexturePath : UpdatableStateObject
    {
        private string path;
        private bool generateMipMaps;
        private PTTexture2D.TextureType type;

        /// <summary>
        /// Path of the texture to load.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public string Path
        {
            get { return path; }
            set { SetPropertyAsDirty(ref path, value); }
        }

        /// <summary>
        /// If the texture this path represents will generate mip maps.
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
        public bool GenerateMipMaps
        {
            get { return generateMipMaps; }
            set { SetPropertyAsDirty(ref generateMipMaps, value); }
        }

        /// <summary>
        /// Type of texture this path represents.
        /// Default: <see cref="PTTexture2D.TextureType.Render"/>
        /// </summary>
        [Util.Reflection.PTSerializableProperty]
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
        /// <param name="generateMipMaps">If the texture this path represents will generate mip maps.</param>
        public PTTexturePath(string path, PTTexture2D.TextureType type, bool generateMipMaps)
        {
            this.path = path;
            this.type = type;
            this.generateMipMaps = generateMipMaps;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">The path of the texture</param>
        /// <param name="type">The type of texture.</param>
        public PTTexturePath(string path, PTTexture2D.TextureType type)
            : this(path, type, false)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">The path of the texture</param>
        /// <param name="generateMipMaps">If the texture this path represents will generate mip maps.</param>
        public PTTexturePath(string path, bool generateMipMaps)
            : this(path, PTTexture2D.TextureType.Render, generateMipMaps)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">The path of the texture</param>
        public PTTexturePath(string path)
            : this(path, PTTexture2D.TextureType.Render)
        {
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
    }
}
