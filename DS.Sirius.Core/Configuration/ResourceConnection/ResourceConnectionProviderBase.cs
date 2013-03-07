namespace DS.Sirius.Core.Configuration.ResourceConnection
{
    public abstract class ResourceConnectionProviderBase : 
        ResourceConnectionSettingsBase,
        IResourceConnectionProvider
    {
        /// <summary>
        /// Creates a new resource connection object from the settings.
        /// </summary>
        /// <returns>Newly created resource object</returns>
        public abstract object GetResourceConnectionFromSettings();
    }
}