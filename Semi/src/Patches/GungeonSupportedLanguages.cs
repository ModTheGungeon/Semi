using System;
using MonoMod;

namespace Semi.Patches {
	/// <summary>
	/// Adds a SEMI_CUSTOM value to GungeonSupportedLanguages for custom languages added by mods.
	/// Due to a bug in MonoMod (might be fixed), does not actually patch the game's enum yet.
	/// </summary>
	public enum GungeonSupportedLanguages {
		ENGLISH,
		RUBEL_TEST,
		FRENCH,
		SPANISH,
		ITALIAN,
		GERMAN,
		BRAZILIANPORTUGUESE,
		JAPANESE,
		KOREAN,
		RUSSIAN,
		POLISH,
		CHINESE,
		SEMI_CUSTOM
	}
}
