using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MonoMod;
using UnityEngine;

namespace Semi.Patches {
	[MonoModPatch("global::FullOptionsMenuController")]
	public class FullOptionsMenuController : global::FullOptionsMenuController {
		private extern IEnumerator orig_Start();
		public static FullOptionsMenuController Instance;
		public dfScrollPanel TabMods;
		public List<dfScrollPanel> ModSpecificTabs;
		internal bool IsInModOptions = false;

		private IEnumerator Start() {
			Instance = this;
			ModSpecificTabs = new List<dfScrollPanel>();

			UI.PrepareCustomOptions();

			var comps = TabGameplay.GetComponents<MonoBehaviour>();
			for (int i = 0; i < comps.Length; i++) {
				Console.WriteLine($"COMP {comps[i].GetType().FullName}");
			}

			var compsx = GameUIRoot.Instance.Manager.GetComponents<Component>();
			for (int i = 0; i < compsx.Length; i++) {
				Console.WriteLine($"COMPX {compsx[i].GetType().FullName}");
			}


			yield return null;

			TabMods = (dfScrollPanel)UI.CreateScrollPanel();

			AddSemiLabelButton();

			yield return orig_Start();

		}

		private void AddSemiLabelButton() {
			var label_button = UI.CreateMenuButton(TabMods, $"Semi Mod Loader v{SemiLoader.VERSION}", act_as_label: true);
			label_button.TextScale = 4;
		}

		public extern void orig_ToggleToPanel(dfScrollPanel targetPanel, bool doFocus = false);
		public void ToggleToPanel(dfScrollPanel targetPanel, bool doFocus = false) {
			TabMods.IsVisible = (targetPanel == TabMods);

			for (var i = 0; i < ModSpecificTabs.Count; i++) {
				if (ModSpecificTabs[i].IsVisible = (targetPanel == ModSpecificTabs[i])) {
					IsInModOptions = true;
				}
			}

			orig_ToggleToPanel(targetPanel, doFocus);
		}

		public void UpAllLevels() {
			InControlInputAdapter.CurrentlyUsingAllDevices = false;
			if (ModalKeyBindingDialog.IsVisible) {
				ClearModalKeyBindingDialog(null, null);
			} else {
				if (IsInModOptions) ToggleToPanel(TabMods);
				else PreOptionsMenu.ReturnToPreOptionsMenu();
			}

			IsInModOptions = false;
		}
	}
}
