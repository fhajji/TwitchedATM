using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System.Threading;
using TwitchLib.Client.Models;
using GenericModConfigMenu;

namespace TwitchedATM
{
    // <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        static Config config;
        private Account account;
        private AccountState accountState;
        private TwitchBot twitchBot;

        private List<StardewValley.Response> atmMenuResponses;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Read global configuration from Mods/TwitchedATM/config.json
            config = helper.ReadConfig<Config>();
            helper.WriteConfig(config);

            // Read account state (contents) from Mods/TwitchedATM/{config.ATM_SAVE_FILE}
            accountState = helper.Data.ReadJsonFile<AccountState>(config.ATM_SAVE_FILE) ?? new AccountState();
            if (! accountState.PermanentLedger.ContainsKey(config.WITHDRAWALS))
                accountState.PermanentLedger[config.WITHDRAWALS] = 0;
            if (!accountState.PermanentLedger.ContainsKey(config.INTERESTS))
                accountState.PermanentLedger[config.INTERESTS] = 0;

            account = new Account(this, config, accountState);

            atmMenuResponses = CreateATMMenuResponses();

            // At the end of each day, update account (with interests), and save it.
            helper.Events.GameLoop.DayEnding += OnNewDay;

            // Only start TwitchBot if explicitly enabled in Config.
            if (config.TwitchIntegrationEnabled)
            {
                twitchBot = new TwitchBot(this, config);
                Task.Run(() => { twitchBot.Run(); });
            }
            else
            {
                this.Monitor.Log($"Twitch integration disabled in Mods/TwitchedATM/config.json", LogLevel.Warn);
            }

            // react to keybinds (e.g. to open ATM menu)
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

            // react to key presses
            // helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            // Integrate GenericModConfigMenu
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            if (Game1.IsMasterGame)
            {
                /*
                 * The following SMAPI console commands are reserved to the main farmer.
                 * Farm hands aren't allowed to access the ATM
                 *   - for game-logical reasons (farm hands have no say in finances)
                 *   - because the account state is NOT replicated to and from the farm hands' computers.
                 */

                // cheater SMAPI console command to add to player's money (FED, central bank function, minting money out of thin air)
                helper.ConsoleCommands.Add("atm_deposit_cheat", $"Add to the player's money.\n\nUsage: atm_deposit_cheat <value> [<sender>]\n- value: the integer amount.\n- sender: name of depositor (default: {config.CHEATER})", this.CommandDepositCheat);

                // SMAPI console command to deposit player's money or (simulated) Twitch bits in the account (honest function).
                helper.ConsoleCommands.Add("atm_deposit", $"Deposit player's money or (simulated) Twitch bits into account.\n\nUsage: atm_deposit <value> [<sender>]\n- value: the integer amount.\n- sender: name of depositor (default: {config.SELF})", this.CommandDeposit);

                // SMAPI console command to withdraw money from account and add it to player's money
                helper.ConsoleCommands.Add("atm_withdraw", "Move money from account into player's money.", this.CommandWithdraw);

                // SMAPI console command to display current account activity on the SMAPI console
                helper.ConsoleCommands.Add("atm_activity_current", "Show current account activity.", this.CommandCurrentActivity);

                // SMAPI console command to display total (summed) account activity on the SMAPI console
                helper.ConsoleCommands.Add("atm_activity_total", "Show total (summed) account activity.", this.CommandTotalActivity);

                // SMAPI console command to save account to config.ATM_SAVE_FILE immediately.
                helper.ConsoleCommands.Add("atm_save_state", "Save account to file NOW.", this.CommandSaveState);

                // SMAPI console command to open the in-game ATM main menu.
                helper.ConsoleCommands.Add("atm_menu", "Open ATM main menu.", this.CommandOpenATMMenu);
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            // this.Monitor.Log($"OnButtonsChanged() called. {config.ATMMenuKey.IsDown()}", LogLevel.Debug);

            if (config.ATMMenuKey.JustPressed())
            {
                // this.Monitor.Log($"OnButtonsChanged(): ATMMenuKey just pressed", LogLevel.Debug);

                // ignore if player hasn't loaded a save yet
                if (!Context.IsWorldReady)
                {
                    this.Monitor.Log("ATMMenu() only available after game has been loaded.", LogLevel.Debug);
                    return;
                }

                // Display ATM menu
                OpenATMMenu();
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

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Monitor.Log("OnGameLaunched() called", LogLevel.Debug);

            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null) {
                this.Monitor.Log("spacechase0.GenericModConfigMenu not installed. Configure options manually in TwitchedATM/config.json.", LogLevel.Info);
                return; // Player didn't install GenericModConfigMenu mod
            }

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => {
                    /* this.Config = new ModConfig() */
                    this.Monitor.Log("configMenu.Register.reset not yet implemented. Edit config.json manually.", LogLevel.Warn);
                },
                save: () => {
                    this.Helper.WriteConfig(config);
                }
            );

            // add some config options

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "Twitch Integration",
                tooltip: () => "Configure Twitch Bot Name and Access Token in Mods/TwitchedATM/config.json and (optionally) override them by setting environment variables"
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Twitch Integration",
                tooltip: () => "Enable Twitch Chat Integration.",
                getValue: () => config.TwitchIntegrationEnabled,
                setValue: value => {
                    if (twitchBot != null && value == false)
                    {
                        // Twitch Integration was enabled (and running). Can't stop it without manual restart (XXX: really?)
                        this.Monitor.Log("Twitch Integration Disabled. Manually Restart Stardew Valley to stop listening to Twitch.", LogLevel.Warn);
                    }

                    config.TwitchIntegrationEnabled = value;
                    if (value == true && twitchBot == null)
                    {
                        // Twitch Integration was disabled. Enable it now!
                        twitchBot = new TwitchBot(this, config);
                        Task.Run(() => { twitchBot.Run(); });
                    }
                }
            );

            configMenu.AddTextOption(
                /*
                 * The monitored Twitch Channel isn't sensitive and can be shown in-Game.
                 * 
                 * Changing it in config.json is possible.
                 * But doing so requires a Stardew Valley restart for the change to take effect.
                 * 
                 * CAVEAT:
                 *   If the TWITCHEDATM_CHANNEL_NAME environment variable is set,
                 *   that setting _always_ overrides what is in config.json.
                 *   
                 *   Therefore, just changing the channel name here (and restarting SV)
                 *   may not have the desired effect.
                 *   
                 * TODO (XXX): set the process and user environment variable too, as shown in
                 *   https://learn.microsoft.com/en-us/dotnet/api/system.environment.setenvironmentvariable?view=net-5.0
                 */
                mod: this.ModManifest,
                name: () => "Twitch Channel",
                tooltip: () => "Name of Twitch Channel (Broadcaster) to monitor for Bits",
                getValue: () => {
                    if (twitchBot != null)
                        return twitchBot.Channel;
                    else
                        return config.TWITCHED_ATM_CHANNEL_NAME;
                    },
                setValue: value =>
                {
                    if (value != config.TWITCHED_ATM_CHANNEL_NAME && twitchBot != null)
                    {
                        config.TWITCHED_ATM_CHANNEL_NAME = value;
                        this.Monitor.Log("Manually restart Stardew Valley for Twitch Channel Name change to take effect", LogLevel.Alert);
                    }                    
                }
            );

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: () => "ATM Settings",
                tooltip: null
            );

            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: () => "ATM Menu Key",
                tooltip: () => "Key(s) to enable the ATM Menu.",
                getValue: () => config.ATMMenuKey,
                setValue: value => config.ATMMenuKey = value
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Minimum Bits to Display in-Game",
                tooltip: () => "Smallest required Bits donation to be displayed in-Game",
                getValue: () => config.MinimumBitsToDisplayInGame,
                setValue: value => config.MinimumBitsToDisplayInGame = Convert.ToInt32(value),
                min: 1,
                max: null
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Bits to G Conversion Factor",
                tooltip: () => "G = Bits * Conversion Factor",
                getValue: () => Convert.ToSingle(config.ConversionFactor),
                setValue: value => config.ConversionFactor = Convert.ToDouble(value),
                min: 1.0f,
                max: null
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Deposit Interest Rate",
                tooltip: () => "How much interest to get from deposits (in %)",
                getValue: () => Convert.ToSingle(config.DepositInterestRate * 100.0),
                setValue: value => config.DepositInterestRate = Convert.ToDouble(value) * 0.01,
                min: 0.0f,
                max: null
            );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Credit Interest Rate",
                tooltip: () => "How much interest to pay for loans (in %) [Not yet implemented]",
                getValue: () => Convert.ToSingle(config.CreditInterestRate * 100.0),
                setValue: value => config.CreditInterestRate = Convert.ToDouble(value) * 0.01,
                min: 0.0f,
                max: null
            );

        }

        private void OnNewDay(object sender, DayEndingEventArgs e)
        {
            // compute and add/subtract interests for the day
            account.OnNewDay();

            // save account state to config.ATM_SAVE_FILE
            this.Helper.Data.WriteJsonFile(config.ATM_SAVE_FILE, accountState);
        }

        private static List<StardewValley.Response> CreateATMMenuResponses()
        {
            List<StardewValley.Response> options = new()
            {
                new StardewValley.Response("ATM_DEPOSIT", "Deposit"),
                new StardewValley.Response("ATM_WITHDRAW", "Withdraw"),
                new StardewValley.Response("ATM_LEDGER", "Leaderboard (current)"),
                new StardewValley.Response("ATM_PERMANENTLEDGER", "Leaderboard (all-times)"),
                new StardewValley.Response("ATM_CLOSE", "Exit Menu")
            };
            return options;
        }

        private void OpenATMMenu()
        {
            if (!Game1.IsMasterGame)
            {
                Game1.addHUDMessage(new HUDMessage("Only the main farmer can use the ATM."));
                return;
            }

            string text = $"Balance: {account.Balance()}";
            Game1.currentLocation.createQuestionDialogue(text, atmMenuResponses.ToArray(), OpenATMMenuNext);
        }

        private void OpenATMMenuNext(Farmer who, string key) {
            if (key == "ATM_CLOSE")
                return;

            if (key == "ATM_DEPOSIT")
            {
                Game1.activeClickableMenu = new NumberSelectionMenu("Deposit amount", (number, price, farmer) => {
                    GameToATM(number, config.SELF);
                    Game1.activeClickableMenu = null;
                }, 0, 0, 1000000, 0);

                // this.Monitor.Log("ATM_DEPOSIT menu called", LogLevel.Debug);
            }
            else if (key == "ATM_WITHDRAW")
            {
                ATMToGame();
                
                // this.Monitor.Log("ATM_WITHDRAW menu called", LogLevel.Debug);
            }
            else if (key == "ATM_LEDGER")
            {
                string ledgerAsString = account.CurrentActivity();
                Game1.currentLocation.createQuestionDialogue(ledgerAsString, new List<StardewValley.Response>().ToArray(), null);

                this.Monitor.Log($"{ledgerAsString}", LogLevel.Debug);
            }
            else if (key == "ATM_PERMANENTLEDGER")
            {
                string ledgerAsString = account.TotalActivity();
                Game1.currentLocation.createQuestionDialogue(ledgerAsString, new List<StardewValley.Response>().ToArray(), null);

                this.Monitor.Log($"{account.TotalActivity()}", LogLevel.Debug);
            }
        }

        private void GameToATM(int amount, string depositor)
        {
            // Non-cheating command. Player can only deposit up to available Player's money, and that amount is subtracted from Player's money.
            int amountToDeposit = Math.Min(amount, Game1.player.Money);
            {
                // Too bad this can't be an atomic transaction, actually.
                Game1.player.Money -= amountToDeposit;
                account.Deposit(depositor, amountToDeposit);
            }
        }

        private void ATMToGame()
        {
            // what we deposited ourselves doesn't count towards totalMoneyEarned!
            int selfDeposits = accountState.Ledger[config.SELF];

            // remove ALL money from account
            int amount = account.Withdraw();

            // add money to player's money
            Game1.player.Money += amount;
            if (amount > 0)
                Game1.player.totalMoneyEarned += (uint)(amount - selfDeposits);

        }

        /// <summary>Add to the player's money when the atm_deposit_cheat command is invoked in the SMAPI console (cheater version).</summary>
        /// <param name="command">atm_deposit_cheat</param>
        /// <param name="args[0]">integer representing the money to add (can be negative too).</param>
        /// <param name="args[1]">name of depositor (default FED)</param>
        private void CommandDepositCheat(string command, string[] args)
        {
            if (args.Length == 0)
                return;

            int amount = Convert.ToInt32(int.Parse(args[0]) * config.ConversionFactor);
            account.Deposit(args.Length == 2 ? args[1] : config.CHEATER, amount);
        }

        /// <summary>Add to the player's money when the atm_deposit command is invoked in the SMAPI console (honest version).\n  The amount to deposit comes either from Twitch bits when depositor is not {SELF},\n  or from player's money when depositor is {SELF}.\n  If it comes from player's money, it can't be more than that.</summary>
        /// <param name="command">atm_deposit</param>
        /// <param name="args[0]">integer representing the money to add (can be negative too).</param>
        /// <param name="args[1]">name of depositor (default SELF)</param>
        private void CommandDeposit(string command, string[] args)
        {
            if (args.Length == 0)
                return;

            int bits = 0;
            int amount = 0;
            try
            {
                bits = int.Parse(args[0]);
                amount = Convert.ToInt32(bits * config.ConversionFactor);
                if (amount <= 0) return;
            }
            catch(Exception e) {
                this.Monitor.Log($"atm_deposit() error. Wrong order of arguments?", LogLevel.Debug);
            }

            string depositor = args.Length == 2 ? args[1] : config.SELF;

            if (depositor == config.SELF)
            {
                GameToATM(amount, config.SELF);                
            }
            else
            {
                // Everyone else contributed bits (evtl. simulated), so not out of thin air. Don't subtract from player's money.
                // Theoretically, we could still cheat because we simulate the Twitch bits here on the SMAPI console, but... yeah.
                account.Deposit(depositor, amount);

                // Provide in-game visual feedback
                if (bits >= config.MinimumBitsToDisplayInGame)
                    Game1.addHUDMessage(new HUDMessage($"{depositor} donated G{amount}"));
            }
        }

        /// <summary>Depositor deposits amount into the account. Helper function needed by TwitchBot.</summary>
        /// <param name="depositor">Name of Twitch donator</param>
        /// <param name="amount">Amount of Gs (= Twitch bits * config.ConversionFactor) donated</param>
        public void Deposit(string depositor, int amount)
        {
            if (amount > 0)
                account.Deposit(depositor, amount);
        }
        
        /// <summary>Withdraw ALL money from account and add it to player's money.</summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public void CommandWithdraw(string command, string[] args) {
            ATMToGame();
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

        private void CommandSaveState(string  command, string[] args)
        {
            this.Helper.Data.WriteJsonFile(config.ATM_SAVE_FILE, accountState);
            this.Monitor.Log($"Account state saved to {config.ATM_SAVE_FILE}", LogLevel.Debug);
        }

        private void CommandOpenATMMenu(string command, string[] args)
        {
            // Open in-game ATM main menu
            OpenATMMenu();
        }
    }
}
