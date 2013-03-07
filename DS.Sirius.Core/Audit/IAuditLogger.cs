using DS.Sirius.Core.Logging;

namespace DS.Sirius.Core.Audit
{
    /// <summary>
    /// This interface defines the responsibility of an audit logger object.
    /// </summary>
    public interface IAuditLogger : ILogger<AuditLogItem>
    {
        /// <summary>
        /// This method can be used to log the item point of an operation
        /// </summary>
        /// <param name="item">Audit log item to record</param>
        /// <returns>Correlation identifier</returns>
        /// <remarks>
        /// The correlation identifier makes it possible to update the audit
        /// event log when an operation has been completed (either with success
        /// or failure).
        /// </remarks>
        string LogStartEvent(AuditLogItem item);
    }
}