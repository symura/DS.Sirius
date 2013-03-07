using DS.Sirius.Core.Logging;

namespace DS.Sirius.Core.Audit
{
    /// <summary>
    /// This class is intended to be the base class of all audit logger object.
    /// </summary>
    public abstract class AuditLoggerBase :
        LoggerBase<AuditLogItem, IAuditLogFormatter>,
        IAuditLogger
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        protected AuditLoggerBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        /// <param name="formatter">Formatter to use with this object</param>
        protected AuditLoggerBase(ILogFormatter<AuditLogItem> formatter)
        {
            Formatter = formatter;
        }

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
        public abstract string LogStartEvent(AuditLogItem item);
    }
}