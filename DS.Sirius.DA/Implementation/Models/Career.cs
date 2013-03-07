// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace DS.Sirius.DA.Implementation.Models
{
    [DataContract]
    public class Career
    {

        [DataMember]
        public IList<Hero> heroes { get; set; }
        [DataMember]
        public int lastHeroPlayed { get; set; }
        [DataMember]
        public int lastUpdated { get; set; }
        //public IList<Artisan> artisans { get; set; }
        //public IList<HardcoreArtisan> hardcoreArtisans { get; set; }
        [DataMember]
        public Kills kills { get; set; }
        [DataMember]
        public TimePlayed timePlayed { get; set; }
        //public IList<FallenHero> fallenHeroes { get; set; }
        [DataMember]
        public string battleTag { get; set; }
        //public Progression progression { get; set; }
        //public HardcoreProgression hardcoreProgression { get; set; }
    }
}
