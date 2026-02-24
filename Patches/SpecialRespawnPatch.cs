using HarmonyLib;

namespace Glissando.Patches;

[HarmonyPatch(typeof(HeroController))]
internal static class SpecialRespawnPatch {

	[HarmonyPatch(nameof(HeroController.FinishedEnteringScene))]
	[HarmonyPrefix]
	private static void OpeningSequenceDetection(HeroController __instance, ref bool __state) {
		__state = __instance.isEnteringFirstLevel;
	}


	[HarmonyPatch(nameof(HeroController.FinishedEnteringScene))]
	[HarmonyPostfix]
	private static void OpeningSequenceSetRespawn(HeroController __instance, ref bool __state) {
		if (__state)
			__instance.SetHazardRespawn(
				__instance.transform.position,
				__instance.cState.facingRight
			);
	}


	[HarmonyPatch(nameof(HeroController.Update10))]
	[HarmonyPostfix]
	private static void OutOfBoundsAutomaticRespawn(HeroController __instance) {
		if (
			__instance.gm.IsGameplayScene() && GlissandoPlugin.GravityIsFlipped
			&& __instance.transform.position.y > __instance.gm.sceneHeight + 20
		) {
			GlissandoPlugin.QueueRespawnHero();
		}
	}

}
