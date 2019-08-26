using System;
using UnityEngine;
using RayAudio;
using System.Collections.Generic;

namespace Semi {
	internal class StreamBufferUpdateBehaviour : MonoBehaviour {
		internal static List<Audio> Tracks;
		internal static bool Paused = true;
		internal static float CachedSoundVolume = 1f;
		internal static float CachedMusicVolume = 1f;
		internal static float CachedUIVolume = 1f;

		internal void Update() {
			if (Paused) return;
			for (int i = 0; i < Tracks.Count; i++) {
				var track = Tracks[i];

				switch (track.Category) {
				case Audio.VolumeCategory.Sound: if (CachedSoundVolume != Audio.SoundVolume) track.UpdateVolume(); break;
				case Audio.VolumeCategory.Music: if (CachedSoundVolume != Audio.MusicVolume) track.UpdateVolume(); break;
				case Audio.VolumeCategory.UI: if (CachedSoundVolume != Audio.UIVolume) track.UpdateVolume(); break;
				}
				CachedSoundVolume = Audio.SoundVolume;
				CachedMusicVolume = Audio.MusicVolume;
				CachedUIVolume = Audio.UIVolume;

				if (track is Music) ((Music)track).RayMusic.Update();
			}
		}
	}
}
