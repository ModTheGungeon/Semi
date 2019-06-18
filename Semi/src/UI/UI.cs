using System;
namespace Semi {
	public static class UI {
		internal static UnityEngine.GameObject YesNoDialogPrefab;
		internal static UnityEngine.GameObject OKDialogPrefab;
		internal static UnityEngine.GameObject OKDialog;

		internal static dfGUIManager MainMenuGUIManager;

		public static bool MainMenuGUIReady {
			get {
				return MainMenuGUIManager != null;
			}
		}

		public static T AddMainMenuControl<T>() where T : dfControl {
			if (!MainMenuGUIReady) throw new InvalidOperationException("Main menu has not been initialized yet.");

			var ctrl = MainMenuGUIManager.AddControl<T>();
			Patches.MainMenuFoyerController.Instance.CustomFadingControls.Add(ctrl);
			return ctrl;
		}
	}
}
