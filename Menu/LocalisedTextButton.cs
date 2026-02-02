using Silksong.ModMenu.Elements;
using TeamCherry.Localization;

namespace VVVVVV.Menu;

internal class LocalisedTextButton : TextButton {

	private LocalisedString localisedText;

	public LocalisedTextButton(LocalisedString text) : base(text.ToString()) {
		localisedText = text;
		Container.name = $"{text.Key}";
		var updater = Container.AddComponentIfNotPresent<OnLanguageUpdatedHelper>();
		updater.OnLanguageChanged += UpdateLocalisation;
	}

	private void UpdateLocalisation() {
		ButtonText.text = localisedText.ToString();
	}

}
