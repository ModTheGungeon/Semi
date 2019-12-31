using System;
using UnityEngine;

namespace Semi {
	public partial class Mod : MonoBehaviour {
		public PickupObject GetItem(ID id) {
			id = GetFullID(id, false);
			if (!Registry.Items.Contains(id)) return null;
			return Registry.Items[id];
		}

		public AIActor GetEnemy(ID id) {
			id = GetFullID(id, false);
			if (!Registry.Enemies.Contains(id)) return null;
			return Registry.Enemies[id];
		}

		public AdvancedSynergyEntry GetSynergy(ID id) {
			id = GetFullID(id, false);
			if (!Registry.Synergies.Contains(id)) return null;
			return Registry.Synergies[id];
		}

		public SpriteCollection? GetSpriteCollection(ID id) {
			id = GetFullID(id, false);
			if (!Registry.SpriteCollections.Contains(id)) return null;
			return Registry.SpriteCollections[id];
		}

		public Sprite? GetSpriteTemplate(ID id) {
			id = GetFullID(id, false);
			if (!Registry.SpriteTemplates.Contains(id)) return null;
			return Registry.SpriteTemplates[id];
		}

		public SpriteAnimation? GetAnimationTemplate(ID id) {
			id = GetFullID(id, false);
			if (!Registry.AnimationTemplates.Contains(id)) return null;
			return Registry.AnimationTemplates[id];
		}

		public I18N.LocalizationSource GetLocalization(ID id) {
			id = GetFullID(id, false);
			if (!Registry.Localizations.Contains(id)) return null;
			return Registry.Localizations[id];
		}

		public I18N.Language GetLanguage(ID id) {
			id = GetFullID(id, false);
			if (!Registry.Languages.Contains(id)) return null;
			return Registry.Languages[id];
		}

		public Audio GetAudioTrack(ID id) {
			id = GetFullID(id, false);
			if (!Registry.ModAudioTracks.Contains(id)) return null;
			return Registry.ModAudioTracks[id];
		}
	}
}
