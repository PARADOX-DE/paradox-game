﻿using VMP_CNR.Module.GTAN;
using VMP_CNR.Module.Items;
using VMP_CNR.Module.Players.Db;

namespace VMP_CNR.Module.Players
{
    public static class PlayerMoney
    {
        /**
         * Takes an specific amount of money from an player
         *
         * @param amount an positive number that represents the amount of money to take from the player
         *
         * @return true if enough money was available, else false
         */

        public static int TakeAnyMoney(this DbPlayer player, int amount, bool ignoreMinuse = false)
        {
            //if (amount < 0) return -1;
            if (player.TakeMoney(amount)) return 0;
            if (player.TakeBankMoney(amount, null, ignoreMinuse)) return 1;
            player.Save();
            return -1;
        }

        public static bool TakeMoney(this DbPlayer player, int money)
        {
            if (money < 0) return false;
            if (player.Money[0] < money) return false;
            if (player.Money[0] - money > player.Money[0]) return false;
            player.Money[0] = player.Money[0] - money;
            player.Player.TriggerNewClient("updateMoney", player.Money[0]);
            player.Save();
            return true;
        }
        public static bool ResetHandMoney(this DbPlayer player)
        {
            player.Money[0] = 0;
            player.Player.TriggerNewClient("updateMoney", player.Money[0]);
            player.Save();
            return true;
        }

        public static bool TakeBlackMoney(this DbPlayer player, int money)
        {
            if (money < 0) return false;
            if (player.BlackMoney[0] < money) return false;
            if (player.BlackMoney[0] - money > player.BlackMoney[0]) return false;
            player.BlackMoney[0] = player.BlackMoney[0] - money;
            player.Player.TriggerNewClient("updateBlackMoney", player.BlackMoney[0]);
            player.Save();
            return true;
        }

        public static bool GiveMoney(this DbPlayer player, int money)
        {
            if (money < 0) return false;
            if (player.Money[0] + money < player.Money[0]) return false;
            player.Money[0] = player.Money[0] + money;
            player.Player.TriggerNewClient("updateMoney", player.Money[0]);
            player.Save();
            return true;
        }


        public static bool GiveBlackMoneyBank(this DbPlayer player, int money)
        {
            if (money < 0) return false;
            if (player.BlackMoneyBank[0] + money < player.BlackMoneyBank[0]) return false;
            player.BlackMoneyBank[0] = player.BlackMoneyBank[0] + money;
            player.Save();
            return true;
        }

        public static bool TakeBlackMoneyBank(this DbPlayer player, int money, string description = null)
        {
            if (money < 0) return false;
            if (player.BlackMoneyBank[0] < money) return false;
            if (player.BlackMoneyBank[0] - money > player.BlackMoneyBank[0]) return false;
            player.BlackMoneyBank[0] = player.BlackMoneyBank[0] - money;
            player.Save();
            return true;
        }

        public static bool GiveBlackMoney(this DbPlayer player, int money)
        {
            if (money < 0) return false;
            if (player.BlackMoney[0] + money < player.BlackMoney[0]) return false;
            player.BlackMoney[0] = player.BlackMoney[0] + money;
            player.Player.TriggerNewClient("updateBlackMoney", player.BlackMoney[0]);
            player.Save();
            return true;
        }

        public static bool TakeMoneyByPaymentState(this DbPlayer player, Payment.PaymentStatus paymentStatus, int money, string description = null, bool ignoreMinus = false)
        {
            switch(paymentStatus)
            {
                case Payment.PaymentStatus.Bank:
                    return TakeBankMoney(player, money, description, ignoreMinus);
                case Payment.PaymentStatus.Wallet:
                    return TakeMoney(player, money);
                default:
                    return false;
            }
        }

        public static bool TakeBankMoney(this DbPlayer player, int money, string description = null, bool ignoreMinus = false)
        {
            if (money < 0) return false;
            if (player.BankMoney[0] < money && !ignoreMinus) return false;
            if (player.BankMoney[0] - money > player.BankMoney[0] && !ignoreMinus) return false;
            player.BankMoney[0] = player.BankMoney[0] - money;
            if (description != null)
            {
                player.AddPlayerBankHistory(-money, description);
            }
            player.Save();
            return true;
        }

        public static void TakeSafeBankMoney(this DbPlayer player, int money, string description = null)
        {
            if (money < 0) return;
            if (player.BankMoney[0] - money > player.BankMoney[0]) return;
            player.BankMoney[0] = player.BankMoney[0] - money;
            if (description != null)
            {
                player.AddPlayerBankHistory(-money, description);
            }
            player.Save();
            return;
        }

        public static bool GiveBankMoney(this DbPlayer player, int money, string description = null)
        {
            if (money < 1) return false;
            if (player.BankMoney[0] + money < player.BankMoney[0]) return false;
            player.BankMoney[0] = player.BankMoney[0] + money;
            if (description != null)
            {
                player.AddPlayerBankHistory(money, description);
            }
            player.Save();
            return true;
        }

        public static bool TakeOrGiveBankMoney(this DbPlayer player, int money, bool canMinus = false)
        {
            if (money < 0)
            {
                if (canMinus)
                {
                    player.TakeSafeBankMoney(-money);
                    return true;
                }
                else return player.TakeBankMoney(-money);
            }
            if (money > 0) return player.GiveBankMoney(money);
            player.Save();
            return false;
        }

        public static void GiveMoneyToPlayer(this DbPlayer dbPlayer, DbPlayer dPlayer, int amount)
        {
            // not a valid destination player
            if (dPlayer == null)
            {
                dbPlayer.SendNewNotification(GlobalMessages.Error.NoPlayer());
                return;
            }

            // destination is source
            if (dbPlayer.Id == dPlayer.Id)
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.PlayerSelfMoney());
                return;
            }

            // not a valid amount
            if (amount + dPlayer.Money[0] < dPlayer.Money[0])
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.InvalidAmount());
                return;
            }

            // Take money from source or error
            if (!dbPlayer.TakeMoney(amount))
            {
                dbPlayer.SendNewNotification(GlobalMessages.Money.NotEnoughMoney(amount));
                return;
            }

            // transfer money to destination
            dPlayer.GiveMoney(amount);
            
            dbPlayer.SendNewNotification(GlobalMessages.Money.PlayerGiveMoney(amount));
            dPlayer.SendNewNotification(GlobalMessages.Money.PlayerGotMoney(amount));

            SaveToPayLog(dbPlayer.GetName(), dPlayer.GetName(), amount);
        }
        
        public static void GiveEarning(this DbPlayer dbPlayer, int amount)
        {
            if (dbPlayer.paycheck[0] + amount < dbPlayer.paycheck[0]) return;
            dbPlayer.paycheck[0] = dbPlayer.paycheck[0] + amount;
        }

        public static void ResetMoney(this DbPlayer dbPlayer)
        {
            dbPlayer.Money[0] = 0;
            dbPlayer.Player.TriggerNewClient("updateMoney", 0);
        }

        private static void SaveToPayLog(string u1, string u2, int value)
        {
            u1 = u1 ?? "undefined";
            u2 = u2 ?? "undefined";
            var query = $"INSERT INTO `paylog` (`s1`,`s2`, `amount`) VALUES ('{u1}', '{u2}', '{value}');";
            MySQLHandler.ExecuteAsync(query);
        }

        public static int GetCapital(this DbPlayer dbPlayer)
        {
            /*var businessMoney = 0;
            foreach (var membership in dbPlayer.BusinessMemberships)
            {
                if (membership.Value.Owner != 1) continue;
                var business = Businesses.Instance.GetById(membership.Value.BusinessId);
                if (business != null)
                {
                    businessMoney = business.Money;
                }
            }*/
            
            return/*businessMoney +*/ dbPlayer.Money[0] + dbPlayer.BankMoney[0];
        }
    }
}