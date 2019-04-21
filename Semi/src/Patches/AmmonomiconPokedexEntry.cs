//using System;
//using System.Collections.Generic;
//using MonoMod;
//using UnityEngine;

//namespace Semi.Patches {
//	/// <summary>
//	/// Patches AmmonomiconPokedexEntry to support named IDs for synergies.
//	/// </summary>
//	[MonoModPatch("global::AmmonomiconPokedexEntry")]
//	public class AmmonomiconPokedexEntry : global::AmmonomiconPokedexEntry {
//		[MonoModIgnore]
//		private tk2dClippedSprite m_childSprite;

//		[MonoModIgnore]
//		private List<tk2dClippedSprite> extantSynergyArrows;

//		[MonoModIgnore]
//		private dfSlicedSprite m_bgSprite;

//		// TODO @ids make this an actual field that's set at the same time pickupID is?
//		public string UniqueItemID {
//			get { return ((PickupObject)(object)PickupObjectDatabase.Instance.Objects[pickupID]).UniqueItemID; }
//		}

//		private void UpdateSynergyHighlights() {
//			if (global::GameManager.Instance.IsSelectingCharacter) {
//				return;
//			}
//			List<AmmonomiconPokedexEntry> pokedexEntries = (List<AmmonomiconPokedexEntry>)(object)AmmonomiconController.Instance.BestInteractingLeftPageRenderer.GetPokedexEntries();
//			List<AmmonomiconPokedexEntry> list = new List<AmmonomiconPokedexEntry>();
//			if (this.activeSynergies != null) {
//				for (int i = 0; i < this.activeSynergies.Count; i++) {
//					PlayerController playerController = global::GameManager.Instance.BestActivePlayer;
//					for (int j = 0; j < global::GameManager.Instance.AllPlayers.Length; j++) {
//						if (global::GameManager.Instance.AllPlayers[j].PlayerIDX == global::GameManager.Instance.LastPausingPlayerID) {
//							playerController = global::GameManager.Instance.AllPlayers[j];
//						}
//					}
//					AdvancedSynergyEntry entry = (AdvancedSynergyEntry)activeSynergies[i];
//					if (entry.ContainsPickup(UniqueItemID) && pokedexEntries != null) {
//						for (int k = 0; k < pokedexEntries.Count; k++) {
//							if (pokedexEntries[k].pickupID >= 0 && pokedexEntries[k].pickupID != this.pickupID) {
//								if (entry.ContainsPickup(pokedexEntries[k].UniqueItemID)) {
//									tk2dClippedSprite tk2dClippedSprite = AmmonomiconController.Instance.CurrentLeftPageRenderer.AddSpriteToPage<tk2dClippedSprite>(AmmonomiconController.Instance.EncounterIconCollection, AmmonomiconController.Instance.EncounterIconCollection.GetSpriteIdByName("synergy_ammonomicon_arrow_001"));
//									tk2dClippedSprite.SetSprite("synergy_ammonomicon_arrow_001");
//									Bounds bounds = pokedexEntries[k].m_childSprite.GetBounds();
//									Bounds untrimmedBounds = pokedexEntries[k].m_childSprite.GetUntrimmedBounds();
//									Vector3 size = bounds.size;
//									tk2dClippedSprite.transform.position = (pokedexEntries[k].m_childSprite.WorldCenter.ToVector3ZisY(0f) + new Vector3(-8f * pokedexEntries[k].m_bgSprite.PixelsToUnits(), size.y / 2f + 32f * pokedexEntries[k].m_bgSprite.PixelsToUnits(), 0f)).WithZ(-0.65f);
//									tk2dClippedSprite.transform.parent = this.m_childSprite.transform.parent;
//									this.extantSynergyArrows.Add(tk2dClippedSprite);
//									pokedexEntries[k].ChangeOutlineColor(SynergyDatabase.SynergyBlue);
//									list.Add(pokedexEntries[k]);
//								}
//							}
//						}
//					}
//				}
//				if (pokedexEntries != null) {
//					for (int l = 0; l < pokedexEntries.Count; l++) {
//						if (pokedexEntries[l] != this && !list.Contains(pokedexEntries[l]) && SpriteOutlineManager.HasOutline(pokedexEntries[l].m_childSprite)) {
//							SpriteOutlineManager.RemoveOutlineFromSprite(pokedexEntries[l].m_childSprite, true);
//							SpriteOutlineManager.AddScaledOutlineToSprite<tk2dClippedSprite>(pokedexEntries[l].m_childSprite, Color.black, 0.1f, 0.05f);
//						}
//					}
//				}
//			}
//		}
//	}
//}
