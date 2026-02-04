using HarmonyLib;
using System.Linq;
using TeamCherry.Localization;
using VVVVVV.Settings;
using VVVVVV.Utils;

namespace VVVVVV.Patches;

[HarmonyPatch]
internal static class FaydownUIPatch {

	private static string[]? langKeys;

	[HarmonyPatch(typeof(Language), nameof(Language.Get), [typeof(string), typeof(string)])]
	[HarmonyPrefix]
	private static void ReplaceFaydownStrings(ref string key, ref string sheetTitle) {
		langKeys ??= [.. Language.GetKeys(LangUtil.SHEET)];

		if (key == "PROMPT_DJ") {
			// this key should be left alone when faydown is normal
			if (V6Plugin.Settings.FaydownState.FlipsGravity())
				sheetTitle = LangUtil.SHEET;
		}
		else if (key == "INV_DESC_DRESS_DJ") {
			sheetTitle = LangUtil.SHEET;
			key = V6Plugin.Settings.FaydownState.InventoryLangKey();
		}
		else if (langKeys.Contains(key))
			sheetTitle = LangUtil.SHEET;
	}

	[HarmonyPatch(typeof(CollectableItemStates), nameof(CollectableItemStates.GetDescription))]
	[HarmonyPrefix]
	private static void AddExtraDescription(CollectableItemStates __instance) {
		if (__instance.name != "Dresses")
			return;

		int index = __instance.GetCurrentStateIndex();

		if (
			__instance.states[index].DescriptionExtra == default(LocalisedString)
			&& __instance.states[index].Test.TestGroups.SelectMany(y => y.Tests)
				.Any(z => z.FieldName == nameof(PlayerData.hasDoubleJump))
		) {
			__instance.states[index] = __instance.states[index] with {
				DescriptionExtra = LangUtil.String("INV_DESC_DRESS_DJ_EXTRA")
			};
		}
	}

}
