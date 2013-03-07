namespace DS.Sirius.Core.Security
{
    /// <summary>
    /// This interface represents an object that can be digitally signed with a certificate.
    /// </summary>
    public interface ISignableDocument
    {
        /// <summary>
        /// Extracts the document from the object to sign.
        /// </summary>
        /// <returns>String representation of the document</returns>
        string GetDocument();

        /// <summary>
        /// Gets the signature string of a signed document.
        /// </summary>
        /// <returns>The string representing the signature of the document</returns>
        string GetSignatureString();
    }
}