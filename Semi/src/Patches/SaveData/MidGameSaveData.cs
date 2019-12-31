using System;
using MonoMod;
using FullSerializer;
using System.Collections.Generic;
using UnityEngine;

namespace Semi.Patches {
	/// <summary>
	/// Patches MidGameSaveData to support loading Semi midgame saves (e.g. with modded item IDs)
	/// </summary>
	[MonoModPatch("global::MidGameSaveData")]
	public class MidGameSaveData : global::MidGameSaveData {
		[fsProperty]
		public bool modded = true;

		public void InitializePlayerData(PlayerController p1, MidGamePlayerData playerData, bool isPlayerOne) {
			IsInitializingPlayerData = true;

			p1.MasteryTokensCollectedThisRun = playerData.MasteryTokensCollected;
			p1.CharacterUsesRandomGuns = playerData.CharacterUsesRandomGuns;
			p1.HasTakenDamageThisRun = playerData.HasTakenDamageThisRun;
			p1.HasFiredNonStartingGun = playerData.HasFiredNonStartingGun;

			ChallengeManager.ChallengeModeType = playerData.ChallengeMode;

			if (levelSaved == GlobalDungeonData.ValidTilesets.FINALGEON) {
				p1.CharacterUsesRandomGuns = false;
			}
			if (levelSaved != GlobalDungeonData.ValidTilesets.FINALGEON || !(p1 is PlayerSpaceshipController)) {
				p1.inventory.DestroyAllGuns();
				p1.RemoveAllPassiveItems();
				p1.RemoveAllActiveItems();

				if (playerData.passiveItems != null) {
					for (int i = 0; i < playerData.passiveItems.Count; i++) {
						var item = (MidGamePassiveItemData)(object)playerData.passiveItems[i];

						EncounterTrackable.SuppressNextNotification = true;
						LootEngine.GivePrefabToPlayer(Registry.Items[item.ItemID].gameObject, p1);
					}
				}
				if (playerData.activeItems != null) {
					for (int j = 0; j < playerData.activeItems.Count; j++) {
						var item = (MidGameActiveItemData)(object)playerData.activeItems[j];

						EncounterTrackable.SuppressNextNotification = true;
						LootEngine.GivePrefabToPlayer(Registry.Items[item.ItemID].gameObject, p1);
					}
				}
				if (playerData.guns != null) {
					for (int k = 0; k < playerData.guns.Count; k++) {
						var gun = (MidGameGunData)(object)playerData.guns[k];
						
						EncounterTrackable.SuppressNextNotification = true;
						LootEngine.GivePrefabToPlayer(Registry.Items[gun.ItemID].gameObject, p1);
					}
					for (int l = 0; l < playerData.guns.Count; l++) {
						var savegun = (MidGameGunData)(object)playerData.guns[l];
						
						for (int m = 0; m < p1.inventory.AllGuns.Count; m++) {
							var invgun = (PickupObject)(global::PickupObject)p1.inventory.AllGuns[m];
							if (invgun.UniqueItemID == savegun.ItemID) {
								p1.inventory.AllGuns[m].MidGameDeserialize(playerData.guns[l].SerializedData);
								for (int n = 0; n < playerData.guns[l].DuctTapedGunIDs.Count; n++) {
									Gun gun = PickupObjectDatabase.GetById(playerData.guns[l].DuctTapedGunIDs[n]) as Gun;
									// TODO @save here's the call site for the duct tape id thing, fix to use a string list eventually
									if (gun) {
										DuctTapeItem.DuctTapeGuns(gun, p1.inventory.AllGuns[m]);
									}
								}
								p1.inventory.AllGuns[m].ammo = playerData.guns[l].CurrentAmmo;
							}
						}
					}
				}
				if (playerData.CurrentHealth <= 0f && playerData.CurrentArmor <= 0f) {
					p1.healthHaver.Armor = 0f;
					p1.DieOnMidgameLoad();
				} else {
					p1.healthHaver.ForceSetCurrentHealth(playerData.CurrentHealth);
					p1.healthHaver.Armor = playerData.CurrentArmor;
				}
				if (isPlayerOne) {
					p1.carriedConsumables.KeyBullets = playerData.CurrentKeys;
					p1.carriedConsumables.Currency = playerData.CurrentCurrency;
				}
				p1.Blanks = Mathf.Max(p1.Blanks, playerData.CurrentBlanks);
				if (playerData.activeItems != null) {
					for (int num = 0; num < playerData.activeItems.Count; num++) {
						for (int num2 = 0; num2 < p1.activeItems.Count; num2++) {
							if (playerData.activeItems[num].PickupID == p1.activeItems[num2].PickupObjectId) {
								p1.activeItems[num2].MidGameDeserialize(playerData.activeItems[num].SerializedData);
								p1.activeItems[num2].CurrentDamageCooldown = playerData.activeItems[num].DamageCooldown;
								p1.activeItems[num2].CurrentRoomCooldown = playerData.activeItems[num].RoomCooldown;
								p1.activeItems[num2].CurrentTimeCooldown = playerData.activeItems[num].TimeCooldown;
								if (p1.activeItems[num2].consumable && playerData.activeItems[num].NumberOfUses > 0) {
									p1.activeItems[num2].numberOfUses = playerData.activeItems[num].NumberOfUses;
								}
							}
						}
					}
				}
				if (playerData.passiveItems != null) {
					for (int num3 = 0; num3 < playerData.passiveItems.Count; num3++) {
						for (int num4 = 0; num4 < p1.passiveItems.Count; num4++) {
							if (playerData.passiveItems[num3].PickupID == p1.passiveItems[num4].PickupObjectId) {
								p1.passiveItems[num4].MidGameDeserialize(playerData.passiveItems[num3].SerializedData);
							}
						}
					}
				}
				if (playerData.ownerlessStatModifiers != null) {
					if (p1.ownerlessStatModifiers == null) {
						p1.ownerlessStatModifiers = new List<StatModifier>();
					}
					for (int num5 = 0; num5 < playerData.ownerlessStatModifiers.Count; num5++) {
						p1.ownerlessStatModifiers.Add(playerData.ownerlessStatModifiers[num5]);
					}
				}
				if (levelSaved == GlobalDungeonData.ValidTilesets.FINALGEON) {
					p1.ResetToFactorySettings(true, true, false);
				}
				if (p1 && p1.stats != null) {
					p1.stats.RecalculateStats(p1, false, false);
				}
				if (playerData.HasBloodthirst) {
					p1.gameObject.GetOrAddComponent<Bloodthirst>();
				}
			}
			IsInitializingPlayerData = false;
			EncounterTrackable.SuppressNextNotification = false;
		}
	}

	//[MonoModPatch("global::MidGamePlayerData")]
	//public class MidGamePlayerData {}
	//
	// not necessary - patches below expose the same API

	/// <summary>
	/// Patches MidGamePassiveItemData to add a string ItemID field.
	/// Ignores numeric PickupObjectId field.
	/// </summary>
	[MonoModPatch("global::MidGamePassiveItemData")]
	public class MidGamePassiveItemData {
		public ID ItemID;
		public List<object> SerializedData;

		public extern void orig_ctor(PassiveItem p);

		[MonoModLinkTo("System.Object", ".ctor")]
		[MonoModForceCall]
		[MonoModRemove]
		public extern void object_ctor();

		[MonoModReplace]
		[MonoModConstructor]
		public void ctor(PassiveItem p) {
			object_ctor();

			ItemID = ((Semi.Patches.PickupObject)(global::PickupObject)p).UniqueItemID;
			SerializedData = new List<object>();
			p.MidGameSerialize(SerializedData);
		}
	}

	/// <summary>
	/// Patches MidGameActiveItemData to add a string ItemID field.
	/// Ignores numeric PickupObjectId field.
	/// </summary>
	[MonoModPatch("global::MidGameActiveItemData")]
	public class MidGameActiveItemData {
		public ID ItemID;
		public bool IsOnCooldown;
		public float DamageCooldown;
		public int RoomCooldown;
		public float TimeCooldown;
		public int NumberOfUses;
		public List<object> SerializedData;

		public extern void orig_ctor(PlayerItem p);

		[MonoModLinkTo("System.Object", ".ctor")]
		[MonoModForceCall]
		[MonoModRemove]
		public extern void object_ctor();

		[MonoModReplace]
		[MonoModConstructor]
		public void ctor(PlayerItem p) {
			object_ctor();

			ItemID = ((Semi.Patches.PickupObject)(global::PickupObject)p).UniqueItemID;
			IsOnCooldown = p.IsOnCooldown;
			DamageCooldown = p.CurrentDamageCooldown;
			RoomCooldown = p.CurrentRoomCooldown;
			TimeCooldown = p.CurrentTimeCooldown;
			NumberOfUses = p.numberOfUses;
			SerializedData = new List<object>();
			p.MidGameSerialize(SerializedData);
		}
	}

	/// <summary>
	/// Patches MidGameGunData to add a string ItemID field.
	/// Ignores numeric PickupObjectId field.
	/// </summary>
	[MonoModPatch("global::MidGameGunData")]
	public class MidGameGunData {
		public ID ItemID;
		public int CurrentAmmo = -1;
		public List<object> SerializedData;
		public List<int> DuctTapedGunIDs;
		// TODO @save this still uses numeric IDs

		public extern void orig_ctor(Gun g);

		[MonoModLinkTo("System.Object", ".ctor")]
		[MonoModForceCall]
		[MonoModRemove]
		public extern void object_ctor();

		[MonoModReplace]
		[MonoModConstructor]
		public void ctor(Gun g) {
			object_ctor();

			ItemID = ((Semi.Patches.PickupObject)(global::PickupObject)g).UniqueItemID;
			CurrentAmmo = g.CurrentAmmo;
			SerializedData = new List<object>();
			g.MidGameSerialize(SerializedData);
			DuctTapedGunIDs = new List<int>();
			if (g.DuctTapeMergedGunIDs != null) {
				DuctTapedGunIDs.AddRange(g.DuctTapeMergedGunIDs);
			}
		}
	}

}
