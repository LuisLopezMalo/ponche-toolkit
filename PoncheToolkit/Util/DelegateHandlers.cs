using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Util
{
    public class DelegateHandlers
    {
        public delegate void IUpdatableStateOnValueChangedHandler(string propertyName, object value);
    }
}
