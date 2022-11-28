using System;
using System.Collections.Generic;

namespace TwitchedATM
{
    public class AccountState
    {
        public Dictionary<string, int> Ledger { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> PermanentLedger { get; set; } = new Dictionary<string, int>();
    }
}
