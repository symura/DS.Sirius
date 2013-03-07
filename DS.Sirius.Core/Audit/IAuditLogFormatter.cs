using DS.Sirius.Core.Logging;

namespace DS.Sirius.Core.Audit
{
    /// <summary>
    /// This interface defines the responsibility of an audit log formatter object.
    /// </summary>
    public interface IAuditLogFormatter : ILogFormatter<AuditLogItem>
    {
    }
}