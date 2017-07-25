using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util.Exceptions
{
    /// <summary>
    /// The base custom exception.
    /// </summary>
    public class PoncheException : Exception, ILoggable
    {
        static string temp;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="formatValues"></param>
        public PoncheException(string message, params string[] formatValues) 
            : this(message, null, formatValues)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="formatValues"></param>
        public PoncheException(string message, Exception innerException, params string[] formatValues) 
            : base((temp = string.Format(message, formatValues)), innerException)
        {
            Log = new Logger(GetType());
            Log.Error(temp, this);
        }

        /// <inheritdoc/>
        public Logger Log { get; set; }
    }
}