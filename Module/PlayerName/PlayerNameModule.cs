﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VMP_CNR.Module.PlayerName
{
    public class PlayerNameModule : SqlModule<PlayerNameModule, PlayerNameModel, uint>
    {
        /**
         * TODO:
         * Rework this shit.
         */

        protected override string GetQuery()
        {
            return "SELECT id, name, forumid, handy, rankId, warns, ausschluss FROM `player`;";
        }
    }
}
