using System;
using UnityEngine;

namespace Semi {
	public partial class Mod : MonoBehaviour {
		/// <summary>
		/// Attaches an instance of a sprite template to a GameObject.
		/// </summary>
		/// <returns>The new sprite instance parented to <c>target</c>.</returns>
		/// <param name="target">Target object.</param>
		/// <param name="id">Global ID of the sprite template.</param>
		public Sprite AttachSpriteInstance(GameObject target, ID id) {
			id = GetFullID(id, true);

			if (!Registry.SpriteTemplates.Contains(id)) throw new ArgumentException($"Invalid (nonexistant) ID: {id}");
			var template = Registry.SpriteTemplates[id];
			var new_sprite = target.AddComponent<tk2dSprite>().Wrap();

			template.CopyTo(new_sprite);
			return new_sprite;
		}

		/// <summary>
		/// Removes an existing sprite instance and attaches a new one to the same GameObject.
		/// </summary>
		/// <returns>The new sprite instance.</returns>
		/// <param name="target">Sprite to replace.</param>
		/// <param name="id">Global ID of the sprite template.</param>
		public Sprite ReplaceSpriteInstance(Sprite target, ID id) {
			var go = target.GameObject;
			UnityEngine.Object.Destroy(target.Wrap);
			return AttachSpriteInstance(go, id);
		}

		/// <summary>
		/// Registers an audio event override (any attempt to fire <code>old_id</code> ends up firing <code>new_id</code>).
		/// </summary>
		/// <param name="old_id">Audio event ID to replace.</param>
		/// <param name="new_id">Audio event ID to replace with.</param>
		public void OverrideAudioEvent(ID old_id, ID new_id) {
			AudioEvent.AudioEventOverrides[GetFullID(old_id, false)] = GetFullID(new_id, false);
		}
	}
}
