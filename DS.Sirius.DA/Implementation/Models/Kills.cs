// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace DS.Sirius.DA.Implementation.Models
{
    [DataContract]
    public class Kills
    {
        [DataMember]
        public int monsters { get; set; }
        [DataMember]
        public int elites { get; set; }
        [DataMember]
        public int hardcoreMonsters { get; set; }
    }
}
