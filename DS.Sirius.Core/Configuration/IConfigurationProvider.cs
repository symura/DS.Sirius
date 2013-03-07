using System.Collections.Generic;
using DS.Sirius.Core.Common;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// This interface describes the provider that allows application
    /// components to access their configuration.
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Gets a setting value with the specified key.
        /// </summary>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>Settings value</returns>
        string GetSettingValue(string settingKey);

        /// <summary>
        /// Gets a compound setting object with the specified key.
        /// </summary>
        /// <typeparam name="TSetting">Type of the configuration setting</typeparam>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>XML element representing the setting</returns>
        TSetting GetSection<TSetting>(string settingKey)
            where TSetting: IXElementRepresentable, new();

        /// <summary>
        /// Sets a setting value with the specified key.
        /// </summary>
        /// <param name="settingKey">Key of application setting</param>
        /// <param name="value">Settings value</param>
        void SetSettingValue(string settingKey, string value);

        /// <summary>
        /// Sets a compound setting object with the specified key.
        /// </summary>
        /// <typeparam name="TSetting">Type of the configuration setting</typeparam>
        /// <param name="settingKey">Key of application setting</param>
        /// <param name="value">Setting value</param>
        void SetSection<TSetting>(string settingKey, TSetting value)
            where TSetting : IXElementRepresentable, new();

        /// <summary>
        /// Checks whether the specified configuration value is defined.
        /// </summary>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>True, if configuration value is defined; otherwise, false</returns>
        bool IsSettingValueDefined(string settingKey);

        /// <summary>
        /// Checks whether the specified configuration section is defined.
        /// </summary>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>True, if configuration section is defined; otherwise, false</returns>
        bool IsSectionDefined(string settingKey);
    }
}