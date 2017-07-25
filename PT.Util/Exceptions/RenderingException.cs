using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Util.Exceptions
{
    /// <summary>
    /// Exception thrown when a device failed to initialize.
    /// </summary>
    public class RenderingException : PoncheException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="formatValues"></param>
        public RenderingException(string message, params string[] formatValues)
            : base(message, formatValues)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="formatValues"></param>
        public RenderingException(string message, Exception innerException, params string[] formatValues)
            : base(message, innerException, formatValues)
        {
        }
    }
}