// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace DS.Sirius.DA.Implementation.Models
{
    [DataContract]
    public class Stats
    {
        [DataMember]
        public int life { get; set; }
        [DataMember]
        public double damage { get; set; }
        [DataMember]
        public double attackSpeed { get; set; }
        [DataMember]
        public int armor { get; set; }
        [DataMember]
        public int strength { get; set; }
        [DataMember]
        public int dexterity { get; set; }
        [DataMember]
        public int vitality { get; set; }
        [DataMember]
        public int intelligence { get; set; }
        [DataMember]
        public int physicalResist { get; set; }
        [DataMember]
        public int fireResist { get; set; }
        [DataMember]
        public int coldResist { get; set; }
        [DataMember]
        public int lightningResist { get; set; }
        [DataMember]
        public int poisonResist { get; set; }
        [DataMember]
        public int arcaneResist { get; set; }
        [DataMember]
        public double critDamage { get; set; }
        [DataMember]
        public double blockChance { get; set; }
        [DataMember]
        public int blockAmountMin { get; set; }
        [DataMember]
        public int blockAmountMax { get; set; }
        [DataMember]
        public double damageIncrease { get; set; }
        [DataMember]
        public double critChance { get; set; }
        [DataMember]
        public double damageReduction { get; set; }
        [DataMember]
        public double thorns { get; set; }
        [DataMember]
        public double lifeSteal { get; set; }
        [DataMember]
        public double lifePerKill { get; set; }
        [DataMember]
        public double goldFind { get; set; }
        [DataMember]
        public double magicFind { get; set; }
        [DataMember]
        public double lifeOnHit { get; set; }
        [DataMember]
        public int primaryResource { get; set; }
        [DataMember]
        public int secondaryResource { get; set; }
    }
}
