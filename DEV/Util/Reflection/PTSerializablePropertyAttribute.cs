using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util.Reflection
{
    /// <summary>
    /// Main attribute to tell the engine that the public property or field will be enabled for serialization.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Parameter | System.AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PTSerializablePropertyAttribute : Attribute
    {
        /// <summary>
        /// Constrctor.
        /// </summary>
        public PTSerializablePropertyAttribute()
        {
        }
    }
}
