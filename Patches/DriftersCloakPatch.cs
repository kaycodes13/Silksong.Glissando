using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;

namespace VVVVVV.Patches;

[HarmonyPatch(typeof(HeroController))]
internal static class DriftersCloakPatch {

	[HarmonyPatch(nameof(HeroController.CanDoubleJump))]
	[HarmonyPostfix]
	private static void AllowFloatOnDownAndJump(HeroController __instance, ref bool __result) {
		if (!__result)
			return;

		UnityEngine.Debug.Log("float control adjustment check");
		__result = __instance.inputHandler.inputActions.Down.IsPressed == false;
	}

	[HarmonyPatch(nameof(HeroController.Start))]
	[HarmonyPostfix]
	private static void AllowFlippedDrifting(HeroController __instance) {
		UnityEngine.Debug.Log("float fsm edit");
		PlayMakerFSM fsm = __instance.umbrellaFSM;
		if (!fsm.Fsm.preprocessed)
			fsm.Preprocess();

		FsmBool isFlipped = fsm.AddBoolVariable($"{V6Plugin.Id} Is Flipped");
		isFlipped.Value = false;

		FsmState floatIdle = fsm.GetState("Float Idle")!;

		// reverse the speed if gravity is flipped

		AccelerateToY accelAction = floatIdle.GetFirstActionOfType<AccelerateToY>()!;
		float origFloatSpeed = accelAction.targetSpeed.Value;

		floatIdle.InsertLambdaMethod(0, finished => {
			if (isFlipped.Value != V6Plugin.GravityIsFlipped) {
				isFlipped.Value = V6Plugin.GravityIsFlipped;
				accelAction.targetSpeed.Value *= -1;
			}
			finished();
		});
	}

}
