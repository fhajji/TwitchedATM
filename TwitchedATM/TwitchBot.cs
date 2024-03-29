﻿using StardewValley;
using System;
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
         * 
         * Alternatively, edit config.json in the Mods/TwitchedATM folder.
         */
        string TWITCHEDATM_BOT_NAME;
        string TWITCHEDATM_ACCESS_TOKEN;
        string TWITCHEDATM_CHANNEL_NAME;

        TwitchClient client;
        ModEntry sv; // links back to Stardew Valley TwitchedATM mod.
        Config config;

        ConnectionCredentials credentials;
        ClientOptions clientOptions;
        WebSocketClient customClient;

        string currentChannelName = "<disconnected>";
        public string Channel
        {
            get { return this.currentChannelName; }
        }

    public TwitchBot(ModEntry sv, Config config)
        {
            this.sv = sv;
            this.config = config;

            /*
             * Get Twitch integration configuration
             *   - first from environment variables
             *   - second from default config (if env variables are not set)
             */
            TWITCHEDATM_BOT_NAME = Environment.GetEnvironmentVariable("TWITCHEDATM_BOT_NAME") ?? config.TWITCHED_ATM_BOT_NAME;
            TWITCHEDATM_ACCESS_TOKEN = Environment.GetEnvironmentVariable("TWITCHEDATM_ACCESS_TOKEN") ?? config.TWITCHED_ATM_ACCESS_TOKEN;
            TWITCHEDATM_CHANNEL_NAME = Environment.GetEnvironmentVariable("TWITCHEDATM_CHANNEL_NAME") ?? config.TWITCHED_ATM_CHANNEL_NAME;

            sv.Monitor.Log($"About to connect to {TWITCHEDATM_CHANNEL_NAME} as {TWITCHEDATM_BOT_NAME}.", StardewModdingAPI.LogLevel.Debug);

            credentials = new(TWITCHEDATM_BOT_NAME, TWITCHEDATM_ACCESS_TOKEN);
            clientOptions = new ClientOptions { };

            customClient = new WebSocketClient(clientOptions);

            client = new TwitchClient(customClient);
            client.Initialize(credentials, TWITCHEDATM_CHANNEL_NAME);

            client.OnConnected += OnConnected;
            client.OnJoinedChannel += OnJoinedChannel;
            client.OnMessageReceived += OnMessageReceived;
        }

        public void Run()
        {
            bool result = client.Connect();
            sv.Monitor.Log($"Connected to Twitch: {result}", StardewModdingAPI.LogLevel.Warn);
        }

        private void OnConnected(object sender, OnConnectedArgs e)
        {
            sv.Monitor.Log($"Connected to Twitch Chat Server as {e.BotUsername}", StardewModdingAPI.LogLevel.Warn);
        }

        private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            currentChannelName = e.Channel;

            sv.Monitor.Log($"Joined channel {e.Channel} as {e.BotUsername}", StardewModdingAPI.LogLevel.Warn);

            // Commented out while developing. Don't spam others' channels!
            // client.SendMessage(e.Channel, "TwitchedATM bot listening to cheers/bits");
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            string BitsSender = e.ChatMessage.DisplayName;
            int BitsAmount = e.ChatMessage.Bits;
            int GAmount = Convert.ToInt32(BitsAmount * config.ConversionFactor);

            if (BitsAmount > 0)
            {
                // User BitsSender sent BitsAmount bits. Add them to the player's funds.
                sv.Deposit(BitsSender, GAmount);

                // Display it in-game!
                if (BitsAmount >= config.MinimumBitsToDisplayInGame)
                    Game1.addHUDMessage(new HUDMessage($"{BitsSender} gifted G{GAmount}"));
            }

            // DEBUG - Display ALL chat messages in SMAPI console. Comment out before making a release.
            // sv.Monitor.Log($"M: {e.ChatMessage.DisplayName} {e.ChatMessage.Message}", StardewModdingAPI.LogLevel.Debug);
        }
    }
}
