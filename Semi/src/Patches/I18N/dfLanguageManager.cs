using System;
using System.Collections.Generic;
using MonoMod;

namespace Semi.Patches {
	/// <summary>
	/// Patches dfLanguageManager to use <c>Semi.I18N</c>.
	/// </summary>
	[MonoModPatch("global::dfLanguageManager")]
	public class dfLanguageManager : global::dfLanguageManager {
		[MonoModIgnore]
		private Dictionary<string, string> strings = new Dictionary<string, string>();

		public static dfLanguageManager Instance;

		public extern void orig_Start();
		public void Start() {
			Instance = this;
			orig_Start();
		}

		public extern void orig_LoadLanguage(dfLanguageCode language, bool forceReload = false);

		public void LoadLanguage(dfLanguageCode language, bool forceReload = false) {
			if (I18N.CurrentUIDict == null) orig_LoadLanguage(language, forceReload);
			else {
				strings = I18N.CurrentUIDict;

				if (forceReload) {
					dfControl[] componentsInChildren = base.GetComponentsInChildren<dfControl>();
					for (int i = 0; i < componentsInChildren.Length; i++) {
						componentsInChildren[i].Localize();
					}
					for (int j = 0; j < componentsInChildren.Length; j++) {
						componentsInChildren[j].PerformLayout();
						if (componentsInChildren[j].LanguageChanged != null) {
							componentsInChildren[j].LanguageChanged(componentsInChildren[j]);
						}
					}
				}
			}
		}

		protected extern internal string orig_getLocalizedValue(string key);
		protected internal string getLocalizedValue(string key) {
			I18N.Logger.Debug($"DF: get '{key}'");
			return orig_getLocalizedValue(key);
		}
	}
}