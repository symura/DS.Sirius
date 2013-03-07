using System;

namespace DS.Sirius.Core.Configuration.ServiceRegistry
{
    /// <summary>
    /// This interface defines the behavior of a service locator
    /// </summary>
    public interface IServiceLocator
    {
        /// <summary>
        /// Gest the service object specified with the input parameter
        /// </summary>
        /// <param name="service">Type of the service</param>
        /// <returns>Service object, or null, if the specified service not found</returns>
        object GetService(Type service);
    }
}