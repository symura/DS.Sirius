using System;
using System.Xml.Linq;
using DS.Sirius.Core.Configuration;

namespace DS.Sirius.Core.Diagnostics.Configuration
{
    /// <summary>
    /// This class describes a logger definition.
    /// </summary>
    public sealed class DiagnosticsLoggerSettings : ObjectSettingsBase<IDiagnosticsLogger>
    {
        /// <summary>
        /// Creates a new LoggerDefinition instance using the specified name and type.
        /// </summary>
        /// <param name="name">Name of the definition</param>
        /// <param name="type">Type implementing the definition</param>
        public DiagnosticsLoggerSettings(string name, Type type)
            : base(name, type)
        {
        }

        /// <summary>
        /// Creates a new LoggerDefinition instance be deserializing the specified XElement.
        /// </summary>
        /// <param name="element">XElement representation</param>
        public DiagnosticsLoggerSettings(XElement element)
            : base(element)
        {
        }
    }
}