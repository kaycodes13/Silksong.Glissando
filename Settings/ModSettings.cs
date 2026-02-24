using BepInEx.Configuration;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Plugin;
using Silksong.ModMenu.Screens;
using UnityEngine;
using Glissando.Utils;

namespace Glissando.Settings;

internal class ModSettings : IModMenuCustomMenu {

	private static ConfigFile Config => GlissandoPlugin.Instance.Config;

	private const FaydownState FAYDOWN_STATE_DEFAULT = FaydownState.DoubleJump;
	private const KeyCode RESPAWN_KEY_DEFAULT = KeyCode.None;

	public FaydownState FaydownState => faydownState?.Value ?? FAYDOWN_STATE_DEFAULT;
	private ConfigEntry<FaydownState>? faydownState;
	private ChoiceElement<FaydownState>? faydownOption;

	public KeyCode RespawnKey => respawnKey?.Value ?? RESPAWN_KEY_DEFAULT;
	private ConfigEntry<KeyCode>? respawnKey;
	private KeyBindElement? respawnKeyOption;

	public void BindConfigEntries() {
		respawnKey = Config.Bind("", "RespawnKeybind", RESPAWN_KEY_DEFAULT);
		faydownState = Config.Bind("", "FayfornsGift", FAYDOWN_STATE_DEFAULT);
	}

	public string ModMenuName() => GlissandoPlugin.Name;

	public AbstractMenuScreen BuildCustomMenu() {
		TextButton respawnBtn = new(LangUtil.String("MENU_RESPAWN_BUTTON")) {
			OnSubmit = GlissandoPlugin.QueueRespawnHero
		};

		respawnKeyOption = new(LangUtil.String("MENU_RESPAWN_KEY_LABEL"));
		SyncEntryAndElement(respawnKey!, respawnKeyOption);

		faydownOption = new(
			LangUtil.String("MENU_FLIPDOWN_LABEL"),
			FaydownStateExt.LocalisedChoiceModel(),
			LangUtil.String("MENU_FLIPDOWN_DESC")
		);
		SyncEntryAndElement(faydownState!, faydownOption);

		SimpleMenuScreen screen = new(ModMenuName());
		MenuElement[] elements = [
			respawnBtn,
			respawnKeyOption,
			faydownOption,
		];
		screen.AddRange(elements);
		// genuinely do not have a clue why this is needed, but it is
		foreach(var elt in elements)
			elt.Container.transform.SetParent(screen.Container.transform, false);

		return screen;
	}

	private static void SyncEntryAndElement<T>(ConfigEntry<T> cfg, SelectableValueElement<T> elt) {
		elt.Value = cfg.Value;
		cfg.SettingChanged += (_, _) => {
			if (elt != null && !Equals(elt.Value, cfg.Value))
				elt.Value = cfg.Value;
		};
		elt.OnValueChanged += newVal => cfg.Value = newVal;
	}
}
