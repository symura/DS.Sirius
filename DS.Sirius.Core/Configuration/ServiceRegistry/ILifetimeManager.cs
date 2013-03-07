namespace DS.Sirius.Core.Configuration.ServiceRegistry
{
    /// <summary>
    /// This interface defines the responsibilities of a lifetime manager.
    /// </summary>
    public interface ILifetimeManager
    {
        /// <summary>
        /// Retrieve an object from the backing store associated with this Lifetime manager.
        /// </summary>
        /// <param name="constructionParameters">ConstructorParameters used to construct the object</param>
        /// <returns>
        /// The object queried, or null if no such object is currently stored.
        /// </returns>
        object GetObject(object[] constructionParameters);
    }
}