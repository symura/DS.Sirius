using System;
using System.Collections.Generic;

namespace DS.Sirius.Core.Configuration.ServiceRegistry
{
    /// <summary>
    /// This class defines the responsibility of a service registry
    /// </summary>
    public interface IServiceRegistry: IServiceLocator
    {
        /// <summary>
        /// Registers the specified object with its related lifetime manager, and the
        /// construction parameters used by the lifetime manager.
        /// </summary>
        /// <param name="serviceType">Type of service to register</param>
        /// <param name="ltManager">Lifetime manager object</param>
        /// <param name="constructionParams">Instance construction parameters</param>
        void Register(Type serviceType, ILifetimeManager ltManager,
                      object[] constructionParams, PropertySettingsCollection properties = null);

        /// <summary>
        /// Removes the specified service from the registry
        /// </summary>
        /// <param name="serviceType"></param>
        void RemoveService(Type serviceType);

        /// <summary>
        /// Gets the collection of registered services.
        /// </summary>
        /// <returns></returns>
        IList<Type> GetRegisteredServices();
    }
}