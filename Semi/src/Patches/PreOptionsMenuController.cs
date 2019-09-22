using System;
using System.Collections;
using System.Reflection;
using MonoMod;
using UnityEngine;

namespace Semi.Patches {
	[MonoModPatch("global::PreOptionsMenuController")]
	public class PreOptionsMenuController : global::PreOptionsMenuController {
		private dfPanel m_panel;
		public dfButton TabModsSelector;

		public static PreOptionsMenuController Instance;
		public dfPanel MainPanel { get { return m_panel; } }

		private extern void orig_Awake();
		private void Awake() {
			Instance = this;
			orig_Awake();
			TabModsSelector = m_panel.AddControl<dfButton>();

			var crosshair_selection_doer_control = TabGameplaySelector.GetComponent<MenuCrosshairSelectionDoer>().controlToPlace;

			var mods_ui_key_controls = TabModsSelector.gameObject.AddComponent<UIKeyControls>();
			var mods_crosshair_doer = TabModsSelector.gameObject.AddComponent<MenuCrosshairSelectionDoer>();

			mods_ui_key_controls.up = TabGameplaySelector;
			mods_ui_key_controls.down = TabControlsSelector;

			var gameplay_ui_key_controls = TabGameplaySelector.GetComponent<UIKeyControls>();
			gameplay_ui_key_controls.down = TabModsSelector;

			var controls_ui_key_controls = TabControlsSelector.GetComponent<UIKeyControls>();
			controls_ui_key_controls.up = TabModsSelector;

			mods_crosshair_doer.controlToPlace = crosshair_selection_doer_control;
			


			//var props = typeof(dfButton).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			//for (int j = 0; j < props.Length; j++) {
			//	var prop = props[j];

			//	if (prop.GetSetMethod() != null) prop.SetValue(TabModsSelector, prop.GetValue(TabGameplaySelector, null), null);
			//}

			TabModsSelector.Font = TabGameplaySelector.Font;
			TabModsSelector.TextScale = TabGameplaySelector.TextScale;
			TabModsSelector.RelativePosition = TabGameplaySelector.RelativePosition;
			TabModsSelector.AutoSize = true;
			TabModsSelector.TextColor = TabGameplaySelector.TextColor;
			TabModsSelector.FocusTextColor = TabGameplaySelector.FocusTextColor;
			TabModsSelector.HoverTextColor = TabGameplaySelector.HoverTextColor;
			TabModsSelector.Atlas = TabGameplaySelector.Atlas;
			TabModsSelector.Height = TabGameplaySelector.Height;
			TabModsSelector.IsEnabled = TabGameplaySelector.IsEnabled;
			TabModsSelector.IsVisible = TabGameplaySelector.IsVisible;
			TabModsSelector.IsLocalized = true;

			TabModsSelector.forceUpperCase = true;
			TabModsSelector.Text = "#semi:TAB_MODS";
			TabModsSelector.Localize();

			TabModsSelector.RelativePosition = new Vector2(m_panel.Width / 2 - TabModsSelector.Width / 2, TabModsSelector.RelativePosition.y);

			TabModsSelector.name = "TabModsSelector";

			Console.WriteLine($"TEXTSCALE {TabModsSelector.TextScale}");
			Console.WriteLine($"TEXTCOL {TabModsSelector.TextColor}");
			Console.WriteLine($"FTEXTCOL {TabModsSelector.FocusTextColor}");
			Console.WriteLine($"HTEXTCOL {TabModsSelector.HoverTextColor}");

			TabGameplaySelector.RelativePosition = new Vector2(TabGameplaySelector.RelativePosition.x, TabGameplaySelector.RelativePosition.y + 2);
			TabModsSelector.RelativePosition = new Vector2(TabModsSelector.RelativePosition.x, TabGameplaySelector.RelativePosition.y + 40);
			TabControlsSelector.RelativePosition = new Vector2(TabControlsSelector.RelativePosition.x, TabModsSelector.RelativePosition.y + 40);
			TabVideoSelector.RelativePosition = new Vector2(TabVideoSelector.RelativePosition.x, TabControlsSelector.RelativePosition.y + 40);
			TabAudioSelector.RelativePosition = new Vector2(TabAudioSelector.RelativePosition.x, TabVideoSelector.RelativePosition.y + 40);

			var comps = TabModsSelector.GetComponents<MonoBehaviour>();
			for (int i = 0; i < comps.Length; i++) {
				Console.WriteLine($"COMP {comps[i].GetType().FullName}");
			}

			comps = TabGameplaySelector.GetComponents<MonoBehaviour>();
			for (int i = 0; i < comps.Length; i++) {
				Console.WriteLine($"COMPX {comps[i].GetType().FullName}");
			}

			TabModsSelector.Click += delegate (dfControl control, dfMouseEventArgs mouseEvent) {
				ToggleToPanel(((FullOptionsMenuController)FullOptionsMenu).TabMods, false, false);
			};

			TabModsSelector.IsVisible = true;

			Console.WriteLine($"GAMEPLAY: {TabGameplaySelector} {TabGameplaySelector.Position.x} {TabGameplaySelector.Position.y} {TabGameplaySelector.IsVisible}");
			Console.WriteLine($"MODS: {TabModsSelector} {TabModsSelector.Position.x} {TabModsSelector.Position.y} {TabModsSelector.IsVisible}");
			Console.WriteLine($"CONTROLS: {TabControlsSelector} {TabControlsSelector.Position.x} {TabControlsSelector.Position.y} {TabControlsSelector.IsVisible}");
			Console.WriteLine($"VIDEO: {TabVideoSelector} {TabVideoSelector.Position.x} {TabVideoSelector.Position.y} {TabVideoSelector.IsVisible}");
			Console.WriteLine($"AUDIO: {TabAudioSelector} {TabAudioSelector.Position.x} {TabAudioSelector.Position.y} {TabAudioSelector.IsVisible}");
		}
	}
}
