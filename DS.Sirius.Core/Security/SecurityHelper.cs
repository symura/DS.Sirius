using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using DS.Sirius.Core.Configuration;

namespace DS.Sirius.Core.Security
{
    public static class SecurityHelper
    {
        /// <summary>
        /// Signs the specified document with the provided certificate
        /// </summary>
        /// <param name="document">Document to sign</param>
        /// <param name="certificateResourceKey">Resource connection key to the certificate</param>
        /// <returns>Base64 encoded digital signature</returns>
        public static string SignDocument(this ISignableDocument document, string certificateResourceKey)
        {
            var certificate = AppConfigurationManager
                .CreateResourceConnection<X509Certificate2>(certificateResourceKey);
            return SignDocument(document, certificate);
        }

        /// <summary>
        /// Signs the specified document with the provided certificate
        /// </summary>
        /// <param name="document">Document to sign</param>
        /// <param name="certificate">Certificate to sigh the document with</param>
        /// <returns>Base64 encoded digital signature</returns>
        public static string SignDocument(this ISignableDocument document, X509Certificate2 certificate)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (certificate == null) throw new ArgumentNullException("certificate");
            var content = document.GetDocument();
            if (content == null) throw new InvalidOperationException("Document content cannot be null");
            var buffer = Encoding.Default.GetBytes(content);
            var privateKey = certificate.PrivateKey as RSACryptoServiceProvider;
            if (privateKey == null) return string.Empty;
            var signature = privateKey.SignData(buffer, new SHA1Managed());
            var sb = new StringBuilder();
            foreach (var piece in signature)
            {
                sb.AppendFormat("{0:X2}", piece);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Verifies if the signature on the specified document is correct
        /// </summary>
        /// <param name="document">Document to verify</param>
        /// <param name="certificate">Certificate used to generate the digital signature</param>
        /// <returns>True, if the signature is valid; otherwise, false</returns>
        public static bool VerifyDocument(this ISignableDocument document, X509Certificate2 certificate)
        {
            var docSignature = document.GetSignatureString();
            if (docSignature.Length%2 == 1) docSignature = "0" + docSignature;
            var length = docSignature.Length/2;
            var signatureBytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                byte oneByte;
                signatureBytes[i] = byte.TryParse(docSignature.Substring(i*2, 2), NumberStyles.HexNumber, 
                    CultureInfo.InstalledUICulture, out oneByte) 
                    ? oneByte : (byte)0;
            }
            var content = document.GetDocument();
            if (content == null) throw new InvalidOperationException("Document content cannot be null");
            var buffer = Encoding.Default.GetBytes(content);
            var publicKey = certificate.PublicKey.Key as RSACryptoServiceProvider;
            if (publicKey == null) throw new InvalidOperationException("Certificate does not support RSA");
            return publicKey.VerifyData(buffer, new SHA1Managed(), signatureBytes);
        }
    }
}