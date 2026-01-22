using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static VVVVVV.Utils.ILUtil;

namespace VVVVVV.Patches.FsmActions;

[HarmonyPatch]
internal static class FallClampActionPatch {

	[HarmonyPatch(typeof(HeroClampFallVelocity), nameof(HeroClampFallVelocity.OnUpdate))]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> InvertClampV1(IEnumerable<CodeInstruction> instructions)
		=> InvertComparison(instructions);

	[HarmonyPatch(typeof(HeroClampFallVelocityV2), nameof(HeroClampFallVelocityV2.OnUpdate))]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> InvertClampV2(IEnumerable<CodeInstruction> instructions)
		=> InvertComparison(instructions);

	private static IEnumerable<CodeInstruction> InvertComparison(IEnumerable<CodeInstruction> instructions) {
		int heroIndex = 0, maxIndex = 0;
		return new CodeMatcher(instructions)
			// find indexes for max fall velocity and hero velocity
			.Start()
			.MatchStartForward([
				new(x => Ldfld(x, nameof(HeroClampFallVelocity.body))),
				new(x => Callvirt(x, $"get_{nameof(Rigidbody2D.linearVelocity)}")),
				new(x => Stloc(x, out heroIndex)),
			])
			.Start()
			.MatchStartForward([
				new(x => Callvirt(x, nameof(HeroController.GetMaxFallVelocity))),
			])
			.MatchStartForward([
				new(x => Stloc(x, out maxIndex)),
			])

			// negate hero velocity for the comparison
			.Start()
			.MatchEndForward([
				new(x => Ldloc(x, heroIndex)),
				new(x => Ldfld(x, "y")),
			])
			.Advance(1)
			.Insert(InvertFloatIfFlipped())

			// negate max fall velocity for the comparison
			.MatchStartForward([
				new(x => Ldloc(x, maxIndex))
			])
			.Advance(1)
			.Insert(InvertFloatIfFlipped())

			.InstructionEnumeration();
	}

}
