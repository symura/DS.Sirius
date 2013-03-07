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
    public class Hero
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public long id { get; set; }
        [DataMember]
        public int level { get; set; }
        [DataMember]
        public bool hardcore { get; set; }
        [DataMember]
        public int paragonLevel { get; set; }
        [DataMember]
        public int gender { get; set; }
        [DataMember]
        [JsonProperty("class")]
        public string characterClass { get; set; }
        [DataMember]
        public int lastUpdated { get; set; }
        [DataMember]
        public bool dead { get; set; }
        [DataMember]
        public Stats stats { get; set; }
        
    }
}
