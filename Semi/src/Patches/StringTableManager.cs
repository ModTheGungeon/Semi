using System;
using System.Collections.Generic;
using MonoMod;

namespace Semi.Patches {
	[MonoModPatch("global::StringTableManager")]
	public static class StringTableManager {
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
