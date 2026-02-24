using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Glissando.Patches;

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
		if (!GlissandoPlugin.GravityIsFlipped || !__instance.ownerObject.TryGetComponent<HeroController>(out var _))
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
		float expectedY = GlissandoPlugin.GravityIsFlipped ? -1 : 1;
		__instance.transform.localScale = __instance.transform.localScale with { y = expectedY };
	}

	[HarmonyPatch(typeof(SoftLandEffect), nameof(SoftLandEffect.OnEnable))]
	[HarmonyPrefix]
	private static void FlipSoftLandEffect(SoftLandEffect __instance) {
		float expectedY = GlissandoPlugin.GravityIsFlipped ? -1 : 1;
		__instance.transform.localScale = __instance.transform.localScale with { y = expectedY };
	}


	[HarmonyPatch(typeof(HeroAnimationController), nameof(HeroAnimationController.GetClip))]
	[HarmonyPrefix]
	private static void FlipLookAnimations(ref string clipName) {
		if (!GlissandoPlugin.GravityIsFlipped)
			return;

		if (downToUp.TryGetValue(clipName, out string downToUpClip))
			clipName = downToUpClip;
		else if (upToDown.TryGetValue(clipName, out string upToDownClip))
			clipName = upToDownClip;
	}

	private static readonly Dictionary<string, string>
		downToUp = new() {
			{ "LookDown", "LookUp" },
			{ "LookDownEnd", "LookUpEnd" },
			{ "LookDownToIdle", "LookUpToIdle" },
			{ "Ring Look Down", "Ring Look Up" },
			{ "Ring Look Down End", "Ring Look Up End" },
			{ "LookDown Updraft", "LookUp Updraft" },
			{ "LookDownEnd Updraft", "LookUpEnd Updraft" },
			{ "LookDown Windy", "LookUp Windy" },
			{ "LookDownEnd Windy", "LookUpEnd Windy" },
			{ "Hurt Look Down", "Hurt Look Up" },
			{ "Hurt Look Down Windy", "Hurt Look Up Windy" },
			{ "Hurt Look Down Windy End", "Hurt Look Up Windy End" },
		},
		upToDown = downToUp.ToDictionary(i => i.Value, i => i.Key);

}
