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
        private TwitchBot twitchBot;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            twitchBot = new TwitchBot(this);

            // react to key presses
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            // cheater console command to add to player's money
            helper.ConsoleCommands.Add("atm_addmoney", "Add to the player's money.\n\nUsage: atm_addmoney <value>\n- value: the integer amount.", this.AddMoney);
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

            // Display current G-money when 'G' is pressed
            if (e.Button.ToString() == "G")
                this.Monitor.Log($"Current balance: {Game1.player.Money}. Total earned: {Game1.player.totalMoneyEarned}", LogLevel.Debug);
        }

        /// <summary>Add to the player's money when the atm_addmoney command is invoked.</summary>
        /// <param name="command">atm_addmoney.</param>
        /// <param name="args">integer representing the money to add (can be negative too).</param>
        private void AddMoney(string command, string[] args)
        {
            int currentMoney = Game1.player.Money;
            int additionalMoney = int.Parse(args[0]);
            Game1.player.Money = currentMoney + additionalMoney;
            if (additionalMoney > 0)
                Game1.player.totalMoneyEarned += (uint)additionalMoney;            
        }
    }
}
