using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Silksong.FsmUtil;
using System;
using System.Linq;
using UnityEngine;

namespace VVVVVV.Utils;
internal static class FsmFlipUtil {

	/// <summary>
	/// Performs an FSM edit which causes all <paramref name="checkStates"/> to check the gravity status, and if gravity is flipped, flip all hero-targeting y motion in all the actions in the <paramref name="affectedStates"/>.
	/// </summary>
	/// <remarks>
	/// The edit will ONLY apply to actions which existed at the time this function was called.
	/// </remarks>
	/// <param name="fsm"></param>
	/// <param name="hc"></param>
	/// <param name="checkStates"></param>
	/// <param name="affectedStates"></param>
	internal static void DoGravityFlipEdit(
		this PlayMakerFSM fsm, HeroController hc,
		FsmState[] checkStates, FsmState[]? affectedStates = null
	) {
		affectedStates ??= fsm.FsmStates;
		FsmStateAction[] affectedActions = [.. affectedStates.SelectMany(x => x.Actions)];

		FsmBool isFlipped = fsm.GetBoolVariable($"{V6Plugin.Id} Is Flipped");

		foreach(var state in checkStates)
			state.InsertLambdaMethod(0, FlipState);

		void FlipState(Action finished) {
			if (isFlipped.Value == V6Plugin.GravityIsFlipped)
				return;

			isFlipped.Value = V6Plugin.GravityIsFlipped;
			affectedActions.FlipHeroMotion(hc);
			finished();
		}
	}

	/// <summary>
	/// Simultaneously flips all hero-targeting y movements performed by the actions
	/// and prunes actions which don't affect hero's y movement out of the input list.
	/// </summary>
	internal static void FlipHeroMotion(this FsmStateAction[] actions, HeroController hc) {
		actions = [.. actions.Where(x => x != null).Where(x => x.FlipHeroMotion(hc))];
	}

	/// <summary>
	/// Flips all hero-targeting y movements performed by the action.
	/// </summary>
	/// <returns>True if the action is of a type which can affect hero y motion.</returns>
	private static bool FlipHeroMotion<T>(this T action, HeroController hc) where T : FsmStateAction {
		GameObject hero = hc.gameObject;
		switch (action) {
			case SetVelocity2d ac:
				if (ac.gameObject.GetSafe(ac) == hero)
					ac.y.Value *= -1;
				return true;
			case SetVelocityByScale ac:
				if (ac.gameObject.GetSafe(ac) == hero)
					ac.ySpeed.Value *= -1;
				return true;
			case AddForce2d ac:
				if (ac.gameObject.GetSafe(ac) == hero)
					ac.y.Value *= -1;
				return true;
			case Translate ac:
				if (ac.gameObject.GetSafe(ac) == hero)
					ac.y.Value *= -1;
				return true;
			case ClampVelocity2D ac:
				if (ac.gameObject.GetSafe(ac) == hero) {
					ac.yMax.Value *= -1;
					ac.yMin.Value *= -1;
					(ac.yMin, ac.yMax) = (ac.yMax, ac.yMin);
				}
				return true;
			case SetGravity2dScale ac:
				if (ac.gameObject.GetSafe(ac) == hero)
					ac.rigidbody2d.gravityScale *= -1;
				return true;
			case SetGravity2dScaleV2 ac:
				if (ac.gameObject.GetSafe(ac) == hero)
					ac.rigidbody2d.gravityScale *= -1;
				return true;
		}
		return false;
	}

}
