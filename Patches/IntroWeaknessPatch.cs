using HarmonyLib;
using Silksong.FsmUtil;
using UnityEngine.SceneManagement;

namespace Glissando.Patches;

[HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
internal class IntroWeaknessPatch {
	private static void Prefix(PlayMakerFSM __instance) {
		if (
			!__instance.gameObject.name.StartsWith("Weakness Scene")
			|| SceneManager.GetActiveScene().name != "Bonetown"
		) {
			return;
		}

		__instance.GetState("Wait For Entry")?.InsertMethod(0, UnflipGravity);
		__instance.GetState("Wait For Entry 2")?.InsertMethod(0, UnflipGravity);

		static void UnflipGravity() {
			if (GlissandoPlugin.GravityIsFlipped)
				GlissandoPlugin.FlipGravity(HeroController.instance, force: true);
		}
	}
}
