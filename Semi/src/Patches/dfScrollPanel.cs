using System;
using MonoMod;
using UnityEngine;

namespace Semi.Patches {
	[MonoModPatch("global::dfScrollPanel")]
	public class dfScrollPanel : global::dfScrollPanel {
		private extern void orig_AutoArrange();

		private void AutoArrange() {
			orig_AutoArrange();

			SuspendLayout();
			try {
				for (int i = 0; i < controls.Count; i++) {
					dfControl dfControl = controls[i];
					if (dfControl && dfControl.IsVisible && dfControl.enabled && dfControl.gameObject.activeSelf) {
						if (!(dfControl == horzScroll) && !(dfControl == vertScroll)) {
							if (centerChildren) dfControl.RelativePosition = dfControl.RelativePosition.WithX(Width / 2f - dfControl.Width / 2f);

							if (dfControl is dfButton) {
								((dfButton)dfControl).AutoSize = false;
								((dfButton)dfControl).autoSizeToText();
							}
						}
					}
				}
			} finally {
				ResumeLayout();
			}
		}

		private bool centerChildren;

		public bool CenterChildren {
			get {
				return centerChildren;
			}
			set {
				if (value != centerChildren) {
					centerChildren = value;
					if (AutoReset || AutoLayout) {
						Reset();
					}
				}
			}
		}
	}
}
