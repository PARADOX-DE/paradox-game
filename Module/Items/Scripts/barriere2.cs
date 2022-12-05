﻿using System.Threading.Tasks;
using GTANetworkAPI;
using VMP_CNR.Module.Chat;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Players.PlayerAnimations;

namespace VMP_CNR.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> barriere2(DbPlayer dbPlayer, ItemModel ItemData)
        {
            if (dbPlayer.IsCuffed || dbPlayer.IsTied ||
                (!dbPlayer.Team.IsCops() && !dbPlayer.Team.IsDpos() && !dbPlayer.Team.IsMedics()) ||
                dbPlayer.RageExtension.IsInVehicle)
            {
                return false;
            }

            if (PoliceObjectModule.Instance.IsMaxReached())
            {
                dbPlayer.SendNewNotification(
                    
                    "Maximale Anzahl an Polizeiabsperrungen erreicht!");
                return false;
            }

            PoliceObjectModule.Instance.Add(-143315610, dbPlayer.Player, ItemData, false);
                dbPlayer.SendNewNotification(
                     ItemData.Name +
                    " erfolgreich platziert!");
                dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
                dbPlayer.Player.TriggerNewClient("freezePlayer", true);
            await Task.Delay(4000);
            dbPlayer.Player.TriggerNewClient("freezePlayer", false);
                dbPlayer.StopAnimation();
            

            return true;
        }
    }
}