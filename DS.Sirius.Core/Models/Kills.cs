// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DS.Sirius.Core.Models
{
    [DataContract]
    public class Kills
    {
        [DataMember]
        [JsonProperty("monsters")]
        public int Monsters { get; set; }
        [DataMember]
        [JsonProperty("elites")]
        public int Elites { get; set; }
        [DataMember]
        [JsonProperty("hardcoreMonsters")]
        public int HardcoreMonsters { get; set; }
    }
}
