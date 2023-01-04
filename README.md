# TwitchedATM

Stardew Valley ATM with Twitch Integration SMAPI Mod.

## What is this?

* This mod adds an ATM simulating a Player's bank account to Stardew Valley.
* Player can deposit and withdraw any amount of G-money -- not exceeding what he/she owns in-game -- to or from the ATM.
* The ATM adds interest rates for deposited G-money at the end of every day.
* When Twitch integration is enabled and configured, the ATM listens to Twitch chat and convert viewers' cheers (bits) into additional G-money and deposits it into Player's account.

## How to use?

### Configuring the Mod

The Mod can be configured

* off-game, by editing the file Mods/TwitchedATM/config.json with any text editor
* in-game, by invoking the TwitchedATM Mod Config Menu

The config.json file is created the first time the Mod starts.

Edit it (with any text editor) while the game is NOT running, otherwise the edits will be lost.

The TwitchedATM config menu appears in-game, if the player installed the [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) pre-requisite dependency. It is probably already installed if you have installed [Stardew Valley Expanded](https://www.nexusmods.com/stardewvalley/mods/3753).

To activate the TwitchedATM config menu in-game:

* before a game (a farm save file) is loaded:
  - click on the cog wheel bottom-left on the main title screen (left of 'New', 'Load', 'Co-Op', 'Exit')
  - select 'TwitchedATM' in the 'Configure Mods' menu
* in a loaded game:
  - open the menu (ESC)
  - select the controller tab
  - scroll down to the bottom and click on 'Mod Options'
  - select 'TwitchedATM' in the 'Configure Mods' menu.

### Twitch Integration

TwitchedATM can act as a listening Twitch Chat Bot, and monitors a Twitch channel for cheers / bits by viewers.
These bits are then converted (according to a configurable conversion factor) to G-money and automatically deposited into the ATM account of the player.

To enable this integration, you need to

* create / register a Twitch Chat Bot
* create a Twitch Access Token for this Chat Bot
* save the credentials (twitchbot username, twitch access token), and the monitored Twitch channel name
  - either in Mods/TwitchedATM/config.json
  - or override config.json values in environment variables

This is how to do it:

* Get an OAuth Token from Twitch, e.g. generated with [TMI Twitch App](https://twitchapps.com/tmi/)
* Set the following environment variables:
  - TWITCHEDATM_BOT_NAME (name of the chatbot joining the broadcaster's channel)
  - TWITCHEDATM_CHANNEL_NAME (name of the broadcaster's channel to monitor for cheers / bits)
  - TWITCHEDATM_ACCESS_TOKEN (OAuth token for TWITCHEDATM_BOT_NAME obtained in previous step)
* Alternatively, add those values to Mods/TwitchedATM/config.json (created the first time the game is started). Those environment variables always override config variables set in config.json.

[How to set environment variable under Windows](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_environment_variables?view=powershell-7.3).

## Using the Mod in-Game

The ATM-Menu can be activated anytime in-game by pressing the Key(-Sequence) assigned to it
* in the file Mods/TwitchedATM/config.json
* in-game in the TwitchedATM Mod Config Menu

The menu offers the following options:

* Deposit: deposit some G-money from the farm into the ATM account.
* Withdraw: withdraw ALL G-money from the ATM account and put it back into the farm.
* Leaderboard (current): shows top-<N> (N = 20 by default) depositors starting after last withdrawal.
* Leaderboard (all-time): shows top-<N> (N = 20 by default) depositors since the ATM account was created.

The Balance is the amount of G-money currently in the ATM. It is the sum of all deposits since last withdrawal, including added daily interests.

### Deposit

* The main player can deposit any specified amount of G-money he/she/they owns into the ATM account.
* That money is subtracted from the farm money and is not available for making purchases etc. while in the ATM account.
* While in the ATM account, it generates interests at the end of each day, according to the configurable Deposit Interest Rate (per Stardew Valley year). It is added to the 'INTEREST' special name for bookkeeping purposes.

If Twitch Integration is enabled, cheers/bits from Twitch chatters are also auto-deposited into the ATM account (multiplied by a configurable Conversion Rate Factor).
This happens behind the scenes without the Player having to invoke the Deposit menu. If the amount of bits cheered is greater-than or equal the Minimum Bits to Display in-Game, the donation (converted in G-money) is displayed in-game as an HUDMessage.

### Withdrawal

* The main player can decide to withdraw ALL G-money from the ATM account.
* The G-money is then subtracted from that account, and added to the farm's G-money.
* The Leaderboard (current) is cleared and starts empty again.
* The amount withdrawn is subtracted from the special name 'WITHDRAWALS' in the Leaderboard (all-time) for bookkeeping purposes.

### Current Leaderboard

* This displays the top-<N> (N = 20 by default) donors with their summed deposits since last withdrawal.
* Even though only N donors are shown, all bits/cheers are added to the ATM account, even if they are from more than N donors.
* The special name INTERESTS sums all interest that were added to the ATM account since last withdrawal.

### All-time Leaderboard

* This displays the all-times top-<N> (N = 20 by default) donors with their summed deposits since the ATM account was created.
* Unlike the Current Leaderboard, this leaderboard persists across withdrawals.
* The special name INTERESTS sums all earned interests since this account was created.
* The special name WITHDRAWALS sums all Gs that were taken out of this account and back into the farm (as a negative number).

## Building instructions

### Dependencies

* Frameworks:
  - NET 5.0 Runtime (yes, it is EOLed, must use this!)
* Packages:
  - Pathoschild.Stardew.ModBuildConfig (4.0.2+)
  - TwitchLib (3.5.3+)
* Mods:
  - [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)

### Compiling instructions

* To compile the TwitchedATM mod, first install the required dependencies:
  - Add NET 5.0 Runtime as a Framework
  - Add ModBuildConfig and TwitchLib as packages in your current Visual Studio C# project using NuGet package manager.
  - Refer to [IDE Reference](https://stardewvalleywiki.com/Modding:IDE_reference) for how to do this.
* Compiling the project in Visual Studio will also copy the files manifest.json, TwitchedATM.dll, TwitchedATM.pdb into the folder Mods/TwitchedATM of the Stardew Valley Steam folder.
* To compile the TwitchLib DLLs and dependencies
  - create and compile a C# Console App with TwitchLib as a (NuGet) Dependency
  - copy the created DLLs over to the Mods/TwitchedATM folder: Newtonsoft.Json.dll, TwitchLib.*.dll, Microsoft.Extensions.*.dll
  - (If you know how to streamline this, please submit a pull request)

Alternatively, just install/unzip the Binary Release into the Stardew Valley Mods folder.

## TO DO

* Implement borrowing credit mechanism: not yet done.
* Add game asset that represents the ATM. Maybe use [ContentPatcher](https://www.nexusmods.com/stardewvalley/mods/1915)?
* Package Mod up and release as a zip file. Add to nexusmods etc...: TBD.

## Credits

The basic idea for the ATM is from [Platonymous ATM mod](https://github.com/Platonymous/Stardew-Valley-Mods/tree/master/ATM).

Variations and Twitch integration are mine.

## Author

Farid Hajji (fhajji) <farid@hajji.name>

## License

Copyright (C) 2022-2023 Farid Hajji. All rights reserved.

Released under GPLv3. See [LICENSE.txt](LICENSE.txt)
