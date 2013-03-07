using System;

namespace DS.Sirius.Core.Configuration.Environment
{
    public interface IEnvironmentInfoProvider
    {
        /// <summary>
        /// Returns the current DateTime as UTC DateTime.
        /// </summary>
        /// <returns>The current UTC DateTime</returns>
        DateTime GetCurrentDateTimeUtc();

        /// <summary>
        /// Returns the caller client's IP address.
        /// </summary>
        /// <returns>The caller client's IP address.</returns>
        string GetClientIpAddress();

        /// <summary>
        /// Gets the NetBIOS name of this local computer.
        /// </summary>
        /// <returns></returns>
        string GetMachineName();
    }
}