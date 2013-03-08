using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Sirius.Core.Models;

namespace DS.Sirius.BL.Definition
{
    public interface IBattleNetClient
    {
        Career GetCareerByBattleTag(string battleTag);
        Hero GetHeroByID(string battleTag, long id);

    }
}
