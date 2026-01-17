using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using System;
using System.Linq;
using UnityEngine;
using VVVVVV.Utils;

namespace VVVVVV.Patches;

[HarmonyPatch]
internal static class HeroFsmsPatch {

	private static bool
		didDownAttackEdit = false;


	[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SetupNewPlayerData))]
	[HarmonyPrefix]
	private static void ResetOnNewGame() => ResetEditedState();


	[HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SetupExistingPlayerData))]
	[HarmonyPrefix]
	private static void ResetOnLoadGame() => ResetEditedState();


	/*
	High priority prefixes because I'm only aiming to affect vanilla game content.
	
	The sheer amount of things you can edit in an fsm, and the fact that someone can
	manually add support for this mod in their mod, makes it impossible for me to cover
	every single possibility here.
	*/

	[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
	[HarmonyPrefix]
	[HarmonyPriority(Priority.First)]
	private static void HeroStart(HeroController __instance) {
		if (!didDownAttackEdit) {
			didDownAttackEdit = true;
			EditDownAttacks(__instance);
		}
	}


	[HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
	[HarmonyPrefix]
	[HarmonyPriority(Priority.First)]
	private static void PFSMStart(PlayMakerFSM __instance) {
		var hc = HeroController.instance;
		if (!didDownAttackEdit && hc && ReferenceEquals(__instance, hc.crestAttacksFSM)) {
			didDownAttackEdit = true;
			EditDownAttacks(hc);
		}
	}

	private static void ResetEditedState() {
		didDownAttackEdit = false;
	}



	private static void EditDownAttacks(HeroController hc) {
		GameObject hero = hc.gameObject;
		PlayMakerFSM fsm = hc.crestAttacksFSM;
		if (!fsm.Fsm.preprocessed)
			fsm.Preprocess();

		FsmState idleState = fsm.GetState("Idle")!;

		fsm.DoGravityFlipEdit(hc, [..idleState.Transitions.Select(x => x.ToFsmState)]);
	}

}
