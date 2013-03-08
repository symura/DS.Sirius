// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace DS.Sirius.Core.Models
{
    [DataContract]
    public class TimePlayed
    {
        [DataMember]
        [JsonProperty("barbarian")]
        public double Barbarian { get; set; }
        [DataMember]
        [JsonProperty("demon-hunter")]
        public double Demonhunter { get; set; }
        [DataMember]
        [JsonProperty("monk")]
        public double Monk { get; set; }
        [DataMember]
        [JsonProperty("witch-doctor")]
        public double Witchdoctor { get; set; }
        [DataMember]
        [JsonProperty("wizard")]
        public double Wizard { get; set; }
    }
}
