using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using StardewValley;

namespace TwitchedATM
{
    public class Account
    {
        ModEntry sv;        // links back to Stardew Valley TwitchedATM mod.
        Config config;      // from default.json
        AccountState state; // Current and permanent ledger (what's in the account)

        public Account(ModEntry sv, Config config, AccountState state)
        {
            this.sv = sv;
            this.config = config;
            this.state = state;
        }

        public void Deposit(string from, int amount)
        {
            if (state.Ledger.ContainsKey(from))
            {
                state.Ledger[from] += amount;
            } else
            {
                state.Ledger[from] = amount;
            }

            if (state.PermanentLedger.ContainsKey(from))
            {
                state.PermanentLedger[from] += amount;
            }
            else
            {
                state.PermanentLedger[from] = amount;
            }

            sv.Monitor.Log($"Deposit({from}, {amount})", StardewModdingAPI.LogLevel.Debug);
        }

        public int Withdraw()
        {
            int total = Balance();
            state.Ledger.Clear();
            state.PermanentLedger[config.WITHDRAWALS] -= total;
            // Important: Don't clear PermanentLedger

            sv.Monitor.Log($"Withdraw(): {total}", StardewModdingAPI.LogLevel.Debug);

            return total;
        }

        public string CurrentActivity()
        {
            // Sorting by values, see: https://code-maze.com/sort-dictionary-by-value-dotnet/
            var sortedKeyValuePairs = (from kv in state.Ledger
                                       orderby kv.Value descending
                                       select kv).ToList();

            var leaderBoard = sortedKeyValuePairs.GetRange(0, Math.Min(sortedKeyValuePairs.Count, 10));

            var opt = new JsonSerializerOptions() { WriteIndented = true };
            string strJson = JsonSerializer.Serialize(leaderBoard, opt);
            return strJson;
        }

        public string TotalActivity()
        {
            // Sorting by values, see: https://code-maze.com/sort-dictionary-by-value-dotnet/
            var sortedKeyValuePairs = (from kv in state.PermanentLedger
                                       orderby kv.Value descending
                                       select kv).ToList();

            var leaderBoard = sortedKeyValuePairs.GetRange(0, Math.Min(sortedKeyValuePairs.Count, 10));

            var opt = new JsonSerializerOptions { WriteIndented = true };
            string strJson = JsonSerializer.Serialize(leaderBoard, opt);
            return strJson;
        }

        /// <summary>Current account balance, based on Ledger.</summary>
        /// <returns>Sum of all current transactions (account balance).</returns>
        public int Balance()
        {
            int total = 0;
            foreach(KeyValuePair<string, int> pair in state.Ledger)
            {
                total += pair.Value;
            }
            return total;
        }

        /// <summary>Add interests of previous day. Ticks every day. Connected in owner of this class to the game.</summary>
        public void OnNewDay()
        {
            if (config.DepositInterestRate > 0)
            {
                int interestDay = (int)Math.Floor((Balance() * config.DepositInterestRate) / config.DAYS_PER_YEAR);
                Deposit(config.INTERESTS, interestDay);
            }
        }
    }
}
