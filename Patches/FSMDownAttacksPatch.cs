using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using System;
using System.Linq;
using UnityEngine;

namespace VVVVVV.Patches;

[HarmonyPatch]
internal static class FSMDownAttacksPatch {

	/*
	High priority prefixes because I'm only aiming to affect vanilla crests' attacks.
	
	Custom crests with fsm down attacks have to go without built-in support, because
	DelegateAction and the fact you can create your own custom actions and the fact that
	you could manually add support for this mod in your crest mod makes it impossible for
	me to account for every single possibility here.
	*/

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
	[HarmonyPrefix]
	[HarmonyPriority(Priority.First)]
	private static void HeroStart(HeroController __instance) {
		if (didEdit)
			return;
		didEdit = true;
		DoFsmEdit(__instance);
	}


	[HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
	[HarmonyPrefix]
	[HarmonyPriority(Priority.First)]
	private static void PFSMStart(PlayMakerFSM __instance) {
		var hc = HeroController.instance;
		if (didEdit || !hc || !ReferenceEquals(__instance, hc.crestAttacksFSM))
			return;
		didEdit = true;
		DoFsmEdit(hc);
	}


	[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SetupNewPlayerData))]
	[HarmonyPrefix]
	private static void ResetEditedState1() => didEdit = false;


	[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SetupExistingPlayerData))]
	[HarmonyPrefix]
	private static void ResetEditedState2() => didEdit = false;


	private static bool didEdit = false;

	private static readonly Type[]
		actionTypes = [
			typeof(SetVelocity2d),
			typeof(SetVelocityByScale),
			typeof(AddForce2d),
			typeof(Translate),
			typeof(ClampVelocity2D),
		];


	private static void DoFsmEdit(HeroController hc) {
		GameObject hero = hc.gameObject;
		PlayMakerFSM fsm = hc.crestAttacksFSM;
		if (!fsm.Fsm.preprocessed)
			fsm.Preprocess();

		FsmStateAction[] vanillaActions = [..
			fsm.FsmStates
			.SelectMany(x => x.Actions)
			.Where(x => actionTypes.Contains(x.GetType()))
		];

		FsmBool isFlipped = fsm.AddBoolVariable($"{nameof(VVVVVV)} Is Flipped");
		FsmState idleState = fsm.GetState("Idle")!;

		foreach (FsmTransition transition in idleState.transitions)
			transition.ToFsmState.InsertLambdaMethod(0, FlipVelocities);

		void FlipVelocities(Action finished) {
			if (isFlipped.Value == V6Plugin.GravityIsFlipped)
				return;

			isFlipped.Value = V6Plugin.GravityIsFlipped;

			foreach (FsmStateAction action in vanillaActions) {
				if (action is SetVelocity2d sv2d && !sv2d.y.UsesVariable && sv2d.gameObject.GetSafe(sv2d) == hero)
					sv2d.y.Value *= -1;
				else if (action is SetVelocityByScale svbs && !svbs.ySpeed.UsesVariable && svbs.gameObject.GetSafe(svbs) == hero)
					svbs.ySpeed.Value *= -1;
				else if (action is AddForce2d af2d && !af2d.y.UsesVariable && af2d.gameObject.GetSafe(af2d) == hero)
					af2d.y.Value *= -1;
				else if (action is Translate tl && !tl.y.UsesVariable && tl.gameObject.GetSafe(tl) == hero)
					tl.y.Value *= -1;
				else if (action is ClampVelocity2D cv2d && !cv2d.yMin.UsesVariable && !cv2d.yMax.UsesVariable && cv2d.gameObject.GetSafe(cv2d) == hero) {
					cv2d.yMax.Value *= -1;
					cv2d.yMin.Value *= -1;
					(cv2d.yMin, cv2d.yMax) = (cv2d.yMax, cv2d.yMin);
				}
			}
		}
	}

}
