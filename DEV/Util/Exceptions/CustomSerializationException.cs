using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util.Exceptions
{
    /// <summary>
    /// Exception thrown when a specific content coult not be loaded into memory.
    /// </summary>
    public class CustomSerializationException : PoncheException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="formatValues"></param>
        public CustomSerializationException(string message, params string[] formatValues)
            : base(string.Format(message, formatValues))
        {
            
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="formatValues"></param>
        public CustomSerializationException(string message, Exception innerException, params string[] formatValues)
            : base(string.Format(message, formatValues), innerException)
        {
        }

        /// <inheritdoc/>
        public override string Message
        {
            get { return base.Message + " - " + Resources.ExceptionTexts.SerializationInformationMessage; }
        }
    }
}