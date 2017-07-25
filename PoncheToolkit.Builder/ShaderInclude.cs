using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Builder
{
    public class ShaderInclude : Include
    {
        public IDisposable Shadow { get; set; }

        private string fullPath;

        public ShaderInclude(string fullPath)
        {
            this.fullPath = fullPath;
        }

        public void Close(Stream stream)
        {
            stream.Close();
        }

        public void Dispose()
        {
            this.Shadow.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            string path = Path.Combine(fullPath, fileName);
            return File.Open(path, FileMode.Open);
        }
    }
}
