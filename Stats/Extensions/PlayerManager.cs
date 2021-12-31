using System;
using System.Collections.Generic;
using System.Reflection;
using UnboundLib;

namespace Stats.Extensions
{
    public static class PlayerManagerExtension
    {

        public static Player GetPlayerWithID(this PlayerManager playerManager, int playerID)
        {
            return PlayerManager.instance.InvokeMethod("GetPlayerWithID", new object[] { playerID }) as Player;
        }
    }
}