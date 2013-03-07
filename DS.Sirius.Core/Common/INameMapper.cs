namespace DS.Sirius.Core.Common
{
    /// <summary>
    /// This interface defines the behavior of an object that is able to map event log related names.
    /// </summary>
    public interface INameMapper
    {
        /// <summary>
        /// Maps the specified event log related name to another name.
        /// </summary>
        /// <param name="name">Source name</param>
        /// <returns>Mapped name</returns>
        string Map(string name);
    }
}