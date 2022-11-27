using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using StardewValley;

namespace TwitchedATM
{
    public class Account
    {
        private readonly Dictionary<string, int> Ledger;
        private readonly Dictionary<string, int> PermanentLedger;

        private static readonly string WITHDRAWALS = "WITHDRAWALS";
        private static readonly string INTERESTS = "INTERESTS";
        private static readonly int DAYS_PER_YEAR = 4 * 28; // 28 days per SV season.

        double depositInterestRate = 0;

        ModEntry sv; // links back to Stardew Valley TwitchedATM mod.

        public Account(ModEntry sv, double depositInterestRate = 0)
        {
            this.sv = sv;

            Ledger = new Dictionary<string, int>();

            // Permanent record of all (summed) deposits and withdrawals.
            PermanentLedger = new Dictionary<string, int>();
            PermanentLedger[WITHDRAWALS] = 0;
            PermanentLedger[INTERESTS] = 0;

            this.depositInterestRate = depositInterestRate;
        }

        public void Deposit(string from, int amount)
        {
            if (Ledger.ContainsKey(from))
            {
                Ledger[from] += amount;
            } else
            {
                Ledger[from] = amount;
            }

            if (PermanentLedger.ContainsKey(from))
            {
                PermanentLedger[from] += amount;
            }
            else
            {
                PermanentLedger[from] = amount;
            }

            sv.Monitor.Log($"Deposit({from}, {amount})", StardewModdingAPI.LogLevel.Debug);
        }

        public int Withdraw()
        {
            int total = Balance();
            Ledger.Clear();
            PermanentLedger[WITHDRAWALS] -= total;
            // Important: Don't clear PermanentLedger

            sv.Monitor.Log($"Withdraw(): {total}", StardewModdingAPI.LogLevel.Debug);

            return total;
        }

        public string CurrentActivity()
        {
            var opt = new JsonSerializerOptions() {  WriteIndented = true };
            string strJson = JsonSerializer.Serialize(Ledger, opt);
            return strJson;
        }

        public string TotalActivity()
        {
            var opt = new JsonSerializerOptions { WriteIndented = true };
            string strJson = JsonSerializer.Serialize(PermanentLedger, opt);
            return strJson;
        }

        /// <summary>Current account balance, based on Ledger.</summary>
        /// <returns>Sum of all current transactions (account balance).</returns>
        public int Balance()
        {
            int total = 0;
            foreach(KeyValuePair<string, int> pair in Ledger)
            {
                total += pair.Value;
            }
            return total;
        }

        /// <summary>Add interests of previous day. Ticks every day. Connected in owner of this class to the game.</summary>
        public void OnNewDay()
        {
            if (depositInterestRate > 0)
            {
                int interestDay = (int)Math.Floor((Balance() * depositInterestRate) / DAYS_PER_YEAR);
                Deposit(INTERESTS, interestDay);
            }
        }
    }
}
