using System;
using System.Collections.Generic;
using DS.Sirius.Core.Aspects;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.Logging;

namespace DS.Sirius.Core.Audit
{
    /// <summary>
    /// This class represents a log item for diagnostics messages
    /// </summary>
    public class AuditLogItem: ILoggable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public AuditLogItem()
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

            // --- Set machine and intance info
            ServerName = AppConfigurationManager.GetMachineName();
            InstanceName = AppConfigurationManager.Settings.InstancePrefix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public AuditLogItem(Guid instanceId)
        {
            OperationInstanceId = instanceId;
        }

        /// <summary>Gets or sets the ID of the tenant</summary>
        public string TenantId { get; set; }

        /// <summary>Gets the instance id of the log item</summary>
        public Guid OperationInstanceId { get; private set; }

        /// <summary>Optional identifier of a correlated audit log item</summary>
        public string CorrelationId { get; set; }

        /// <summary>Gets or sets the collection of audit log parameters</summary>
        public List<AuditLogParameter> Parameters { get; set; }

        /// <summary>Gets or sets the identifier of the operation</summary>
        public string OperationId { get; set; }

        /// <summary>Gets or sets the information about the user executing the operation</summary>
        public string UserInfo { get; set; }

        /// <summary>Gets or sets the time of the event occurrence</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>Gets the flag indicating whether the operation was successful</summary>
        public bool IsSuccessfull { get; set; }

        /// <summary>Gets or sets the information about the client</summary>
        public string ClientInfo { get; set; }

        /// <summary>Gets or sets the information about the server</summary>
        public string ServerName { get; set; }

        /// <summary>Gets or sets the information about the instance</summary>
        public string InstanceName { get; set; }

        /// <summary>Gets or sets error information in case of failure</summary>
        public string ErrorInfo { get; set; }

        /// <summary>Gets or sets the execution time of the operation in milliseconds</summary>
        public double? ExecutionTime { get; set; }
    }
}