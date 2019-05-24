using System;
using System.Collections.Generic;
using MonoMod;

namespace Semi.Patches {
	/// <summary>
	/// Patches StringTableManager to use <c>Semi.I18N</c> methods for changing the language.
	/// This allows the game to load custom localizations when the language is changed through the UI.
	/// </summary>
	[MonoModPatch("global::StringTableManager")]
	public static class StringTableManager {
		internal static bool CreeperAwMan;

		internal static string RetrieveString(byte[] b) {
			return System.Text.Encoding.ASCII.GetString(b);
		}

		[MonoModIgnore]
		public static Dictionary<string, global::StringTableManager.StringCollection> m_introTable;

		//private static Dictionary<string, global::StringTableManager.StringCollection> m_backupItemsTable;

		//private static Dictionary<string, global::StringTableManager.StringCollection> m_backupCoreTable;

		[MonoModIgnore]
		public static Dictionary<string, global::StringTableManager.StringCollection> m_synergyTable;

		//private static Dictionary<string, global::StringTableManager.StringCollection> m_backupIntroTable;

		[MonoModIgnore]
		public static Dictionary<string, global::StringTableManager.StringCollection> m_uiTable;

		[MonoModIgnore]
		public static Dictionary<string, global::StringTableManager.StringCollection> m_enemiesTable;

		[MonoModIgnore]
		public static Dictionary<string, global::StringTableManager.StringCollection> m_itemsTable;

		[MonoModIgnore]
		public static Dictionary<string, global::StringTableManager.StringCollection> m_coreTable;

		public static void SetNewLanguage(global::StringTableManager.GungeonSupportedLanguages language, bool force = false) {
			I18N.Logger.Debug($"Redirecting StringTableManager.SetNewLanguage to I18N.ChangeLanguage");
			I18N.ChangeLanguage(language);
		}
	}
}
