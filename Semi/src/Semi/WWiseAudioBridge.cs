using System;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

namespace Semi {
	internal static class WWiseAudioBridge {
		internal static Logger Logger = new Logger("WWiseAudioBridge");

		/// <summary>
		/// Turns a Semi ID into the appropriate WWise event name if the namespace is gungeon:.
		/// </summary>
		/// <returns>The WWise event name, or null if the ID doesn't represent a WWise event.</returns>
		/// <param name="id">Identifier.</param>
		internal static string GetWWiseEventNameFromID(string id) {
			if (id.StartsWithInvariant("gungeon:")) return id.Replace("gungeon:", "");;
			if (!id.Contains(":")) return id;
			return null;
		}

		/// <summary>
		/// Plays the RayAudio audio track assigned to the ID, or returns the WWise event name.
		/// This method is used in the AkSoundEngine.PostEvent patch to allow for posting audio events
		/// using Semi IDs, including RayAudio tracks and streams.
		/// </summary>
		/// <returns>Null if the ID matched a RayAudio audio track (which also plays it), or the WWise event name if the ID points to a WWise event.</returns>
		/// <param name="id">ID of the audio track.</param>
		/// <param name="source">Source GameObject.</param>
		internal static string PostEventOrReturnWWiseEventName(string id, GameObject source) {
			id = IDPool<Audio>.Resolve(id);
			IDPool<Audio>.VerifyID(id);

			if (Audio.AudioOverrides.ContainsKey(id)) {
				return PostEventOrReturnWWiseEventName(Audio.AudioOverrides[id], source);
			}

			if (!Gungeon.ModAudioTracks.ContainsID(id)) {
				var event_name = GetWWiseEventNameFromID(id);
				if (event_name != null) {
					return event_name;
				} else {
					Logger.Error($"Audio event with ID '{id}' doesn't exist.");
					return null;
				}
			} else {
				var track = Gungeon.ModAudioTracks[id];
				track.Play();
				return null;
			}
		}
	}
}
