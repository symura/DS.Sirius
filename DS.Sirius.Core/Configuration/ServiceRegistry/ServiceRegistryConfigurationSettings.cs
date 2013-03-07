using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace DS.Sirius.Core.Configuration.ServiceRegistry
{
    public class ServiceRegistryConfigurationSettings: ConfigurationSettingsBase
    {
        private const string MAPPINGS = "Mappings";
        private const string MAP = "Map";
        private const string SERVICE = "service";
        private const string IMPLEMENTATION = "implementation";
        private const string LIFETIME = "lifetime";
        private const string CONSTRUCT = "Construct";
        private const string PROPERTIES = "Properties";
        private const string PARAM = "Param";
        private const string TYPE = "type";
        private const string VALUE = "value";
        private const string LIFETIMEMANAGERS = "LifetimeManagers";
        private const string MANAGER = "Manager";
        private const string ALIAS = "alias";

        private readonly Dictionary<string, Mapping> _mappings = 
            new Dictionary<string, Mapping>();
        private readonly Dictionary<string, LifetimeManagerDefinition> _managers =
            new Dictionary<string, LifetimeManagerDefinition>();

        /// <summary>
        /// The configuration section describing service registry
        /// </summary>
        public const string ROOT = "ServiceRegistry";

        public ServiceRegistryConfigurationSettings() : base(ROOT)
        {
        }

        /// <summary>
        /// Creates an instance from the specified XML element.
        /// </summary>
        /// <param name="element"></param>
        public ServiceRegistryConfigurationSettings(XElement element): this()
        {
            ReadFromXml(element);
        }

        /// <summary>
        /// Gets the service registry mappings
        /// </summary>
        public Dictionary<string, Mapping> Mappings
        {
            get { return _mappings; }
        }

        /// <summary>
        /// Gets the lifetime manager mappings
        /// </summary>
        public Dictionary<string, LifetimeManagerDefinition> LifetimeManagers
        {
            get { return _managers; }
        }

        /// <summary>
        /// Writes the object to an XElement instance using the specified root element name.
        /// </summary>
        /// <param name="rootElement">Root element name</param>
        /// <returns>XElement representation of the object</returns>
        public override XElement WriteToXml(XName rootElement)
        {
            return new XElement
                (ROOT,
                 new XElement(
                     LIFETIMEMANAGERS,
                     from manager in _managers.Values
                     select new XElement(
                         MANAGER,
                         new XAttribute(ALIAS, manager.Alias),
                         new XAttribute(TYPE, manager.Type),
                         from par in manager.Parameters
                         select new XElement(PARAM,
                                             // ReSharper disable AssignNullToNotNullAttribute
                                             new XAttribute(TYPE,
                                                            par.Type
                                                               .AssemblyQualifiedName),
                                             // ReSharper restore AssignNullToNotNullAttribute
                                             new XAttribute(VALUE, par.Value)))),
                 new XElement(
                     MAPPINGS,
                     from map in _mappings.Values
                     select new XElement(
                         MAP,
                         new XAttribute(SERVICE, map.Service),
                         new XAttribute(IMPLEMENTATION, map.Implementation),
                         new XAttribute(LIFETIME, map.LifetimeManager),
                         map.ConstructorParameters.WriteToXml(CONSTRUCT),
                         map.Properties.WriteToXml(PROPERTIES))));
        }

        /// <summary>
        /// Parse the specified configuration settings
        /// </summary>
        /// <param name="element">Element holding configuration settings</param>
        protected override void ParseFrom(XElement element)
        {
            var resolver = AppConfigurationManager.TypeResolver;
            // ReSharper disable ImplicitlyCapturedClosure
            element.ProcessOptionalContainer(LIFETIMEMANAGERS, MANAGER, manager =>
            // ReSharper restore ImplicitlyCapturedClosure
                {
                    var alias = manager.StringAttribute(ALIAS);
                    var type = manager.TypeAttribute(TYPE, resolver);
                    var managerDef = new LifetimeManagerDefinition
                        {
                            Alias = alias,
                            Type = type,
                            Parameters = new List<PropertySettings>()
                        };
                    manager.ProcessItems(PARAM, parItem =>
                    {
                        var parType = parItem.TypeAttribute(TYPE, resolver);
                        var value = parItem.StringAttribute(VALUE);
                        managerDef.Parameters.Add(new PropertySettings { Type = parType, Value = value });
                    });
                });
            element.ProcessContainerItems(MAPPINGS, MAP, mapItem =>
                {
                    var srvType = mapItem.TypeAttribute(SERVICE, resolver);
                    var impType = mapItem.TypeAttribute(IMPLEMENTATION, resolver);
                    var ltmType = mapItem.OptionalStringAttribute(LIFETIME, "percall");
                    var mapping = new Mapping
                        {
                            Parent = this,
                            Service = srvType,
                            Implementation = impType,
                            LifetimeManager = ltmType,
                        };
                    // ReSharper disable AssignNullToNotNullAttribute
                    _mappings.Add(srvType.AssemblyQualifiedName, mapping);
                    // ReSharper restore AssignNullToNotNullAttribute
                    mapItem.ProcessOptionalElement(CONSTRUCT, item => mapping.ConstructorParameters.ReadFromXml(item));
                    mapItem.ProcessOptionalElement(PROPERTIES, item => mapping.Properties.ReadFromXml(item));
                });
        }

        /// <summary>
        /// Represents a mapping
        /// </summary>
        public class Mapping
        {
            public ServiceRegistryConfigurationSettings Parent;
            public Type Service;
            public Type Implementation;
            public string LifetimeManager;
            public UnnamedPropertySettingsCollection ConstructorParameters = new UnnamedPropertySettingsCollection(PARAM);
            public PropertySettingsCollection Properties = new PropertySettingsCollection();

            public void AddContructorParameter(Type type, string value)
            {
                ConstructorParameters.Add(new PropertySettings { Type = type, Value = value });
            }

            public void AddProperty(string name, string value)
            {
                Properties.Add(new PropertySettings { Name = name, Value = value });
            }
        }

        /// <summary>
        /// Represents a lifetime manager
        /// </summary>
        public class LifetimeManagerDefinition
        {
            public string Alias;
            public Type Type;
            public List<PropertySettings> Parameters = new List<PropertySettings>();
        }
    }
}