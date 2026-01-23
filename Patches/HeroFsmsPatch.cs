using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VVVVVV.Utils;

namespace VVVVVV.Patches;

[HarmonyPatch]
internal static class HeroFsmsPatch {

	private static bool
		didDownAttackEdit = false,
		didSprintEdit = false,
		didChargeAttackEdit = false,
		didScrambleEdit = false,
		didToolsEdit = false;


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
		EditDrifterCloak(__instance);

		if (!didDownAttackEdit)
			EditDownAttacks(__instance);
		if (!didSprintEdit)
			EditSprint(__instance);
		if (!didChargeAttackEdit)
			EditChargeAttacks(__instance);
		if (!didScrambleEdit)
			EditWallScramble(__instance);
		if (!didToolsEdit)
			EditTools(__instance);
	}

	[HarmonyPatch(typeof(PlayMakerFSM), nameof(PlayMakerFSM.Start))]
	[HarmonyPrefix]
	[HarmonyPriority(Priority.First)]
	private static void PFSMStart(PlayMakerFSM __instance) {
		if (__instance.gameObject.name.StartsWith("Tool Lightning Rod")) {
			EditVoltvesselSpear(__instance);
			return;
		}
		if (__instance.gameObject.name.StartsWith("Hero Conch Projectile")) {
			EditConchCutter(__instance);
			return;
		}

		HeroController? hc = __instance.GetComponent<HeroController>();
		if (!hc) return;

		if (!didDownAttackEdit && ReferenceEquals(__instance, hc.crestAttacksFSM))
			EditDownAttacks(hc);
		else if (!didSprintEdit && ReferenceEquals(__instance, hc.sprintFSM))
			EditSprint(hc);
		else if (!didChargeAttackEdit && __instance.FsmName == "Nail Arts")
			EditChargeAttacks(hc);
		else if (!didScrambleEdit && ReferenceEquals(__instance, hc.wallScrambleFSM))
			EditWallScramble(hc);
		else if (!didToolsEdit && ReferenceEquals(__instance, hc.toolsFSM))
			EditTools(hc);
	}

	private static void ResetEditedState() {
		didDownAttackEdit
			= didSprintEdit
			= didChargeAttackEdit
			= didScrambleEdit
			= didToolsEdit
			= false;
	}


	private static void EditDrifterCloak(HeroController hc) {
		PlayMakerFSM fsm = hc.umbrellaFSM;
		if (!fsm.Fsm.preprocessed)
			fsm.Preprocess();

		FsmState
			bumpL = fsm.GetState("Bump L")!;

		fsm.DoGravityFlipEdit(
			hc,
			checkStates: [fsm.GetState("Antic")!],
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

	private static void EditDownAttacks(HeroController hc) {
		didDownAttackEdit = true;

		GameObject hero = hc.gameObject;
		PlayMakerFSM fsm = hc.crestAttacksFSM;
		if (!fsm.Fsm.preprocessed)
			fsm.Preprocess();

		string[] varNames = [
			"Rpr DownSlash",
			"Witch Downslash",
			"Followup Slash",
			"SpinSlash",
			"SpinSlashRage",
			"Shaman Downslash",
			"Toolmaster Downslash",
			"Toolmaster Downslash Charged",
		];

		FsmGameObject[] downAttacks = [..
			varNames.Select(fsm.FindGameObjectVariable)
					.Where(x => x != null).Cast<FsmGameObject>()
		];

		FsmState idleState = fsm.GetState("Idle")!;

		fsm.DoGravityFlipEdit(
			hc,
			checkStates: [.. idleState.Transitions.Select(x => x.ToFsmState)],
			otherEdits: FlipKnockbackDirection
		);

		void FlipKnockbackDirection() {
			foreach(GameObject attack in downAttacks.Select(x => x.Value)) {
				DamageEnemies damager = attack.GetComponent<DamageEnemies>();
				if (!damager)
					continue;
				int direction = DirectionUtils.GetCardinalDirection(damager.direction);
				if (
					(V6Plugin.GravityIsFlipped && direction == DirectionUtils.Down)
					|| (!V6Plugin.GravityIsFlipped && direction == DirectionUtils.Up)
				) {
					damager.FlipDirection();
				}
			}

		}
	}

	private static void EditSprint(HeroController hc) {
		didSprintEdit = true;

		GameObject hero = hc.gameObject;
		PlayMakerFSM fsm = hc.sprintFSM;
		if (!fsm.Fsm.preprocessed)
			fsm.Preprocess();
		string pirouetteAnimName = "Shuttlecock";
		FsmBool
			isFlipped = fsm.GetBoolVariable(FsmFlipUtil.FLIP_BOOL_NAME),
			shouldPirouette = fsm.GetBoolVariable($"{V6Plugin.Id} Should Pirouette");
		FsmString
			sprintAirAnim = fsm.FindStringVariable("Sprint Air Anim")!;

		FsmState
			airSprintL = fsm.GetState("Air Sprint L")!,
			airSprintR = fsm.GetState("Air Sprint R")!,
			airSprintLoop = fsm.GetState("Air Sprint Loop")!,
			jumpAntic = fsm.GetState("Jump Antic")!,
			dashedState = fsm.GetState("Dashed")!;

		dashedState.InsertAction(0, new SetBoolValue {
			boolVariable = shouldPirouette,
			boolValue = false
		});
		jumpAntic.InsertAction(0, new SetBoolValue {
			boolVariable = shouldPirouette,
			boolValue = true
		});

		foreach (var state in new FsmState[] { airSprintL, airSprintR }) {
			int index = Array.FindIndex(
				state.Actions,
				x => x is Tk2dPlayAnimationWithEvents t2d
					&& t2d.clipName.UsesVariable
					&& t2d.clipName.Name == sprintAirAnim.Name
			);
			state.InsertAction(index, new ConvertBoolToString {
				boolVariable = shouldPirouette,
				stringVariable = sprintAirAnim,
				falseString = sprintAirAnim,
				trueString = pirouetteAnimName
			});
		}

		int loopIndex = Array.FindIndex(
			airSprintLoop.Actions,
			x => x is Tk2dPlayAnimation t2d
				&& t2d.clipName.Value == "Sprint Air Loop"
		);
		airSprintLoop.InsertAction(1 + loopIndex, new tk2dPlayAnimationConditional {
			Target = new FsmOwnerDefault(),
			AnimName = pirouetteAnimName,
			Condition = shouldPirouette
		});

		fsm.DoGravityFlipEdit(
			hc,
			checkStates: [
				fsm.GetState("Start Sprint")!,
				airSprintL,
				airSprintR,
				fsm.GetState("Start Attack")!,
			],
			affectedStates: [
				fsm.GetState("Start Sprint")!,
				fsm.GetState("Bump Up")!,
				fsm.GetState("Bump Up 2")!,
				fsm.GetState("Witch Lash Start")!,
				fsm.GetState("Reaper End")!,
				fsm.GetState("Wanderer Recoil")!,
				fsm.GetState("RecoilStab Dash")!,
				fsm.GetState("Warrior Leap")!,
				fsm.GetState("Warrior Slash")!,
				fsm.GetState("Shaman Leap")!,
				fsm.GetState("Shaman Slash")!,
			],
			otherEdits: ReaperCurveFloat
		);

		void ReaperCurveFloat() {
			CurveFloat curveFloat = fsm.GetState("Reaper Upper")!.GetFirstActionOfType<CurveFloat>()!;
			AnimationCurve anim = curveFloat.animCurve.curve;
			List<Keyframe> newKeys = [];
			foreach (Keyframe key in anim.keys)
				newKeys.Add(new(key.time, -key.value, key.inTangent, key.outTangent, key.inWeight, key.outWeight));
			anim.SetKeys([.. newKeys]);
		}
	}

	private static void EditChargeAttacks(HeroController hc) {
		didChargeAttackEdit = true;

		PlayMakerFSM fsm = hc.gameObject.GetFsmPreprocessed("Nail Arts")!;

		fsm.DoGravityFlipEdit(
			hc,
			checkStates:
			[fsm.GetState("Take Control")!],
			otherEdits: ConditionalVelocities
		);

		void ConditionalVelocities() {
			// architect, beast
			ConvertBoolToFloat[] vels = [
				fsm.GetState("Antic Drill")!.GetFirstActionOfType<ConvertBoolToFloat>()!,
				fsm.GetState("Warrior2 Leap")!.GetFirstActionOfType<ConvertBoolToFloat>()!,
			];
			foreach(var v in vels) {
				v.trueValue.Value *= -1;
				v.falseValue.Value *= -1;
			}

			// witch and other spinning attacks
			FloatClamp witch = fsm.GetState("Queued Spin")!.GetFirstActionOfType<FloatClamp>()!;
			witch.minValue.Value *= -1;
			witch.maxValue.Value *= -1;
			(witch.minValue, witch.maxValue) = (witch.maxValue, witch.minValue);
		}
	}

	private static void EditWallScramble(HeroController hc) {
		didScrambleEdit = true;

		PlayMakerFSM fsm = hc.wallScrambleFSM;
		if (!fsm.Fsm.preprocessed)
			fsm.Preprocess();

		FsmState
			startScramble = fsm.GetState("Start Scramble")!,
			scrambleUp = fsm.GetState("Scramble Up")!;

		fsm.DoGravityFlipEdit(
			hc,
			checkStates: [startScramble],
			otherEdits: ConditionalVelocities
		);

		void ConditionalVelocities() {
			ConvertBoolToFloat startVel =
				startScramble.GetActionsOfType<ConvertBoolToFloat>()
				.First(x => x.floatVariable.Name == "Scramble Speed");
			startVel.trueValue.Value *= -1;
			startVel.falseValue.Value *= -1;

			ConvertBoolToFloat downForce =
				scrambleUp.GetActionsOfType<ConvertBoolToFloat>()
				.First(x => x.floatVariable.Name == "Down Force");
			downForce.trueValue.Value *= -1;
			downForce.falseValue.Value *= -1;

			FloatCompare speedChecker =
				scrambleUp.GetActionsOfType<FloatCompare>()
				.First(x => x.float1.UsesVariable && x.float1.Name == "Y Speed");
			speedChecker.float2.Value *= -1;
			(speedChecker.lessThan, speedChecker.greaterThan) = (speedChecker.greaterThan, speedChecker.lessThan);
		}
	}

	private static void EditTools(HeroController hc) {
		didToolsEdit = true;

		GameObject hero = hc.gameObject;
		PlayMakerFSM fsm = hc.toolsFSM;
		if (!fsm.Fsm.preprocessed)
			fsm.Preprocess();

		FsmBool isFlipped = fsm.GetBoolVariable(FsmFlipUtil.FLIP_BOOL_NAME);
		string[] checkStateNames = [
			"Take Control",
			"TriPin Type",
		];

		fsm.DoGravityFlipEdit(
			hc,
			checkStates: [.. checkStateNames.Select(fsm.GetState)!],
			affectedStates: [.. fsm.FsmStates.Where(x =>
				x.Name is var n
				&& !n.Contains("Scuttle") && !n.Contains("Rosary Cannon")
				&& !n.StartsWith("RC") && !n.Contains("Shoot"))
			],
			otherEdits: EditSilkshotA
		);

		EditTripin();
		EditPlasmiumPhial();
		EditSnareSetter();
		EditDelverDrill();

		void EditSilkshotA() {
			FsmState state = fsm.GetState("WebShot A Dir")!;
			foreach (FsmStateAction action in state.Actions) {
				if (action is RandomFloat rf && rf.storeResult?.Name == "Angle") {
					rf.min.Value *= -1;
					rf.max.Value *= -1;
					(rf.min, rf.max) = (rf.max, rf.min);
				}
				if (action is SetFloatValue sfv && sfv.floatVariable?.Name == "Angle Change") {
					sfv.floatValue.Value *= -1;
				}
			}
		}

		void EditTripin() {
			FsmState[] states = [
				fsm.GetState("TriPin Ground L")!,
				fsm.GetState("TriPin Ground R")!,
				fsm.GetState("TriPin Air L")!,
				fsm.GetState("TriPin Air R")!,
			];
			FsmFloat rotationAngle = fsm.GetFloatVariable($"{V6Plugin.Id} Projectile Rotation Angle");
			foreach (FsmState state in states) {
				foreach (var nextSpawnObj in state.GetActionsOfType<SpawnObjectFromGlobalPool>().Skip(1)) {
					int index = Array.IndexOf(state.Actions, nextSpawnObj);
					state.InsertMethod(index, FlipProjectile);
				}
				state.AddMethod(FlipProjectile);
			}

			void FlipProjectile() {
				FsmGameObject projectile = fsm.FindGameObjectVariable("Projectile")!;
				GameObject go = projectile.Value;
				Transform t = go.transform;
				Rigidbody2D rb2d = go.GetComponent<Rigidbody2D>();

				t.localScale = t.localScale with { y = V6Plugin.GravityIsFlipped ? -1 : 1 };

				if (!V6Plugin.GravityIsFlipped)
					return;

				Vector2 spawnPt = fsm.FindGameObjectVariable("Self")!.Value.transform.position;
				float yOffset = Mathf.Abs(spawnPt.y - t.position.y);

				t.position = t.position with { y = t.position.y + (2 * yOffset) };
				t.eulerAngles = -t.eulerAngles;
				rb2d.linearVelocityY = -rb2d.linearVelocityY;
			}
		}

		void EditPlasmiumPhial() {
			FsmState syringeAntic = fsm.GetState("Syringe Antic")!;
			var boolToFloat = syringeAntic.GetFirstActionOfType<ConvertBoolToFloat>()!;
			int index = 1 + Array.IndexOf(syringeAntic.Actions, boolToFloat);
			syringeAntic.InsertMethod(index, () => {
				if (isFlipped.Value)
					boolToFloat.floatVariable.Value *= -1;
			});
		}

		void EditSnareSetter() {
			FsmState snareLand = fsm.GetState("Snare Land")!;
			var loopEffect = fsm.FindGameObjectVariable("Loop Effect")!;
			int loopIndex = Array.FindLastIndex(
				snareLand.Actions,
				x => x is SpawnObjectFromGlobalPool ac && ac.storeObject == loopEffect
			);
			snareLand.InsertMethod(1 + loopIndex, () => {
				Transform t = loopEffect.Value.transform;
				t.localScale = t.localScale with { y = isFlipped.Value ? -1 : 1 };
			});


			FsmState snareSet = fsm.GetState("Snare Set")!;
			var spawned = fsm.FindGameObjectVariable("Spawned")!;
			int setIndex = Array.FindLastIndex(
				snareSet.Actions,
				x => x is SetGameObject ac && ac.variable == spawned
			);
			snareSet.InsertMethod(setIndex, () => {
				Transform t = spawned.Value.transform;
				t.localScale = t.localScale with { y = isFlipped.Value ? -1 : 1 };
			});
		}

		void EditDelverDrill() {
			FsmState screwAntic = fsm.GetState("Screw Antic")!;
			var boolToFloat = screwAntic.GetFirstActionOfType<ConvertBoolToFloat>()!;
			int index = 1 + Array.IndexOf(screwAntic.Actions, boolToFloat);
			screwAntic.InsertMethod(index, () => {
				if (isFlipped.Value)
					boolToFloat.floatVariable.Value *= -1;
			});
		}

	}

	private static void EditVoltvesselSpear(PlayMakerFSM fsm) {
		// check if we edited this one yet or not, because these are pooled
		if (fsm.FindBoolVariable(V6Plugin.Id) != null)
			return;
		fsm.AddBoolVariable(V6Plugin.Id);

		// make floor detection work on ceilings too
		FsmState angleDetectState = fsm.GetState("Land Angle")!;
		FsmFloat normalYpos = fsm.GetFloatVariable("Normal Y Pos");
		int index = Array.FindIndex(
			angleDetectState.Actions,
			x => x is GetVector2XY ac && ac.vector2Variable.Name == "Hit Normal"
		);
		angleDetectState.InsertActions(
			1 + index,
			new SetFloatValue {
				floatVariable = normalYpos,
				floatValue = fsm.FindFloatVariable("Normal Y")!
			},
			new FloatAbs { floatVariable = normalYpos }
		);
		angleDetectState.GetActionsOfType<FloatCompare>()
			.First(x => x.lessThan.Name == "FLOOR")
			.float2 = normalYpos;

		// flip the angles for floor hits
		FsmFloat
			landAngle = fsm.FindFloatVariable("Land Rotation")!,
			rightAngle = fsm.FindFloatVariable("Hit Rotation R")!,
			leftAngle = fsm.FindFloatVariable("Hit Rotation L")!,
			upAngle = fsm.FindFloatVariable("Hit Rotation U")!,
			downAngle = fsm.FindFloatVariable("Hit Rotation D")!;

		fsm.GetState("Floor")!.AddMethod(FlipSpearAngles);

		void FlipSpearAngles() {
			if (V6Plugin.GravityIsFlipped) {
				upAngle.Value
					= downAngle.Value
					= landAngle.Value
					= 90;

				leftAngle.Value = 45;
				rightAngle.Value = 135;
			}
		}
	}

	private static void EditConchCutter(PlayMakerFSM fsm) {
		// check if we edited this one yet or not, because these are pooled
		if (fsm.FindBoolVariable(V6Plugin.Id) != null)
			return;
		fsm.AddBoolVariable(V6Plugin.Id);

		// allow it to start in an upward motion if gravity is flipped
		FsmFloat scaleX = fsm.FindFloatVariable("Scale X")!;
		FsmState dirState = fsm.GetState("Dir")!;
		int index = Array.FindIndex(dirState.Actions, x => x is FloatSignTest);

		dirState.InsertMethod(index, PickDirection);
		dirState.AddTransition("UL", "UL");
		dirState.AddTransition("UR", "UR");

		void PickDirection() {
			string
				vertical = V6Plugin.GravityIsFlipped ? "U" : "D",
				horizontal = scaleX.Value < 0 ? "L" : "R",
				eventName = $"{vertical}{horizontal}";

			fsm.Fsm.Event(eventName);
		}
	}

}
