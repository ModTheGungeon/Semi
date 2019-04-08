#pragma warning disable 0626
#pragma warning disable 0649

using System;
using MonoMod;

namespace Semi.Patches {
	[MonoModPatch("global::MainMenuFoyerController")]
	public class MainMenuFoyerController : global::MainMenuFoyerController {
		public static MainMenuFoyerController Instance = null;

		private extern void orig_Awake();

		private void Awake() {
			orig_Awake();
			if (Instance == null) {
				Instance = this;
			}
			VersionLabel.Text = $"SEMI | {VersionLabel.Text}";
		}
	}
}
