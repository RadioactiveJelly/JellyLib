# JellyLib
A library mod that contains useful functions I use for my mutators.

[![Static Badge](https://img.shields.io/badge/Release-0.1.0-green)](https://github.com/RadioactiveJelly/JellyLib/releases/tag/0.1.0)
[![Static Badge](https://img.shields.io/badge/Discord-JellyfishFields-7289da)](https://discord.gg/8MEyVPDf4Q)

### Installation

This mod requires BepInEx to run. To ensure stability, please install the latest 5.x.x version  of BepInEx. Using BepInEx 6 will not work.

You may find BepInEx (and how to install it) [here](https://github.com/BepInEx/BepInEx).

Once BepInEx is installed, run the game once. Afterwards, place the JellyLib.dll into the plugins folder. When you run the game, check the top right corner of the main menu. If the indicator is present, then the mod successfully installed.

### So What Is This?

If you're just a player who needs to install the mod as a dependency, this next part isn't important. JellyLib doesn't change any gameplay elements on its own. If you install this without any mods that actually make use of its features it won't change your game much if at all. The mod's main purpose is to expand Ravenscript's capabilities. A lot of what's on here are things that are unlikely to be added to the vanilla game for various reasons.

If you're a mod developer interested in what's possible with this plugin, documentation will be made that goes over all the features provided. In the meantime, feel free to reach out to me on Discord. This mod is still in active development so the API may see some changes overtime.

### Features
* File Read/Writing
* Extended Ravenscript events
  * onProjectileLandedOnTerrain
  * onMedipackResupply
  * onAmmoBoxResupply
  * onPlayerFireWeapon
 * Extended Damage System
   * Will effectively replace DamageCore
  * Weapon Utilities
    * Will effectively replace WeaponTweaker

More features are planned and will be added overtime as development of this mod continues.
