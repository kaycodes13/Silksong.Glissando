using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static VVVVVV.Utils.ILUtil;
using CollisionSide = GlobalEnums.CollisionSide;

namespace VVVVVV.Patches;

[HarmonyPatch(typeof(HeroController))]
internal static class HeroPhysicsPatch {


	[HarmonyPatch(nameof(HeroController.FindCollisionDirection))]
	[HarmonyPostfix]
	private static void FlippedCollisionDirection(ref CollisionSide __result) {
		if (V6Plugin.GravityIsFlipped) {
			if (__result == CollisionSide.top)
				__result = CollisionSide.bottom;
			else if (__result == CollisionSide.bottom)
				__result = CollisionSide.top;
		}
	}


	[HarmonyPatch(nameof(HeroController.CheckTouchingGround), [typeof(bool)])]
	[HarmonyPostfix]
	private static void FlippedGroundCheck(HeroController __instance, ref bool __result) {
		if (!V6Plugin.GravityIsFlipped)
			return;

		Bounds bounds = __instance.col2d.bounds;
		Vector3
			min = bounds.min,
			max = bounds.max;

		Vector3
			center = new(bounds.center.x, max.y),
			left = new(min.x, max.y),
			right = new(max.x, max.y);

		__result = __instance.checkTouchGround.IsTouchingGround
			= IsRayHitting(left) || IsRayHitting(right) || IsRayHitting(center);

		if (__result)
			__instance.cState.onGround = true;

		static bool IsRayHitting(Vector2 origin){
			Debug.DrawRay(origin, Vector2.up * 0.32f, Color.magenta, 2f);
			return Helper.IsRayHittingNoTriggers(
				origin, Vector2.up, 1f, HeroController.GROUND_LAYERS
			);
		}
	}


	[HarmonyPatch(nameof(HeroController.CheckNearRoof))]
	[HarmonyPostfix]
	private static void FlippedRoofCheck(HeroController __instance, ref bool __result) {
		if (!V6Plugin.GravityIsFlipped)
			return;

		Bounds bounds = __instance.col2d.bounds;
		Vector3
			min = bounds.min,
			max = bounds.max,
			center = bounds.center,
			size = bounds.size;
		Vector2
			origin = min,
			origin2 = new(max.x, min.y),
			origin3 = new(center.x + size.x / 4f, min.y),
			origin4 = new(center.x - size.x / 4f, min.y),
			direction = new(-0.5f, -1f),
			direction2 = new(0.5f, -1f),
			down = Vector2.down;

		if (
			!Helper.IsRayHittingNoTriggers(origin2, direction, 2f, HeroController.GROUND_LAYERS)
			&& !Helper.IsRayHittingNoTriggers(origin, direction2, 2f, HeroController.GROUND_LAYERS)
			&& !Helper.IsRayHittingNoTriggers(origin3, down, 1f, HeroController.GROUND_LAYERS)
		) {
			__result = Helper.IsRayHittingNoTriggers(origin4, down, 1f, HeroController.GROUND_LAYERS);
			return;
		}
		__result = true;
	}


	[HarmonyPatch(nameof(HeroController.FixedUpdate))]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> FlippedPhysicsUpdate(IEnumerable<CodeInstruction> instructions) {
		return new CodeMatcher(instructions)
			// if (rb2d.linearVelocity.y < -maxFallVelocity && ...
			.Start()
			.MatchEndForward([
				new(x => Call(x, nameof(HeroController.GetMaxFallVelocity))),
				new(Stloc),
			])
			.MatchEndForward([
				new(x => Callvirt(x, $"get_{nameof(Rigidbody2D.linearVelocity)}")),
				new(x => Ldfld(x, "y")),
			])
			.ThrowIfInvalid("clamp comparison - yvel")
			.Advance(1)
			.Insert( // negating hero velocity for the comparison only
				InvertFloatIfFlipped()
			)
			.MatchEndForward([
				new(Ldloc),
				new(OpCodes.Neg),
			])
			.ThrowIfInvalid("clamp comparison - maxvel")
			.Insert( // negating max fall velocity for the comparison only
				InvertFloatIfFlipped()
			)

			// if (!didAirHang && !cState.onGround && rb2d.linearVelocity.y < 0f)
			.MatchEndForward([
				new(x => Ldfld(x, nameof(HeroController.didAirHang))),
				new(Brtrue)
			])
			.MatchEndForward([
				new(x => Callvirt(x, $"get_{nameof(Rigidbody2D.linearVelocity)}")),
				new(x => Ldfld(x, "y")),
				new(OpCodes.Ldc_R4, 0f),
			])
			.ThrowIfInvalid("air hang comparison")
			.Insert( // negating hero y velocity for the comparison only
				InvertFloatIfFlipped()
			)

			// if (rb2d.gravityScale < DEFAULT_GRAVITY && !controlReqlinquished ...
			.MatchStartForward([
				new(x => Callvirt(x, $"get_{nameof(Rigidbody2D.gravityScale)}")),
				new(OpCodes.Ldarg_0),
				new(x => Ldfld(x, nameof(HeroController.DEFAULT_GRAVITY))),
			])
			.ThrowIfInvalid("default grav comparison - air hang")
			.Advance(1)
			.Insert( // negating gravity scale for the comparison only
				InvertFloatIfFlipped()
			)
			.Advance(3)
			.Insert( // negating default gravity for the comparison only
				InvertFloatIfFlipped()
			)

			// if (rb2d.gravityScale < DEFAULT_GRAVITY && !inputHandler.inputActions.Jump.IsPressed ...
			.MatchStartForward([
				new(x => Callvirt(x, $"get_{nameof(Rigidbody2D.gravityScale)}")),
				new(OpCodes.Ldarg_0),
				new(x => Ldfld(x, nameof(HeroController.DEFAULT_GRAVITY))),
			])
			.ThrowIfInvalid("default grav comparison - lt")
			.Advance(1)
			.Insert( // negating gravity scale for the comparison only
				InvertFloatIfFlipped()
			)
			.Advance(3)
			.Insert( // negating default gravity for the comparison only
				InvertFloatIfFlipped()
			)

			// if (rb2d.gravityScale > DEFAULT_GRAVITY)
			.MatchStartForward([
				new(x => Callvirt(x, $"get_{nameof(Rigidbody2D.gravityScale)}")),
				new(OpCodes.Ldarg_0),
				new(x => Ldfld(x, nameof(HeroController.DEFAULT_GRAVITY))),
			])
			.ThrowIfInvalid("default grav comparison - gt")
			.Advance(1)
			.Insert( // negating gravity scale for the comparison only
				InvertFloatIfFlipped()
			)
			.Advance(3)
			.Insert( // negating default gravity for the comparison only
				InvertFloatIfFlipped()
			)

			.InstructionEnumeration();
	}


	[HarmonyPatch(nameof(HeroController.JumpReleased))]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> FlippedJumpReleased(IEnumerable<CodeInstruction> instructions) {
		return new CodeMatcher(instructions)
			.Start()
			.MatchEndForward([
				new(x => Callvirt(x, $"get_{nameof(Rigidbody2D.linearVelocity)}")),
				new(x => Ldfld(x, "y")),
			])
			.Advance(1)
			.Insert(
				InvertFloatIfFlipped()
			)
			.InstructionEnumeration();
	}


	[HarmonyPatch(nameof(HeroController.AffectedByGravity))]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> FlippedAffectedByGravity(IEnumerable<CodeInstruction> instructions) {
		return new CodeMatcher(instructions)
			.Start()
			.MatchEndForward([
				new(x => Callvirt(x, $"get_{nameof(Rigidbody2D.gravityScale)}")),
				new(OpCodes.Ldsfld),
			])
			.Insert(
				InvertFloatIfFlipped()
			)
			.MatchEndForward([
				new(x => Callvirt(x, $"get_{nameof(Rigidbody2D.gravityScale)}")),
				new(OpCodes.Ldsfld),
			])
			.Insert(
				InvertFloatIfFlipped()
			)
			.InstructionEnumeration();
	}

}
