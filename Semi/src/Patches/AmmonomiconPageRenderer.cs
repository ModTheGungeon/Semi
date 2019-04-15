using System;
using System.Collections.Generic;
using MonoMod;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Semi {
	/// <summary>
	/// Debug patch, will be removed.
	/// </summary>
	[MonoModPatch("global::AmmonomiconPageRenderer")]
	public class AmmonomiconPageRenderer : global::AmmonomiconPageRenderer {
		[MonoModLinkTo("global::AmmonomiconPageRenderer", "RectangleLineInfo")]
		[MonoModIgnore]
		public struct RectangleLineInfo {
			public int numberOfElements;

			public float lineHeightUnits;

			public float initialXOffset;
		}

		[MonoModIgnore]
		public List<dfButton> m_prevLineButtons;
		[MonoModIgnore]
		public List<AmmonomiconPokedexEntry> m_pokedexEntries;

		public extern void orig_InitializeGunsPageLeft();

		public void InitializeGunsPageLeft() {
			var def = AmmonomiconController.ForceInstance.EncounterIconCollection.GetSpriteDefinition("example_mod:clown");
			if (Gungeon.Items != null) {
				var rogue_special = Gungeon.Items["gungeon:rogue_special"];
			}
			orig_InitializeGunsPageLeft();

			var def_after = AmmonomiconController.ForceInstance.EncounterIconCollection.GetSpriteDefinition("example_mod:clown");
			if (Gungeon.Items != null) {
				var rogue_special = Gungeon.Items["gungeon:rogue_special"];
			}
		}
	}

	[MonoModPatch("global::AmmonomiconPageRenderer/<ConstructRectanglePageLayout>c__Iterator2")]
	public class ConstructRectanglePageLayoutIterator2 {
		[MonoModIgnore]
		internal bool hideButtons;

		internal int d_PC {
			get {
				return (int)typeof(ConstructRectanglePageLayoutIterator2).GetField("$PC", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this);
			}
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("$PC", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal bool d_disposing {
			get { return (bool)typeof(ConstructRectanglePageLayoutIterator2).GetField("$disposing", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("$disposing", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal object d_current {
			get { return (int)typeof(ConstructRectanglePageLayoutIterator2).GetField("$current", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("$current", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal AmmonomiconPageRenderer d_this {
			get { return (AmmonomiconPageRenderer)typeof(ConstructRectanglePageLayoutIterator2).GetField("$this", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("$this", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		[MonoModIgnore]
		internal List<AdvancedSynergyEntry> activeSynergies;

		internal GameObject _pokedexBox_x__0 {
			get { return (GameObject)typeof(ConstructRectanglePageLayoutIterator2).GetField("<pokedexBox>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<pokedexBox>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal dfButton _prevButton_x__0 {
			get { return (dfButton)typeof(ConstructRectanglePageLayoutIterator2).GetField("<prevButton>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<prevButton>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal float _currentYLineTop_x__0 {
			get { return (float)typeof(ConstructRectanglePageLayoutIterator2).GetField("<currentYLineTop>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<currentYLineTop>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		[MonoModIgnore]
		internal Vector2 elementPaddingPx;

		[MonoModIgnore]
		internal List<EncounterDatabaseEntry> journalEntries;

		internal int _accumulatedSpriteIndex_x__0 {
			get { return (int)typeof(ConstructRectanglePageLayoutIterator2).GetField("<accumulatedSpriteIndex>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<accumulatedSpriteIndex>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal float _totalUnitHeight_x__0 {
			get { return (float)typeof(ConstructRectanglePageLayoutIterator2).GetField("<totalUnitHeight>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<totalUnitHeight>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal AmmonomiconPageRenderer.RectangleLineInfo _currentLineInfo_x__0 {
			get { return (AmmonomiconPageRenderer.RectangleLineInfo)typeof(ConstructRectanglePageLayoutIterator2).GetField("<currentLineInfo>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<currentLineInfo>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal List<AmmonomiconPageRenderer.RectangleLineInfo> _lineInfos_x__0 {
			get { return (List<AmmonomiconPageRenderer.RectangleLineInfo>)typeof(ConstructRectanglePageLayoutIterator2).GetField("<lineInfos>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<lineInfos>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal float _remainingLineWidth_x__0 {
			get { return (float)typeof(ConstructRectanglePageLayoutIterator2).GetField("<remainingLineWidth>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<remainingLineWidth>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal float _panelWidthUnits_x__0 {
			get { return (float)typeof(ConstructRectanglePageLayoutIterator2).GetField("<panelWidthUnits>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<panelWidthUnits>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		[MonoModIgnore]
		internal Vector2 panelPaddingPx;

		[MonoModIgnore]
		internal dfPanel sourcePanel;

		internal float _p2u_x__0 {
			get { return (float)typeof(ConstructRectanglePageLayoutIterator2).GetField("<p2u>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<p2u>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal float _boxyBox_x__0 {
			get { return (float)typeof(ConstructRectanglePageLayoutIterator2).GetField("<boxyBox>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<boxyBox>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		internal tk2dSpriteCollectionData _iconCollection_x__0 {
			get { return (tk2dSpriteCollectionData)typeof(ConstructRectanglePageLayoutIterator2).GetField("<iconCollection>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this); }
			set {
				typeof(ConstructRectanglePageLayoutIterator2).GetField("<iconCollection>__0", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, value);
			}
		}

		public bool MoveNext() {
			uint num = (uint)d_PC;
			d_PC = -1;
			switch (num) {
			case 0u:
				_boxyBox_x__0 = (float)((!hideButtons) ? 20 : 8);
				_p2u_x__0 = d_this.guiManager.PixelsToUnits();
				_panelWidthUnits_x__0 = (sourcePanel.Width - panelPaddingPx.x * 2f) * _p2u_x__0;
				_remainingLineWidth_x__0 = _panelWidthUnits_x__0;
				_lineInfos_x__0 = new List<AmmonomiconPageRenderer.RectangleLineInfo>();
				_currentLineInfo_x__0 = default(AmmonomiconPageRenderer.RectangleLineInfo);
				_totalUnitHeight_x__0 = 0f;
				_iconCollection_x__0 = AmmonomiconController.ForceInstance.EncounterIconCollection;
				for (int i = 0; i < journalEntries.Count; i++) {
					if (journalEntries[i] != null) {
						string text = journalEntries[i].journalData.AmmonomiconSprite;
						if (text.StartsWith("gunderfury_LV")) {
							text = "gunderfury_LV" + (GunderfuryController.GetCurrentTier() + 1) + "0_idle_001";
						}

						try {
						} catch {
						}
						tk2dSpriteDefinition tk2dSpriteDefinition = null;
						if (!string.IsNullOrEmpty(text)) {
							tk2dSpriteDefinition = _iconCollection_x__0.GetSpriteDefinition(text);
						}
						Bounds bounds;
						if (tk2dSpriteDefinition != null) {
							bounds = tk2dSpriteDefinition.GetBounds();
						} else {
							bounds = _iconCollection_x__0.GetSpriteDefinition(AmmonomiconController.AmmonomiconErrorSprite).GetBounds();
						}
						Vector2 a = bounds.size * 16f;
						Vector2 vector = (a * 4f + elementPaddingPx * 2f) * _p2u_x__0;
						if (_remainingLineWidth_x__0 < vector.x) {
							_totalUnitHeight_x__0 += _currentLineInfo_x__0.lineHeightUnits;
							if (d_this.pageType == AmmonomiconPageRenderer.PageType.EQUIPMENT_LEFT) {
								_remainingLineWidth_x__0 += elementPaddingPx.x * _p2u_x__0 * 2f + 4f * _p2u_x__0;
							}
							var _tmp_x = _currentLineInfo_x__0;
							_tmp_x.initialXOffset = (_remainingLineWidth_x__0 / 2f).Quantize(_p2u_x__0 * 4f);
							_currentLineInfo_x__0 = _tmp_x;
							_lineInfos_x__0.Add(_currentLineInfo_x__0);
							_currentLineInfo_x__0 = default(AmmonomiconPageRenderer.RectangleLineInfo);
							_remainingLineWidth_x__0 = _panelWidthUnits_x__0;
						}
						var _tmp_y = _currentLineInfo_x__0;
						_tmp_y.numberOfElements = _currentLineInfo_x__0.numberOfElements + 1;
						_tmp_y.lineHeightUnits = Mathf.Max(_currentLineInfo_x__0.lineHeightUnits, vector.y);
						_currentLineInfo_x__0 = _tmp_y;
						_remainingLineWidth_x__0 -= vector.x;
					}
				}
				if (d_this.pageType == AmmonomiconPageRenderer.PageType.EQUIPMENT_LEFT) {
					_remainingLineWidth_x__0 += elementPaddingPx.x * _p2u_x__0 * 2f + 4f * _p2u_x__0;
				}
				_totalUnitHeight_x__0 += _currentLineInfo_x__0.lineHeightUnits;
				var _tmp_z = _currentLineInfo_x__0;
				_tmp_z.initialXOffset = (_remainingLineWidth_x__0 / 2f).Quantize(_p2u_x__0 * 4f);
				_currentLineInfo_x__0 = _tmp_z;
				_lineInfos_x__0.Add(_currentLineInfo_x__0);
				_accumulatedSpriteIndex_x__0 = 0;
				_currentYLineTop_x__0 = -(panelPaddingPx.y * _p2u_x__0);
				_prevButton_x__0 = null;
				if (d_this.m_prevLineButtons == null) {
					d_this.m_prevLineButtons = new List<dfButton>();
				}
				_pokedexBox_x__0 = (GameObject)BraveResources.Load("Global Prefabs/Pokedex Box", ".prefab");
				for (int j = 0; j < _lineInfos_x__0.Count; j++) {
					_currentLineInfo_x__0 = _lineInfos_x__0[j];
					List<dfButton> list = new List<dfButton>();
					for (int k = 0; k < _currentLineInfo_x__0.numberOfElements; k++) {
						EncounterDatabaseEntry encounterDatabaseEntry = journalEntries[_accumulatedSpriteIndex_x__0];
						string text2 = encounterDatabaseEntry.journalData.AmmonomiconSprite;

						try {
						} catch {
						}

						if (text2.StartsWith("gunderfury_LV")) {
							text2 = "gunderfury_LV60_idle_001";
						}
						int spriteIdByName = _iconCollection_x__0.GetSpriteIdByName(text2, -1);
						if (spriteIdByName < 0) {
							Debug.LogWarning("Missing sprite " + text2 + "; add to the Ammonomicon Icon Collection.");
							spriteIdByName = _iconCollection_x__0.GetSpriteIdByName(AmmonomiconController.AmmonomiconErrorSprite);
						}
						dfButton dfButton = sourcePanel.AddPrefab(_pokedexBox_x__0) as dfButton;
						dfButton.MakePixelPerfect();
						dfButton.PerformLayout();
						tk2dClippedSprite tk2dClippedSprite = d_this.AddSpriteToPage<tk2dClippedSprite>(_iconCollection_x__0, spriteIdByName);
						if (journalEntries[_accumulatedSpriteIndex_x__0].path.Contains("ResourcefulRatNote")) {
							tk2dClippedSprite.SetSprite("resourcefulrat_note_base_001");
						}
						float num2 = (tk2dClippedSprite.GetBounds().size * 16f * 4f * _p2u_x__0).x / tk2dClippedSprite.scale.x;
						dfButton.Size = new Vector2(num2 / _p2u_x__0 + _boxyBox_x__0 * 2f, _currentLineInfo_x__0.lineHeightUnits / _p2u_x__0 - (elementPaddingPx.y * 2f - _boxyBox_x__0 * 2f));
						if (text2.StartsWith("gunderfury_LV")) {
							text2 = "gunderfury_LV" + (GunderfuryController.GetCurrentTier() + 1) + "0_idle_001";
							spriteIdByName = _iconCollection_x__0.GetSpriteIdByName(text2, -1);
							tk2dClippedSprite.SetSprite(spriteIdByName);
						}
						tk2dClippedSprite.transform.parent = dfButton.transform.Find("CenterPoint");
						tk2dClippedSprite.PlaceAtLocalPositionByAnchor(Vector3.zero, tk2dBaseSprite.Anchor.MiddleCenter);
						tk2dClippedSprite.transform.position = tk2dClippedSprite.transform.position.Quantize(4f * _p2u_x__0);
						if (hideButtons) {
							SpriteOutlineManager.AddScaledOutlineToSprite<tk2dClippedSprite>(tk2dClippedSprite, Color.black, 0.1f, 0.05f);
						}
						if (j == 0 && k == 0) {
							dfButton.RelativePosition = new Vector3(_currentLineInfo_x__0.initialXOffset / _p2u_x__0 + (elementPaddingPx.x - _boxyBox_x__0), -_currentYLineTop_x__0 / _p2u_x__0 + (elementPaddingPx.y - _boxyBox_x__0), 0f);
						} else if (k == 0) {
							dfButton.RelativePosition = new Vector3((_currentLineInfo_x__0.initialXOffset / _p2u_x__0).Quantize(4f), _prevButton_x__0.RelativePosition.y + _prevButton_x__0.Height + 4f, 0f);
						}
						if (k > 0) {
							dfButton.RelativePosition = _prevButton_x__0.RelativePosition + new Vector3(_prevButton_x__0.Width + 4f, 0f, 0f);
						}
						dfButton.RelativePosition = dfButton.RelativePosition.Quantize(4f);
						dfButton.PerformLayout();
						AmmonomiconPokedexEntry component = dfButton.GetComponent<AmmonomiconPokedexEntry>();
						component.IsEquipmentPage = hideButtons;
						component.IsGunderfury = text2.StartsWith("gunderfury_LV");
						component.AssignSprite(tk2dClippedSprite);
						component.linkedEncounterTrackable = journalEntries[_accumulatedSpriteIndex_x__0];
						if (hideButtons) {
							component.pickupID = component.linkedEncounterTrackable.pickupObjectId;
							component.activeSynergies = activeSynergies;
						}
						if (journalEntries[_accumulatedSpriteIndex_x__0].ForceEncounterState) {
							component.encounterState = AmmonomiconPokedexEntry.EncounterState.KNOWN;
							component.ForceEncounterState = true;
						}
						component.UpdateEncounterState();
						d_this.m_pokedexEntries.Add(component);
						UIKeyControls uIKeyControls = component.gameObject.AddComponent<UIKeyControls>();
						uIKeyControls.selectOnAction = true;
						if (k > 0) {
							uIKeyControls.left = _prevButton_x__0;
							_prevButton_x__0.GetComponent<UIKeyControls>().right = dfButton;
						} else {
							UIKeyControls expr_80C = uIKeyControls;
							expr_80C.OnLeftDown = (Action)Delegate.Combine(expr_80C.OnLeftDown, new Action(d_this.ReturnFocusToBookmarks));
						}
						if (hideButtons) {
							dfButton.Opacity = 0.01f;
						}
						list.Add(dfButton);
						_prevButton_x__0 = dfButton;
						_accumulatedSpriteIndex_x__0++;
					}
					if (j == 0) {
						Debug.Log(d_this.m_prevLineButtons + "|" + ((d_this.m_prevLineButtons != null) ? d_this.m_prevLineButtons.Count.ToString() : "null"));
					}
					if (d_this.m_prevLineButtons != null && d_this.m_prevLineButtons.Count > 0) {
						for (int l = 0; l < d_this.m_prevLineButtons.Count; l++) {
							int num3 = Mathf.RoundToInt((float)l / ((float)(d_this.m_prevLineButtons.Count - 1) * 1f) * (float)(list.Count - 1));
							num3 = Mathf.Clamp(num3, 0, list.Count - 1);
							UIKeyControls component2 = d_this.m_prevLineButtons[l].GetComponent<UIKeyControls>();
							if (component2 != null && num3 >= 0 && num3 < list.Count) {
								component2.down = list[num3];
							}
						}
						for (int m = 0; m < list.Count; m++) {
							int num4 = Mathf.RoundToInt((float)m / ((float)(list.Count - 1) * 1f) * (float)(d_this.m_prevLineButtons.Count - 1));
							num4 = Mathf.Clamp(num4, 0, d_this.m_prevLineButtons.Count - 1);
							UIKeyControls component3 = list[m].GetComponent<UIKeyControls>();
							if (component3 != null && num4 >= 0 && num4 < d_this.m_prevLineButtons.Count) {
								component3.up = d_this.m_prevLineButtons[num4];
							}
						}
					}
					d_this.m_prevLineButtons = list;
				}
				sourcePanel.Height = _totalUnitHeight_x__0 / _p2u_x__0 + 2f * panelPaddingPx.y + (float)(8 * _lineInfos_x__0.Count);
				d_current = null;
				if (!d_disposing) {
					d_PC = 1;
				}
				return true;
			case 1u:
				if (!hideButtons) {
					d_this.SetRightDataPageUnknown(AmmonomiconController.Instance.IsTurningPage);
				}
				d_PC = -1;
				break;
			}
			return false;
		}
	}
}
