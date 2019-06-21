using System;
using System.Collections.Generic;
using UnityEngine;

namespace Semi {
	public static class UI {
		public static dfFontBase GungeonFont;
		internal static dfPanel LoadInfoPanel;
		internal static dfGUIManager MainMenuGUIManager;

		internal static dfLabel LoadErrorTitle;
		internal static dfLabel LoadErrorSubtitle;
		internal static dfButton LoadErrorOKButton;
		internal static List<dfLabel> LoadErrorModLabels;

		internal static bool MainMenuGUIReady {
			get {
				return MainMenuGUIManager != null;
			}
		}

		internal static T AddMainMenuControl<T>() where T : dfControl {
			if (!MainMenuGUIReady) throw new InvalidOperationException("Main menu has not been initialized yet.");

			var ctrl = MainMenuGUIManager.AddControl<T>();
			Patches.MainMenuFoyerController.Instance.CustomFadingControls.Add(ctrl);
			return ctrl;
		}

		internal static void UpdateLoadErrorScreen() {
			LoadErrorTitle.Text = StringTableManager.GetString("#semi:LOADERROR_TITLE");
			LoadErrorSubtitle.Text = StringTableManager.GetString("#semi:LOADERROR_SUBTITLE");
			SemiLoader.Logger.Debug($"ERROR TITLE: {StringTableManager.GetString("#semi:LOADERROR_TITLE")}");
			SemiLoader.Logger.Debug($"ERROR SUBTITLE: {StringTableManager.GetString("#semi:LOADERROR_SUBTITLE")}");

			LoadErrorTitle.Position = new Vector3(-LoadErrorTitle.Size.x / 2, Screen.height / 2, 0);
			LoadErrorSubtitle.Position = new Vector3(-LoadErrorSubtitle.Size.x / 2, LoadErrorTitle.Position.y - LoadErrorTitle.Size.y / 2 - 15);
			LoadErrorOKButton.Position = new Vector3(-LoadErrorOKButton.Size.x / 2, -Screen.height / 2);

			dfLabel prev_label = null;
			for (int i = 0; i < LoadErrorModLabels.Count; i++) {
				var label = LoadErrorModLabels[i];

				if (i == 0) { // first mod name
					label.Position = new Vector3(-label.Size.x / 2, LoadErrorSubtitle.Position.y - LoadErrorSubtitle.Size.y / 2 - 120);
				} else if (i == 1) { // first mod error
					label.Position = new Vector3(-label.Size.x / 2, prev_label.Position.y - prev_label.Size.y / 2 - 15);
				} else if (i % 2 == 0) { // other mod name
					label.Position = new Vector3(-label.Size.x / 2, prev_label.Position.y - prev_label.Size.y / 2 - 60);
				} else { // other mod error
					label.Position = new Vector3(-label.Size.x / 2, prev_label.Position.y - prev_label.Size.y / 2 - 15);
				}

				prev_label = label;
			}
		}

		public static void OpenLoadErrorScreen(List<ModError> errors) {
			GameUIRoot.Instance.Manager.RenderCamera.depth += 10f;
			GameUIRoot.Instance.Manager.RenderCamera.enabled = true;
			GameUIRoot.Instance.Manager.overrideClearFlags = CameraClearFlags.Color;
			GameUIRoot.Instance.Manager.RenderCamera.backgroundColor = Color.black;

			LoadErrorModLabels = new List<dfLabel>();
			for (int i = 0; i < errors.Count; i++) {
				var mod_name = GameUIRoot.Instance.Manager.AddControl<dfLabel>();
				mod_name.zindex = 3;
				mod_name.AutoSize = true;
				mod_name.Text = errors[i].DisplayName;
				mod_name.BackgroundColor = Color.black;
				mod_name.Color = Color.white;
				mod_name.Font = GungeonFont;
				mod_name.TextScale = 3;
				mod_name.IsVisible = false;

				var mod_err = GameUIRoot.Instance.Manager.AddControl<dfLabel>();
				mod_err.zindex = 3;
				mod_err.AutoSize = true;
				mod_err.Text = LocalizeException(errors[i].Exception);
				mod_err.BackgroundColor = Color.black;
				mod_err.Color = Color.gray;
				mod_err.Font = UI.GungeonFont;
				mod_err.TextScale = 3;
				mod_err.IsVisible = false;
				mod_err.WordWrap = true;
				mod_err.MaximumSize = new Vector3(Screen.width * 0.95f, Screen.height);

				LoadErrorModLabels.Add(mod_name);
				LoadErrorModLabels.Add(mod_err);
			}

			UpdateLoadErrorScreen();
			LoadErrorTitle.Show();
			LoadErrorSubtitle.Show();
			for (int i = 0; i < LoadErrorModLabels.Count; i++) LoadErrorModLabels[i].Show();
			LoadErrorOKButton.Show();
			LoadErrorOKButton.Focus();
		}

		internal static void CloseLoadErrorScreen() {
			GameUIRoot.Instance.Manager.RenderCamera.depth -= 10;
			GameUIRoot.Instance.Manager.RenderCamera.enabled = false;
			GameUIRoot.Instance.Manager.overrideClearFlags = CameraClearFlags.Depth;

			LoadErrorTitle.Hide();
			LoadErrorSubtitle.Hide();
			LoadErrorOKButton.Hide();

			for (int i = 0; i < LoadErrorModLabels.Count; i++) {
				LoadErrorModLabels[i].Hide();
				UnityEngine.Object.Destroy(LoadErrorModLabels[i]);
			}
		}

		internal static string LocalizeException(Exception e) {
			if (e is ChecksumMismatchException) return StringTableManager.GetString("#semi:ERROR_CHECKSUM");
			return e.Message;
		}
	}
}
