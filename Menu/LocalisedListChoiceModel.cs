using Silksong.ModMenu.Models;
using System.Collections.Generic;
using System.Linq;
using TeamCherry.Localization;

namespace VVVVVV.Menu;

internal class LocalisedListChoiceModel<T> : ListChoiceModel<T> {

	readonly List<LocalisedString> names;

	public LocalisedListChoiceModel(List<(T value, LocalisedString name)> values) : base([..values.Select(x => x.value)]) {
		this.names = [.. values.Select(x => x.name)];
	}

	public override string DisplayString() => names[Index].ToString();

}
