using System;
using System.Text;
using System.Threading;
using DS.Sirius.Core.Aspects;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.Logging;

namespace DS.Sirius.Core.Diagnostics
{
    /// <summary>
    /// This class represents a log item for diagnostics messages
    /// </summary>
    public class DiagnosticsLogItem: ILoggable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DiagnosticsLogItem()
        {
            // --- Set operation instance ID from context
            var operationInstanceData = CallContextHandler.GetData<OperationInstanceIdContextItem>();
            var operationInstanceId = operationInstanceData == null
                               ? Guid.NewGuid()
                               : operationInstanceData.Id;
            OperationInstanceId = operationInstanceId;

            // --- Set tenant ID from context
            var tenantId = CallContextHandler.GetData<TenantIdContextItem>();
            TenantId = tenantId == null ? null : tenantId.Id;


            // --- Set machine and thread information
            ServerName = AppConfigurationManager.GetMachineName();
            InstanceName = AppConfigurationManager.Settings.InstancePrefix;
            ThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DiagnosticsLogItem(Guid instanceId)
        {
            OperationInstanceId = instanceId;
        }

        /// <summary>Gets or sets the ID of the tenant</summary>
        public string TenantId { get; set; }

        /// <summary>Gets the instance id of the log entry</summary>
        public Guid OperationInstanceId { get; private set; }

        /// <summary>Gets or sets the time of the event occurrence</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>Gets or sets the source of the entry</summary>
        public string Source { get; set; }

        /// <summary>Gets or sets the message of the entry</summary>
        public string Message { get; set; }

        /// <summary>Gets or sets the message related data of the entry</summary>
        public string DetailedMessage { get; set; }

        /// <summary>Gets or sets the type of the message</summary>
        public DiagnosticsLogItemType Type { get; set; }

        /// <summary>Gets or sets the optional server name</summary>
        public string ServerName { get; set; }

        /// <summary>Gets or sets the optional instance name</summary>
        public string InstanceName { get; set; }

        /// <summary>Gets or sets the Id of the thread raising the message</summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// Creates a string representation of this object using the log data properties
        /// </summary>
        /// <returns>String representation of this object</returns>
        /// <remarks>
        /// This method is for debugging purposes and not for real productive use.
        /// </remarks>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("{0:yyyy.MM.dd hh:mm:ss.fff} - {1}: {2} {{{3}}}\n",
                Timestamp,  // {0}
                OperationInstanceId, // {1}
                Source,     // {2}
                Message     // {3}
                );
            return builder.ToString();
        }
    }
}