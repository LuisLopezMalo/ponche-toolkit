using PoncheToolkit.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D.Effects
{
    /// <summary>
    /// The collection of parameters that will be passed to the shader.
    /// </summary>
    public class ShaderParameters : IDictionary<string, object>, ILoggable
    {
        private Dictionary<string, object> instance;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ShaderParameters()
        {
            instance = new Dictionary<string, object>();
            Log = new Logger(GetType());
        }

        /// <inheritdoc/>
        public object this[string key]
        {
            get { return instance[key.ToLower()]; } 
            set { instance[key.ToLower()] = value; }
        }

        /// <inheritdoc/>
        public int Count { get { return instance.Count; } }

        /// <inheritdoc/>
        public bool IsReadOnly { get { return false; } }

        /// <inheritdoc/>
        public ICollection<string> Keys { get { return instance.Keys; } }

        /// <inheritdoc/>
        public Logger Log { get; set; }

        /// <inheritdoc/>
        public ICollection<object> Values { get { return instance.Values; } }

        /// <inheritdoc/>
        public void Add(KeyValuePair<string, object> item)
        {
            instance.Add(item.Key.ToLower(), item.Value);
        }

        /// <inheritdoc/>
        public void Add(string key, object value)
        {
            instance.Add(key.ToLower(), value);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            instance.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return instance.Contains(item);
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return instance.ContainsKey(key.ToLower());
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return instance.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<string, object> item)
        {
            return instance.Remove(item.Key.ToLower());
        }

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            return instance.Remove(key.ToLower());
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, out object value)
        {
            try
            {
                value = instance[key.ToLower()];
                return true;
            }catch (Exception ex)
            {
                Log.Error("Error trying to get value -{0}- from ShaderParameters.", ex, key);
            }

            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return instance.GetEnumerator();
        }
    }
}
