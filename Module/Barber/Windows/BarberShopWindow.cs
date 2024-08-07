﻿using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using VMP_CNR.Module.ClientUI.Windows;
using VMP_CNR.Module.Customization;
using VMP_CNR.Module.Logging;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Barber.Windows
{
    public class BarberShopWindow : Window<Func<DbPlayer, ListJsonBarberObject, bool>>
    {
        public class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "player")] private BarberPlayerObject BarberPlayerObject { get; }
            [JsonProperty(PropertyName = "barber")] private ListJsonBarberObject ListJsonBarberObject { get; }

            public ShowEvent(DbPlayer dbPlayer, ListJsonBarberObject listJsonBarberObject) : base(dbPlayer)
            {

                try
                {
                    BarberPlayerObject = new BarberPlayerObject
                    {
                        Hair = dbPlayer.Customization.Hair.Hair,
                        HairColor = dbPlayer.Customization.Hair.Color,
                        HairColor2 = dbPlayer.Customization.Hair.HighlightColor,
                        Beard = dbPlayer.Customization.Appearance[1].Value,
                        BeardColor = dbPlayer.Customization.BeardColor,
                        BeardOpacity = dbPlayer.Customization.Appearance[1].Opacity,
                        Chest = dbPlayer.Customization.Appearance[10].Value,
                        ChestHairColor = dbPlayer.Customization.ChestHairColor,
                        ChestHairOpacity = dbPlayer.Customization.Appearance[10].Opacity
                    };
                    ListJsonBarberObject = listJsonBarberObject;
                }
                catch (Exception e)
                {
                    Console.WriteLine("#### ERROR Reading Customization " + e.Message);
                }
            }
        }

        public BarberShopWindow() : base("Barber")
        {
        }

        public override Func<DbPlayer, ListJsonBarberObject, bool> Show()
        {
            return (player, listJsonBarberObject) => OnShow(new ShowEvent(player, listJsonBarberObject));
        }

        [RemoteEvent]
        public void barberShopBuy(Player client, int cost, String responseString, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            try
            {
                var newCustomization = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (dbPlayer.TakeMoney(cost))
                {
                    dbPlayer.Customization.Hair.Hair = (int)newCustomization.hair;
                    dbPlayer.Customization.Hair.Color = (byte)newCustomization.hairColor;
                    dbPlayer.Customization.Hair.HighlightColor = (byte)newCustomization.hairColor2;

                    dbPlayer.Customization.BeardColor = (int)newCustomization.beardColor;
                    dbPlayer.Customization.Appearance[1] = new AppearanceItem((byte)newCustomization.beard, (float)newCustomization.beardOpacity);

                    dbPlayer.Customization.ChestHairColor = (int)newCustomization.chestHairColor;
                    dbPlayer.Customization.Appearance[10] = new AppearanceItem((byte)newCustomization.chestHair, (float)newCustomization.chestHairOpacity);

                    dbPlayer.SaveCustomization();
                    dbPlayer.SendNewNotification($"Du hast {cost}$ bezahlt", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                }
                else
                {
                    dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(cost), notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.ApplyCharacter();
                    return;
                }

                dbPlayer.ApplyCharacter();
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }
        }

        [RemoteEvent]
        public void barberShopExit(Player client, string key)
        {
            if (!client.CheckRemoteEventKey(key)) return;
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.ApplyCharacter();
        }

        public class BarberPlayerObject
        {
            [JsonProperty(PropertyName = "hair")] public int Hair { get; set; }
            [JsonProperty(PropertyName = "hairColor")] public int HairColor { get; set; }
            [JsonProperty(PropertyName = "hairColor2")] public int HairColor2 { get; set; }
            [JsonProperty(PropertyName = "beard")] public int Beard { get; set; }
            [JsonProperty(PropertyName = "beardColor")] public int BeardColor { get; set; }
            [JsonProperty(PropertyName = "beardOpacity")] public float BeardOpacity { get; set; }
            [JsonProperty(PropertyName = "chestHair")] public int Chest { get; set; }
            [JsonProperty(PropertyName = "chestHairColor")] public int ChestHairColor { get; set; }
            [JsonProperty(PropertyName = "chestHairOpacity")] public float ChestHairOpacity { get; set; }
        }
    }
}
