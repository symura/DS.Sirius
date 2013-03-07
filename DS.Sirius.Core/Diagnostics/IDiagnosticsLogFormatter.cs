using DS.Sirius.Core.Logging;

namespace DS.Sirius.Core.Diagnostics
{
    /// <summary>
    /// This interface defines the responsibility of a log formatter object.
    /// </summary>
    public interface IDiagnosticsLogFormatter : ILogFormatter<DiagnosticsLogItem>
    {
    }
}