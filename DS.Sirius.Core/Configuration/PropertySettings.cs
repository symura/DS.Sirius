using System;

namespace DS.Sirius.Core.Configuration
{
    /// <summary>
    /// Stores a property of a configuration setting
    /// </summary>
    public struct PropertySettings
    {
        public string Name;
        public Type Type;
        public string Value;
    }
}