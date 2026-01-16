using HarmonyLib;

namespace VVVVVV.Patches;

[HarmonyPatch(typeof(HeroController))]
internal static class FlippedDownspikePatch {

	// The low priority is because I know Needleforge also patches the downspike methods
	// and I want at least SOME custom crests to play nice with this mod.

	[HarmonyPatch(nameof(HeroController.DownAttack))]
	[HarmonyPostfix]
	[HarmonyPriority(Priority.Last)]
	private static void FlipDownspikeStart(HeroController __instance) {
		if (V6Plugin.GravityIsFlipped && __instance.Config.DownSlashType == HeroControllerConfig.DownSlashTypes.DownSpike && __instance.Config.DownspikeThrusts) {
			UnityEngine.Debug.Log("downspike START");
			V6Plugin.FlipHeroVelocity();
		}
	}

	[HarmonyPatch(nameof(HeroController.Downspike))]
	[HarmonyPostfix]
	[HarmonyPriority(Priority.Last)]
	private static void FlipDownspikeMiddle(HeroController __instance) {
		if (V6Plugin.GravityIsFlipped && __instance.Config.DownspikeThrusts) {
			UnityEngine.Debug.Log("downspike middle");
			V6Plugin.FlipHeroVelocity();
		}
	}

	[HarmonyPatch(nameof(HeroController.FinishDownspike), [typeof(bool)])]
	[HarmonyPostfix]
	[HarmonyPriority(Priority.Last)]
	private static void FlipDownspikeEnd(HeroController __instance) {
		if (V6Plugin.GravityIsFlipped && !__instance.cState.floating && !__instance.startWithBalloonBounce) {
			UnityEngine.Debug.Log("downspike END");
			V6Plugin.FlipHeroVelocity();
		}
	}

}
