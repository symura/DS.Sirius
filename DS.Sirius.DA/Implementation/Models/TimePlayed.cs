// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace DS.Sirius.DA.Implementation.Models
{
    [DataContract]
    public class TimePlayed
    {
        [DataMember]
        public double barbarian { get; set; }
        [DataMember]
        [JsonProperty("demon-hunter")]
        public double demonhunter { get; set; }
        [DataMember]
        public double monk { get; set; }
        [DataMember]
        [JsonProperty("witch-doctor")]
        public double witchdoctor { get; set; }
        [DataMember]
        public double wizard { get; set; }
    }
}
