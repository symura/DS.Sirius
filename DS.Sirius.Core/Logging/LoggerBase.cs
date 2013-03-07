using System;

namespace DS.Sirius.Core.Logging
{
    /// <summary>
    /// This class is intended to be the base class of all loggers.
    /// </summary>
    /// <typeparam name="TLogData">Data structure representing an entry to log</typeparam>
    /// <typeparam name="TFormatter">Formatter object that can format the log entry</typeparam>
    /// <remarks>
    /// <para>
    /// The key operation of the logger is <see cref="Log"/>. Although you can override this 
    /// method in derived logger classes, to change the way of logging entirely, you'd better 
    /// override one of the <see cref="OnSetup"/>, <see cref="OnLogging"/>, 
    /// <see cref="OnLogException"/> and <see cref="OnLogCleanUp"/> methods.
    /// </para>
    /// <para>
    /// The <see cref="Log"/> method is not thread safe.
    /// </para>
    /// <para>
    /// The <see cref="OnSetup"/> method is called only once when the <see cref="Log"/> method
    /// is first called. Override this method to set up the logger instance, and do not initialize
    /// the logger instance in its constructor! If, in any case, you should execute the 
    /// <see cref="OnSetup"/> method later, call the <see cref="RequestSetup"/> method.
    /// </para>
    /// <para>
    /// When logging an entry, the <see cref="OnLogging"/> method is called, you must handle thread-
    /// safety on your own within this method.
    /// </para>
    /// <para>
    /// If any kind of exception occurres during <see cref="Log"/>, the <see cref="OnLogException"/>
    /// method is invoked. It may handle the exception on its own way. By default this method
    /// raises the exception.
    /// </para>
    /// <para>
    /// When the logger object is about to be disposed, the <see cref="OnLogCleanUp"/> method is
    /// invoked, here you must clean up all resources used by your logger implementation.
    /// </para>
    /// </remarks>
    public abstract class LoggerBase<TLogData, TFormatter> : ILogger<TLogData>, IDisposable
        where TLogData : ILoggable
        where TFormatter : ILogFormatter<TLogData>
    {
        /// <summary>Indicates whether the initial setup has been done.</summary>
        protected bool _setupInvoked;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        protected LoggerBase()
        {
            Formatter = new DefaultLogFormatter<TLogData>();
        }

        /// <summary>
        /// Initializes a new instance of this class with the specified formatter.
        /// </summary>
        /// <param name="formatter">Formatter to use with this object</param>
        protected LoggerBase(TFormatter formatter)
        {
            Formatter = formatter;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        /// <remarks>
        /// By default, this method invokes <see cref="OnLogCleanUp"/>
        /// </remarks>
        public virtual void Dispose()
        {
            OnLogCleanUp();
        }

        /// <summary>
        /// This method asks for running the setup method when the next Log 
        /// operation is started.
        /// </summary>
        public void RequestSetup()
        {
            _setupInvoked = false;
        }

        /// <summary>
        /// Gets the formatter belonging to this logger
        /// </summary>
        public ILogFormatter<TLogData> Formatter { get; protected set; }

        /// <summary>
        /// Writes the specified <paramref name="entry"/> to the log.
        /// </summary>
        /// <param name="entry">Entry describing the message to be logged</param>
        public virtual void Log(TLogData entry)
        {
            try
            {
                if (!_setupInvoked)
                {
                    // --- This flag is intentionally set to true, before calling OnSetup,
                    // --- only one setup attempt is allowed.
                    _setupInvoked = true;
                    OnSetup();
                }
                OnLogging(entry);
            }
            catch (Exception ex)
            {
                OnLogException(entry, ex);
            }
        }

        /// <summary>
        /// Override this method to set up the logger just right before the
        /// first item is to be written to the log.
        /// </summary>
        protected virtual void OnSetup()
        {
        }

        /// <summary>
        /// Override this method to carry out writing the entry to the log.
        /// </summary>
        /// <param name="entry">Log entry to be written to the log.</param>
        protected virtual void OnLogging(TLogData entry)
        { }

        /// <summary>
        /// Override this method to handle the exception raised during the log
        /// operation.
        /// </summary>
        /// <param name="entry">Entry to log</param>
        /// <param name="exception">Exception raised during logging</param>
        /// <remarks>
        /// This operation hides the exception be default.
        /// </remarks>
        protected virtual void OnLogException(TLogData entry, Exception exception)
        {
            throw exception;
        }

        /// <summary>
        /// Override this method to define activities when cleaning up the logger.
        /// </summary>
        protected virtual void OnLogCleanUp()
        {
        }
    }
}