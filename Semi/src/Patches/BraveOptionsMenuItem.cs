using System;
using MonoMod;

namespace Semi.Patches {
	[MonoModPatch("global::BraveOptionsMenuItem")]
	public class BraveOptionsMenuItem : global::BraveOptionsMenuItem {
		public dfTextbox textboxControl;
		public Action<bool> CheckboxChanged;
		public Action<string> SelectionChanged;
		private int m_selectedIndex;

		public extern void orig_Awake();

		public void Awake() {
			orig_Awake();
			if (itemType == (BraveOptionsMenuItemType)BraveOptionsMenuItemTypeExtended.SemiTextbox && textboxControl != null) {
				//textboxControl.TextChanged += (self, ev) => HandleValueChanged();
			}
		}

		private extern void orig_ToggleCheckbox(dfControl control, dfMouseEventArgs args);

		private void ToggleCheckbox(dfControl control, dfMouseEventArgs args) {
            if (CheckboxChanged != null) CheckboxChanged.Invoke(m_selectedIndex != 1);
			orig_ToggleCheckbox(control, args);
		}

		private extern void orig_IncrementArrow(dfControl control, dfMouseEventArgs mouseEvent);

		private void IncrementArrow(dfControl control, dfMouseEventArgs mouseEvent) {
			if (SelectionChanged != null) SelectionChanged.Invoke(labelOptions[m_selectedIndex]);
			orig_IncrementArrow(control, mouseEvent);
		}

		private extern void orig_DecrementArrow(dfControl control, dfMouseEventArgs mouseEvent);

		private void DecrementArrow(dfControl control, dfMouseEventArgs mouseEvent) {
            if (SelectionChanged != null) SelectionChanged.Invoke(labelOptions[m_selectedIndex]);
			orig_DecrementArrow(control, mouseEvent);
		}

		public enum BraveOptionsMenuItemTypeExtended {
			LeftRightArrow,
			LeftRightArrowInfo,
			Fillbar,
			Checkbox,
			Button,

			SemiTextbox
		}
	}
}
