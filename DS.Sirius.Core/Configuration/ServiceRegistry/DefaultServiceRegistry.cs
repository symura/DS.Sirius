using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace DS.Sirius.Core.Configuration.ServiceRegistry
{
    /// <summary>
    /// This class implements the default service registry used by the 
    /// <see cref="AppConfigurationManager"/> class.
    /// </summary>
    internal class DefaultServiceRegistry: IServiceRegistry
    {
        private readonly Dictionary<Type, Tuple<ILifetimeManager, object[], PropertySettingsCollection>> _registry =
            new Dictionary<Type, Tuple<ILifetimeManager, object[], PropertySettingsCollection>>();

        /// <summary>
        /// Gest the service object specified with the input parameter
        /// </summary>
        /// <param name="service">Type of the service</param>
        /// <returns>Service object, or null, if the specified service not found</returns>
        public object GetService(Type service)
        {
            Tuple<ILifetimeManager, object[], PropertySettingsCollection> backing;
            if (!_registry.TryGetValue(service, out backing))
            {
                return null;
            }
            var obj = backing.Item1.GetObject(backing.Item2);
            ConfigurationHelper.InjectProperties(ref obj, backing.Item3);
            return obj;
        }

        /// <summary>
        /// Registers the specified object with its related lifetime manager, and the
        /// construction parameters used by the lifetime manager.
        /// </summary>
        /// <param name="serviceType">Type of service to register</param>
        /// <param name="ltManager">Lifetime manager object</param>
        /// <param name="constructionParams">Instance construction parameters</param>
        /// <param name="properties">Initial property values</param>
        public void Register(Type serviceType, ILifetimeManager ltManager, 
            object[] constructionParams, PropertySettingsCollection properties = null)
        {
            if (_registry.ContainsKey(serviceType))
            {
                throw new ConfigurationErrorsException(
                    String.Format("'{0}' type already registered with DefaultServiceRegistry", 
                    serviceType));
            }
            _registry[serviceType] = new Tuple<ILifetimeManager, object[], PropertySettingsCollection>(
                ltManager, constructionParams, properties);
        }

        /// <summary>
        /// Removes the specified service from the registry
        /// </summary>
        /// <param name="serviceType"></param>
        public void RemoveService(Type serviceType)
        {
            _registry.Remove(serviceType);
        }

        /// <summary>
        /// Gets the collection of registered services.
        /// </summary>
        /// <returns></returns>
        public IList<Type> GetRegisteredServices()
        {
            return _registry.Keys.ToList();
        }
    }
}