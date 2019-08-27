using System;
using UnityEngine;

namespace Semi {
	public partial class Mod : MonoBehaviour {
		public PickupObject GetItem(string id) {
			id = GetFullID(id, false);
			if (!Gungeon.Items.ContainsID(id)) return null;
			return Gungeon.Items[id];
		}

		public AIActor GetEnemy(string id) {
			id = GetFullID(id, false);
			if (!Gungeon.Enemies.ContainsID(id)) return null;
			return Gungeon.Enemies[id];
		}

		public AdvancedSynergyEntry GetSynergy(string id) {
			id = GetFullID(id, false);
			if (!Gungeon.Synergies.ContainsID(id)) return null;
			return Gungeon.Synergies[id];
		}

		public SpriteCollection? GetSpriteCollection(string id) {
			id = GetFullID(id, false);
			if (!Gungeon.SpriteCollections.ContainsID(id)) return null;
			return Gungeon.SpriteCollections[id];
		}

		public Sprite? GetSpriteTemplate(string id) {
			id = GetFullID(id, false);
			if (!Gungeon.SpriteTemplates.ContainsID(id)) return null;
			return Gungeon.SpriteTemplates[id];
		}

		public SpriteAnimation? GetAnimationTemplate(string id) {
			id = GetFullID(id, false);
			if (!Gungeon.AnimationTemplates.ContainsID(id)) return null;
			return Gungeon.AnimationTemplates[id];
		}

		public I18N.LocalizationSource GetLocalization(string id) {
			id = GetFullID(id, false);
			if (!Gungeon.Localizations.ContainsID(id)) return null;
			return Gungeon.Localizations[id];
		}

		public I18N.Language GetLanguage(string id) {
			id = GetFullID(id, false);
			if (!Gungeon.Languages.ContainsID(id)) return null;
			return Gungeon.Languages[id];
		}

		public Audio GetAudioTrack(string id) {
			id = GetFullID(id, false);
			if (!Gungeon.ModAudioTracks.ContainsID(id)) return null;
			return Gungeon.ModAudioTracks[id];
		}
	}
}
