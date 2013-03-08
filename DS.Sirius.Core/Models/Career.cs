// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;


namespace DS.Sirius.Core.Models
{
    [DataContract]
    public class Career
    {

        [DataMember]
        [JsonProperty("heroes")]
        public IList<Hero> Heroes { get; set; }
        [DataMember]
        [JsonProperty("lastHeroPlayed")]
        public int LastHeroPlayed { get; set; }
        [DataMember]
        [JsonProperty("lastUpdated")]
        public int LastUpdated { get; set; }
        //public IList<Artisan> artisans { get; set; }
        //public IList<HardcoreArtisan> hardcoreArtisans { get; set; }
        [DataMember]
        [JsonProperty("kills")]
        public Kills Kills { get; set; }
        [DataMember]
        [JsonProperty("timePlayed")]
        public TimePlayed TimePlayed { get; set; }
        //public IList<FallenHero> fallenHeroes { get; set; }
        [DataMember]
        [JsonProperty("battleTag")]
        public string BattleTag { get; set; }
        //public Progression progression { get; set; }
        //public HardcoreProgression hardcoreProgression { get; set; }
    }
}
