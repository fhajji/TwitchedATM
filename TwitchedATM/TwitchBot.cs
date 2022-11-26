using StardewValley;
using System;
// using System.Text.Json;
// using System.Text.Json.Serialization;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchedATM
{
    public class TwitchBot
    {
        /*
         * Populate environment variables like this:
         *   TWITCHEDATM_BOT_NAME: username of your Twitch bot account (or main account)
         *   TWITCHEDATM_ACCESS_TOKEN: OAuth token of the bot account. Generate via https://twitchapps.com/tmi/
         *   TWITCHEDATM_CHANNEL_NAME: name of Twitch channel to join (as TWITCHEDATM_BOT_NAME)
         * 
         * IMPORTANT: Don't write the access token directly into the source code,
         * as it may inadvertently land in a public (GitHub) repository!
         */
        static readonly string TWITCHEDATM_BOT_NAME = Environment.GetEnvironmentVariable("TWITCHEDATM_BOT_NAME") ?? "<your_bot_name>";
        static readonly string TWITCHEDATM_ACCESS_TOKEN = Environment.GetEnvironmentVariable("TWITCHEDATM_ACCESS_TOKEN") ?? "<your_bots_access_token>";
        static readonly string TWITCHEDATM_CHANNEL_NAME = Environment.GetEnvironmentVariable("TWITCHEDATM_CHANNEL_NAME") ?? "<channel_to_join_and_monitor>";

        TwitchClient client;
        ModEntry sv; // links back to Stardew Valley TwitchedATM mod.

        public TwitchBot(ModEntry sv)
        {
            this.sv = sv;

            sv.Monitor.Log($"About to connect to {TWITCHEDATM_CHANNEL_NAME} as {TWITCHEDATM_BOT_NAME}.", StardewModdingAPI.LogLevel.Debug);

            ConnectionCredentials credentials = new(TWITCHEDATM_BOT_NAME, TWITCHEDATM_ACCESS_TOKEN);
            var clientOptions = new ClientOptions { };

            WebSocketClient customClient = new WebSocketClient(clientOptions);

            client = new TwitchClient(customClient);
            client.Initialize(credentials, TWITCHEDATM_CHANNEL_NAME);

            client.OnConnected += OnConnected;
            client.OnJoinedChannel += OnJoinedChannel;
            client.OnMessageReceived += OnMessageReceived;

            client.Connect();
        }

        private void OnConnected(object sender, OnConnectedArgs e)
        {
            sv.Monitor.Log($"Connected to {e.AutoJoinChannel}", StardewModdingAPI.LogLevel.Debug);
        }

        private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            sv.Monitor.Log($"Joined channel {e.ToString()}", StardewModdingAPI.LogLevel.Debug);

            // Commented out while developing. Don't spam others' channels!
            // client.SendMessage(e.Channel, "TwitchedATM bot listening to cheers/bits");
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            string BitsSender = e.ChatMessage.DisplayName;
            int BitsAmount = e.ChatMessage.Bits;

            if (BitsAmount > 0)
            {
                sv.Monitor.Log($"Cheers! {BitsSender} sent {BitsAmount} bits.", StardewModdingAPI.LogLevel.Debug);

                // XXX add them as G-money to (main player, or the atm balance).
                StardewValley.Game1.player.Money += BitsAmount;
            }

            // DEBUG - Display ALL chat messages in SMAPI console. Comment out before making a release.
            sv.Monitor.Log($"M: {e.ChatMessage.DisplayName} {e.ChatMessage.Message}", StardewModdingAPI.LogLevel.Debug);
        }
    }
}
