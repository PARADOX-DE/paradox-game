﻿using MySql.Data.MySqlClient;
using VMP_CNR.Module.Barber;

namespace VMP_CNR.Module.Assets.Chest
{
    public class AssetsChest : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public int CustomisationId { get; }
        public int Price { get; }
        
        public int BarberShopId { get; }

        public AssetsChest(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            CustomisationId = reader.GetInt32("customisation_id");
            Price = reader.GetInt32("price");
            BarberShopId = reader.GetInt16("shop_id");


        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}