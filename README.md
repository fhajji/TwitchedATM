# TwitchedATM

Stardew Valley ATM with Twitch Integration SMAPI Mod.

## What is this?

* This mod adds an ATM simulating a Player's bank account to Stardew Valley.
* Player can deposit and withdraw any amount of G-money -- not exceeding what he/she owns in-game -- to or from the ATM.
* The ATM adds season-dependent interest rates for deposited G-money.
* The ATM subtracts season-dependent interest rates in case of a negative balance. Overdraft limit is configurable.
* When Twitch integration is enabled and configured, the ATM listens to Twitch chat and convert viewers' cheers (bits) into additional G-money and deposits it into Player's account.

## Twitch Integration

* Get an OAuth Token from Twitch, e.g. generated with [TMI Twitch App](https://twitchapps.com/tmi/)
* Set the following environment variables:
  - TWITCHED_ATM_ACCESS_TOKEN (OAuth token obtained in previous step)
  - TWITCHED_ATM_CHANNEL_NAME (name of the broadcaster)

[How to set environment variable under Windows](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_environment_variables?view=powershell-7.3).

## Building instructions

_TODO_

In the mean time, refer to the Stardew Valley Wiki's [IDE Reference](https://stardewvalleywiki.com/Modding:IDE_reference)

## Dependencies

* Frameworks:
  - NET 5.0 Runtime (yes, it is EOLed, must use this!)
* Packages:
  - Pathoschild.Stardew.ModBuildConfig (4.0.2+)
  - TwitchLib (3.5.3+)

## Changelog

_Work in Progress: nothing implemented yet_

* Initial Boilerplate Code from [Get Started](https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started): done.

## Credits

The basic idea for the ATM is from [Platonymous ATM mod](https://github.com/Platonymous/Stardew-Valley-Mods/tree/master/ATM).

Variations and Twitch integration are mine.

## Author

Farid Hajji (fhajji) <farid@hajji.name>

## License

Copyright (C) 2022 Farid Hajji. All rights reserved.

Released under GPLv3. See [LICENSE.txt](LICENSE.txt)
