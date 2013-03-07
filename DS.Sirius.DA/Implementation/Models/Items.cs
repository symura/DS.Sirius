// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace DS.Sirius.DA.Implementation.Models
{

    [DataContract]
    public class Items
    {
        [DataMember]
        public Head head { get; set; }
        //public Torso torso { get; set; }
        //public Feet feet { get; set; }
        //public Hands hands { get; set; }
        //public Shoulders shoulders { get; set; }
        //public Legs legs { get; set; }
        //public Bracers bracers { get; set; }
        //public MainHand mainHand { get; set; }
        //public OffHand offHand { get; set; }
        //public Waist waist { get; set; }
        //public RightFinger rightFinger { get; set; }
        //public LeftFinger leftFinger { get; set; }
        //public Neck neck { get; set; }
    }
}
