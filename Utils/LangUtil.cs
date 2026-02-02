using TeamCherry.Localization;

namespace VVVVVV.Utils;

internal static class LangUtil {

	internal const string SHEET = $"Mods.{V6Plugin.Id}";

	public static LocalisedString String(string key)
		=> new(SHEET, key);

}
