﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DS.Sirius.Core.Models;

namespace DS.Sirius.DA.Definition
{
    public interface IBattleNetRepository : IDisposable
    {
        Career GetCareerByBattleTag(string battleTag);
        Hero GetHeroById(string battleTag, long id);


    }
}
