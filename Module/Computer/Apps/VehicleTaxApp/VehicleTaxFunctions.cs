﻿using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkMethods;
using MySql.Data.MySqlClient;
using VMP_CNR.Module.Configurations;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Computer.Apps.VehicleTaxApp
{
    public class VehicleTaxFunctions
    {


        public static List<VehicleTaxOverview> GetVehicleTaxOverviews(DbPlayer dbPlayer, String searchString)
        {
            List<VehicleTaxOverview> overviewVehicles = new List<VehicleTaxOverview>();

            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = "SELECT model, fuel, price, tax, inv_size, inv_weight, mod_car_name, is_business_vehicle FROM vehicledata WHERE model LIKE @searchString OR mod_car_name LIKE @searchString LIMIT 15";
                cmd.Parameters.AddWithValue("@searchString", $"%{searchString}%");
                cmd.Prepare();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var mod_car_name = reader.GetString("mod_car_name");
                            VehicleTaxOverview overview = new VehicleTaxOverview
                            {
                                Model = String.IsNullOrWhiteSpace(mod_car_name) ? reader.GetString("model") : mod_car_name,
                                Tax = reader.GetInt32("tax"),
                                Slots = reader.GetInt32("inv_size"),
                                Weight = reader.GetInt32("inv_weight")/1000,
                                Fuel = reader.GetInt32("fuel"),
                                Biz = reader.GetInt32("is_business_vehicle"),
                                Price = reader.GetInt32("price")
                            };

                            overviewVehicles.Add(overview);


                        }
                    }
                }
            }
            return overviewVehicles;
        }
    }
}
