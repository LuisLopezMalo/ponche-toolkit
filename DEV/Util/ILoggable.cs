using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util
{
    /// <summary>
    /// Interface that contains an instance of the Logger.
    /// It can be instantiated at any point.
    /// </summary>
    public interface ILoggable
    {
        /// <summary>
        /// The custom logger.
        /// Get the configuration from the NLog.config file.
        /// </summary>
        Logger Log { get; set; }
    }
}
