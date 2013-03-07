using System;
using System.Xml.Linq;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// Thsi class represents the fundamental application configuration settings.
    /// </summary>
    public class AppConfigurationSettings: ConfigurationSettingsBase
    {
        private const string INSTANCE_PREFIX = "instancePrefix";
        private const string INSTANCE_NAME = "instanceName";
        private const string PROVIDER = "provider";
        private const string CONSTRUCT = "Construct";
        private const string PROPERTIES = "Properties";
        private const string PARAM = "Param";

        /// <summary>
        /// Application configuration settings root
        /// </summary>
        public const string ROOT = "AppConfiguration";

        /// <summary>
        /// Gets the instance prefix of this application
        /// </summary>
        public string InstancePrefix { get; private set; }

        /// <summary>
        /// Gest the instance name of this application
        /// </summary>
        public string InstanceName { get; private set; }

        /// <summary>
        /// Gets the application configuration provider
        /// </summary>
        public Type Provider { get; private set; }

        /// <summary>
        /// Gets the construction parameters
        /// </summary>
        public UnnamedPropertySettingsCollection ConstructorParameters { get; private set; } 
             
        /// <summary>
        /// Gets the properties of the configuration provider
        /// </summary>
        public PropertySettingsCollection Properties { get; private set; }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public AppConfigurationSettings() : base(ROOT)
        {
            Init();
        }

        /// <summary>
        /// Creates an instance initializing it with the specified parameters.
        /// </summary>
        /// <param name="instancePrefix">Application instance prefix</param>
        /// <param name="instanceName">Application instance name</param>
        /// <param name="provider">Application configuration provider</param>
        /// <param name="constructorParams">Constructor parameters</param>
        /// <param name="properties">Configuration instance properties</param>
        public AppConfigurationSettings(string instancePrefix, string instanceName, Type provider, 
            UnnamedPropertySettingsCollection constructorParams = null,
            PropertySettingsCollection properties = null): this()
        {
            Init();
            InstancePrefix = instancePrefix;
            InstanceName = instanceName;
            Provider = provider;
            if (constructorParams != null) ConstructorParameters = constructorParams;
            if (properties != null) Properties = properties;
        }

        /// <summary>
        /// Initializes this instance
        /// </summary>
        private void Init()
        {
            ConstructorParameters = new UnnamedPropertySettingsCollection(PARAM);
            Properties = new PropertySettingsCollection();
        }

        /// <summary>
        /// Creates a new instance of this class using the specified XML element
        /// </summary>
        /// <param name="element">XML element holding configuration setting</param>
        public AppConfigurationSettings(XElement element)
            : this()
        {
            ReadFromXml(element);
        }

        /// <summary>
        /// Writes the object to an XElement instance using the specified root element name.
        /// </summary>
        /// <param name="rootElement">Root element name</param>
        /// <returns>XElement representation of the object</returns>
        public override XElement WriteToXml(XName rootElement)
        {
            return new XElement(ROOT,
                new XAttribute(INSTANCE_PREFIX, InstancePrefix ?? String.Empty),
                new XAttribute(INSTANCE_NAME, InstanceName ?? String.Empty),
                // ReSharper disable AssignNullToNotNullAttribute
                new XAttribute(PROVIDER, Provider == null ? String.Empty : Provider.AssemblyQualifiedName),
                ConstructorParameters.WriteToXml(CONSTRUCT),
                Properties.WriteToXml(PROPERTIES));
        }

        /// <summary>
        /// Parse the specified configuration settings
        /// </summary>
        /// <param name="element">Element holding configuration settings</param>
        protected override void ParseFrom(XElement element)
        {
            InstancePrefix = element.OptionalStringAttribute(INSTANCE_PREFIX);
            InstanceName = element.OptionalStringAttribute(INSTANCE_NAME);
            var providerValue = element.OptionalStringAttribute(PROVIDER);
            Provider = String.IsNullOrWhiteSpace(providerValue)
                           ? typeof (AppConfigProvider)
                           : Type.GetType(providerValue);
            element.ProcessOptionalElement(CONSTRUCT, item => ConstructorParameters.ReadFromXml(item));
            element.ProcessOptionalElement(PROPERTIES, item => Properties.ReadFromXml(item));
        }
    }
}