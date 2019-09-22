using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Semi {
	public static class UI {
		public abstract class MenuOption {
			internal abstract void Insert(dfControl target);
		}

		public class CheckboxMenuOption : MenuOption {
			public Action<bool> Changed = (v) => { };
			public string Label;

			internal CheckboxMenuOption() { }

			internal CheckboxMenuOption(string label) {
				Label = label;
			}

			internal CheckboxMenuOption Create(string label) => new CheckboxMenuOption(label);

			internal override void Insert(dfControl target) {
				var menu_item = CreateMenuEntryCheckbox(target, Label, true);
				menu_item.CheckboxChanged += Changed;
			}
		}

		public class ListMenuOption : MenuOption {
			public Action<string> Changed = (v) => { };			
			public string Label;
			public string[] Options;

			internal ListMenuOption() { }

			internal ListMenuOption(string label, string[] options) {
				Label = label;
				Options = options;
			}

			internal ListMenuOption Create(string label, string[] options) => new ListMenuOption(label, options);

			internal override void Insert(dfControl target) {
				var menu_item = CreateMenuEntryOption(target, Label, Options, true);
				menu_item.SelectionChanged += Changed;
			}
		}

		public static dfFontBase GungeonFont;
		internal static dfPanel LoadInfoPanel;
		internal static dfGUIManager MainMenuGUIManager;

		internal static dfLabel LoadErrorTitle;
		internal static dfLabel LoadErrorSubtitle;
		internal static dfButton LoadErrorOKButton;
		internal static List<dfLabel> LoadErrorModLabels;

		internal static dfPanel CachedCentralPanel;
		internal static dfAtlas CachedOptionsAtlas;
		internal static dfScrollbar CachedOptionsScrollbar;
		internal static dfControl CachedCrosshairSelectionDoerControl;

		internal static dfPanel CachedCheckboxMenuEntry;
		internal static dfPanel CachedOptionMenuEntry;
		internal static dfPanel CachedTextboxMenuEntry;

		internal static bool MainMenuGUIReady {
			get {
				return MainMenuGUIManager != null;
			}
		}

		public static readonly int MenuTextScale = 3;
		public static readonly Color MenuTextColor = new Color(85 / 255f, 85 / 255f, 85 / 255f);
		public static readonly Color MenuHoverTextColor = new Color(254 / 255f, 254 / 255f, 254 / 255f);
		public static readonly Color MenuFocusTextColor = new Color(254 / 255f, 254 / 255f, 254 / 255f);

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

		internal static void PrepareCustomOptions() {
			CachedCentralPanel = Patches.FullOptionsMenuController.Instance.TabGameplay.transform.parent.gameObject.GetComponent<dfPanel>();
			CachedOptionsAtlas = Patches.FullOptionsMenuController.Instance.TabGameplay.Atlas;
			CachedOptionsScrollbar = Patches.FullOptionsMenuController.Instance.TabGameplay.VertScrollbar;
			CachedCrosshairSelectionDoerControl = Patches.PreOptionsMenuController.Instance.TabGameplaySelector.GetComponent<MenuCrosshairSelectionDoer>().controlToPlace;

			CachedOptionMenuEntry = Patches.FullOptionsMenuController.Instance.TabGameplay.transform.GetChild(1).gameObject.GetComponent<dfPanel>();
			CachedCheckboxMenuEntry = Patches.FullOptionsMenuController.Instance.TabGameplay.transform.GetChild(0).gameObject.GetComponent<dfPanel>();
		}

		internal static dfScrollPanel CreateModOptionsPage(SemiLoader.ModInfo mod) {
			var tab = (Patches.dfScrollPanel)CreateScrollPanel();

			Patches.FullOptionsMenuController.Instance.ModSpecificTabs.Add(tab);

			string str = "";

			if (mod.Config.Name != null) str = mod.Config.Name;
			else str = mod.Config.ID;

			if (mod.Config.Version != null) str += $" {mod.Config.Version}";
			if (mod.Config.Author != null) str += $" by {mod.Config.Author}";
					
			CreateMenuButton(tab, str, false, true);
			if (mod.Config.Description != null) CreateMenuButton(tab, mod.Config.Description, false, true);
			var namespace_str = string.Format(I18N.GetString(I18N.StringTable.UI, "#semi:REGISTERED_UNDER"), mod.Config.ID);
			CreateMenuButton(tab, namespace_str, false, true).LanguageChanged += (obj) => {
				((dfButton)obj).Text = string.Format(I18N.GetString(I18N.StringTable.UI, "#semi:REGISTERED_UNDER"), mod.Config.ID);
			};

			if (mod.Instance.MenuOptions.Count == 0) CreateMenuButton(tab, "#semi:NO_MOD_OPTIONS", true, true);

			var button = CreateMenuButton(Patches.FullOptionsMenuController.Instance.TabMods, mod.Config.Name ?? mod.Config.ID, false);
			button.Click += (control, mouseEvent) => {
				Patches.FullOptionsMenuController.Instance.ToggleToPanel(tab);
			};

			return tab;
		}

		public static dfScrollPanel CreateScrollPanel() {
			var central_panel = CachedCentralPanel;

			var scroll_bar = central_panel.AddControl<dfScrollbar>();
			scroll_bar.name = "Scrollbar";
			scroll_bar.Anchor = dfAnchorStyle.Right | dfAnchorStyle.CenterVertical;
			scroll_bar.IsEnabled = true;
			scroll_bar.IsVisible = false;
			scroll_bar.IsInteractive = true;
			scroll_bar.Pivot = dfPivotPoint.TopLeft;
			scroll_bar.zindex = 7;
			scroll_bar.Size = new Vector2(21, 34);
			scroll_bar.Atlas = CachedOptionsAtlas;
			scroll_bar.Orientation = dfControlOrientation.Vertical;
			scroll_bar.MaxValue = 423;
			scroll_bar.StepSize = 1;
			scroll_bar.ScrollSize = 423;
			scroll_bar.IncrementAmount = 100;
			scroll_bar.AutoHide = true;
			scroll_bar.RelativePosition = CachedOptionsScrollbar.RelativePosition;

			var scrollbar_cursor_sprite = scroll_bar.AddControl<dfSprite>();
			scrollbar_cursor_sprite.Atlas = ((dfSprite)CachedOptionsScrollbar.Thumb).Atlas;
			scrollbar_cursor_sprite.zindex = 1;
			scrollbar_cursor_sprite.FillAmount = 1f;
			scrollbar_cursor_sprite.SpriteName = "scrollbar_cursor";
			scrollbar_cursor_sprite.Size = new Vector2(21, 45);
			scrollbar_cursor_sprite.MinimumSize = new Vector2(21, 45);
			scrollbar_cursor_sprite.Anchor = dfAnchorStyle.Top;
			scrollbar_cursor_sprite.IsVisible = true;
			scrollbar_cursor_sprite.RelativePosition = CachedOptionsScrollbar.Thumb.RelativePosition;
			scrollbar_cursor_sprite.Height = 45;

			var scrollbar_bar_sprite = scroll_bar.AddControl<dfSprite>();
			scrollbar_bar_sprite.Atlas = ((dfSprite)CachedOptionsScrollbar.Track).Atlas;
			scrollbar_bar_sprite.zindex = 0;
			scrollbar_bar_sprite.FillAmount = 1f;
			scrollbar_bar_sprite.SpriteName = "scrollbar_bar";
			scrollbar_bar_sprite.Size = new Vector2(9, 468);
			scrollbar_bar_sprite.Anchor = dfAnchorStyle.Right | dfAnchorStyle.CenterVertical;
			scrollbar_bar_sprite.IsVisible = true;
			scrollbar_bar_sprite.RelativePosition = CachedOptionsScrollbar.Track.RelativePosition;

			scroll_bar.Thumb = scrollbar_cursor_sprite;
			scroll_bar.Track = scrollbar_bar_sprite;

			var tab = central_panel.AddControl<dfScrollPanel>();


			tab.IsEnabled = true;
			tab.Anchor = dfAnchorStyle.Top | dfAnchorStyle.CenterHorizontal;
			tab.IsVisible = false;
			tab.IsInteractive = true;
			tab.Pivot = dfPivotPoint.TopCenter;
			tab.zindex = 4;
			tab.Size = new Vector2(830, 523);
			tab.ClipChildren = false;
			tab.InverseClipChildren = false;
			tab.TabIndex = -1;
			tab.HotZoneScale = new Vector2(1, 1);
			tab.AllowSignalEvents = true;
			tab.Atlas = CachedOptionsAtlas;
			tab.AutoLayout = true;
			tab.ScrollPadding = new RectOffset(0, 0, 250, 100);
			tab.AutoScrollPadding = new RectOffset(0, 0, 0, 15);
			tab.FlowPadding = new RectOffset(0, 0, 15, 0);
			tab.FlowDirection = global::dfScrollPanel.LayoutDirection.Vertical;
			tab.ScrollWheelAmount = 10;
			tab.WheelScrollDirection = dfControlOrientation.Vertical;
			tab.AutoFitVirtualTiles = true;
			tab.VertScrollbar = scroll_bar;


			// Dev note
			// This took 7 hours to debug to get the entries to not overflow the dialog
			// And ~10 hours total to get everything working right

			return tab;
		}

		public static dfButton CreateMenuButton(dfControl parent, string text, bool localize = false, bool act_as_label = false) {
			var button = parent.AddControl<dfButton>();
			button.IsLocalized = localize;
			button.Text = text;
			//button.Position = ((PreOptionsMenuController)PreOptionsMenu).TabModsSelector.Position;
			button.Font = GungeonFont;
			button.IsVisible = true;
			button.IsEnabled = true;
			button.TextColor = UI.MenuTextColor;
			button.FocusTextColor = act_as_label ? UI.MenuTextColor : UI.MenuFocusTextColor;
			button.HoverTextColor = act_as_label ? UI.MenuTextColor : UI.MenuHoverTextColor;
			button.TextScale = UI.MenuTextScale;
			button.TextAlignment = TextAlignment.Center;
			button.AutoSize = true;
			button.WordWrap = true;
			button.MaximumSize = new Vector2(parent.Width, 999999);

			

			Console.WriteLine($"XXXDEBUG button {button.Width} {button.Height}");
			Console.WriteLine($"XXXDEBUG parent {parent.Width}");


			Console.WriteLine($"XXXDEBUG newbutton {button.Width} {button.Height}");

			button.gameObject.SetActive(false); // Delay BraveOptionsMenuItem.Awake to allow us to set buttonControl
			var menu_item = button.gameObject.AddComponent<BraveOptionsMenuItem>();
			menu_item.buttonControl = button;
			menu_item.itemType = BraveOptionsMenuItem.BraveOptionsMenuItemType.Button;
			button.gameObject.SetActive(true);

			button.Anchor = dfAnchorStyle.CenterHorizontal;

			var ui_key_controls = button.gameObject.AddComponent<UIKeyControls>();

			var crosshair_selection_doer = button.gameObject.AddComponent<MenuCrosshairSelectionDoer>();
			crosshair_selection_doer.controlToPlace = CachedCrosshairSelectionDoerControl;
			crosshair_selection_doer.targetControlToEncrosshair = button;

			if (parent is dfScrollPanel) {
				var panel = (dfScrollPanel)parent;

				var diff = panel.FlowPadding.top + panel.FlowPadding.bottom + button.Height;
				//panel.Height += diff;
				panel.VertScrollbar.MaxValue += diff;
				//panel.VertScrollbar.ScrollSize += diff;

				Console.WriteLine($"height {panel.Height} max val {panel.VertScrollbar.MaxValue} scroll {panel.VertScrollbar.ScrollSize}");
			}

			return button;
		}

		public static dfPanel CreateMenuEntryBase(dfControl parent, BraveOptionsMenuItem.BraveOptionsMenuItemType type) {
			var top_panel = parent.AddControl<dfPanel>();
			top_panel.Anchor = (dfAnchorStyle)5;
			top_panel.Size = new Vector2(828, 39);
			top_panel.Atlas = CachedOptionsAtlas;
			top_panel.IsVisible = true;

			top_panel.gameObject.SetActive(false); // Delay BraveOptionsMenuItem.Awake to allow us to set buttonControl
			var menu_item = top_panel.gameObject.AddComponent<BraveOptionsMenuItem>();
			menu_item.itemType = type;
			top_panel.gameObject.SetActive(true);

			var ui_key_controls = top_panel.gameObject.AddComponent<UIKeyControls>();

			var crosshair_selection_doer = top_panel.gameObject.AddComponent<MenuCrosshairSelectionDoer>();
			crosshair_selection_doer.controlToPlace = CachedCrosshairSelectionDoerControl;
			crosshair_selection_doer.targetControlToEncrosshair = top_panel;

			var inside_panel = top_panel.AddControl<dfPanel>();

			inside_panel.Anchor = (dfAnchorStyle)65;
			inside_panel.IsVisible = true;
			inside_panel.Size = new Vector2(300, 39);
			inside_panel.Atlas = CachedOptionsAtlas;

			if (parent is dfScrollPanel) {
				var panel = (dfScrollPanel)parent;

				var diff = panel.FlowPadding.top + panel.FlowPadding.bottom + top_panel.Height;
				panel.VertScrollbar.MaxValue += diff;
			}

			return top_panel;
		}

		public static Patches.BraveOptionsMenuItem CreateMenuEntryCheckbox(dfControl parent, string text, bool localize = false) {
			var ent = parent.AddPrefab(CachedCheckboxMenuEntry.gameObject) as dfPanel;

			var checkbox = (Patches.dfCheckbox)ent.transform.GetChild(0).GetChild(1).gameObject.GetComponent<dfCheckbox>();
			checkbox.Label.Text = text;
			checkbox.Label.IsLocalized = localize;

			var menu_item = ent.GetComponent<BraveOptionsMenuItem>();
			menu_item.optionType = BraveOptionsMenuItem.BraveOptionsOptionType.NONE;


			return (Patches.BraveOptionsMenuItem)menu_item;
		}

		public static Patches.BraveOptionsMenuItem CreateMenuEntryOption(dfControl parent, string text, string[] entries, bool localize = false) {
			var ent = parent.AddPrefab(CachedOptionMenuEntry.gameObject) as dfPanel;

			var label = ent.transform.GetChild(0).GetChild(0).gameObject.GetComponent<dfLabel>();
			label.Text = text;
			label.IsLocalized = localize;

			var menu_item = ent.GetComponent<BraveOptionsMenuItem>();
			menu_item.labelOptions = entries;
			menu_item.optionType = BraveOptionsMenuItem.BraveOptionsOptionType.NONE;

			return (Patches.BraveOptionsMenuItem)menu_item;
		}

		public static dfTextbox TestTest;

		internal static IEnumerator CreateMenuEntryTextboxCoroutine(dfControl panel, dfControl parent, string text, bool localize = false) {
			Console.WriteLine($"XXXENTRY");

			var inner_panel = panel.transform.GetChild(0);

			var label = inner_panel.GetChild(0).gameObject.GetComponent<dfLabel>();
			label.IsLocalized = localize;
			label.Text = text;

			UnityEngine.Object.Destroy(inner_panel.GetChild(1)?.gameObject);
			UnityEngine.Object.Destroy(inner_panel.GetChild(2)?.gameObject);
			UnityEngine.Object.Destroy(inner_panel.GetChild(3)?.gameObject);

			Console.WriteLine($"XXXDESTROYED");

			yield return null;

			Console.WriteLine($"XXXNEXTFRAME");

			var textbox = inner_panel.gameObject.GetComponent<dfPanel>().AddControl<dfTextbox>();
			textbox.Font = GungeonFont;
			textbox.Color = Color.white;
			textbox.Width = 200;
			textbox.IsVisible = true;
			textbox.Height = 50;
			TestTest = textbox;

			var menu_item = panel.GetComponent<BraveOptionsMenuItem>();
			menu_item.itemType = (BraveOptionsMenuItem.BraveOptionsMenuItemType)Patches.BraveOptionsMenuItem.BraveOptionsMenuItemTypeExtended.SemiTextbox;
			menu_item.optionType = BraveOptionsMenuItem.BraveOptionsOptionType.NONE;
			menu_item.selectedLabelControl = null;
			((Patches.BraveOptionsMenuItem)menu_item).textboxControl = textbox;
		}

		public static dfPanel CreateMenuEntryTextbox(dfControl parent, string text, bool localize = false) {
			var ent = parent.AddPrefab(CachedOptionMenuEntry.gameObject) as dfPanel;

			CachedCentralPanel.StartCoroutine(CreateMenuEntryTextboxCoroutine(ent, parent, text, localize));

			return ent;
		}
	}
}
