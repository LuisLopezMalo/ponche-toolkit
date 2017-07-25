using Assimp;
using PoncheToolkit.Graphics2D;
using PoncheToolkit.Graphics3D;
using PoncheToolkit.Graphics3D.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management.Content
{
    /// <summary>
    /// Class that holds the contents that are loaded into memory just as <see cref="PTShader"/> or <see cref="PTTexture2D"/>.
    /// The <see cref="ContentManager11"/> will check for this objects to just instantiate one of each.
    /// </summary>
    public class ContentsPool
    {
        private static object lockObj = new object();

        #region Shaders
        private static Dictionary<string, PTShader> shadersInstance;

        /// <summary>
        /// Dictionary that store all the shaders that are currently loaded into memory.
        /// The key is the absolute path for the physical .fx file.
        /// </summary>
        internal static Dictionary<string, PTShader> Shaders
        {
            get
            {
                lock (lockObj)
                {
                    if (shadersInstance == null)
                        shadersInstance = new Dictionary<string, PTShader>();
                    return shadersInstance;
                }
            }
        }

        /// <summary>
        /// Add a shader to the <see cref="Shaders"/> dictionary.
        /// If the key is already added, it is replaced with the new shader.
        /// </summary>
        /// <param name="shader">The shader object.</param>
        /// <param name="key">The string key to add. Typically the path in lower.</param>
        public static void AddShader(PTShader shader, string key)
        {
            AddShader(shader, key, -1);
        }

        /// <summary>
        /// Add a shader to the <see cref="Shaders"/> dictionary.
        /// If the key is already added, it is replaced with the new shader.
        /// </summary>
        /// <param name="shader">The shader object.</param>
        /// <param name="key">The string key to add. Typically the path in lower.</param>
        /// <param name="customPixelShaderSlot">The custom pixel shader slot used to compile this shader.</param>
        public static void AddShader(PTShader shader, string key, int customPixelShaderSlot)
        {
            lock (lockObj)
            {
                string path = key + "_slot_" + customPixelShaderSlot;
                if (Shaders.ContainsKey(path))
                {
                    Shaders[path] = shader;
                    return;
                }

                Shaders.Add(path, shader);
            }
        }

        /// <summary>
        /// Get a shader from the <see cref="Shaders"/> dictionary.
        /// </summary>
        /// <param name="key">The string key. Typically the path in lower.</param>
        public static PTShader GetShader(string key)
        {
            return GetShader(key, -1);
        }

        /// <summary>
        /// Get a shader from the <see cref="Shaders"/> dictionary.
        /// </summary>
        /// <param name="key">The string key. Typically the path in lower.</param>
        /// <param name="customPixelShaderSlot">The custom pixel shader slot used to compile this shader.</param>
        public static PTShader GetShader(string key, int customPixelShaderSlot)
        {
            lock (lockObj)
            {
                string path = key + "_slot_" + customPixelShaderSlot;
                if (Shaders.ContainsKey(path))
                    return Shaders[path];

                return null;
            }
        }

        /// <summary>
        /// Dispose all the shaders and clear the Shaders dictionary.
        /// </summary>
        public static void ClearShader(string shaderKey)
        {
            lock (lockObj)
            {
                if (Shaders.ContainsKey(shaderKey.ToLower()))
                    Shaders[shaderKey.ToLower()].Dispose();
            }
        }

        /// <summary>
        /// Dispose all the shaders and clear the Shaders dictionary.
        /// </summary>
        public static void ClearShader(PTShader shader)
        {
            lock (lockObj)
            {
                List<PTShader> toDispose = new List<PTShader>();
                foreach (string path in shader.Paths.Values)
                {
                    if (Shaders.ContainsKey(path.ToLower()))
                        toDispose.Add(Shaders[path.ToLower()]);
                }

                for (int i = 0; i < toDispose.Count; i++)
                {
                    toDispose[i].Dispose();
                }
            }
        }

        /// <summary>
        /// Dispose all the shaders and clear the Shaders dictionary.
        /// </summary>
        public static void DisposeShaders()
        {
            lock (lockObj)
            {
                foreach (PTShader shader in Shaders.Values)
                    shader.Dispose();

                Shaders.Clear();
            }
        }
        #endregion

        #region Textures
        private static Dictionary<string, PTTexture2D> texturesInstance;

        /// <summary>
        /// Dictionary that store all the textures that are currently loaded into memory.
        /// The key is the absolute path for the physical texture file.
        /// </summary>
        public static Dictionary<string, PTTexture2D> Textures
        {
            get
            {
                lock (lockObj)
                {
                    if (texturesInstance == null)
                        texturesInstance = new Dictionary<string, PTTexture2D>();
                    return texturesInstance;
                }
            }
        }

        /// <summary>
        /// Dispose a given texture.
        /// </summary>
        public static bool ClearTexture(string textureKey)
        {
            lock (lockObj)
            {
                if (Textures.ContainsKey(textureKey.ToLower()))
                {
                    Textures[textureKey.ToLower()].Dispose();
                    Textures.Remove(textureKey.ToLower());
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Dispose all the textures and clear the Textures dictionary.
        /// </summary>
        public static void DisposeTextures()
        {
            lock (lockObj)
            {
                foreach (PTTexture2D texture in Textures.Values)
                    texture.Dispose();

                Textures.Clear();
            }
        }
        #endregion

        #region Models and Meshes
        private static Dictionary<string, KeyValuePair<PTModel, Scene>> importedModelsInstance;

        /// <summary>
        /// Dictionary that store all the models that are currently loaded into memory.
        /// The key is the absolute path for the physical Model file.
        /// </summary>
        public static Dictionary<string, KeyValuePair<PTModel, Scene>> ImportedModels
        {
            get
            {
                lock (lockObj)
                {
                    if (importedModelsInstance == null)
                        importedModelsInstance = new Dictionary<string, KeyValuePair<PTModel, Scene>>();
                    return importedModelsInstance;
                }
            }
        }

        /// <summary>
        /// Dispose a given model.
        /// </summary>
        public static void ClearModel(string modelPath)
        {
            lock (lockObj)
            {
                if (ImportedModels.ContainsKey(modelPath.ToLower()))
                {
                    KeyValuePair<PTModel, Scene> pair = ImportedModels[modelPath.ToLower()];
                    pair.Value.Clear();
                    pair.Key.Dispose();
                    ImportedModels.Remove(modelPath.ToLower());
                }
            }
        }

        /// <summary>
        /// Dispose all the models and clear the Models dictionary.
        /// </summary>
        public static void DisposeModels()
        {
            lock (lockObj)
            {
                foreach (string path in ImportedModels.Keys)
                    ClearModel(path);

                ImportedModels.Clear();
            }
        }
        #endregion
    }
}
