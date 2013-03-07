using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace DS.Sirius.Core.Configuration.Environment
{
    public class DefaultEnvironmentInfoProvider : IEnvironmentInfoProvider
    {
        /// <summary>
        /// Returns the current DateTime as UTC DateTime.
        /// </summary>
        /// <returns>The current UTC DateTime</returns>
        public DateTime GetCurrentDateTimeUtc()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// Returns the caller client's IP address.
        /// </summary>
        /// <returns>The caller client's IP address.</returns>
        public string GetClientIpAddress()
        {
            string result = null;
            OperationContext operationContext = OperationContext.Current;
            if (operationContext != null)
            {
                var clientInfo = operationContext.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                if (clientInfo != null)
                {
                    result = clientInfo.Address;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the NetBIOS name of this local computer.
        /// </summary>
        /// <returns></returns>
        public string GetMachineName()
        {
            return System.Environment.MachineName;
        }
    }
}