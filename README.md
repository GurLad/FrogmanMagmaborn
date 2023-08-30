# Frogman Magmaborn
Frogman Magmaborn is an open-source rogue-like strategy RPG made in Unity. It is now complete! You can download the latest version here:
 - [Steam](https://store.steampowered.com/app/1768830/Frogman_Magmaborn)
 - [itch.io](https://disc-o-key.itch.io/frogman-magmaborn)
 - [GitHub](https://github.com/GurLad/FrogmanMagmaborn/releases)
 - [The Disc-O-Key website](https://disk-o-key.com/frogmanMagmaborn.html)
## Building the game
Frogman Magmaborn was made in Unity 2020.3.38f1. It has two build modes, which you can change using the `Scripting Define Symbols` option in the build settings:
 - Normal (no symbols) - all game data must be loaded in Unity, and will not change during run-time. Equivalent to the base `Frogman Magmaborn` release.
 - Moddable (use the `MODDABLE_BUILD` symbol) - all game data will be loaded during run-time from the `Data` folder. Equivalent to the Game folder in the base `Frog Forge` (not gameless) release.

In order to edit the game's data for either version, it's heavily recommended to use Frog Forge.
## [Frog Forge](../../../FrogForge)
Frog Forge is the modding tool I made to edit game files (maps, conversations, classes...). You can view more information about it in its repository.
