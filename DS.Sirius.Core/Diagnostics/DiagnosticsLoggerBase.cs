using DS.Sirius.Core.Logging;

namespace DS.Sirius.Core.Diagnostics
{
    /// <summary>
    /// This class is intended to be the base class of all diagnostics logger object.
    /// </summary>
    public abstract class DiagnosticsLoggerBase :
        LoggerBase<DiagnosticsLogItem, IDiagnosticsLogFormatter>,
        IDiagnosticsLogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        protected DiagnosticsLoggerBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="formatter">Formatter to use with this object</param>
        protected DiagnosticsLoggerBase(ILogFormatter<DiagnosticsLogItem> formatter)
        {
            Formatter = formatter;
        }
    }
}