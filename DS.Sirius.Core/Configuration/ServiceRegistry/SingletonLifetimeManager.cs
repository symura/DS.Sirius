using System;

namespace DS.Sirius.Core.Configuration.ServiceRegistry
{
    /// <summary>
    /// This lifetime manager provides a singleton object 
    /// </summary>
    public class SingletonLifetimeManager : ILifetimeManager
    {
        private readonly Type _implType;
        private object _instance;

        public SingletonLifetimeManager(object instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            _instance = instance;
        }

        /// <summary>
        /// Creates a lifetime manager with no instance construction parameters
        /// </summary>
        /// <param name="implType">Implementation type</param>
        public SingletonLifetimeManager(Type implType)
        {
            _implType = implType;
            _instance = null;
        }

        /// <summary>
        /// Retrieve an object from the backing store associated with this Lifetime manager.
        /// </summary>
        /// <param name="constructionParameters">ConstructorParameters used to construct the object</param>
        /// <returns>
        /// The object queried, or null if no such object is currently stored.
        /// </returns>
        object ILifetimeManager.GetObject(object[] constructionParameters)
        {
            return _instance ?? (_instance = Activator.CreateInstance(_implType, constructionParameters));
        }
    }
}