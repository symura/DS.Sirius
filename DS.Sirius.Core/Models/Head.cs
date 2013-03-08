﻿// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DS.Sirius.Core.Models
{
    [DataContract]
    public class Head
    {
        [DataMember]
        [JsonProperty("id")]
        public string Id { get; set; }
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }
        [DataMember]
        [JsonProperty("icon")]
        public string Icon { get; set; }
        [DataMember]
        [JsonProperty("displayColor")]
        public string DisplayColor { get; set; }
        [DataMember]
        [JsonProperty("tooltipParams")]
        public string TooltipParams { get; set; }
    }
}
