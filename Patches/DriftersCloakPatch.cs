using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using System;
using VVVVVV.Utils;

namespace VVVVVV.Patches;

[HarmonyPatch(typeof(HeroController))]
internal static class DriftersCloakPatch {

	[HarmonyPatch(nameof(HeroController.CanDoubleJump))]
	[HarmonyPostfix]
	private static void AllowFloatOnDownAndJump(HeroController __instance, ref bool __result) {
		if (!__result)
			return;

		__result = __instance.inputHandler.inputActions.Down.IsPressed == false;
	}

	[HarmonyPatch(nameof(HeroController.Start))]
	[HarmonyPostfix]
	private static void AllowFlippedDrifting(HeroController __instance) {
		PlayMakerFSM fsm = __instance.umbrellaFSM;
		if (!fsm.Fsm.preprocessed)
			fsm.Preprocess();

		FsmState
			bumpL = fsm.GetState("Bump L")!;

		fsm.DoGravityFlipEdit(__instance,
			checkStates: [
				fsm.GetState("Antic")!
			],
			affectedStates: [
				fsm.GetState("Inflate")!,
				fsm.GetState("Start")!,
				fsm.GetState("Float Idle")!,
				bumpL,
				fsm.GetState("Bump R")!,
			],
			otherEdits: FlipBumpL
		);

		void FlipBumpL() {
			FloatClamp clamp = (FloatClamp)Array.Find(
				bumpL.Actions,
				x => x is FloatClamp fc
					&& fc.floatVariable.Name.Contains("Velocity")
			);

			clamp.minValue.Value *= -1;
			clamp.maxValue.Value *= -1;
			(clamp.minValue, clamp.maxValue) = (clamp.maxValue, clamp.minValue);
		}
	}

}
