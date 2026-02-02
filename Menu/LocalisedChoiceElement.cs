using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using System.Collections.Generic;
using TeamCherry.Localization;

namespace VVVVVV.Menu;

internal class LocalisedChoiceElement<T> : ChoiceElement<T> {

	private LocalisedString localisedLabel;
	private LocalisedString? localisedDesc;

	public LocalisedChoiceElement(LocalisedString label, IChoiceModel<T> model, LocalisedString? description = null)
		: base(label.ToString(), model, description ?? "")
	{
		localisedLabel = label;
		localisedDesc = description;
		Container.name = $"{label.Key}";
		var updater = Container.AddComponentIfNotPresent<OnLanguageUpdatedHelper>();
		updater.OnLanguageChanged += UpdateLocalisation;
	}

	public LocalisedChoiceElement(LocalisedString label, List<T> items, LocalisedString? description = null)
		: this(label, ChoiceModels.ForValues(items), description) { }

	private void UpdateLocalisation() {
		LabelText.text = localisedLabel.ToString();
		DescriptionText.text = localisedDesc?.ToString() ?? "";
	}

}
