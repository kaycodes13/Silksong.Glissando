using BepInEx.Configuration;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Plugin;
using Silksong.ModMenu.Screens;
using System.Collections.Generic;
using UnityEngine;
using VVVVVV.Menu;
using VVVVVV.Utils;

namespace VVVVVV.Settings;

internal class ModSettings : IModMenuCustomMenu {

	private static ConfigFile Config => V6Plugin.Instance.Config;

	private const FaydownState FAYDOWN_STATE_DEFAULT = FaydownState.DoubleJump;
	private const KeyCode RESPAWN_KEY_DEFAULT = KeyCode.None;

	public FaydownState FaydownState => faydownState?.Value ?? FAYDOWN_STATE_DEFAULT;
	private ConfigEntry<FaydownState>? faydownState;
	private LocalisedChoiceElement<FaydownState>? faydownOption;

	public KeyCode RespawnKey => respawnKey?.Value ?? RESPAWN_KEY_DEFAULT;
	private ConfigEntry<KeyCode>? respawnKey;
	private LocalisedChoiceElement<KeyCode>? respawnKeyOption;
	private static readonly List<KeyCode> bindableKeys = [
		KeyCode.None,
		KeyCode.F3,
		KeyCode.F4,
		KeyCode.F5,
		KeyCode.F6,
		KeyCode.F7,
		KeyCode.F8,
		KeyCode.F9,
		KeyCode.F10,
		KeyCode.Backspace,
		KeyCode.Tab,
		KeyCode.Backslash,
		KeyCode.Slash,
		KeyCode.LeftAlt,
		KeyCode.RightAlt,
	];

	public void BindConfigEntries() {
		respawnKey = Config.Bind("", "RespawnKeybind", RESPAWN_KEY_DEFAULT);
		if (!bindableKeys.Contains(respawnKey.Value))
			respawnKey.Value = KeyCode.None;

		faydownState = Config.Bind("", "FayfornsGift", FAYDOWN_STATE_DEFAULT);
	}

	public string ModMenuName() => V6Plugin.Name;

	public AbstractMenuScreen BuildCustomMenu() {
		LocalisedTextButton respawnBtn = new(LangUtil.String("MENU_RESPAWN_BUTTON")) {
			OnSubmit = V6Plugin.QueueRespawnHero
		};

		respawnKeyOption = new(
			LangUtil.String("MENU_RESPAWN_KEY_LABEL"),
			bindableKeys,
			LangUtil.String("MENU_RESPAWN_KEY_DESC")
		);
		SyncEntryAndElement(respawnKey!, respawnKeyOption);

		faydownOption = new(
			LangUtil.String("MENU_FLIPDOWN_LABEL"),
			FaydownStateExt.LocalisedChoiceModel(),
			LangUtil.String("MENU_FLIPDOWN_DESC")
		);
		SyncEntryAndElement(faydownState!, faydownOption);

		SimpleMenuScreen screen = new(ModMenuName());
		screen.AddRange([respawnBtn, respawnKeyOption, faydownOption]);

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
