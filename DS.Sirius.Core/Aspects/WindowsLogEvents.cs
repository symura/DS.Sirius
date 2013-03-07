using System.Diagnostics;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.WindowsEventLog;

namespace DS.Sirius.Core.Aspects
{

    /// <summary>
    /// This class represents an event when the aspect infrastructure raises an exception
    /// </summary>
    [EventType(EventLogEntryType.Error)]
    [EventLogName(ConfigurationConstants.CORE_LOG_NAME)]
    [EventSource(ConfigurationConstants.AZURE_COMPONENTS_SOURCE)]
    [EventCategoryId(ConfigurationConstants.CORE_CATEGORY)]
    [EventId(ConfigurationConstants.ASPECT_INFRASTRUCTURE_ID)]
    [EventMessage("The aspect infrastructure raised an exception.")]
    public sealed class AspectInfrastructureErrorEvent : LogEventBase
    { }
}