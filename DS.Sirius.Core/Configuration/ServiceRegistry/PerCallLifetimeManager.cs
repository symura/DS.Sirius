using System;

namespace DS.Sirius.Core.Configuration.ServiceRegistry
{
    /// <summary>
    /// This class provides an instance of the specified type for each resolution request.
    /// </summary>
    public class PerCallLifetimeManager : ILifetimeManager
    {
        private readonly Type _implType;

        /// <summary>
        /// Creates a lifetime manager with no instance construction parameters
        /// </summary>
        /// <param name="implType">Implementation type</param>
        public PerCallLifetimeManager(Type implType)
        {
            _implType = implType;
        }

        /// <summary>
        /// Retrieve an object from the backing store associated with this Lifetime manager.
        /// </summary>
        /// <param name="constructionParameters">ConstructorParameters used to construct the object</param>
        /// <returns>
        /// The object queried, or null if no such object is currently stored.
        /// </returns>
        public object GetObject(object[] constructionParameters)
        {
            return Activator.CreateInstance(_implType, constructionParameters);
        }
    }
}