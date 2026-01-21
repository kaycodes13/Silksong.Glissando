using HarmonyLib;
using UnityEngine;

namespace VVVVVV.Patches;

[HarmonyPatch]
internal static class VisualEffectsPatch {

	[HarmonyPatch(typeof(RunEffects), nameof(RunEffects.Update))]
	[HarmonyPrefix]
	private static void FlipRunEffects(RunEffects __instance) {
		if (!__instance.isHeroEffect || !__instance.hasHC)
			return;

		var hc = __instance.hc;
		var transform = __instance.transform;

		float expectedY =
			(transform.parent == hc.transform) ? 1 : hc.transform.localScale.y;

		if (transform.localScale.y != expectedY)
			transform.localScale = transform.localScale with { y = expectedY };
	}

	[HarmonyPatch(typeof(JumpEffects), nameof(JumpEffects.Play))]
	[HarmonyPrefix]
	private static void FlipJumpEffects(JumpEffects __instance, GameObject owner) {
		if (!owner.TryGetComponent<HeroController>(out var hc))
			return;
		var transform = __instance.transform;
		transform.localScale = transform.localScale with { y = hc.transform.localScale.y };
		// This works because this function gets called by HeroJump,
		// which runs in its entirety BEFORE we flip gravity
	}

	[HarmonyPatch(typeof(JumpEffects), nameof(JumpEffects.CheckForFall))]
	[HarmonyPostfix]
	private static void FlipJumpEffectsFallCheck(JumpEffects __instance) {
		if (!V6Plugin.GravityIsFlipped || !__instance.ownerObject.TryGetComponent<HeroController>(out var _))
			return;

		if ((__instance.ownerPos.y - __instance.previousOwnerPos.y) / Time.deltaTime > 0f) {
			__instance.jumpPuff.SetActive(value: false);
			__instance.dustTrail.GetComponent<ParticleSystem>().Stop();
			__instance.checkForFall = false;
		}
	}

	[HarmonyPatch(typeof(HardLandEffect), nameof(HardLandEffect.OnEnable))]
	[HarmonyPrefix]
	private static void FlipHardLandEffect(HardLandEffect __instance) {
		float expectedY = V6Plugin.GravityIsFlipped ? -1 : 1;
		__instance.transform.localScale = __instance.transform.localScale with { y = expectedY };
	}

	[HarmonyPatch(typeof(SoftLandEffect), nameof(SoftLandEffect.OnEnable))]
	[HarmonyPrefix]
	private static void FlipSoftLandEffect(SoftLandEffect __instance) {
		float expectedY = V6Plugin.GravityIsFlipped ? -1 : 1;
		__instance.transform.localScale = __instance.transform.localScale with { y = expectedY };
	}

}
