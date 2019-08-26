using System;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

namespace Semi {
	internal static class WWiseAudioBridge {
		internal static Logger Logger = new Logger("WWiseAudioBridge");

		internal static string GetWWiseEventNameFromID(string id) {
			if (id.StartsWithInvariant("gungeon:")) return id.Replace("gungeon:", "");;
			if (!id.Contains(":")) return id;
			return null;
		}

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
