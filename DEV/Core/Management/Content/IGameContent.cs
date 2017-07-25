using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Core.Management.Content
{
    /// <summary>
    /// Represent a piece of content that will be loaded into memory.
    /// </summary>
    public interface IGameContent : IDisposable, IConvertible
    {
    }
}
