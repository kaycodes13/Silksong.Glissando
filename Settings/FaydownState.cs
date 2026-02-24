using Silksong.ModMenu.Models;
using System;
using System.Linq;
using Glissando.Utils;

namespace Glissando.Settings;

internal enum FaydownState {
	FlipGravity,
	DoubleJump,
	Disabled,
}

internal static class FaydownStateExt {

	public static bool FlipsGravity(this FaydownState state)
		=> state == FaydownState.FlipGravity;

	public static ListChoiceModel<FaydownState> LocalisedChoiceModel() 
		=> new([.. Enum.GetValues(typeof(FaydownState)).Cast<FaydownState>()]) {
			DisplayFn = (int _, FaydownState val) => LangUtil.String(val.MenuLangKey())
		};

	public static string MenuLangKey(this FaydownState state)
		=> state.LangKey("MENU_FLIPDOWN");

	public static string InventoryLangKey(this FaydownState state)
		=> state.LangKey("INV_DESC_DRESS_DJ");

	private static string LangKey(this FaydownState state, string prefix)
		=> state switch {
			FaydownState.FlipGravity => $"{prefix}_FLIPS",
			FaydownState.DoubleJump => $"{prefix}_JUMPS",
			FaydownState.Disabled => $"{prefix}_OFF",
			_ => throw new ArgumentOutOfRangeException(nameof(state))
		};

}
