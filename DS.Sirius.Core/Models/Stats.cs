// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DS.Sirius.Core.Models
{
    [DataContract]
    public class Stats
    {
        [DataMember]
        [JsonProperty("life")]
        public int Life { get; set; }
        [DataMember]
        [JsonProperty("damage")]
        public double Damage { get; set; }
        [DataMember]
        [JsonProperty("attackSpeed")]
        public double AttackSpeed { get; set; }
        [DataMember]
        [JsonProperty("armor")]
        public int Armor { get; set; }
        [DataMember]
        [JsonProperty("strength")]
        public int Strength { get; set; }
        [DataMember]
        [JsonProperty("dexterity")]
        public int Dexterity { get; set; }
        [DataMember]
        [JsonProperty("vitality")]
        public int Vitality { get; set; }
        [DataMember]
        [JsonProperty("intelligence")]
        public int Intelligence { get; set; }
        [DataMember]
        [JsonProperty("physicalResist")]
        public int PhysicalResist { get; set; }
        [DataMember]
        [JsonProperty("fireResist")]
        public int FireResist { get; set; }
        [DataMember]
        [JsonProperty("coldResist")]
        public int ColdResist { get; set; }
        [DataMember]
        [JsonProperty("lightningResist")]
        public int LightningResist { get; set; }
        [DataMember]
        [JsonProperty("poisonResist")]
        public int PoisonResist { get; set; }
        [DataMember]
        [JsonProperty("arcaneResist")]
        public int ArcaneResist { get; set; }
        [DataMember]
        [JsonProperty("critDamage")]
        public double CritDamage { get; set; }
        [DataMember]
        [JsonProperty("blockChance")]
        public double BlockChance { get; set; }
        [DataMember]
        [JsonProperty("blockAmountMin")]
        public int BlockAmountMin { get; set; }
        [DataMember]
        [JsonProperty("blockAmountMax")]
        public int BlockAmountMax { get; set; }
        [DataMember]
        [JsonProperty("damageIncrease")]
        public double DamageIncrease { get; set; }
        [DataMember]
        [JsonProperty("critChance")]
        public double CritChance { get; set; }
        [DataMember]
        [JsonProperty("damageReduction")]
        public double DamageReduction { get; set; }
        [DataMember]
        [JsonProperty("thorns")]
        public double Thorns { get; set; }
        [DataMember]
        [JsonProperty("lifeSteal")]
        public double LifeSteal { get; set; }
        [DataMember]
        [JsonProperty("lifePerKill")]
        public double LifePerKill { get; set; }
        [DataMember]
        [JsonProperty("goldFind")]
        public double GoldFind { get; set; }
        [DataMember]
        [JsonProperty("magicFind")]
        public double MagicFind { get; set; }
        [DataMember]
        [JsonProperty("lifeOnHit")]
        public double LifeOnHit { get; set; }
        [DataMember]
        [JsonProperty("primaryResource")]
        public int PrimaryResource { get; set; }
        [DataMember]
        [JsonProperty("secondaryResource")]
        public int SecondaryResource { get; set; }
    }
}
