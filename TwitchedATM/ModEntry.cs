using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace TwitchedATM
{
    // <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private readonly double INTEREST_RATE = 0.06; // 6 percent per SV-anno for deposits.

        private readonly string SELF = "__SELF__"; // player deposits money he/she owns(!) into account.
        private readonly string CHEATER = "__FED__"; // player creates money out of thin air and deposits it into account.

        private Account account;
        // private TwitchBot twitchBot;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            account = new Account(this, INTEREST_RATE);

            // At the end of each day, update account (with interests)
            helper.Events.GameLoop.DayEnding += OnNewDay;

            // twitchBot = new TwitchBot(this);

            // react to key presses
            // helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            if (Game1.IsMasterGame)
            {
                /*
                 * The following SMAPI console commands are reserved to the main farmer.
                 * Farm hands aren't allowed to access the ATM
                 *   - for game-logical reasons (farm hands have no say in finances)
                 *   - because the account state is NOT replicated to and from the farm hands' computers.
                 */

                // cheater SMAPI console command to add to player's money (FED, central bank function, minting money out of thin air)
                helper.ConsoleCommands.Add("atm_deposit_cheat", $"Add to the player's money.\n\nUsage: atm_deposit_cheat <value> [<sender>]\n- value: the integer amount.\n- sender: name of depositor (default: {CHEATER})", this.CommandDepositCheat);

                // SMAPI console command to deposit player's money or (simulated) Twitch bits in the account (honest function).
                helper.ConsoleCommands.Add("atm_deposit", $"Deposit player's money or (simulated) Twitch bits into account.\n\nUsage: atm_deposit <value> [<sender>]\n- value: the integer amount.\n- sender: name of depositor (default: {SELF})", this.CommandDeposit);

                // SMAPI console command to withdraw money from account and add it to player's money
                helper.ConsoleCommands.Add("atm_withdraw", "Move money from account into player's money.", this.CommandWithdraw);

                // SMAPI console command to display current account activity on the SMAPI console
                helper.ConsoleCommands.Add("atm_activity_current", "Show current account activity.", this.CommandCurrentActivity);

                // SMAPI console command to display total (summed) account activity on the SMAPI console
                helper.ConsoleCommands.Add("atm_activity_total", "Show total (summed) account activity.", this.CommandTotalActivity);
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            // Display player's money when G is pressed.
            if (e.Button.ToString() == "G")
                this.Monitor.Log($"Current balance: {Game1.player.Money}. Total earned: {Game1.player.totalMoneyEarned}", LogLevel.Debug);
        }

        private void OnNewDay(object sender, DayEndingEventArgs e)
        {
            account.OnNewDay();
        }

        /// <summary>Add to the player's money when the atm_deposit_cheat command is invoked in the SMAPI console (cheater version).</summary>
        /// <param name="command">atm_deposit_cheat</param>
        /// <param name="args[0]">integer representing the money to add (can be negative too).</param>
        /// <param name="args[1]">name of depositor (default FED)</param>
        private void CommandDepositCheat(string command, string[] args)
        {
            if (args.Length == 0)
                return;

            int amount = int.Parse(args[0]);
            account.Deposit(args.Length == 2 ? args[1] : CHEATER, amount);
        }

        /// <summary>Add to the player's money when the atm_deposit command is invoked in the SMAPI console (honest version).\n  The amount to deposit comes either from Twitch bits when depositor is not {SELF},\n  or from player's money when depositor is {SELF}.\n  If it comes from player's money, it can't be more than that.</summary>
        /// <param name="command">atm_deposit</param>
        /// <param name="args[0]">integer representing the money to add (can be negative too).</param>
        /// <param name="args[1]">name of depositor (default SELF)</param>
        private void CommandDeposit(string command, string[] args)
        {
            if (args.Length == 0)
                return;

            int amount = int.Parse(args[0]);
            if (amount <= 0) return;

            string depositor = args.Length == 2 ? args[1] : SELF;

            if (depositor == SELF)
            {
                // Non-cheating command. Player can only deposit up to available Player's money, and that amount is subtracted from Player's money.
                int amountToDeposit = Math.Min(amount, Game1.player.Money);
                {
                    // Too bad this can't be an atomic transaction, actually.
                    Game1.player.Money -= amountToDeposit;
                    account.Deposit(SELF, amountToDeposit);
                }
                
            }
            else
            {
                // Everyone else contributed bits (evtl. simulated), so not from thin air. Don't subtract from player's money.
                // Theoretically, we could still cheat because we simulate the Twitch bits here on the SMAPI console, but... yeah.
                account.Deposit(depositor, amount);

                // Provide in-game visual feedback
                Game1.addHUDMessage(new HUDMessage($"{depositor} donated G{amount}"));
            }
        }

        /// <summary>Depositor deposits amount into the account. Helper function needed TwitchBot.</summary>
        /// <param name="depositor">Name of Twitch donator</param>
        /// <param name="amount">Amount of Twitch bits donated</param>
        public void Deposit(string depositor, int amount)
        {
            if (amount > 0)
                account.Deposit(depositor, amount); // XXX multiply amount by some factor?
        }
        
        /// <summary>Withdraw ALL money from account and add it to player's money.</summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public void CommandWithdraw(string command, string[] args) {
            // remove ALL money from account
            int amount = account.Withdraw();

            // add money to player's money
            Game1.player.Money += amount;
            if (amount > 0)
                Game1.player.totalMoneyEarned += (uint)amount;
        }

        /// <summary>Display current account activity on SMAPI console.</summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        private void CommandCurrentActivity(string command, string[] args)
        {
            this.Monitor.Log($"{account.CurrentActivity()}", LogLevel.Debug);
        }

        /// <summary>Display total (summed) account activity on SMAPI console.</summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        private void CommandTotalActivity(string command, string[] args)
        {
            this.Monitor.Log($"{account.TotalActivity()}", LogLevel.Debug);
        }
    }
}
