using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PoncheToolkit.Core.Management.Content
{
    /// <summary>
    /// Class used to include files from the #include directive in hlsl files.
    /// The path sent to the constructor is after the "Content" directory, meaning that if the file is in "Content/Effect"
    /// directory, there is only need to send "Effects" as parameter.
    /// </summary>
    public class PTInclude : Include
    {
        /// <inheritdoc/>
        public IDisposable Shadow { get; set; }

        private List<string> relativePathsFromContentDirectory;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="relativePathsFromContentDirectory">The list of paths to be included when reading the shader.</param>
        public PTInclude(List<string> relativePathsFromContentDirectory)
        {
            this.relativePathsFromContentDirectory = relativePathsFromContentDirectory;
        }

        /// <inheritdoc/>
        public void Close(Stream stream)
        {
            stream.Close();
        }

        /// <inheritdoc/>
        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            foreach (string p in relativePathsFromContentDirectory)
            {
                string path = Path.Combine(Game.Instance.ContentDirectoryFullPath, p, fileName);
                if (File.Exists(path))
                    return File.Open(path, FileMode.Open);
            }

            return null;

            //string path = Path.Combine(Game.Instance.ContentDirectoryFullPath, relativePathFromContentDirectory, fileName);

        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Shadow.Dispose();
        }
    }
}
