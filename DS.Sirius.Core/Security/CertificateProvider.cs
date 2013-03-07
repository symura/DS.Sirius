using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using DS.Sirius.Core.Configuration;
using DS.Sirius.Core.Configuration.ResourceConnection;

namespace DS.Sirius.Core.Security
{
    [DisplayName("Certificate")]
    public class CertificateProvider : ResourceConnectionProviderBase
    {
        private const string STORE_NAME = "storeName";
        private const string CLUE = "clue";

        /// <summary>
        /// Gets the name of the certification store
        /// </summary>
        public string Store { get; private set; }

        public string Clue { get; private set; }

        /// <summary>
        /// Creates a new instance of this class from the specified parameters
        /// </summary>
        /// <param name="clue">Certificate clue</param>
        /// <param name="storeName">Optional store name</param>
        public CertificateProvider(string clue, string storeName = null)
        {
            Clue = clue;
            Store = storeName;
        }

        /// <summary>
        /// Creates an instance of this class from the specified XML element
        /// </summary>
        /// <param name="element">XML element</param>
        public CertificateProvider(XElement element)
        {
            ReadFromXml(element);
        }

        /// <summary>
        /// Adds additional XElement and XAttribute settings to the XML representation
        /// </summary>
        /// <returns></returns>
        public override List<XObject> GetAdditionalSettings()
        {
            var settings = base.GetAdditionalSettings();
            if (Store != null) settings.Add(new XAttribute(STORE_NAME, Store));
            settings.Add(new XAttribute(CLUE, Clue));
            return settings;
        }

        /// <summary>
        /// Parse the specified configuration settings
        /// </summary>
        /// <param name="element">Element holding configuration settings</param>
        protected override void ParseFrom(XElement element)
        {
            base.ParseFrom(element);
            Store = element.OptionalStringAttribute(STORE_NAME, null);
            Clue = element.StringAttribute(CLUE);
        }

        /// <summary>
        /// Creates a new resource connection object from the settings.
        /// </summary>
        /// <returns>Newly created resource object</returns>
        public override object GetResourceConnectionFromSettings()
        {
            var storeName = Store == null
                                ? StoreName.My
                                : (StoreName) Enum.Parse(typeof (StoreName), Store);
            var store = new X509Store(storeName, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates
                .Cast<X509Certificate2>()
                .FirstOrDefault(cert => cert.SubjectName.Name == Clue || cert.Thumbprint == Clue);
        }
    }
}