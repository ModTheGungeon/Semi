using System;
using System.Collections.Generic;
using FullSerializer;
using MonoMod;

namespace Semi {
	/// <summary>
	/// Patches GameStatsManager to allow for mod-specific save data.
	/// </summary>
	[MonoModPatch("global::GameStatsManager")]
	public class GameStatsManager : global::GameStatsManager {
		[MonoModIgnore]
		private static GameStatsManager m_instance;

		[fsProperty]
		private Dictionary<string, string> m_semiModData = new Dictionary<string, string>();

		public extern static void orig_Load();

		public static void Load() {
			orig_Load();
			foreach (var mod in SemiLoader.Mods) {
				string mod_data = null;
				if (m_instance != null && m_instance.m_semiModData != null) {
					m_instance.m_semiModData.TryGetValue(mod.Key, out mod_data);
				}
				mod.Value.Instance.Deserialize(mod_data);
			}
		}

		public extern static bool orig_Save();

		public static bool Save() {
			m_instance.m_semiModData = new Dictionary<string, string>();
			foreach (var mod in SemiLoader.Mods) {
				m_instance.m_semiModData[mod.Key] = mod.Value.Instance.Serialize();
			}
			return orig_Save();
		}
	}
}
