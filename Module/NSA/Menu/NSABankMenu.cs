﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMP_CNR.Handler;
using VMP_CNR.Module.ClientUI.Components;
using VMP_CNR.Module.Menu;
using VMP_CNR.Module.NSA.Observation;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.Windows;
using VMP_CNR.Module.Telefon.App;

namespace VMP_CNR.Module.NSA.Menu
{
    public class NSABankMenu : MenuBuilder
    {
        public NSABankMenu() : base(PlayerMenu.NSABankMenu)
        {

        }

        public override Module.Menu.NativeMenu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.HasData("nsa_target_player_id")) return null;

            DbPlayer l_Target = Players.Players.Instance.FindPlayerById(p_DbPlayer.GetData("nsa_target_player_id"));
            if (l_Target == null || !l_Target.IsValid()) return null;

            var l_Menu = new Module.Menu.NativeMenu(Menu, "NSA Finanzen (" + l_Target.GetName() + ")");
            l_Menu.Add($"Schließen");
            foreach (var l_History in l_Target.BankHistory)
            {
                if (l_History.Name.ToLower().Contains("geldautomat") ||
                    l_History.Name.ToLower().Contains("makler") ||
                    l_History.Name.ToLower().Contains("einkommen") ||
                    l_History.Name.ToLower().Contains("ueberweisung") ||
                    l_History.Name.ToLower().Contains("einkommen") ||
                    l_History.Name.ToLower().Contains("gehalt"))
                {
                    l_Menu.Add($"{l_History.Date.ToShortDateString()} | {l_History.Name} ({l_History.Value})");
                }
            }

            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (!dbPlayer.HasData("nsa_target_player_id"))
                    return false;

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
