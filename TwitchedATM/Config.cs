using StardewModdingAPI.Utilities;

namespace TwitchedATM
{
    public class Config
    {
        public string TWITCHED_ATM_BOT_NAME { get; set; } = "<your chatbot name here>";
        public string TWITCHED_ATM_ACCESS_TOKEN { get; set; } = "<your access token here>";
        public string TWITCHED_ATM_CHANNEL_NAME { get; set; } = "<name of channel to monitor for bits here>";

        public bool TwitchIntegrationEnabled = true;

        public string ATM_SAVE_FILE { get; } = "ATM.json"; // relative to Mods/TwitchedATM folder

        public KeybindList ATMMenuKey { get; set; } = KeybindList.Parse("RightShift");

        public double DepositInterestRate { get; set; } = 0.06;
        public double CreditInterestRate { get; set; } = 0.15;
        public double ConversionFactor { get; set; } = 10.0; // in-game-Gs = bits*ConversionFactor

        public int MinimumBitsToDisplayInGame { get; set; } = 100;
 
        public string WITHDRAWALS { get; } = "WITHDRAWALS";
        public string INTERESTS { get; } = "INTERESTS";
        public string SELF { get; } = "__SELF__"; // Key appearing in Ledger, when main farmer honestly deposits
        public string CHEATER { get; } = "__FED__"; // Key appearing in Ledger, when main farmer cheat-deposits (prints money)

        public int DAYS_PER_YEAR { get; } = 4 * 28; // Each SV-season has 28 days.
    }
}
