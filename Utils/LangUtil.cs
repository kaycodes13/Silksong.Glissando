using TeamCherry.Localization;

namespace Glissando.Utils;

internal static class LangUtil {

	internal const string SHEET = $"Mods.{GlissandoPlugin.Id}";

	public static LocalisedString String(string key)
		=> new(SHEET, key);

}
