﻿using System.Collections.Generic;
using GTANetworkAPI;
using VMP_CNR.Module.ClientUI.Apps;
using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using Newtonsoft.Json;
using System;
using VMP_CNR.Module.Teams;
using System.Linq;

namespace VMP_CNR.Module.LifeInvader.App
{
    public class LifeInvaderApp : SimpleApp
    {
        public LifeInvaderApp() : base("LifeInvaderApp")
        {
        }

        public class AdsFound
        {
            [JsonProperty(PropertyName = "id")] public uint Id { get; }
            [JsonProperty(PropertyName = "title")] public string Title { get; }
            [JsonProperty(PropertyName = "content")]
            public string Content { get; }
            [JsonProperty(PropertyName = "timeStamp")]
            public DateTime DateTime { get; }

            public AdsFound(uint id, string title, string content)
            {
                Id = id;
                Title = title;
                Content = content;
                DateTime = DateTime.Now;
            }

            public AdsFound(uint id, string title, string content, DateTime dateTime)
            {
                Id = id;
                Title = title;
                Content = content;
                DateTime = dateTime;
            }
        }

        [RemoteEvent]
        public void requestAd(Player player, string key)
        {
            if (!player.CheckRemoteEventKey(key)) return;
            SendAdsList(player);
        }

        public void addAd(Player player, string title, string content)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            // Add News
            title = title.Replace("\"", "");
            content = content.Replace("\"", "");

            title = title.Replace("\"\"", "");
            content = content.Replace("\"\"", "");

            Main.adList.Add(new AdsFound((uint) Main.adList.Count + 1, title, content, DateTime.Now));

            Main.adList.Sort(delegate (AdsFound x, AdsFound y)
            {
                return y.Id.CompareTo(x.Id);
            });

            // Update NewsList
            this.SendAdsList(player);
        }


        private void SendAdsList(Player player)
        {
            var l_AdsCopy = Main.adList.ToList();
            TriggerNewClient(player, "updateLifeInvaderAds", NAPI.Util.ToJson(l_AdsCopy));
        }
    }
}