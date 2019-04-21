using System;
using System.Collections.Generic;
using MonoMod;
using UnityEngine;

namespace Semi.Patches {
	/// <summary>
	/// Patches AdvancedSynergyEntry to use named IDs for synergies.
	/// </summary>
	[MonoModPatch("global::AdvancedSynergyEntry")]
	public class AdvancedSynergyEntry : global::AdvancedSynergyEntry {
		// TODO @ids make everything actually use named IDs as originally intended instead of this workaround

		[MonoModIgnore]
		private extern bool PlayerHasSynergyCompletionItem(PlayerController p);

		[MonoModIgnore]
		[Obsolete("Use OptionalGuns")]
		public new List<int> OptionalGunIDs;

		[MonoModIgnore]
		[Obsolete("Use MandatoryGuns")]
		public new List<int> MandatoryGunIDs;

		[MonoModIgnore]
		[Obsolete("Use OptionalItems")]
		public new List<int> OptionalItemIDs;

		[MonoModIgnore]
		[Obsolete("Use MandatoryItems")]
		public new List<int> MandatoryItemIDs;

		public string[] OptionalGuns;
		public string[] MandatoryGuns;
		public string[] OptionalItems;
		public string[] MandatoryItems;
		public string UniqueID;

		public bool IsActive {
			get {
				return ActivationStatus != SynergyEntry.SynergyActivation.ACTIVE_UNBOOSTED && ActivationStatus != SynergyEntry.SynergyActivation.INACTIVE && ActivationStatus != SynergyEntry.SynergyActivation.DEMO;
			}
		}

		public extern bool orig_ContainsPickup(int id);
		public new bool ContainsPickup(int id) {
			for (int i = 0; i < OptionalGuns.Length; i++) {
				if (Gungeon.Items[OptionalGuns[i]].PickupObjectId == id) return true;
			}
			for (int i = 0; i < MandatoryGuns.Length; i++) {
				if (Gungeon.Items[MandatoryGuns[i]].PickupObjectId == id) return true;
			}
			for (int i = 0; i < OptionalItems.Length; i++) {
				if (Gungeon.Items[OptionalItems[i]].PickupObjectId == id) return true;
			}
			for (int i = 0; i < MandatoryItems.Length; i++) {
				if (Gungeon.Items[MandatoryItems[i]].PickupObjectId == id) return true;
			}
			return false;
		}

		private extern bool orig_PlayerHasPickup(PlayerController p, int pickupID);
		private bool PlayerHasPickup(PlayerController p, int pickupID) {
			return orig_PlayerHasPickup(p, pickupID);
		}

		public bool ContainsPickup(string pickup_id) {
			pickup_id = IDPool<PickupObject>.Resolve(pickup_id);
			return OptionalGuns.Contains(pickup_id) || MandatoryGuns.Contains(pickup_id) || OptionalItems.Contains(pickup_id) || MandatoryItems.Contains(pickup_id);
		}

		public bool PlayerHasPickup(PlayerController p, string pickup_id) {
			//TODO @ids patch player to use named IDs as well, for now this is a workaround
			if (!Gungeon.Items.ContainsID(pickup_id)) throw new ArgumentException(nameof(pickup_id));
			var id = Gungeon.Items[pickup_id].PickupObjectId;
			return orig_PlayerHasPickup(p, id);
		}

		public new bool SynergyIsActive(PlayerController p, PlayerController p2) {
			bool flag = MandatoryGuns.Length > 0 || (RequiresAtLeastOneGunAndOneItem && OptionalGuns.Length > 0);
			return !flag || ActiveWhenGunUnequipped || (p && p.CurrentGun && (
				MandatoryGuns.Contains(((PickupObject)(object)p.CurrentGun).UniqueItemID) ||
				OptionalGuns.Contains(((PickupObject)(object)p.CurrentGun).UniqueItemID)
			))|| (p2 && p2.CurrentGun && (
				MandatoryGuns.Contains(((PickupObject)(object)p2.CurrentGun).UniqueItemID) ||
				OptionalGuns.Contains(((PickupObject)(object)p2.CurrentGun).UniqueItemID)
			)) || (p && p.CurrentSecondaryGun && (
				MandatoryGuns.Contains(((PickupObject)(object)p.CurrentSecondaryGun).UniqueItemID) ||
				OptionalGuns.Contains(((PickupObject)(object)p.CurrentSecondaryGun).UniqueItemID)
			)) || (p2 && p2.CurrentSecondaryGun && (
				MandatoryGuns.Contains(((PickupObject)(object)p2.CurrentSecondaryGun).UniqueItemID) ||
				OptionalGuns.Contains(((PickupObject)(object)p2.CurrentSecondaryGun).UniqueItemID)
			));
		}

		public bool SynergyIsAvailable(PlayerController p, PlayerController p2, int additionalID = -1) {
			if (ActivationStatus == SynergyEntry.SynergyActivation.INACTIVE) {
				return false;
			}
			if (ActivationStatus == SynergyEntry.SynergyActivation.DEMO) {
				return false;
			}
			
			bool flag = PlayerHasSynergyCompletionItem(p) || PlayerHasSynergyCompletionItem(p2);
			if (IgnoreLichEyeBullets) {
				flag = false;
			}
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < MandatoryGuns.Length; i++) {
				if (PlayerHasPickup(p, MandatoryGuns[i]) || PlayerHasPickup(p2, MandatoryGuns[i]) || Gungeon.Items[MandatoryGuns[i]]?.PickupObjectId == additionalID) {
					num++;
				}
			}
			for (int j = 0; j < MandatoryItems.Length; j++) {
				if (PlayerHasPickup(p, MandatoryItems[j]) || PlayerHasPickup(p2, MandatoryItems[j]) || Gungeon.Items[MandatoryItems[j]]?.PickupObjectId == additionalID) {
					num2++;
				}
			}
			int num3 = 0;
			int num4 = 0;
			for (int k = 0; k < OptionalGuns.Length; k++) {
				if (PlayerHasPickup(p, OptionalGuns[k]) || PlayerHasPickup(p2, OptionalGuns[k]) || Gungeon.Items[OptionalGuns[k]]?.PickupObjectId == additionalID) {
					num3++;
				}
			}
			for (int l = 0; l < OptionalItems.Length; l++) {
				if (PlayerHasPickup(p, OptionalItems[l]) || PlayerHasPickup(p2, OptionalItems[l]) || Gungeon.Items[OptionalItems[l]]?.PickupObjectId == additionalID) {
					num4++;
				}
			}
			bool flag2 = MandatoryItems.Length > 0 && MandatoryGuns.Length == 0 && OptionalGuns.Length > 0 && OptionalItems.Length == 0;
			if (((MandatoryGuns.Length > 0 && num > 0) || (flag2 && num3 > 0)) && flag) {
				num++;
				num2++;
			}
			if (num < MandatoryGuns.Length || num2 < MandatoryItems.Length) {
				return false;
			}
			int num5 = MandatoryItems.Length + MandatoryGuns.Length + num3 + num4;
			int num6 = MandatoryGuns.Length + num3;
			int num7 = MandatoryItems.Length + num4;
			if (num6 > 0 && (MandatoryGuns.Length > 0 || flag2 || (RequiresAtLeastOneGunAndOneItem && num6 > 0)) && flag) {
				num7++;
				num6++;
				num5 += 2;
			}
			if (RequiresAtLeastOneGunAndOneItem && OptionalGuns.Length + MandatoryGuns.Length > 0 && OptionalItems.Length + MandatoryItems.Length > 0 && (num6 < 1 || num7 < 1)) {
				return false;
			}
			int num8 = Mathf.Max(2, NumberObjectsRequired);
			return num5 >= num8;
		}

	}
}
