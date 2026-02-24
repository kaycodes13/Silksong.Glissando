# Glissando

A Hollow Knight: Silksong mod that replaces jumping with flipping gravity.

Double jumping also flips gravity. To make traversal a little more reasonable, once you've unlocked both float and double jump, pressing down and jump in the air will always cause you to float.

Donation Link: https://ko-fi.com/kaykao

## Installation

Install via r2modman or Thunderstore Mod Manager. Glissando's dependencies should be installed automatically.

For manual installation, extract the `.zip` file and place the resulting **folder** in the `BepInEx/plugins` directory for your Silksong install. You'll also have to manually install:
* [FsmUtil](<https://thunderstore.io/c/hollow-knight-silksong/p/silksong_modding/FsmUtil/>)
* [I18N](<https://thunderstore.io/c/hollow-knight-silksong/p/silksong_modding/I18N/>)
* [ModMenu](<https://thunderstore.io/c/hollow-knight-silksong/p/silksong_modding/ModMenu/>) and its dependencies

---

## For Mod Developers

If you're making a mod that changes Hornet's velocity at any point - for example, a custom crest with custom FSM edits for its charged/dash/down attacks - the y velocities you set *won't* automatically flip when VVVVVV flips gravity.

If you want your mod to be fully compatible with VVVVVV, you can check the value of the static boolean property `V6Plugin.GravityIsFlipped` and invert y velocities/movement when it's true.
