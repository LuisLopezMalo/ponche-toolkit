using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Graphics3D
{
    using PoncheToolkit.Util;

    /// <summary>
    /// Describes all the values pertaining to a particular texture slot in a material.
    /// </summary>
    [Serializable]
    internal struct MaterialTexture
    {
        /// <summary>
        /// Gets the texture file path.
        /// </summary>
        public string FilePath;

        /// <summary>
        /// Gets the texture type semantic.
        /// </summary>
        public AssimpTextureType TextureType;

        /// <summary>
        /// Gets the texture index in the material.
        /// </summary>
        public int TextureIndex;

        /// <summary>
        /// Gets the texture mapping.
        /// </summary>
        public AssimpTextureMapping Mapping;

        /// <summary>
        /// Gets the UV channel index that corresponds to this texture from the mesh.
        /// </summary>
        public int UVIndex;

        /// <summary>
        /// Gets the blend factor.
        /// </summary>
        public float BlendFactor;

        /// <summary>
        /// Gets the texture operation.
        /// </summary>
        public assimpTextureOperation Operation;

        /// <summary>
        /// Gets the texture wrap mode for the U coordinate.
        /// </summary>
        public AssimpTextureWrapMode WrapModeU;

        /// <summary>
        /// Gets the texture wrap mode for the V coordinate.
        /// </summary>
        public AssimpTextureWrapMode WrapModeV;

        /// <summary>
        /// Gets misc flags.
        /// </summary>
        public int Flags;

        /// <summary>
        /// Constructs a new TextureSlot.
        /// </summary>
        /// <param name="filePath">Texture filepath</param>
        /// <param name="typeSemantic">Texture type semantic</param>
        /// <param name="texIndex">Texture index in the material</param>
        /// <param name="mapping">Texture mapping</param>
        /// <param name="uvIndex">UV channel in mesh that corresponds to this texture</param>
        /// <param name="blendFactor">Blend factor</param>
        /// <param name="texOp">Texture operation</param>
        /// <param name="wrapModeU">Texture wrap mode for U coordinate</param>
        /// <param name="wrapModeV">Texture wrap mode for V coordinate</param>
        /// <param name="flags">Misc flags</param>
        public MaterialTexture(string filePath, AssimpTextureType typeSemantic, int texIndex, AssimpTextureMapping mapping, int uvIndex, float blendFactor,
            assimpTextureOperation texOp, AssimpTextureWrapMode wrapModeU, AssimpTextureWrapMode wrapModeV, int flags)
        {
            FilePath = (filePath == null) ? string.Empty : filePath;
            TextureType = typeSemantic;
            TextureIndex = texIndex;
            Mapping = mapping;
            UVIndex = uvIndex;
            BlendFactor = blendFactor;
            Operation = texOp;
            WrapModeU = wrapModeU;
            WrapModeV = wrapModeV;
            Flags = flags;
        }
    }
}
