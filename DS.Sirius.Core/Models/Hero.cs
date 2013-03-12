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
    public class Hero
    {
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }
        [DataMember]
        [JsonProperty("id")]
        public long Id { get; set; }
        [DataMember]
        [JsonProperty("level")]
        public int Level { get; set; }
        [DataMember]
        [JsonProperty("hardcore")]
        public bool Hardcore { get; set; }
        [DataMember]
        [JsonProperty("paragonLevel")]
        public int ParagonLevel { get; set; }
        [DataMember]
        [JsonProperty("gender")]
        public int Gender { get; set; }
        [DataMember]
        [JsonProperty("class")]
        public string CharacterClass { get; set; }
        [DataMember]
        [JsonProperty("lastUpdated")]
        public int LastUpdated { get; set; }
        [DataMember]
        [JsonProperty("Dead")]
        public bool Dead { get; set; }
        [DataMember]
        [JsonProperty("stats")]
        public Stats Stats { get; set; }
        [DataMember]
        [JsonProperty("items")]
        public Items Items { get; set; }

        
    }
}
