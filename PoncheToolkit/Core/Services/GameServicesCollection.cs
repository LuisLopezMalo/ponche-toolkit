using System;
using System.Collections;
using System.Collections.Generic;
using PoncheToolkit.Util.Exceptions;

namespace PoncheToolkit.Core.Services
{
    /// <summary>
    /// Read-only dictionary that contains all the services.
    /// To add a service the AddService method must be called.
    /// </summary>
    public class GameServicesCollection : IReadOnlyDictionary<Type, IGameService>
    {
        #region Fields
        private Dictionary<Type, IGameService> instance;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public IGameService this[Type key]
        {
            get { return instance[key]; }
        }

        /// <inheritdoc/>
        public int Count
        {
            get { return instance.Count; }
        }

        /// <inheritdoc/>
        public IEnumerable<Type> Keys
        {
            get { return instance.Keys; }
        }

        /// <inheritdoc/>
        public IEnumerable<IGameService> Values
        {
            get { return instance.Values; }
        }
        #endregion

        #region Initialization
        public GameServicesCollection()
        {
            instance = new Dictionary<Type, IGameService>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a service.
        /// If the service hasn't been initialized, it is initialized here.
        /// </summary>
        /// <param name="service">The service implementation</param>
        public void AddService(IGameService service)
        {
            if (service == null)
                throw new NullReferenceException("Service is null.");

            // Initialize service if necessary.
            if (!service.IsInitialized)
                service.Initialize();

            // Check for duplicate.
            if (this.ContainsKey(service.GetType()))
                throw new DuplicateServiceException(string.Format("Service of type -{0}- already exists as a service.", service.GetType().Name));
            
            instance.Add(service.GetType(), service);
        }

        /// <summary>
        /// Get a service by its type.
        /// </summary>
        /// <param name="type">The type of the service to be retrieved.</param>
        /// <returns></returns>
        public IGameService GetService(Type type)
        {
            if (!this.ContainsKey(type))
                throw new NullReferenceException(string.Format("The service for the type {0} was not found.", type.Name));

            return this[type];
        }

        /// <summary>
        /// Remove a service by its type.
        /// </summary>
        /// <param name="type"></param>
        public void RemoveService(Type type)
        {
            if (this.ContainsKey(type))
                instance.Remove(type);
        }

        /// <summary>
        /// Check if the collection has a type of service.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(Type key)
        {
            return instance.ContainsKey(key);
        }

        /// <summary>
        /// Get the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<Type, IGameService>> GetEnumerator()
        {
            return instance.GetEnumerator();
        }

        /// <summary>
        /// Try parse a key to get the corresponding value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(Type key, out IGameService value)
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
        #endregion
    }
}
