﻿using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using VMP_CNR.Handler;
using VMP_CNR.Module.Logging;

namespace VMP_CNR.Module.Vehicles
{
    public static class VehicleTuning
    {
        public static void SetNeon(this SxVehicle sxVeh, string rgba)
        {
            try
            {
                if (sxVeh == null) return;
                
                // Remove all Veh Mods
                sxVeh.Entity.Neons = false;

                if (rgba == "") return;

                var mods = rgba.Split(',');
                if (mods[0] == "") mods[0] = "0";
                if (mods[1] == "") mods[1] = "0";
                if (mods[2] == "") mods[2] = "0";

                sxVeh.Entity.Neons = true;
                sxVeh.Entity.NeonColor = new Color(Main.CToInt(mods[0]), Main.CToInt(mods[1]),
                    Main.CToInt(mods[2]));
            }
            catch (Exception e)
            {
                Logger.Print(e.Message);
            }
        }

        public static void LoadNeon(this SxVehicle sxVeh)
        {
            sxVeh.SetNeon(sxVeh.neon);
        }
    }
}