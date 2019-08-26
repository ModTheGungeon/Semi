#pragma warning disable 0626

using System;
using MonoMod;

namespace Semi.Patches {
	/// <summary>
	/// Patches SaveManager to change save file location to avoid corruption or crashes if the save format is changed.
	/// </summary>
	[MonoModPatch("global::SaveManager")]
	public static class SaveManager {
		[MonoModIgnore]
		public static global::SaveManager.SaveType GameSave;

		[MonoModIgnore]
		public static global::SaveManager.SaveType OptionsSave;

		[MonoModIgnore]
		public static global::SaveManager.SaveType MidGameSave;

		[MonoModOriginal]
		public static void orig_ctor_global() { }

		[MonoModConstructor]
		[MonoModOriginalName("orig_ctor_global")]
		public static void ctor_global() {
			orig_ctor_global();
			MidGameSave.filePattern = "modded_Active{0}.game";
			MidGameSave.backupPattern = "modded_Active{0}.backup.{1}";
			OptionsSave.filePattern = "modded_Slot{0}.options";
			GameSave.filePattern = "modded_Slot{0}.save";
			GameSave.backupPattern = "modded_Slot{0}.backup.{1}";
		}
	}
}
