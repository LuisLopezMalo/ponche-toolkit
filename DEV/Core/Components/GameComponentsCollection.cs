using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoncheToolkit.Util.Exceptions;
using PoncheToolkit.Graphics3D.Cameras;

namespace PoncheToolkit.Core.Components
{
    /// <summary>
    /// Sorted Dictionary that contain components to be updated.
    /// <para>The key is the name of the component. The name can be set to the component before calling <see cref="AddComponent(IGameComponent)"/>
    /// or it can be set calling the <see cref="AddComponent{T}(T, string)"/></para>
    /// </summary>
    public class GameComponentsCollection : IReadOnlyDictionary<string, IGameComponent>, IDisposable
    {
        #region Fields
        private Dictionary<string, IGameComponent> instance;
        #endregion

        #region Properties
        /// <summary>
        /// Retrieve the IGameComponent object for the specified key.
        /// </summary>
        /// <param name="key">Name of the component to be retrieved. It is not case sensitive.</param>
        /// <returns>IGameComponent object.</returns>
        public IGameComponent this[string key]
        {
            get
            {
                if (!instance.ContainsKey(key.ToLower()))
                    throw new KeyNotFoundException(string.Format("The key -{0}- was not found.", key));
                return instance[key.ToLower()];
            }
        }

        /// <summary>
        /// Retrieve the IGameComponent object for the specified key.
        /// </summary>
        /// <param name="index">Index of the component to be retrieved.</param>
        /// <returns>IGameComponent object.</returns>
        public IGameComponent this[int index]
        {
            get
            {
                if (instance.Keys.Count < index)
                    throw new IndexOutOfRangeException(string.Format("The index is out of range -{0}-", index));
                return instance.ToList()[index].Value;
            }
        }

        /// <summary>
        /// Get the number of components added.
        /// </summary>
        public int Count
        {
            get { return instance.Count; }
        }

        /// <summary>
        /// Get a list of the keys.
        /// </summary>
        public IEnumerable<string> Keys
        {
            get { return instance.Keys; }
        }

        /// <summary>
        /// Get a list of the values.
        /// </summary>
        public IEnumerable<IGameComponent> Values
        {
            get { return instance.Values; }
        }

        /// <summary>
        /// Handler to create custom OnComponentAdded event.
        /// </summary>
        public delegate void OnComponentAddedHandler(IGameComponent component);
        /// <summary>
        /// Event raised when a component has been added successfuly.
        /// </summary>
        public event OnComponentAddedHandler OnComponentAdded;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public GameComponentsCollection()
        {
            instance = new Dictionary<string, IGameComponent>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a component.
        /// </summary>
        /// <param name="component">The component implementation</param>
        public void AddComponent<T>(IGameComponent component) where T : IGameComponent
        {
            if (component == null || string.IsNullOrEmpty(component.Name))
                throw new NullReferenceException("Component is null or has not name assigned. " + (component != null ? "Type: " + component.GetType().Name : ""));

            // Check for duplicate.
            if (this.ContainsKey(component.Name.ToLower()))
                throw new DuplicateComponentException(string.Format("Component of type -{0}- with name -{1}- already exists.", component.GetType().Name, component.Name));

            instance.Add(component.Name, component);

            OnComponentAdded?.Invoke(component);
        }

        /// <summary>
        /// Add a component with a specified name.
        /// </summary>
        /// <param name="component">The component implementation</param>
        /// <param name="name">The component name if it has not been specified.</param>
        public void AddComponent<T>(T component, string name) where T : IGameComponent
        {
            if (component == null || string.IsNullOrEmpty(name))
                throw new NullReferenceException("Component is null or has not name assigned. " + (component != null ? "Type: " + component.GetType().Name : ""));

            component.Name = name.ToLower();
            AddComponent<T>(component);
        }

        /// <summary>
        /// Retreive a component by its key.
        /// It is the same as retrieving an item by its key in [ ].
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IGameComponent GetComponent(string key)
        {
            return this[key.ToLower()];
        }

        /// <summary>
        /// Remove a service by its name.
        /// </summary>
        /// <param name="name"></param>
        public void RemoveComponent(string name)
        {
            if (ContainsKey(name))
                instance.Remove(name);
        }

        /// <summary>
        /// Remove a service by its instance.
        /// It checks the existance of the component by its name.
        /// </summary>
        /// <param name="component"></param>
        public void RemoveComponent(GameComponent component)
        {
            if (ContainsKey(component.Name))
                instance.Remove(component.Name);
        }

        /// <summary>
        /// Check if the collection has a component by its name.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return instance.ContainsKey(key.ToLower());
        }

        /// <summary>
        /// Get the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, IGameComponent>> GetEnumerator()
        {
            return instance.GetEnumerator();
        }

        /// <summary>
        /// Try parse a key to get the corresponding value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out IGameComponent value)
        {
            return instance.TryGetValue(key, out value);
        }

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return instance.GetEnumerator();
        }

        /// <summary>
        /// Dispose all the <see cref="IGameComponent"/> components.
        /// </summary>
        public void Dispose()
        {
            foreach (IGameComponent comp in Values)
            {
                comp.Dispose();
            }
        }
        #endregion
    }
}
