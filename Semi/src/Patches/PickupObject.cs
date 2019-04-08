using System;
using MonoMod;

namespace Semi.Patches {
	[MonoModPatch("global::PickupObject")]
	public class PickupObject : global::PickupObject {
		public string UniqueItemID;

		[MonoModIgnore]
		public override void Pickup(PlayerController player) {
			// I don't need this lol
		}
	}
}
