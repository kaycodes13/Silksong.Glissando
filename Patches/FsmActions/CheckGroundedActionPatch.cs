using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static VVVVVV.Utils.ILUtil;

namespace VVVVVV.Patches.FsmActions;

[HarmonyPatch(typeof(CheckIsCharacterGrounded), nameof(CheckIsCharacterGrounded.DoAction))]
internal static class CheckGroundedActionPatch {

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
		return new CodeMatcher(instructions)
			.Start()
			.MatchEndForward([
				new(x => Callvirt(x, $"get_{nameof(Rigidbody2D.linearVelocity)}")),
				new(x => Ldfld(x, "y")),
			])
			.Advance(1)
			.Insert([
				new(OpCodes.Ldarg_0),
				Transpilers.EmitDelegate(InvertIfHeroAndFlipped),
			])
			.InstructionEnumeration();

		static float InvertIfHeroAndFlipped(float value, CheckIsCharacterGrounded self) {
			if (self.isHero && V6Plugin.GravityIsFlipped)
				return -value;
			return value;
		}
	}

}
