﻿using VMP_CNR.Module.Players;
using VMP_CNR.Module.Players.Db;
using VMP_CNR.Module.Tasks;

namespace VMP_CNR.Module.Business.Tasks
{
    public class BusinessSalaryTask : SqlTask
    {
        private readonly Business business;
        private readonly DbPlayer dbPlayer;
        private readonly int amount;

        public BusinessSalaryTask(Business business, DbPlayer dbPlayer, int amount)
        {
            this.business = business;
            this.dbPlayer = dbPlayer;
            this.amount = amount;
        }

        public override string GetQuery()
        {
            return $"UPDATE `business` SET money = money - '{amount}' WHERE id = '{business.Id}' AND money >= '{amount}';";
        }

        public override void OnFinished(int result)
        {
            if (result == 1)
            {
                business.Money -= amount;
                var businessResult =
                    ExecuteNonQuery($"UPDATE `player` SET BankMoney = BankMoney + '{amount}' WHERE id = '{dbPlayer.Id}'");
                if (businessResult == 1)
                {
                    dbPlayer.GiveBankMoney(amount);
                }
            }
        }
    }
}