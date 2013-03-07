using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using DS.Sirius.Core.Configuration;

namespace DS.Sirius.Core.Diagnostics.Configuration
{
    /// <summary>
    /// This class defines the configuration settings for diagnostics.
    /// </summary>
    public class DiagnosticsConfigurationSettings: ConfigurationSettingsBase
    {
        private const string ENABLED = "enabled";
        private const string LOG_ROUTE = "LogRoute";

        private readonly List<LogRouteSettings> _routes = new List<LogRouteSettings>();

        /// <summary>
        /// Default configuration section within app config files
        /// </summary>
        public const string ROOT = "Diagnostics";

        /// <summary>
        /// Gets the flag indicating if diagnostics logging is enabled or not
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// Gets the routes used for diagnostics
        /// </summary>
        public ReadOnlyCollection<LogRouteSettings> Routes
        {
            get { return new ReadOnlyCollection<LogRouteSettings>(_routes); }
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public DiagnosticsConfigurationSettings()
        {
        }

        /// <summary>
        /// Creates a new settings from the specified arguments.
        /// </summary>
        /// <param name="enabled">Is diagnostics enabled?</param>
        /// <param name="routes">Routes used in logging</param>
        public DiagnosticsConfigurationSettings(bool enabled, List<LogRouteSettings> routes)
        {
            Enabled = enabled;
            _routes = routes;
        }

        /// <summary>
        /// Creates a new instance from the specified element
        /// </summary>
        /// <param name="element"></param>
        public DiagnosticsConfigurationSettings(XElement element)
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
            return new XElement(rootElement, 
                new XAttribute(ENABLED, Enabled),
                from route in Routes
                select route.WriteToXml(LOG_ROUTE));
        }

        /// <summary>
        /// Parse the specified configuration settings
        /// </summary>
        /// <param name="element">Element holding configuration settings</param>
        protected override void ParseFrom(XElement element)
        {
            Enabled = element.OptionalBoolAttribute(ENABLED, true);
            element.ProcessItems(LOG_ROUTE, item => _routes.Add(new LogRouteSettings(item)));
        }
    }
}