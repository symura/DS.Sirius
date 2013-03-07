using DS.Sirius.Core.Logging;

namespace DS.Sirius.Core.Diagnostics
{
    /// <summary>
    /// This interface defines the responsibility of a diagnostics logger object.
    /// </summary>
    public interface IDiagnosticsLogger : ILogger<DiagnosticsLogItem>
    {
    }
}