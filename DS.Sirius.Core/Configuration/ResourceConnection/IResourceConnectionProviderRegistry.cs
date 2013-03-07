using System;
namespace DS.Sirius.Core.Configuration.ResourceConnection
{
    /// <summary>
    /// This interface defines the behavior of a resource connection registry
    /// </summary>
    public interface IResourceConnectionProviderRegistry: IResourceConnectionProviderLocator
    {
        /// <summary>
        /// Registers a resource connection type.
        /// </summary>
        /// <param name="type">Type representing the resource connection</param>
        /// <remarks>
        /// The type must implement the <see cref="IResourceConnectionSettings"/>
        /// interface.
        /// </remarks>
        void RegisterResourceConnectionProvider(Type type);
    }
}