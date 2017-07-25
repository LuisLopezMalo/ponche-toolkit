using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util.Exceptions
{
    /// <summary>
    /// Exception thrown when a device failed to initialize.
    /// </summary>
    public class ComponentNotInitializedException : PoncheException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        public ComponentNotInitializedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ComponentNotInitializedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}