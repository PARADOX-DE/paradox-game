﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Schwarzgeld
{
    public class SchwarzgeldEvents : Script
    {
        [RemoteEvent]
        public void BlackmoneyWithdraw(Player player, string returnstring, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (Int32.TryParse(returnstring, out int withdraw))
            {
                if (withdraw <= 0) return;

                if (dbPlayer.DimensionType[0] == DimensionType.MoneyKeller)
                {
                    House xHouse = HouseModule.Instance.Get((uint)dbPlayer.Player.Dimension);
                    if (xHouse != null && (dbPlayer.HouseKeys.Contains(xHouse.Id) || dbPlayer.ownHouse[0] == xHouse.Id))
                    {
                        if (dbPlayer.Player.Position.DistanceTo(SchwarzgeldModule.BlackMoneyEndPoint) < 1.5f)
                        {
                            if(xHouse.BLAmount < withdraw)
                            {
                                dbPlayer.SendNewNotification("So viel befindet sich nicht in der Gelddruckmaschine!");
                                return;
                            }
                            else
                            {
                                xHouse.BLAmount -= withdraw;
                                dbPlayer.GiveMoney(withdraw);
                                xHouse.SaveBlackMoney();

                                dbPlayer.SendNewNotification($"Sie haben {withdraw}$ entnommen!");
                                return;
                            }
                        }
                    }
                }
                return;
            }
            dbPlayer.SendNewNotification("Ungültiger Betrag!");
            return;
        }
    }
}
