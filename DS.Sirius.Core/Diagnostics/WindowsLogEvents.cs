using System.Diagnostics;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.WindowsEventLog;

namespace DS.Sirius.Core.Diagnostics
{
    /// <summary>
    /// This class represents an event when the AzureTableDiagnosticsLogger raises an exception
    /// </summary>
    [EventType(EventLogEntryType.Error)]
    [EventLogName(ConfigurationConstants.CORE_LOG_NAME)]
    [EventSource(ConfigurationConstants.AZURE_COMPONENTS_SOURCE)]
    [EventCategoryId(ConfigurationConstants.CORE_CATEGORY)]
    [EventId(ConfigurationConstants.DIAGNOSTICS_LOG_FAILED_ID)]
    [EventMessage("Failed to log diagnostics information.")]
    public sealed class SqlTableDiagnosticsLoggerError : LogEventBase
    {
    }
}