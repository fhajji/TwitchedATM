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
  - TWITCHEDATM_BOT_NAME (name of the chatbot joining the broadcaster's channel)
  - TWITCHEDATM_CHANNEL_NAME (name of the broadcaster's channel to monitor for cheers / bits)
  - TWITCHEDATM_ACCESS_TOKEN (OAuth token for TWITCHEDATM_BOT_NAME obtained in previous step)

[How to set environment variable under Windows](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_environment_variables?view=powershell-7.3).

## Building instructions

_TODO_

In the mean time, refer to the Stardew Valley Wiki's [IDE Reference](https://stardewvalleywiki.com/Modding:IDE_reference)

### Manually install dependencies in the Mods folder

The following TwitchLib dependencies need to be added to the Mods/TwitchedATM folder in addition to TwitchedATM.{dll,pdb} and manifest.json. For some reason, the build system only copies the TwitchedATM DLL (and PDB and manifest.json) over.

```
Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a----        11/24/2022   8:39 AM            269 manifest.json
-a----        11/26/2022   4:33 AM           8192 TwitchedATM.dll
-a----        11/26/2022   4:33 AM          12036 TwitchedATM.pdb
-a----         3/17/2021   9:03 PM         695336 Newtonsoft.Json.dll
-a----         11/1/2022   7:47 PM          76800 TwitchLib.Api.Core.dll
-a----         11/1/2022   7:47 PM          10752 TwitchLib.Api.Core.Enums.dll
-a----         11/1/2022   7:47 PM           7680 TwitchLib.Api.Core.Interfaces.dll
-a----         11/1/2022   7:47 PM           7168 TwitchLib.Api.Core.Models.dll
-a----         11/1/2022   7:47 PM          53248 TwitchLib.Api.dll
-a----         11/1/2022   7:47 PM          65024 TwitchLib.Api.Helix.dll
-a----         11/1/2022   7:47 PM         136704 TwitchLib.Api.Helix.Models.dll
-a----         11/4/2022  12:15 AM          80896 TwitchLib.Client.dll
-a----         11/4/2022  12:15 AM           7168 TwitchLib.Client.Enums.dll
-a----         11/4/2022  12:15 AM          89600 TwitchLib.Client.Models.dll
-a----          4/8/2022  10:47 PM          36352 TwitchLib.Communication.dll
-a----         11/3/2022   1:42 AM          44032 TwitchLib.EventSub.Core.dll
-a----         11/3/2022   1:59 AM          60928 TwitchLib.EventSub.Websockets.dll
-a----         11/3/2022   2:21 AM          84480 TwitchLib.PubSub.dll
-a----        10/23/2021   1:48 AM          43632 Microsoft.Extensions.DependencyInjection.Abstractions.dll
-a----        10/23/2021   1:49 AM          81536 Microsoft.Extensions.DependencyInjection.dll
-a----        10/23/2021   1:51 AM          62064 Microsoft.Extensions.Logging.Abstractions.dll
-a----        10/23/2021   1:50 AM          44656 Microsoft.Extensions.Logging.dll
-a----        10/23/2021   1:50 AM          59008 Microsoft.Extensions.Options.dll
-a----        10/23/2021   1:51 AM          40048 Microsoft.Extensions.Primitives.dll
```

Generated by creating a console application instead of a class.

## Dependencies

* Frameworks:
  - NET 5.0 Runtime (yes, it is EOLed, must use this!)
* Packages:
  - Pathoschild.Stardew.ModBuildConfig (4.0.2+)
  - TwitchLib (3.5.3+)

## Changelog

_Work in Progress: figuring things out_

* Initial Boilerplate Code from [Get Started](https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started): done.
* Connected to Twitch
* Detected cheers/bits
* Added bits to player's money (it shows up in the HUD too)
* Display bits/cheers (and in debugging all Twitch chat messages) in SMAPI console
* Added a 'atm_addmoney' "cheating" command to SMAPI console

## Credits

The basic idea for the ATM is from [Platonymous ATM mod](https://github.com/Platonymous/Stardew-Valley-Mods/tree/master/ATM).

Variations and Twitch integration are mine.

## Author

Farid Hajji (fhajji) <farid@hajji.name>

## License

Copyright (C) 2022 Farid Hajji. All rights reserved.

Released under GPLv3. See [LICENSE.txt](LICENSE.txt)
