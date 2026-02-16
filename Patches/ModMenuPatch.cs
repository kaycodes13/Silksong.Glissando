using HarmonyLib;
using Silksong.ModMenu.Elements;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace VVVVVV.Patches;

/*
If you are reading this, DO NOT copy this because it's wicked fragile and incredibly cursed.

Without this patch, MenuMod v0.4.1 is throwing an NRE because the component being
added immediately OnEnable's and attempts to access a member of its 'textField' member
before that actually is set to anything.

I need to investigate and figure out if that's somehow my fault or not.
And then file a bug report if it's not me. And then remove this patch.
*/
[HarmonyPatch(typeof(LocalizedTextExtensions), nameof(LocalizedTextExtensions.set_LocalizedText))]
internal static class ModMenuPatch {

	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		=> new CodeMatcher(instructions)

			// component = self.gameObject.AddComponent<AutoLocalizeTextUI>();
			.MatchStartForward([
				new(x => x.opcode == OpCodes.Callvirt && x.operand is MethodInfo m && m.Name == "AddComponent"),
			])
			.Insert([
				Transpilers.EmitDelegate(DeactivateAndReturnGO)
			])

			// component.textField = self;
			.MatchStartForward([
				new(x => x.opcode == OpCodes.Stfld && x.operand is FieldInfo f && f.Name == "textField"),
			])
			.Advance(1)
			.Insert([
				new(OpCodes.Ldarg_0),
				new(OpCodes.Callvirt, typeof(Component).GetMethod("get_gameObject")),
				Transpilers.EmitDelegate(ActivateAndConsumeGO),
			])

			.InstructionEnumeration();

	static GameObject DeactivateAndReturnGO(GameObject go) {
		go.SetActive(false);
		return go;
	}

	static void ActivateAndConsumeGO(GameObject go)
		=> go.SetActive(true);

}
