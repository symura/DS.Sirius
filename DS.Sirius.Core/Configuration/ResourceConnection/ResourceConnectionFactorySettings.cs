using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;

namespace DS.Sirius.Core.Configuration.ResourceConnection
{
    /// <summary>
    /// This class holds seetings of a resource connection factory
    /// </summary>
    public class ResourceConnectionFactorySettings: ConfigurationSettingsBase
    {
        private readonly ResourceConnectionProviderCollection _providers =
            new ResourceConnectionProviderCollection();

        private IResourceConnectionProviderRegistry _registry;

        /// <summary>
        /// Default root node of the resource connection provider configuration.
        /// </summary>
        public const string ROOT = "ResourceConnections";

        /// <summary>
        /// Gets or sets the connection provider registry behind this factory
        /// </summary>
        public IResourceConnectionProviderRegistry Registry
        {
            get { return _registry; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _registry = value;
            }
    }

        /// <summary>
        /// Gets the dictionary of providers registered with this factory
        /// </summary>
        public ReadOnlyDictionary<string, ResourceConnectionProviderBase> Providers
        {
            get { return _providers.ProviderDictionary; }
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        public ResourceConnectionFactorySettings(): this(new ResourceConnectionProviderCollection())
        {
        }

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="providers">Resource connection provider definitions</param>
        public ResourceConnectionFactorySettings(ResourceConnectionProviderCollection providers)
        {
            if (providers == null) throw new ArgumentNullException("providers");
            _providers = providers;
            _registry = AppConfigurationManager.ResourceConnectionProviderRegistry;
        }

        /// <summary>
        /// Gets the settings from the specified XML element.
        /// </summary>
        /// <param name="element">XML element with settings</param>
        /// <param name="registry">Registry used with this factory</param>
        public ResourceConnectionFactorySettings(XElement element, IResourceConnectionProviderRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            Registry = registry;
            ReadFromXml(element);
        }

        /// <summary>
        /// Writes the object to an XElement instance using the specified root element name.
        /// </summary>
        /// <param name="rootElement">Root element name</param>
        /// <returns>XElement representation of the object</returns>
        public override XElement WriteToXml(XName rootElement)
        {
            return new XElement(rootElement,
                from provider in _providers.ProviderDictionary.Values
                select provider.WriteToXml(
                    DefaultResourceConnectionProviderRegistry.GetProviderName(provider.GetType())));
        }

        /// <summary>
        /// Parse the specified configuration settings
        /// </summary>
        /// <param name="element">Element holding configuration settings</param>
        protected override void ParseFrom(XElement element)
        {
            foreach (var provider in element.Descendants())
            {
                // --- Obtain the appropriate provider instance
                var type = Registry.GetResourceConnectionProvider(provider.Name.LocalName);
                if (type == null)
                {
                    throw new ConfigurationErrorsException(
                        String.Format("{0} is an unknown resource connection provider type.",
                        provider.Name.LocalName));
                }
                var instance = Activator.CreateInstance(type, new object[] { provider }) as ResourceConnectionProviderBase;
                if (instance == null)
                {
                    throw new ConfigurationErrorsException(String.Format(
                        "The registered {0} type does not support the correct provider.", type));
                }
                _providers.Add(instance);
            }
        }
    }
}