using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Xml.Linq;
using DS.Sirius.Core.Audit;
using DS.Sirius.Core.Common;
using DS.Sirius.Core.Configuration.Environment;
using DS.Sirius.Core.Configuration.ResourceConnection;
using DS.Sirius.Core.Configuration.ServiceRegistry;
using DS.Sirius.Core.Configuration.TypeResolution;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// This class is responsible for managing the application configuration.
    /// </summary>
    public static class AppConfigurationManager
    {
        private static AppConfigurationSettings s_AppConfigSetting;
        private static IConfigurationProvider s_ConfigurationProvider;
        private static ITypeResolver s_TypeResolver;
        private static IServiceRegistry s_ServiceRegistry;
        private static IEnvironmentInfoProvider s_EnvironmentInfo;
        private static IResourceConnectionProviderRegistry s_ConnectionProviderRegistry;
        private static IResourceConnectionFactory s_ResourceConnectionFactory;
        private static IAuditLogger s_AuditLogger;

        /// <summary>
        /// Initializes the static members of this class.
        /// </summary>
        static AppConfigurationManager()
        {
            Reset();
        }

        /// <summary>
        /// Resets the configuration manager to its default state.
        /// </summary>
        public static void Reset()
        {
            // --- Obtain default configuration from the application configuration file, if there is
            // --- any specification there
            try
            {
                var config = ConfigurationManager.GetSection(AppConfigurationSettings.ROOT) as XElement;
                Settings = new AppConfigurationSettings(config);
            }
            catch (Exception)
            {
                Settings = new AppConfigurationSettings(String.Empty, String.Empty,
                                                        typeof (AppConfigProvider));
            }

            // --- Set up the default type resolution
            s_TypeResolver = new DefaultTypeResolver();

            // --- Set up the default service registry
            s_ServiceRegistry = new DefaultServiceRegistry();

            // --- Set up the default environment info provider
            s_EnvironmentInfo = new DefaultEnvironmentInfoProvider();

            // --- Set up the default resource connection provider registry
            ResourceConnectionProviderRegistry = new DefaultResourceConnectionProviderRegistry();
        }

        /// <summary>
        /// This event is raised when the application connfiguration is changed.
        /// </summary>
        public static event EventHandler<AppConfigurationChangedEventArgs> OnConfigurationChanged;

        /// <summary>
        /// This event is raised when the resource connection provider registry is set.
        /// </summary>
        public static event EventHandler OnConnectionProviderRegisterSet;

        /// <summary>
        /// Gets the application configuration settings
        /// </summary>
        public static AppConfigurationSettings Settings
        {
            get { return s_AppConfigSetting; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (s_AppConfigSetting == value) return;
                s_AppConfigSetting = value;
                s_ConfigurationProvider = ConfigurationHelper.PrepareInstance(
                    value.Provider, value.ConstructorParameters, value.Properties) 
                    as IConfigurationProvider;
                RaiseOnConfigurationChanged();
            }
        }

        #region Application configuration operations

        /// <summary>
        /// Gets or sets the configuration provider to use
        /// </summary>
        public static IConfigurationProvider ConfigurationProvider 
        {
            get { return s_ConfigurationProvider; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                s_ConfigurationProvider = value;
            }
        }

        /// <summary>
        /// Gets a setting value with the specified key.
        /// </summary>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>Settings value</returns>
        public static object GetSettingValue(string settingKey)
        {
            return s_ConfigurationProvider.GetSettingValue(settingKey);
        }

        /// <summary>
        /// Gets a setting value with the specified key.
        /// </summary>
        /// <typeparam name="TVal">Value type of configuration parameter</typeparam>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>Settings value</returns>
        public static TVal GetSettingValue<TVal>(string settingKey)
        {
            return  (TVal)Convert.ChangeType(s_ConfigurationProvider.GetSettingValue(settingKey), typeof(TVal));
        }

        /// <summary>
        /// Gets a compound setting object with the specified key.
        /// </summary>
        /// <typeparam name="TSetting">Type of the configuration setting</typeparam>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>XML element representing the setting</returns>
        public static TSetting GetSettings<TSetting>(string settingKey)
            where TSetting : IXElementRepresentable, new()
        {
            return s_ConfigurationProvider.GetSection<TSetting>(settingKey);
        }

        /// <summary>
        /// Checks if the specified configuration value is defined
        /// </summary>
        /// <param name="settingKey">Configuration value key</param>
        /// <returns>True, if configuration value is defined; otherwise, false</returns>
        public static bool IsSettingValueDefined(string settingKey)
        {
            return s_ConfigurationProvider.IsSettingValueDefined(settingKey);
        }

        /// <summary>
        /// Checks if the specified configuration section is defined
        /// </summary>
        /// <param name="settingKey">Configuration section key</param>
        /// <returns>True, if configuration section is defined; otherwise, false</returns>
        public static bool IsSectionDefined(string settingKey)
        {
            return s_ConfigurationProvider.IsSectionDefined(settingKey);
        }

        #endregion

        #region Type Resolver operations

        /// <summary>
        /// Gets or sets the type resolver instance used by the application configuration
        /// </summary>
        /// <returns></returns>
        public static ITypeResolver TypeResolver
        {
            get { return s_TypeResolver; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                s_TypeResolver = value;
            }
        }

        /// <summary>
        /// Uses the type resolver to resolve a type name to a type instance.
        /// </summary>
        /// <param name="name">Type name</param>
        /// <returns>Type instance, if resolution successful; otherwise, null</returns>
        public static Type ResolveTypeFromName(string name)
        {
            return s_TypeResolver.Resolve(name);
        }

        #endregion

        #region Service registry operations

        /// <summary>
        /// Gets or sets the service registry object to use.
        /// </summary>
        public static IServiceRegistry ServiceRegistry
        {
            get { return s_ServiceRegistry; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                s_ServiceRegistry = value;
            }
        }

        /// <summary>
        /// Registers the specified service object with its related lifetime manager, and the
        /// construction parameters used by the lifetime manager.
        /// </summary>
        /// <param name="serviceType">Type of service to register</param>
        /// <param name="ltManager">Lifetime manager object</param>
        /// <param name="constructionParams">Instance construction parameters</param>
        /// <param name="properties">Property injection values</param>
        public static void RegisterService(Type serviceType, ILifetimeManager ltManager,
            object[] constructionParams = null, List<PropertySettings> properties = null)
        {
            s_ServiceRegistry.Register(serviceType, ltManager, constructionParams);
        }

        /// <summary>
        /// Registers a service with per call lifetime management.
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <typeparam name="TImpl">Implementation type</typeparam>
        /// <param name="contructionParams">Service instance construction parameters</param>
        /// <param name="properties">Property injection values</param>
        public static void RegisterService<TService, TImpl>(object[] contructionParams = null, 
            PropertySettingsCollection properties = null)
        {
            s_ServiceRegistry.Register(typeof (TService), 
                new PerCallLifetimeManager(typeof (TImpl)), contructionParams, properties);
        }

        /// <summary>
        /// Registers a service with a singleton instance lifetime management.
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        /// <typeparam name="TImpl">Implementation type</typeparam>
        /// <param name="instance">Service instance</param>
        public static void RegisterServiceInstance<TService, TImpl>(TImpl instance)
        {
            s_ServiceRegistry.Register(typeof (TService), new SingletonLifetimeManager(instance), null);
        }

        /// <summary>
        /// Gets the specified service object
        /// </summary>
        /// <param name="serviceType">Type of service object</param>
        /// <returns></returns>
        public static object GetService(Type serviceType)
        {
            return s_ServiceRegistry.GetService(serviceType);
        }

        /// <summary>
        /// Gets the service type specified by TService
        /// </summary>
        /// <typeparam name="TService">Service type to resolve</typeparam>
        /// <returns></returns>
        public static TService GetService<TService>()
        {
            return (TService) s_ServiceRegistry.GetService(typeof (TService));
        }

        /// <summary>
        /// Removes the specified service from the registry
        /// </summary>
        /// <param name="serviceType"></param>
        public static void RemoveService(Type serviceType)
        {
            s_ServiceRegistry.RemoveService(serviceType);
        }

        /// <summary>
        /// Gets the collection of registered services.
        /// </summary>
        /// <returns></returns>
        public static IList<Type> GetRegisteredServices()
        {
            return s_ServiceRegistry.GetRegisteredServices();
        }

        /// <summary>
        /// Configures service registry from configuration file.
        /// </summary>
        /// <param name="merge">
        /// True merges the configuration with the existing one; false clears the preset configuration
        /// before reading the one from the configuration file.
        /// </param>
        public static void ConfigureServiceRegistry(bool merge = true)
        {
            var settings = GetSettings<ServiceRegistryConfigurationSettings>(ServiceRegistryConfigurationSettings.ROOT);
            ConfigureServiceRegistry(settings, merge);
        }

        /// <summary>
        /// Configures service registry from the settings provided
        /// </summary>
        /// <param name="settings">Service registry setting</param>
        /// <param name="merge">
        /// True merges the configuration with the existing one; false clears the preset configuration
        /// before reading the one from the configuration file.
        /// </param>
        public static void ConfigureServiceRegistry(ServiceRegistryConfigurationSettings settings, bool merge = true)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            // --- Erase the registry if merge is not an option
            if (!merge)
            {
                foreach (var service in s_ServiceRegistry.GetRegisteredServices())
                {
                    s_ServiceRegistry.RemoveService(service);
                }
            }

            // --- Process mappings
            foreach (var mapping in settings.Mappings.Values)
            {
                // --- Remove existing service
                if (s_ServiceRegistry.GetService(mapping.Service) != null)
                {
                    s_ServiceRegistry.RemoveService(mapping.Service);
                }


                var ltManager = CreateLifetimeManager(settings, mapping);

                // --- Create an array of construction parameters
                var parameters = new object[mapping.ConstructorParameters.Count];
                for (var i = 0; i < mapping.ConstructorParameters.Count; i++)
                {
                    var converter = TypeDescriptor.GetConverter(mapping.ConstructorParameters[i].Type);
                    parameters[i] = converter.ConvertFromString(mapping.ConstructorParameters[i].Value);
                }

                // --- Register the service entry
                s_ServiceRegistry.Register(mapping.Service, ltManager, parameters, mapping.Properties);
            }
        }


        #endregion

        #region EnvironmentInfo operations

        /// <summary>
        /// Gets or sets the provider of environment info
        /// </summary>
        public static IEnvironmentInfoProvider EnvironmentInfoProvider
        {
            get { return s_EnvironmentInfo; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                s_EnvironmentInfo = value;
            }
        }

        /// <summary>
        /// Returns the current DateTime as UTC DateTime.
        /// </summary>
        /// <returns>The current UTC DateTime</returns>
        public static DateTime GetCurrentDateTimeUtc()
        {
            return s_EnvironmentInfo.GetCurrentDateTimeUtc();
        }

        /// <summary>
        /// Returns the caller client's IP address.
        /// </summary>
        /// <returns>The caller client's IP address.</returns>
        public static string GetClientIpAddress()
        {
            return s_EnvironmentInfo.GetClientIpAddress();
        }

        /// <summary>
        /// Gets the NetBIOS name of this local computer.
        /// </summary>
        /// <returns></returns>
        public static string GetMachineName()
        {
            return s_EnvironmentInfo.GetMachineName();
        }

        #endregion

        #region Connection providers

        /// <summary>
        /// Gets or sets the resource connection provider registry
        /// </summary>
        public static IResourceConnectionProviderRegistry ResourceConnectionProviderRegistry
        {
            get { return s_ConnectionProviderRegistry; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                s_ConnectionProviderRegistry = value;
                RaiseOnConnectionProviderRegisterSet();
            }
        }

        /// <summary>
        /// Gets or sets the resource connection factory
        /// </summary>
        public static IResourceConnectionFactory ResourceConnectionFactory
        {
            get { return s_ResourceConnectionFactory; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                s_ResourceConnectionFactory = value;
            }
        }

        /// <summary>
        /// Registers the specified type as a connection provider
        /// </summary>
        /// <param name="provider">Provider type</param>
        public static void RegisterConnectionProvider(Type provider)
        {
            s_ConnectionProviderRegistry.RegisterResourceConnectionProvider(provider);
        }

        /// <summary>
        /// Creates a connection with the specified connection type
        /// </summary>
        /// <typeparam name="TConnection">Connection object to create</typeparam>
        /// <param name="name">Resource connection name</param>
        /// <returns>Connection instance</returns>
        public static TConnection CreateResourceConnection<TConnection>(string name)
        {
            EnsureResourceConnectionFactory();
            return s_ResourceConnectionFactory.CreateResourceConnection<TConnection>(name);
        }

        #endregion

        #region Audit logger

        /// <summary>
        /// Gets the audit logger used by this application
        /// </summary>
        public static IAuditLogger AuditLogger
        {
            get { return s_AuditLogger ?? (s_AuditLogger = GetService<IAuditLogger>()); }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Raises the OnConfigurationChanged event
        /// </summary>
        private static void RaiseOnConfigurationChanged()
        {
            var handler = OnConfigurationChanged;
            if (handler != null) handler(null, new AppConfigurationChangedEventArgs(Settings));
        }

        /// <summary>
        /// Raises the OnConfigurationChanged event
        /// </summary>
        private static void RaiseOnConnectionProviderRegisterSet()
        {
            var handler = OnConnectionProviderRegisterSet;
            if (handler != null) handler(null, EventArgs.Empty);
        }

        /// <summary>
        /// Creates a lifetime manager for the specified mapping, using the settings passed.
        /// </summary>
        /// <returns>Lifetime manager instance</returns>
        private static ILifetimeManager CreateLifetimeManager(ServiceRegistryConfigurationSettings settings, 
            ServiceRegistryConfigurationSettings.Mapping mapping)
        {
            ILifetimeManager ltManager;
            ServiceRegistryConfigurationSettings.LifetimeManagerDefinition definition;
            if (settings.LifetimeManagers.TryGetValue(mapping.LifetimeManager, out definition))
            {
                // --- Create an array of construction parameters
                var ltParams = new object[definition.Parameters.Count];
                for (var i = 0; i < definition.Parameters.Count; i++)
                {
                    var converter = TypeDescriptor.GetConverter(definition.Parameters[i].Type);
                    ltParams[i] = converter.ConvertFromString(definition.Parameters[i].Value);
                }
                ltManager = Activator.CreateInstance(definition.Type,
                                                     new object[] { mapping.Implementation, ltParams }) as ILifetimeManager;
                if (ltManager == null)
                {
                    throw new ConfigurationErrorsException(
                        String.Format("LifetimeManager must implement ILifetimeManager in mapping entry for {0}",
                                      mapping.Service));
                }
            }
            else if (mapping.LifetimeManager == "percall")
            {
                ltManager = new PerCallLifetimeManager(mapping.Implementation);
            }
            else if (mapping.LifetimeManager == "singleton")
            {
                ltManager = new SingletonLifetimeManager(mapping.Implementation);
            }
            else
            {
                throw new ConfigurationErrorsException(
                    String.Format("LifetimeManager is unknown in mapping entry for {0}",
                                  mapping.Service));
            }
            return ltManager;
        }

        /// <summary>
        /// Ensures that there is always a usable resource connection factory.
        /// </summary>
        private static void EnsureResourceConnectionFactory()
        {
            if (s_ResourceConnectionFactory != null) return;
            ResourceConnectionFactorySettings factorySettings;
            try
            {
                factorySettings = GetSettings<ResourceConnectionFactorySettings>(ResourceConnectionFactorySettings.ROOT);
            }
            catch (Exception)
            {
                factorySettings = new ResourceConnectionFactorySettings();
            }
            s_ResourceConnectionFactory = new DefaultResourceConnectionFactory(factorySettings);
        }

        #endregion
    }
}