using System.Xml.Linq;

namespace DS.Sirius.Core.Common
{
    /// <summary>
    /// This interface defines the behavior of an object that can represent itself as an XElement object.
    /// </summary>
    public interface IXElementRepresentable
    {
        /// <summary>
        /// Writes the object to an XElement instance.
        /// </summary>
        /// <returns>XElement representation of the object</returns>
        XElement WriteToXml(XName name);

        /// <summary>
        /// Reads the object from an XElement instance.
        /// </summary>
        /// <param name="element">XElement object</param>
        /// <returns>Object read from the XElement</returns>
        void ReadFromXml(XElement element);
    }
}