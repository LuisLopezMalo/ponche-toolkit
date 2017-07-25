using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Util.Exceptions
{
    /// <summary>
    /// Exception thrown when a resource as failed to compile.
    /// </summary>
    public class ResourceNotSupportedException : PoncheException
    {
        /// <summary>
        /// Constuctor.
        /// </summary>
        /// <param name="message"></param>
        public ResourceNotSupportedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ResourceNotSupportedException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}