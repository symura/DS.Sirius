// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DS.Sirius.Core.Models
{

    [DataContract]
    public class Items
    {
        [DataMember]
        [JsonProperty("head")]
        public GearItem Head { get; set; }
        [DataMember]
        [JsonProperty("torso")]
        public GearItem Torso { get; set; }
        [DataMember]
        [JsonProperty("feet")]
        public GearItem Feet { get; set; }
        [DataMember]
        [JsonProperty("hands")]
        public GearItem Hands { get; set; }
        [DataMember]
        [JsonProperty("shoulders")]
        public GearItem Shoulders { get; set; }
        [DataMember]
        [JsonProperty("legs")]
        public GearItem Legs { get; set; }
        [DataMember]
        [JsonProperty("bracers")]
        public GearItem Bracers { get; set; }
        [DataMember]
        [JsonProperty("mainHand")]
        public GearItem MainHand { get; set; }
        [DataMember]
        [JsonProperty("offHand")]
        public GearItem OffHand { get; set; }
        [DataMember]
        [JsonProperty("waist")]
        public GearItem Waist { get; set; }
        [DataMember]
        [JsonProperty("rightFinger")]
        public GearItem RightFinger { get; set; }
        [DataMember]
        [JsonProperty("leftFinger")]
        public GearItem LeftFinger { get; set; }
        [DataMember]
        [JsonProperty("neck")]
        public GearItem Neck { get; set; }
    }
}
