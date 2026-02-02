using System;
using TeamCherry.Localization;
using UnityEngine;

namespace VVVVVV.Menu;

internal class OnLanguageUpdatedHelper : MonoBehaviour {

	private LanguageCode? prevLang;
	public event Action? OnLanguageChanged;

	public void Update() {
		if (prevLang == null || prevLang != Language._currentLanguage) {
			prevLang = Language._currentLanguage;
			OnLanguageChanged?.Invoke();
		}
	}

}
