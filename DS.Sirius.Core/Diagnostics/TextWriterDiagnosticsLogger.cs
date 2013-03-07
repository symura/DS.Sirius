using System;
using System.IO;
using DS.Sirius.Core.Logging;

namespace DS.Sirius.Core.Diagnostics
{
    /// <summary>
    /// This class writes a log entry to a specified <see cref="TextWriter"/> object.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IDisposable"/> interface. The default 
    /// implementation closes the TextWriter object passed to the constructor.
    /// </remarks>
    public class TextWriterDiagnosticsLogger :
        TextWriterLoggerBase<DiagnosticsLogItem, IDiagnosticsLogFormatter>,
        IDiagnosticsLogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="formatter">Formatter to use with this object</param>
        /// <param name="writer">TextWriter object to write a log entry to</param>
        public TextWriterDiagnosticsLogger(IDiagnosticsLogFormatter formatter, TextWriter writer)
            : base(formatter, writer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="writer">TextWriter object to write a log entry to</param>
        public TextWriterDiagnosticsLogger(TextWriter writer)
            : base(writer)
        {
        }

        /// <summary>
        /// Initializes a logger instance and lets derived classes to set up its members
        /// </summary>
        protected TextWriterDiagnosticsLogger()
        {
        }
    }
}