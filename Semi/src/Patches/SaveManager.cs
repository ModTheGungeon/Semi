#pragma warning disable 0626

using System;
using MonoMod;

namespace Semi.Patches {
	/// <summary>
	/// Patches SaveManager to change save file location to avoid corruption or crashes if the save format is changed.
	/// Currently broken/not used.
	/// </summary>
	[MonoModPatch("global::SaveManager")]
	public class SaveManager {
		public static global::SaveManager.SaveType MidGameSave = new global::SaveManager.SaveType {
			filePattern = "modded_Active{0}.game",
			legacyFilePattern = "activeSlot{0}.txt",
			encrypted = true,
			backupCount = 0,
			backupPattern = "modded_Active{0}.backup.{1}",
			backupMinTimeMin = 60
		};

		public static global::SaveManager.SaveType OptionsSave = new global::SaveManager.SaveType {
			filePattern = "modded_Slot{0}.options",
			legacyFilePattern = "optionsSlot{0}.txt"
		};

		public static global::SaveManager.SaveType GameSave = new global::SaveManager.SaveType {
			filePattern = "modded_Slot{0}.save",
			encrypted = true,
			backupCount = 3,
			backupPattern = "modded_Slot{0}.backup.{1}",
			backupMinTimeMin = 45,
			legacyFilePattern = "gameStatsSlot{0}.txt"
		};
	}
}
