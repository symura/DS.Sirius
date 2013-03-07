using System.IO;

namespace DS.Sirius.Core.Logging
{
    /// <summary>
    /// This class is intended to be the base class of loggers that write their output 
    /// to a <see cref="TextWriter"/> object.
    /// </summary>
    /// <typeparam name="TLogData">Data structure representing an entry to log</typeparam>
    /// <typeparam name="TFormatter">Formatter object that can format the log entry</typeparam>
    public abstract class TextWriterLoggerBase<TLogData, TFormatter> : LoggerBase<TLogData, TFormatter>
        where TLogData : ILoggable
        where TFormatter : ILogFormatter<TLogData>
    {
        private TextWriter _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="formatter">Formatter to use with this object</param>
        /// <param name="writer">TextWriter object to write a log entry to</param>
        protected TextWriterLoggerBase(TFormatter formatter, TextWriter writer)
            : base(formatter)
        {
            _writer = writer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <param name="writer">TextWriter object to write a log entry to</param>
        protected TextWriterLoggerBase(TextWriter writer)
        {
            _writer = writer;
        }

        /// <summary>
        /// Initializes a logger instance and lets derived classes to set up its members
        /// </summary>
        protected TextWriterLoggerBase()
        {
        }

        /// <summary>
        /// Gets the <see cref="TextWriter"/> object.
        /// </summary>
        public TextWriter Writer
        {
            get { return _writer; }
            protected set { _writer = value; }
        }

        /// <summary>
        /// Writes the specified <paramref name="entry"/> to the log.
        /// </summary>
        /// <param name="entry">Entry describing the message to be logged</param>
        protected override void OnLogging(TLogData entry)
        {
            _writer.Write(Formatter.Format(entry));
        }

        /// <summary>
        /// Override this method to define activities when cleaning up the logger.
        /// </summary>
        protected override void OnLogCleanUp()
        {
            if (_writer != null)
            {
                _writer.Close();
            }
            base.OnLogCleanUp();
        }
    }
}