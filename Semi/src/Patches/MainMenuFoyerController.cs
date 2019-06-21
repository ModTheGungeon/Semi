#pragma warning disable 0626
#pragma warning disable 0649

using System;
using MonoMod;
using System.Collections.Generic;

namespace Semi.Patches {
	/// <summary>
	/// Patches MainMenuFoyerController to modify the version label and add a list of controls to do
	/// the fade out effect on (for custom UI elements).
	/// </summary>
	[MonoModPatch("global::MainMenuFoyerController")]
	public class MainMenuFoyerController : global::MainMenuFoyerController {
		public static MainMenuFoyerController Instance = null;
		public List<dfControl> CustomFadingControls = null;
		private static bool _OpenedLoadErrorScreen = false;

		private extern void orig_Awake();

		private void Awake() {
			orig_Awake();
			if (Instance == null) {
				Instance = this;
			}
			CustomFadingControls = new List<dfControl>();
			VersionLabel.Text = $"SEMI | {VersionLabel.Text}";
		}

		public extern void orig_InitializeMainMenu();

		public new void InitializeMainMenu() {
			orig_InitializeMainMenu();
			SemiLoader.InitializeMainMenuUIHelpers();
		}

		[MonoModIgnore]
		private extern bool IsDioramaRevealed(bool doReveal = false);

		private extern void orig_Update();
		private void Update() {
			orig_Update();
			if (!_OpenedLoadErrorScreen && IsDioramaRevealed(false)) {
				_OpenedLoadErrorScreen = true;
				SemiLoader.OpenLoadErrorScreenIfNecessary();
			}
		}
	}

	/// <summary>
	/// Patches MainMenuFoyerController's ToggleFade coroutine to process custom UI elements.
	/// </summary>
	[MonoModPatch("MainMenuFoyerController/<ToggleFade>c__Iterator0")]
	public class MainMenuFoyerControllerToggleFadeCR {
		public extern bool orig_MoveNext();

		private float __t____1 {
			get {
				return (float)GetType().GetField("<t>__1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this);
			}
		}

		private MainMenuFoyerController _this {
			get {
				return (MainMenuFoyerController)GetType().GetField("$this", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this);
			}
		}

		public bool MoveNext() {
			if (orig_MoveNext()) {
				for (int i = 0; i < _this.CustomFadingControls.Count; i++) {
					var el = _this.CustomFadingControls[i];
					if (el != null) el.Opacity = __t____1;
				}
				return true;
			}
			return false;
		}
	}
}
