using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util.Exceptions
{
    /// <summary>
    /// Exception thrown when a service of a type that already exists is added.
    /// </summary>
    public class DuplicateServiceException : PoncheException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        public DuplicateServiceException(string message) : base(message)
        {
            
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public DuplicateServiceException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
