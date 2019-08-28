using System;

namespace Semi {
	public static class PickupObjectExt {
		public static string GetUniqueItemID(this PickupObject pickup) {
			return ((Patches.PickupObject)(object)pickup).UniqueItemID;
		}
	}
}
