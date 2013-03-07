// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace DS.Sirius.DA.Implementation.Models
{
    [DataContract]
    public class Head
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string icon { get; set; }
        [DataMember]
        public string displayColor { get; set; }
        [DataMember]
        public string tooltipParams { get; set; }
    }
}
