using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.Sirius.Core.Common
{
    public static class BattleNetConstants
    {
        public const string BattleNetApiBaseUri = "http://{0}.battle.net/api/d3/";
        public const string BattleNetApiBaseUriEU = "http://eu.battle.net/d3/en/{0}";
        public const string BattleNetApiProfile = "profile/{0}/";
        public const string BattleNetApiHero = "profile/{0}/hero/{1}";
        public const string BattleNetMediaLargeItemIconUrl = "http://media.blizzard.com/d3/icons/items/large/{0}.png";
    }
}
