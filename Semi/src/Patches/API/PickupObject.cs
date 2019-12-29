using System;
using MonoMod;

namespace Semi.Patches {
	/// <summary>
	/// Patches PickupObject to add a UniqueItemID string field for the Semi item ID.
	/// This is a named ID equivalent for the numeric PickupObjectId field, which shouldn't be used.
	/// </summary>
	[MonoModPatch("global::PickupObject")]
	public class PickupObject : global::PickupObject {
		public ID UniqueItemID;

		[MonoModIgnore]
		public override void Pickup(PlayerController player) {
			// I don't need this lol
		}
	}
}
