using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;
using System.Reflection.Emit;
using static VVVVVV.Utils.ILUtil;

namespace VVVVVV.Patches;

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
		return new CodeMatcher(instructions)
			.Start()
			.MatchEndForward([
				new(x => Callvirt(x, nameof(HeroController.GetMaxFallVelocity))),
				new(OpCodes.Neg),
			])
			.Insert( // negating max fall velocity for the comparison only
				InvertFloatIfFlipped()
			)
			.InstructionEnumeration();
	}

}
