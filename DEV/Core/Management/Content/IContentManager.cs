using PoncheToolkit.Core.Services;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Effects;
using PoncheToolkit.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management.Content
{
    /// <summary>
    /// Main interface to implement to have content loading and management.
    /// </summary>
    public interface IContentManager : IGameService
    {
        /// <summary>
        /// Load in memory a texture2D.
        /// This texture will be rendered using the back buffer. (<see cref="IGraphicsRenderer.Context2D"/>)
        /// </summary>
        /// <param name="assetName">The local path and name of the texture.</param>
        /// <param name="generateMipMaps">If the texture will generate mip map chain.</param>
        /// <param name="referencePath">A path for reference to combine with the assetName.</param>
        /// <returns></returns>
        Graphics2D.PTTexture2D LoadTexture2D(string assetName, string referencePath = null, bool generateMipMaps = false);

        /// <summary>
        /// Load in memory a texture2D.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="context">The render target where the texture will be drawn.</param>
        /// <param name="referencePath">A path for reference to combine with the assetName.</param>
        /// <param name="generateMipMaps">If the texture will generate mip maps.</param>
        /// <returns></returns>
        Graphics2D.PTTexture2D LoadTexture2D(string assetName, SharpDX.Direct2D1.DeviceContext context, string referencePath = null, bool generateMipMaps = false);

        /// <summary>
        /// Load in memory a texture2D.
        /// </summary>
        /// <param name="texturePaths"></param>
        /// <returns></returns>
        //public Graphics2D.PTTexture2D LoadTexture2DArray(string[] assetNames)
        Graphics2D.PTTexture2D LoadTexture2DArray(List<PTTexturePath> texturePaths);

        /// <summary>
        /// Load in memory a 3D model.
        /// The supported types are defined in the <see cref="VALID_EXTENSIONS"/> property.
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        PTModel LoadModel(string assetName);

        /// <summary>
        /// Load in memory a shader (.fx) file.
        /// This method calls the <see cref="IPTEffect.LoadContent"/> method, so if you want to inherit from <see cref="IPTEffect"/>
        /// to create your own shader, you must instance the new Effect and call the <see cref="IPTEffect.LoadContent"/> method manually.
        /// </summary>
        /// <param name="effectPath">The path of the .fx file</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <returns>The <see cref="PoncheToolkit.Graphics3D.Effects.PTEffect"/> object.</returns>
        T LoadEffect<T>(string effectPath, List<string> includePaths) where T : IPTEffect;

        /// <summary>
        /// Load into memory an object of any supported type from the source code.
        /// ** It works only for Shaders for now. **
        /// </summary>
        /// <param name="source">The complete source code to be compiled.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="saveCompiledFileName">Optional file name to save the compiled shader to.</param>
        /// <returns>The object loaded in memory.</returns>
        /// <exception cref = "ResourceNotSupportedException" > When the resource is not found.</exception>
        PTShader LoadShaderFromSource(string source, List<string> includePaths, string saveCompiledFileName = null);

        #region Load Shader
        /// <summary>
        /// <para>
        /// Load in memory a shader (.fx) file. This method return a <see cref="PTShader"/> compiled file.
        /// It does not return an <see cref="PoncheToolkit.Graphics3D.Effects.PTEffect"/> file, for that use the <see cref="LoadEffect{T}(string, List{string})"/> method.
        /// Check in the <see cref="ContentsPool"/> if the Shader has already been loaded previously.
        /// </para>
        /// It also saves the compiled shaders into a physical file.
        /// </summary>
        /// <param name="assetName">The name of the .fx file</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="vertexShaderEntry">The main entry method for the vertex shader.</param>
        /// <param name="pixelShaderEntry">The main entry method for the pixel shader.</param>
        /// <returns></returns>
        PTShader LoadShader(string assetName, List<string> includePaths, string vertexShaderEntry = "VertexShaderEntry", string pixelShaderEntry = "PixelShaderEntry");

        /// <summary>
        /// <para>
        /// Load in memory a shader (.fx) file. This method return a <see cref="PTShader"/> compiled file.
        /// It does not return an <see cref="PoncheToolkit.Graphics3D.Effects.PTEffect"/> file, for that use the <see cref="LoadEffect{T}(string, List{string})"/> method.
        /// Check in the <see cref="ContentsPool"/> if the Shader has already been loaded previously.
        /// </para>
        /// It also saves the compiled shaders into a physical file.
        /// </summary>
        /// <param name="assetName">The name of the .fx file</param>
        /// <param name="referenceShader">The <see cref="PTShader"/> object where the vertexShader will be loaded into.</param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="vertexShaderEntry">The main entry method for the vertex shader.</param>
        /// <returns></returns>
        void LoadVertexShaderInto(string assetName, ref PTShader referenceShader, List<string> includePaths, string vertexShaderEntry = "VertexShaderEntry");

        /// <summary>
        /// <para>
        /// Load in memory a shader (.fx) file. This method return a <see cref="PTShader"/> compiled file.
        /// It does not return an <see cref="PoncheToolkit.Graphics3D.Effects.PTEffect"/> file, for that use the <see cref="LoadEffect{T}(string, List{string})"/> method.
        /// Check in the <see cref="ContentsPool"/> if the Shader has already been loaded previously.
        /// </para>
        /// It also saves the compiled shaders into a physical file.
        /// </summary>
        /// <param name="assetName">The name of the .fx file</param>
        /// <param name="referenceShader">The <see cref="PTShader"/> object where the pixelShader will be loaded into.</param>
        /// <param name="customPixelShaderSlot">If this is a custom shader (Not the 'Linking' shaders), tell which slot will be used. The Engine support up to 10 custom shaders.
        /// <para>The custom shader must be added inside the 'Content/Effects' folder and must be called: 'Custom_ShaderPS0.fx', the 0 means the slot used.
        /// If the default shaders want to be used, use -1 as value.</para>
        /// </param>
        /// <param name="includePaths">The list of paths after the "Content" directory, to take in consideration for including when compiling.</param>
        /// <param name="pixelShaderEntry">The main entry method for the pixel shader.</param>
        /// <returns></returns>
        void LoadPixelShaderInto(string assetName, ref PTShader referenceShader, int customPixelShaderSlot, List<string> includePaths, string pixelShaderEntry = "PixelShaderEntry");
        #endregion
    }
}
