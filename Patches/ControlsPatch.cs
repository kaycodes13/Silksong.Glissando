using HarmonyLib;
using Glissando.Settings;

namespace Glissando.Patches;

[HarmonyPatch(typeof(HeroController))]
internal static class ControlsPatch {

	[HarmonyPatch(nameof(HeroController.CanSuperJump))]
	[HarmonyPostfix]
	private static void DisableSilkSoar(ref bool __result) {
		if (GlissandoPlugin.GravityIsFlipped)
			__result = false;
	}

	[HarmonyPatch(nameof(HeroController.CanDoubleJump))]
	[HarmonyPostfix]
	private static void AllowFloatOnDownAndJump(HeroController __instance, ref bool __result) {
		if (!__result)
			return;

		__result = GlissandoPlugin.Settings.FaydownState switch {
			FaydownState.Disabled => false,
			_ => !__instance.inputHandler.inputActions.Down.IsPressed
		};
	}

	[HarmonyPatch(nameof(HeroController.Awake))]
	[HarmonyPostfix]
	private static void FixFlippingOnReload() {
		GlissandoPlugin.GravityIsFlipped = false;
	}

	[HarmonyPatch(nameof(HeroController.HeroJump), [typeof(bool)])]
	[HarmonyPostfix]
	private static void FlipOnJump(HeroController __instance) {
		GlissandoPlugin.FlipGravity(__instance, jumpBoost: true);
		__instance.CancelHeroJump();
	}

	[HarmonyPatch(nameof(HeroController.HeroJumpNoEffect))]
	[HarmonyPostfix]
	private static void FlipOnBackflip(HeroController __instance) {
		GlissandoPlugin.FlipGravity(__instance, jumpBoost: true);
		__instance.CancelHeroJump();
	}

	[HarmonyPatch(nameof(HeroController.DoDoubleJump))]
	[HarmonyPostfix]
	private static void FlipOnDoubleJump(HeroController __instance) {
		if (GlissandoPlugin.Settings.FaydownState.FlipsGravity()) {
			GlissandoPlugin.FlipGravity(__instance);
			__instance.CancelHeroJump();
		}
	}

	[HarmonyPatch(nameof(HeroController.HazardRespawn))]
	[HarmonyPrefix]
	private static void FlipOnHazardRespawn(HeroController __instance) {
		if (GlissandoPlugin.GravityIsFlipped)
			GlissandoPlugin.FlipGravity(__instance, force: true);
	}

	[HarmonyPatch(nameof(HeroController.Respawn))]
	[HarmonyPrefix]
	private static void FlipOnRespawn(HeroController __instance) {
		if (GlissandoPlugin.GravityIsFlipped)
			GlissandoPlugin.FlipGravity(__instance, force: true);
	}

}
