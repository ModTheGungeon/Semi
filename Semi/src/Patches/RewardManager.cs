//using System;
//using MonoMod;

//namespace Semi.Patches {
//	/// <summary>
//	/// Patches RewardManager to use named IDs for synergies.
//	/// </summary>
//	[MonoModPatch("global::RewardManager")]
//	public class RewardManager {
//		public static extern bool orig_PlayerHasItemInSynergyContainingOtherItem(PlayerController player, PickupObject prefab, ref bool usesStartingItem);
//		public static bool PlayerHasItemInSynergyContainingOtherItem(PlayerController player, PickupObject prefab, ref bool usesStartingItem) {
//			var id = prefab.UniqueItemID;

//			var synergies = (AdvancedSynergyEntry[])global::GameManager.Instance.SynergyManager.synergies;
//			for (int i = 0; i < synergies.Length; i++) {
//				var entry = synergies[i];
//				if (entry.IsActive) {
//					if (entry.ContainsPickup(id)) {
//						bool item_synergized = false;
//						for (int j = 0; j < player.inventory.AllGuns.Count; j++) {
//							item_synergized = entry.ContainsPickup(((PickupObject)(object)player.inventory.AllGuns[j]).UniqueItemID);
//							if (item_synergized) {
//								item_synergized = global::RewardManager.TestItemWouldCompleteSpecificSynergy(entry, prefab);
//								usesStartingItem |= player.startingGunIds.Contains(player.inventory.AllGuns[j].PickupObjectId);
//								usesStartingItem |= player.startingAlternateGunIds.Contains(player.inventory.AllGuns[j].PickupObjectId);
//							}
//						}
//						if (!item_synergized) {
//							for (int k = 0; k < player.activeItems.Count; k++) {
//								item_synergized = entry.ContainsPickup(((PickupObject)(object)player.activeItems[k]).UniqueItemID);
//								if (item_synergized) {
//									item_synergized = global::RewardManager.TestItemWouldCompleteSpecificSynergy(entry, prefab);
//									usesStartingItem |= player.startingActiveItemIds.Contains(player.activeItems[k].PickupObjectId);
//								}
//							}
//						}
//						if (!item_synergized) {
//							for (int l = 0; l < player.passiveItems.Count; l++) {
//								item_synergized = entry.ContainsPickup(((PickupObject)(object)player.passiveItems[l]).UniqueItemID);
//								if (item_synergized) {
//									item_synergized = global::RewardManager.TestItemWouldCompleteSpecificSynergy(entry, prefab);
//									usesStartingItem |= player.startingPassiveItemIds.Contains(player.passiveItems[l].PickupObjectId);
//								}
//							}
//						}
//						//TODO @ids still uses numeric
//						if (!item_synergized && SynercacheManager.UseCachedSynergyIDs) {
//							for (int m = 0; m < SynercacheManager.LastCachedSynergyIDs.Count; m++) {
//								item_synergized |= entry.ContainsPickup(SynercacheManager.LastCachedSynergyIDs[m]);
//								item_synergized |= entry.ContainsPickup(SynercacheManager.LastCachedSynergyIDs[m]);
//							}
//						}
//						if (item_synergized) return true;
//					}
//				}
//			}
//			return false;
//		}
//	}
//}
