// JSON C# Class Generator
// http://at-my-window.blogspot.com/?page=json-class-generator

using System;
using System.Collections.Generic;


namespace DS.Sirius.Core.BattleNet.Models
{

    public class FallenHero
    {
        public Stats stats { get; set; }
        public Kills2 kills { get; set; }
        public Items items { get; set; }
        public Death death { get; set; }
        public string name { get; set; }
        public int level { get; set; }
        public bool hardcore { get; set; }
        public int heroId { get; set; }
        public int gender { get; set; }
    }
}
