﻿using GTANetworkAPI;
using System;
using VMP_CNR.Handler;
using VMP_CNR.Module.Freiberuf;
using VMP_CNR.Module.Freiberuf.Mower;
using VMP_CNR.Module.Government;
using VMP_CNR.Module.Houses;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.ShopTakeover.Models;
using VMP_CNR.Module.Teams;
using VMP_CNR.Module.Teams.Shelter;
using VMP_CNR.Module.Vehicles.Data;
using VMP_CNR.Module.Vehicles.Garages;

namespace VMP_CNR.Module.ShopTakeover
{
    public class ShopTakeoverAttackMenu : MenuBuilder
    {
        public ShopTakeoverAttackMenu() : base(PlayerMenu.ShopTakeoverAttackMenu) { }

        public override NativeMenu Build(DbPlayer dbPlayer)
        {
            uint shopTakeoverId = dbPlayer.GetData("shopTakeoverId");

            ShopTakeoverModel shopTakeover = ShopTakeoverModule.Instance[shopTakeoverId];
            if (shopTakeover == null) return null;

            var menu = new NativeMenu(Menu, "Shop-Takeover - Angriff", $"Schutzgeld: (${shopTakeover.Money})");

            menu.Add("Schliessen");
            menu.Add($"Angreifen");
            menu.Add($"Informationen");

            if(shopTakeover.Team.Id == dbPlayer.Team.Id)
                menu.Add("Schutzgeld einsammeln");

            return menu;
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                uint shopTakeoverId = dbPlayer.GetData("shopTakeoverId");

                ShopTakeoverModel shopTakeover = ShopTakeoverModule.Instance[shopTakeoverId];
                if (shopTakeover == null) return true;

                switch (index)
                {
                    case 0: return true;
                    case 1:
                        ShopTakeoverAttackModule.Instance.Attack(dbPlayer, shopTakeover);
                        break;
                    case 3:
                        dbPlayer.SendNewNotification("Informationen:");
                        break;
                    case 4:
                        var shopTakeoverMoney = shopTakeover.Money;
                        shopTakeover.ClearMoney();

                        var playerShopTakeoverMoney = shopTakeoverMoney * 0.8;
                        var shelterShopTakeoverMoney = shopTakeoverMoney * 0.2;

                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney((int)shelterShopTakeoverMoney);
                        dbPlayer.GiveMoney((int)playerShopTakeoverMoney);

                        // Maybe send team notification?
                        dbPlayer.SendNewNotification($"Das Schutzgeld wurde erfolgreich eingesammelt. Vom Schutzgeld gingen ${shelterShopTakeoverMoney} (20%) gingen an die Fraktionskasse.");

                        break;
                }

                return true;
            }
        }

        public override IMenuEventHandler GetEventHandler() => new EventHandler();
    }
}