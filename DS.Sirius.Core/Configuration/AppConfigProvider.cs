using System;
using System.Configuration;
using System.Xml.Linq;
using DS.Sirius.Core.Common;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// This class represents a configuration provider that uses the 
    /// application configuration file to obtain and write configuration settings
    /// </summary>
    public class AppConfigProvider: IConfigurationProvider
    {
        /// <summary>
        /// Gets a setting value with the specified key.
        /// </summary>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>Settings value</returns>
        public string GetSettingValue(string settingKey)
        {
            return ConfigurationManager.AppSettings[settingKey];
        }

        /// <summary>
        /// Gets a compound setting object with the specified key.
        /// </summary>
        /// <typeparam name="TSetting">Type of the configuration setting</typeparam>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>XML element representing the setting</returns>
        public TSetting GetSection<TSetting>(string settingKey) 
            where TSetting : IXElementRepresentable, new()
        {
            var section = ConfigurationManager.GetSection(settingKey);
            if (section == null)
            {
                throw new ConfigurationErrorsException(
                    String.Format("Configuration section '{0}' cannot be found",
                    settingKey));
            }
            var element = (XElement) section;
            var result = new TSetting();
            result.ReadFromXml(element);
            return result;
        }

        /// <summary>
        /// Sets a setting value with the specified key.
        /// </summary>
        /// <param name="settingKey">Key of application setting</param>
        /// <param name="value">Settings value</param>
        public void SetSettingValue(string settingKey, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var setting = config.AppSettings.Settings[settingKey];
            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                config.AppSettings.Settings.Add(settingKey, value);
            }
            config.Save();
        }

        /// <summary>
        /// Sets a compound setting object with the specified key.
        /// </summary>
        /// <typeparam name="TSetting">Type of the configuration setting</typeparam>
        /// <param name="settingKey">Key of application setting</param>
        /// <param name="value">Setting value</param>
        public void SetSection<TSetting>(string settingKey, TSetting value) where TSetting : IXElementRepresentable, new()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.GetSection(settingKey).SectionInformation.SetRawXml(value.WriteToXml(settingKey).ToString());
            config.Save();
        }

        /// <summary>
        /// Checks whether the specified configuration value is defined.
        /// </summary>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>True, if configuration value is defined; otherwise, false</returns>
        public bool IsSettingValueDefined(string settingKey)
        {
            return ConfigurationManager.AppSettings[settingKey] != null;
        }

        /// <summary>
        /// Checks whether the specified configuration section is defined.
        /// </summary>
        /// <param name="settingKey">Key of application setting</param>
        /// <returns>True, if configuration section is defined; otherwise, false</returns>
        public bool IsSectionDefined(string settingKey)
        {
            return ConfigurationManager.GetSection(settingKey) != null;
        }
    }
}