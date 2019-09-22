using System;
using MonoMod;

namespace Semi.Patches {
	[MonoModPatch("global::dfCheckbox")]
	public class dfCheckbox : global::dfCheckbox {
		[MonoModIgnore]
		public new event PropertyChangedEventHandler<bool> CheckChanged;

		public void ResetCheckChanged() {
			CheckChanged = null;
		}
	}
}
