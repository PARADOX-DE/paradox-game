﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.UHaft
{
    class UHaftEventModule : Script
    {
        [RemoteEvent]
        public void SetUhaftPlayer(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var findPlayer = Players.Players.Instance.FindPlayer(returnstring);
            if(findPlayer == null || !findPlayer.IsValid())
            {
                dbPlayer.SendNewNotification("Spieler nicht gefunden!");
                return;
            }

            // Check Jail and 

            if(findPlayer.UHaftTime > 0 || findPlayer.JailTime[0] > 0)
            {
                dbPlayer.SendNewNotification("Spieler bereits in Untersuchungshaft oder im Gefaengnis!");
                return;
            }

            // Is In Range
            if(findPlayer.Player.Position.DistanceTo(UHaftmodule.UhaftComputerPosition) > 4.0f)
            {
                dbPlayer.SendNewNotification("Spieler ist nicht in der Naehe!");
                return;
            }

            findPlayer.UHaftTime = 1;
            findPlayer.SendNewNotification("Sie befinden sich nun in Untersuchungshaft!");
            dbPlayer.SendNewNotification($"{findPlayer.GetName()} in Untersuchungshaft gesetzt!");
        }
    }
}
