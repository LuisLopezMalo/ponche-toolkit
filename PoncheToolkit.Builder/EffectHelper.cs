using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Builder
{
    internal class EffectHelper
    {
        public enum Commands
        {
            InputPath,
            IncludePath,
            OutputPath,
            Filename,
            VertexShaderEntry,
            PixelShaderEntry,
            ShaderVersion
        }

        /// <summary>
        /// The name of the compiled vertex shader file.
        /// </summary>
        public static readonly string VERTEX_SHADER_COMPILED_NAME_EXTENSION = "_Vertex.ptfx";
        /// <summary>
        /// The name of the compiled pixel shader file.
        /// </summary>
        public static readonly string PIXEL_SHADER_COMPILED_NAME_EXTENSION = "_Pixel.ptfx";
        /// <summary>
        /// The name of the compiled tesselation shader file.
        /// </summary>
        public static readonly string TESSELATION_SHADER_COMPILED_NAME_EXTENSION = "_Tesselation.ptfx";
        /// <summary>
        /// The name of the compiled geometry shader file.
        /// </summary>
        public static readonly string GEOMETRY_SHADER_COMPILED_NAME_EXTENSION = "_Geometry.ptfx";

        //private static Logger Log = new Logger(typeof(EffectHelper));

        private static Dictionary<string, Commands> commandsDictionary = new Dictionary<string, Commands>()
        {
            { "-i", Commands.InputPath },
            { "-in", Commands.IncludePath },
            { "-o", Commands.OutputPath },
            { "-f", Commands.Filename },
            { "-ve", Commands.VertexShaderEntry },
            { "-pe", Commands.PixelShaderEntry },
        };

        private Dictionary<Commands, string> parsedArguments;
        public void ParseArguments(string[] args)
        {
            parsedArguments = new Dictionary<Commands, string>();

            // Default parameters
            parsedArguments.Add(Commands.ShaderVersion, "5");
            parsedArguments.Add(Commands.VertexShaderEntry, "VertexShaderEntry");
            parsedArguments.Add(Commands.PixelShaderEntry, "PixelShaderEntry");
            parsedArguments.Add(Commands.InputPath, ".");
            parsedArguments.Add(Commands.IncludePath, ".");
            parsedArguments.Add(Commands.OutputPath, ".");
            parsedArguments.Add(Commands.Filename, "");

            for (int i = 0; i<args.Length; i++)
            {
                if (commandsDictionary.ContainsKey(args[i].ToLower()))
                    parsedArguments[commandsDictionary[args[i].ToLower()]] = args[i + 1];
            }
        }

        #region Compile Shaders
        /// <summary>
        /// Compile a Vertex Shader.
        /// </summary>
        /// <param name="assetNameOrSource">The path of the effect.</param>
        /// <param name="entryPoint">String of the name of the shader method to work as the vertex shader entry point.</param>
        /// <param name="signature">Set the ShaderSignature.</param>
        /// <param name="fromSource">Value to indicate what the assetNameOrSource represent to search a file or compile the sent code.</param>
        /// <param name="saveFileName">Optional file name to save the compiled shader to.</param>
        /// <returns>Return the VertexShader object of the compiled shader.</returns>
        /// <exception cref="PoncheToolkit.Util.Exceptions.ResourceCompilationException"/>
        public bool CompileVertexShader(string assetNameOrSource, string entryPoint, bool fromSource = false, string saveFileName = null)
        {
            ShaderFlags flags = ShaderFlags.None;
#if DEBUG
            flags |= ShaderFlags.Debug;
#endif

            if (fromSource)
            {
                using (var compiledVertexShader = ShaderBytecode.Compile(assetNameOrSource, entryPoint, getVersionName(parsedArguments[Commands.ShaderVersion], "vs"), flags))
                {
                    if (compiledVertexShader.Bytecode == null || compiledVertexShader.HasErrors)
                    {
                        Console.Error.WriteLine("Error compiling vertex shader. - " + compiledVertexShader.Message);
                        return false;
                    }

                    saveShader(saveFileName, compiledVertexShader, VERTEX_SHADER_COMPILED_NAME_EXTENSION);
                }
                return true;
            }

            // Compile Vertex Shader
            using (var compiledVertexShader = ShaderBytecode.CompileFromFile(assetNameOrSource, entryPoint, getVersionName(parsedArguments[Commands.ShaderVersion], "vs"), flags, include: new ShaderInclude(parsedArguments[Commands.IncludePath])))
            {
                if (compiledVertexShader.Bytecode == null || compiledVertexShader.HasErrors)
                {
                    Console.Error.WriteLine("Error compiling vertex shader. - " + compiledVertexShader.Message);
                    return false;
                }

                saveShader(saveFileName, compiledVertexShader, VERTEX_SHADER_COMPILED_NAME_EXTENSION);
                return true;
            }
        }

        /// <summary>
        /// Compile a pixel shader.
        /// </summary>
        /// <param name="assetNameOrSource">The path of the effect.</param>
        /// <param name="entryPoint">String of the name of the shader method to work as the pixel shader entry point.</param>
        /// <param name="fromSource">Value to indicate what the assetNameOrSource represent to search a file or compile the sent code.</param>
        /// <param name="saveFileName">Optional file name to save the compiled shader to.</param>
        /// <returns>Return the PixelShader object of the compiled shader.</returns>
        /// <exception cref="PoncheToolkit.Util.Exceptions.ResourceCompilationException"/>
        public bool CompilePixelShader(string assetNameOrSource, string entryPoint, bool fromSource = false, string saveFileName = null)
        {
            ShaderFlags flags = ShaderFlags.None;
#if DEBUG
            flags |= ShaderFlags.Debug;
#endif

            // Compile Pixel shader from source
            if (fromSource)
            {
                using (var compiledPixelShader = ShaderBytecode.Compile(assetNameOrSource, entryPoint, getVersionName(parsedArguments[Commands.ShaderVersion], "ps"), flags))
                {
                    if (compiledPixelShader.Bytecode == null || compiledPixelShader.HasErrors)
                    {
                        Console.Error.WriteLine("Error compiling vertex shader. - " + compiledPixelShader.Message);
                        return false;
                    }

                    saveShader(saveFileName, compiledPixelShader, PIXEL_SHADER_COMPILED_NAME_EXTENSION);
                }

                return true;
            }

            // Compile Pixel shader from file
            using (var compiledPixelShader = ShaderBytecode.CompileFromFile(assetNameOrSource, entryPoint, getVersionName(parsedArguments[Commands.ShaderVersion], "ps"), flags, include: new ShaderInclude(parsedArguments[Commands.IncludePath])))
            {
                if (compiledPixelShader.Bytecode == null || compiledPixelShader.HasErrors)
                {
                    Console.Error.WriteLine("Error compiling vertex shader. - " + compiledPixelShader.Message);
                    return false;
                }

                saveShader(saveFileName, compiledPixelShader, PIXEL_SHADER_COMPILED_NAME_EXTENSION);
                return true;
            }
        }

        /// <summary>
        /// <para>
        /// Load in memory a shader (.fx) file. This method return a <see cref="PTShader"/> compiled file.
        /// It does not return an <see cref="PoncheToolkit.Graphics3D.Effects.PTEffect"/> file, for that use the <see cref="LoadEffect(string)"/> method.
        /// Check in the <see cref="ContentsPool"/> if the Shader has already been loaded previously.
        /// </para>
        /// It also saves the compiled shaders into a physical file.
        /// </summary>
        /// <returns></returns>
        internal bool CompileShader()
        {
            try
            {
                // If no filename was given, the whole directory will be compiled.
                if (string.IsNullOrEmpty(parsedArguments[Commands.Filename]))
                {
                    string[] filenames = Directory.EnumerateFiles(parsedArguments[Commands.InputPath]).ToArray();
                    for (int i = 0; i < filenames.Length; i++)
                    {
                        string contentPath = Path.GetFullPath(Path.Combine(parsedArguments[Commands.InputPath], Path.GetFileName(filenames[i])));
                        CompileVertexShader(contentPath, parsedArguments[Commands.VertexShaderEntry], saveFileName: Path.GetFileName(filenames[i]));
                        CompilePixelShader(contentPath, parsedArguments[Commands.PixelShaderEntry], saveFileName: Path.GetFileName(filenames[i]));
                    }
                }
                else
                {
                    string contentPath = fileValidations();
                    CompileVertexShader(contentPath, parsedArguments[Commands.VertexShaderEntry], saveFileName: parsedArguments[Commands.Filename]);
                    CompilePixelShader(contentPath, parsedArguments[Commands.PixelShaderEntry], saveFileName: parsedArguments[Commands.Filename]);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error compiling shader.", ex);
                return false;
            }

            Console.WriteLine("Loaded Shader from path: -{0}-", parsedArguments[Commands.Filename]);

            return true;
        }
        #endregion

        /// <summary>
        /// Make validations of the file.
        /// </summary>
        /// <param name="assetName">Relative path of the file.</param>
        /// <param name="referencePath">A path for reference to combine with the assetName.</param>
        /// <returns>Return the correct path of the file with the ContentManager root directory.</returns>
        /// <exception cref="FileNotFoundException">When the resource is not found.</exception>
        /// <exception cref="ResourceNotSupportedException">When the resource is not found.</exception>
        private string fileValidations()
        {
            string contentPath = Path.GetFullPath(Path.Combine(parsedArguments[Commands.InputPath], parsedArguments[Commands.Filename]));
            if (!File.Exists(contentPath))
            {
                Console.Error.WriteLine(string.Format("The resource -{0}- was not found.", contentPath));
                throw new FileNotFoundException(string.Format("The resource -{0}- was not found.", contentPath));
            }

            return contentPath;
        }

        private string getVersionName(string shaderVersion, string prefix)
        {
            return prefix + "_" + shaderVersion + "_0";
        }

        private void saveShader(string fileRelativePath, CompilationResult compiledShader, string typeWithExtension)
        {
            if (!string.IsNullOrEmpty(fileRelativePath))
            {
                string path = Path.Combine(parsedArguments[Commands.OutputPath], System.IO.Path.GetDirectoryName(fileRelativePath));
                string fileName = Path.GetFileNameWithoutExtension(fileRelativePath);
                using (FileStream stream = File.Create(Path.Combine(path, fileName + typeWithExtension)))
                {
                    compiledShader.Bytecode.Save(stream);
                }
            }
        }
    }
}
