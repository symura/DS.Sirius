using System.Diagnostics;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.WindowsEventLog;

namespace DS.Sirius.Core.Audit
{
    /// <summary>
    /// This class represents the event when an audit log operation failed.
    /// </summary>
    [EventType(EventLogEntryType.Error)]
    [EventLogName(ConfigurationConstants.CORE_LOG_NAME)]
    [EventSource(ConfigurationConstants.AZURE_COMPONENTS_SOURCE)]
    [EventCategoryId(ConfigurationConstants.CORE_CATEGORY)]
    [EventId(ConfigurationConstants.AUDIT_LOG_FAILED_ID)]
    [EventMessage("Audit log failed.")]
    public sealed class AuditLogFailed : LogEventBase
    {
    }

    /// <summary>
    /// This class represents the event that the AuditLogManager was not configured.
    /// </summary>
    [EventType(EventLogEntryType.Error)]
    [EventLogName(ConfigurationConstants.CORE_LOG_NAME)]
    [EventSource(ConfigurationConstants.AZURE_COMPONENTS_SOURCE)]
    [EventCategoryId(ConfigurationConstants.CORE_CATEGORY)]
    [EventId(ConfigurationConstants.AUDIT_LOG_MANAGER_WAS_CONFIGURED_ID)]
    [EventMessage("The AuditLogManager was not configured before usage")]
    public sealed class AuditLogManagerWasNotConfigured : LogEventBase
    {
    }

    /// <summary>
    /// This class represents an event when the SqlTableAuditLogger raises an exception
    /// </summary>
    [EventType(EventLogEntryType.Error)]
    [EventLogName(ConfigurationConstants.CORE_LOG_NAME)]
    [EventSource(ConfigurationConstants.AZURE_COMPONENTS_SOURCE)]
    [EventCategoryId(ConfigurationConstants.CORE_CATEGORY)]
    [EventId(ConfigurationConstants.AUDIT_LOG_FAILED_ID)]
    [EventMessage("Failed to log diagnostics information.")]
    public sealed class SqlTableAuditLoggerError : LogEventBase
    {
    }
}
