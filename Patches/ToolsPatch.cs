using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static VVVVVV.Utils.ILUtil;

namespace VVVVVV.Patches;

[HarmonyPatch(typeof(HeroController), nameof(HeroController.ThrowTool))]
internal static class ToolsPatch {

	// See HeroFsmsPatch for the FSM-controlled tools and the tools with FSMs

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
		return new CodeMatcher(instructions)

			// Vector2 linearVelocity = ((num2 != 0) ? usage.ThrowVelocityAlt : usage.ThrowVelocity).MultiplyElements(num);
			.Start()
			.MatchStartForward([
				new(x => Ldfld(x, nameof(ToolItem.UsageOptions.ThrowVelocity))),
			])
			.MatchStartForward([
				new(Stloc)
			])
			.Insert(Transpilers.EmitDelegate(InvertVectorIfFlipped))

			// GameObject gameObject = usage.ThrowPrefab.Spawn(transform.TransformPoint(vector4));
			.Start()
			.MatchEndForward([
				new(x => Call(x, nameof(ObjectPoolExtensions.Spawn))),
				new(Stloc)
			])
			.Insert(Transpilers.EmitDelegate(InvertToolScaleIfFlipped))

			.InstructionEnumeration();

		static Vector2 InvertVectorIfFlipped(Vector2 vec) {
			if (V6Plugin.GravityIsFlipped)
				return new(vec.x, -vec.y);
			return vec;
		}
		static GameObject InvertToolScaleIfFlipped(GameObject go) {
			if (!go.TryGetComponent<ClockworkHatchling>(out var _))
				go.transform.FlipLocalScale(y: V6Plugin.GravityIsFlipped);
			return go;
		}
	}

}
