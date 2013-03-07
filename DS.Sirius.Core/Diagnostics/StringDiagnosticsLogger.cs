using System.IO;
using System.Text;

namespace DS.Sirius.Core.Diagnostics
{
    /// <summary>
    /// This type implements a logger that stores log messages in a string within the memory.
    /// </summary>
    public class StringDiagnosticsLogger : TextWriterDiagnosticsLogger
    {
        private readonly StringBuilder _builder = new StringBuilder(1000);

        /// <summary>
        /// Constructs a new writer that redirects the log entries into a StringBuilder.
        /// </summary>
        public StringDiagnosticsLogger()
        {
            Writer = new StringWriter(_builder);
        }

        /// <summary>
        /// Retrieves the string within the StringBuilder.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}