using System;
using UnityEngine;
using RayAudio;
using System.Collections.Generic;
using System.Reflection;

namespace Semi {
	internal class StreamBufferUpdateBehaviour : MonoBehaviour {
		internal static List<Audio> Tracks;
		internal static bool Paused = true;
		internal static float CachedSoundVolume = 1f;
		internal static float CachedMusicVolume = 1f;
		internal static float CachedUIVolume = 1f;
		internal static StreamBufferUpdateBehaviour Instance;

		internal void Awake() {
			Instance = this;
		}

		internal void Update() {
			if (Input.GetKeyDown(KeyCode.X)) {
				var fields = typeof(dfTextbox).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

				Console.WriteLine("=== SCROLL DATA ===");
				for (var i = 0; i < fields.Length; i++) {
					var field = fields[i];

					Console.WriteLine($"{field.Name} = {field.GetValue(UI.TestTest)}");
				}
			}

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
