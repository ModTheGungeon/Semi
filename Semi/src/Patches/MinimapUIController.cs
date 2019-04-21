//using System;
//using System.Collections.Generic;
//using MonoMod;
//using UnityEngine;

//namespace Semi.Patches {
//	/// <summary>
//	/// Patches MinimapUIController to add named ID support for synergies.
//	/// </summary>
//	[MonoModPatch("global::MinimapUIController")]
//	public class MinimapUIController : global::MinimapUIController {
//		[MonoModIgnore]
//		private int m_selectedDockItemIndex;

//		[MonoModIgnore]
//		private int m_targetDockIndex;

//		[MonoModIgnore]
//		private extern void DeselectDockItem();

//		[MonoModIgnore]
//		private List<Tuple<tk2dSprite, PassiveItem>> dockItems;

//		[MonoModIgnore]
//		private List<Tuple<tk2dSprite, PassiveItem>> secondaryDockItems;

//		[MonoModIgnore]
//		private extern void CreateArrow(tk2dBaseSprite targetSprite, dfControl targetParent);

//		private void SelectDockItem(int i, int targetPlayerID) {
//			if (m_selectedDockItemIndex == i && m_targetDockIndex == targetPlayerID) {
//				return;
//			}
//			DeselectDockItem();
//			List<Tuple<tk2dSprite, PassiveItem>> list = dockItems;
//			if (targetPlayerID == 1) {
//				list = secondaryDockItems;
//			}
//			if (i < list.Count) {
//				SpriteOutlineManager.AddOutlineToSprite(list[i].First, Color.white);
//				tk2dSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites(list[i].First);
//				for (int j = 0; j < outlineSprites.Length; j++) {
//					outlineSprites[j].scale = list[i].First.scale;
//				}
//			}
//			m_targetDockIndex = targetPlayerID;
//			m_selectedDockItemIndex = i;
//			PassiveItem second = list[i].Second;
//			DropItemButton.IsEnabled = second.CanActuallyBeDropped(second.Owner);
//			DropItemSprite.Color = ((!DropItemButton.IsEnabled) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white);
//			DropItemLabel.Color = ((!DropItemButton.IsEnabled) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white);
//			DropItemSpriteForeign.Color = ((!DropItemButton.IsEnabled) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white);
//			DropItemLabelForeign.Color = ((!DropItemButton.IsEnabled) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white);
//			if (second) {
//				UpdateSynergyHighlights(((PickupObject)(object)second).UniqueItemID);
//			}
//		}

//		private void UpdateSynergyHighlights(string selected_id) {
//			AdvancedSynergyDatabase synergyManager = global::GameManager.Instance.SynergyManager;
//			dfControl rootContainer = DockSprite.GetRootContainer();
//			rootContainer.BringToFront();
//			for (int i = 0; i < global::GameManager.Instance.AllPlayers.Length; i++) {
//				PlayerController playerController = global::GameManager.Instance.AllPlayers[i];
//				for (int j = 0; j < synergyManager.synergies.Length; j++) {
//					if (playerController.ActiveExtraSynergies.Contains(j)) {
//						AdvancedSynergyEntry entry = (AdvancedSynergyEntry)synergyManager.synergies[j];
//						if (entry.ContainsPickup(selected_id)) {
//							for (int k = 0; k < dockItems.Count; k++) {
//								var pickup_object_id = ((PickupObject)(object)dockItems[k].Second).UniqueItemID;
//								if (pickup_object_id != selected_id && entry.ContainsPickup(pickup_object_id)) {
//									SpriteOutlineManager.AddOutlineToSprite(dockItems[k].First, SynergyDatabase.SynergyBlue);
//									CreateArrow(dockItems[k].First, rootContainer);
//								}
//							}
//							for (int l = 0; l < secondaryDockItems.Count; l++) {
//								var pickup_object_id = ((PickupObject)(object)secondaryDockItems[l].Second).UniqueItemID;
//								if (pickup_object_id != selected_id && entry.ContainsPickup(pickup_object_id)) {
//									SpriteOutlineManager.AddOutlineToSprite(secondaryDockItems[l].First, SynergyDatabase.SynergyBlue);
//									CreateArrow(secondaryDockItems[l].First, rootContainer);
//								}
//							}
//							for (int m = 0; m < playerController.inventory.AllGuns.Count; m++) {
//								var pickup_object_id = ((PickupObject)(object)playerController.inventory.AllGuns[m]).UniqueItemID;
//								if (pickup_object_id != selected_id && entry.ContainsPickup(pickup_object_id)) {
//									int num = playerController.inventory.AllGuns.IndexOf(playerController.CurrentGun);
//									int gunIndex = playerController.inventory.AllGuns.Count - (num - m + playerController.inventory.AllGuns.Count - 1) % playerController.inventory.AllGuns.Count - 1;
//									tk2dClippedSprite spriteForUnfoldedGun = GameUIRoot.Instance.GetSpriteForUnfoldedGun(playerController.PlayerIDX, gunIndex);
//									if (spriteForUnfoldedGun) {
//										SpriteOutlineManager.RemoveOutlineFromSprite(spriteForUnfoldedGun, false);
//										SpriteOutlineManager.AddOutlineToSprite<tk2dClippedSprite>(spriteForUnfoldedGun, SynergyDatabase.SynergyBlue);
//										CreateArrow(spriteForUnfoldedGun, spriteForUnfoldedGun.transform.parent.parent.GetComponent<dfControl>());
//									}
//								}
//							}
//							for (int n = 0; n < playerController.activeItems.Count; n++) {
//								var pickup_object_id = ((PickupObject)(object)playerController.activeItems[n]).UniqueItemID;
//								if (pickup_object_id != selected_id && entry.ContainsPickup(pickup_object_id)) {
//									int num2 = playerController.activeItems.IndexOf(playerController.CurrentItem);
//									int itemIndex = playerController.activeItems.Count - (num2 - n + playerController.activeItems.Count - 1) % playerController.activeItems.Count - 1;
//									tk2dClippedSprite spriteForUnfoldedItem = GameUIRoot.Instance.GetSpriteForUnfoldedItem(playerController.PlayerIDX, itemIndex);
//									if (spriteForUnfoldedItem) {
//										SpriteOutlineManager.RemoveOutlineFromSprite(spriteForUnfoldedItem, false);
//										SpriteOutlineManager.AddOutlineToSprite<tk2dClippedSprite>(spriteForUnfoldedItem, SynergyDatabase.SynergyBlue);
//										CreateArrow(spriteForUnfoldedItem, spriteForUnfoldedItem.transform.parent.parent.GetComponent<dfControl>());
//									}
//								}
//							}
//						}
//					}
//				}
//			}
//		}
//	}
//}
