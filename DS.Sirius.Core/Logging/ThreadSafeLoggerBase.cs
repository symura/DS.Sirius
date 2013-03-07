using System.Runtime.CompilerServices;

namespace DS.Sirius.Core.Logging
{
    /// <summary>
    /// This class is intended to be the base class of all loggers that provide thread safe log
    /// operations.
    /// </summary>
    /// <typeparam name="TLogData">Data structure representing an entry to log</typeparam>
    /// <typeparam name="TFormatter">Formatter object that can format the log entry</typeparam>
    /// <remarks>
    /// <para>
    /// This class inherits its behavior from <see cref="LoggerBase{TLogData,TFormatter}"/>, so you
    /// can read the detailed documentation of using this logger class.
    /// </para>
    /// <para>
    /// In addition to <see cref="LoggerBase{TLogData,TFormatter}"/>, this class provides a 
    /// thread-safe <see cref="LoggerBase{TLogData,TFormatter}.OnLogging"/> method.
    /// </para>
    /// </remarks>
    public abstract class ThreadSafeLoggerBase<TLogData, TFormatter> : LoggerBase<TLogData, TFormatter> 
        where TLogData : ILoggable where TFormatter : ILogFormatter<TLogData>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        protected ThreadSafeLoggerBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of this class with the specified formatter.
        /// </summary>
        /// <param name="formatter">Formatter to use with this object</param>
        protected ThreadSafeLoggerBase(TFormatter formatter)
            : base(formatter)
        {
        }

        /// <summary>
        /// Writes the specified <paramref name="entry"/> to the log.
        /// </summary>
        /// <param name="entry">Entry describing the message to be logged</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Log(TLogData entry)
        {
            base.Log(entry);
        }
    }
}