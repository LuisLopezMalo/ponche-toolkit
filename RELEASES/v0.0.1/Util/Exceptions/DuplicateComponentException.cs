using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util.Exceptions
{
    /// <summary>
    /// Exception thrown when a name component that already exists is added.
    /// </summary>
    public class DuplicateComponentException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        public DuplicateComponentException(string message) : base(message)
        {
            
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public DuplicateComponentException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
