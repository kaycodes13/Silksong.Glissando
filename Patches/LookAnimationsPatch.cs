using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace VVVVVV.Patches;

[HarmonyPatch(typeof(HeroAnimationController), nameof(HeroAnimationController.GetClip))]
internal static class LookAnimationsPatch {

	private static void Prefix(ref string clipName) {
		if (!V6Plugin.GravityIsFlipped)
			return;

		if (downToUp.TryGetValue(clipName, out string downToUpClip))
			clipName = downToUpClip;
		else if (upToDown.TryGetValue(clipName, out string upToDownClip))
			clipName = upToDownClip;
	}

	private static readonly Dictionary<string, string>
		downToUp = new(){
			{ "LookDown", "LookUp" },
			{ "LookDownEnd", "LookUpEnd" },
			{ "LookDownToIdle", "LookUpToIdle" },
			{ "Ring Look Down", "Ring Look Up" },
			{ "Ring Look Down End", "Ring Look Up End" },
			{ "LookDown Updraft", "LookUp Updraft" },
			{ "LookDownEnd Updraft", "LookUpEnd Updraft" },
			{ "LookDown Windy", "LookUp Windy" },
			{ "LookDownEnd Windy", "LookUpEnd Windy" },
			{ "Hurt Look Down", "Hurt Look Up" },
			{ "Hurt Look Down Windy", "Hurt Look Up Windy" },
			{ "Hurt Look Down Windy End", "Hurt Look Up Windy End" },
		},
		upToDown = downToUp.ToDictionary(i => i.Value, i => i.Key);

}
