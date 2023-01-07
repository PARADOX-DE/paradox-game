﻿using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Spawners;
using VMP_CNR.Module.Teams;

namespace VMP_CNR.Module.Players.JumpPoints
{
    //Todo: test jumpppints very close in different dimensions, and migrate to SqlModule
    public sealed class JumpPointModule : Module<JumpPointModule>
    {
        public Dictionary<int, JumpPoint> jumpPoints;

        public override Type[] RequiredModules()
        {
            return new[] { typeof(TeamModule) };
        }

        protected override bool OnLoad()
        {
            if (jumpPoints != null)
            {
                foreach (var jumpPoint in jumpPoints)
                {
                    jumpPoint.Value.ColShape?.Delete();
                }

                jumpPoints.Clear();
            }

            jumpPoints = new Dictionary<int, JumpPoint>();

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"SELECT * FROM `jump_points` WHERE destionation >= 0;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var jumpPoint = new JumpPoint(reader);
                            jumpPoints.Add(jumpPoint.Id, jumpPoint);
                        }
                    }
                }
            }

            foreach (var jumpPoint in jumpPoints)
            {
                if (jumpPoint.Value.DestinationId == jumpPoint.Value.Id)
                    continue;
                if (jumpPoints.TryGetValue(jumpPoint.Value.DestinationId, out var destinationJumpPoint))
                {
                    jumpPoint.Value.Destination = destinationJumpPoint;
                    OnJumpPointSpawn(jumpPoint.Value);
                }
                else
                {
                    Logging.Logger.Crash(new ArgumentException(
                        $"JumpPoint{jumpPoint.Key} has invalid destination {jumpPoint.Value.DestinationId}"));
                }
            }

            return true;
        }

        private static void OnJumpPointSpawn(JumpPoint jumpPoint)
        {
            int objectModel;
            var objectPosition = jumpPoint.Position.Copy();

            objectPosition.Z -= 1f;
            objectModel = -1916383162;

            if (!jumpPoint.HideInfos)
            {
                jumpPoint.Object =
                    ObjectSpawn.Create(objectModel, objectPosition, new Vector3(), jumpPoint.Dimension);
            }
        }

        public JumpPoint Get(int id)
        {
            return jumpPoints.TryGetValue(id, out var jumpPoint) ? jumpPoint : null;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (!dbPlayer.TryData("jumpPointId", out int jumpPointId)) return false;
            var jumpPoint = Get(jumpPointId);
            if (jumpPoint == null) return false;
            switch (key)
            {
                case Key.E:
                    return jumpPoint.TravelThrough(dbPlayer);
                case Key.L:
                    //if (HalloweenModule.isActive) return false;
                    return jumpPoint.ToggleLock(dbPlayer);
                default: return false;
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!colShape.HasData("jumpPointId")) return false;
            if (!Configuration.Instance.JumpPointsEnabled) return false;
            int jumpPointId = colShape.GetData<int>("jumpPointId");
            try
            {
                switch (colShapeState)
                {
                    case ColShapeState.Enter:
                        var jumpPoint = Get(jumpPointId);
                        if (jumpPoint == null || dbPlayer == null || !dbPlayer.IsValid()) return false;

                        if (jumpPoint.EnterOnColShape)
                        {
                            jumpPoint.TravelThrough(dbPlayer);
                        }
                        else
                        {
                            if (Configuration.Instance.ShowAllJumppoints)
                            {
                                dbPlayer.SendNewNotification($"[DEBUG] Benutze E um vom {jumpPoint.Name} nach {jumpPoint.Destination.Name} zu gehen!", title: jumpPoint.Name);
                                dbPlayer.SetData("jumpPointId", jumpPoint.Id);
                            }
                            else
                            {
                                if (jumpPoint.Teams.Count > 0)
                                {
                                    if (jumpPoint.Teams.Contains(dbPlayer.Team))
                                    {
                                        dbPlayer.SendNewNotification($"Benutze E um vom {jumpPoint.Name} nach {jumpPoint.Destination.Name} zu gehen!", title: jumpPoint.Name);
                                    }
                                }
                                else
                                {
                                    if (!jumpPoint.HideInfos || !jumpPoint.Disabled)
                                    {
                                        dbPlayer.SendNewNotification($"Benutze E um vom {jumpPoint.Name} nach {jumpPoint.Destination.Name} zu gehen!", title: jumpPoint.Name);
                                    }
                                }
                            }

                            dbPlayer.SetData("jumpPointId", jumpPoint.Id);
                        }

                        return true;
                    case ColShapeState.Exit:
                        if (dbPlayer == null || !dbPlayer.IsValid()) return false;
                        if (!dbPlayer.HasData("jumpPointId")) return false;

                        int playerJumpPointId = dbPlayer.GetData("jumpPointId");

                        if (jumpPointId == playerJumpPointId)
                        {
                            dbPlayer.ResetData("jumpPointId");
                        }

                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
            }
            return false;
        }
    }
}