using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

namespace Semi {

	public abstract class Audio : IDisposable {
		public enum VolumeCategory {
			Sound,
			Music,
			UI
		}
		internal static Dictionary<string, string> AudioOverrides = new Dictionary<string, string>();

		public VolumeCategory Category;

		public static float SoundVolume {
			get {
				return GameManager.Options.SoundVolume / 100f;
			}
			set {
				GameManager.Options.SoundVolume = value * 100f;
			}
		}

		public static float MusicVolume {
			get {
				return GameManager.Options.MusicVolume / 100f;
			}
			set {
				GameManager.Options.MusicVolume = value * 100f;
			}
		}

		public static float UIVolume {
			get {
				return GameManager.Options.UIVolume / 100f;
			}
			set {
				GameManager.Options.UIVolume = value * 100f;
			}
		}

		public static void Play(string id, GameObject source = null) {
			global::AkSoundEngine.PostEvent(id, source);
		}

		public abstract void Play();
		public abstract void Stop();
		public abstract void Pause();
		public abstract void Resume();
		public abstract void Dispose();

		public abstract float Volume { get; set; }
		public abstract float Pitch { get; set; }
		public abstract bool IsPlaying { get; }

		internal abstract void UpdateVolume();
	}

	public class Sound : Audio {
		internal RayAudio.Sound RaySound;

		internal Sound(RayAudio.Sound sound) { RaySound = sound; Category = VolumeCategory.Sound; }
		internal float OwnVolume = 1f;
		public override bool IsPlaying { get { return RaySound.IsPlaying; } }

		public override float Pitch {
			get {
				return RaySound.Pitch;
			}
			set {
				RaySound.Pitch = value;
			}
		}

		public override float Volume {
			get {
				return OwnVolume;
			}

			set {
				OwnVolume = value;
				UpdateVolume();
			}
		}

		public override void Dispose() {
			RaySound.Dispose();
		}

		public override void Pause() {
			RaySound.Pause();
		}

		public override void Play() {
			RaySound.Play();
		}

		public override void Resume() {
			RaySound.Resume();
		}

		public override void Stop() {
			RaySound.Stop();
		}

		internal override void UpdateVolume() {
			switch (Category) {
				case VolumeCategory.Sound: RaySound.Volume = OwnVolume * Audio.SoundVolume; break;
				case VolumeCategory.Music: RaySound.Volume = OwnVolume * Audio.MusicVolume; break;
				case VolumeCategory.UI: RaySound.Volume = OwnVolume * Audio.UIVolume; break;
			}
		}
	}

	public class Music : Audio {
		internal RayAudio.MusicStream RayMusic;

		internal Music(RayAudio.MusicStream music) { RayMusic = music; Category = VolumeCategory.Music; }
		internal float OwnVolume = 1f;
		public override bool IsPlaying { get { return RayMusic.IsPlaying; } }

		public override float Pitch {
			get {
				return RayMusic.Pitch;
			}
			set {
				RayMusic.Pitch = value;
			}
		}

		public override float Volume {
			get {
				return OwnVolume;
			}

			set {
				OwnVolume = value;
				UpdateVolume();
			}
		}

		public float Length { get { return RayMusic.Length; } }
		public float TimePlayed { get { return RayMusic.TimePlayed; } }

		public override void Dispose() {
			RayMusic.Dispose();
		}

		public override void Pause() {
			RayMusic.Pause();
		}

		public override void Play() {
			RayMusic.Play();
		}

		public override void Resume() {
			RayMusic.Resume();
		}

		public override void Stop() {
			RayMusic.Stop();
		}

		internal override void UpdateVolume() {
			switch (Category) {
			case VolumeCategory.Sound: RayMusic.Volume = OwnVolume * Audio.SoundVolume; break;
			case VolumeCategory.Music: RayMusic.Volume = OwnVolume * Audio.MusicVolume; break;
			case VolumeCategory.UI: RayMusic.Volume = OwnVolume * Audio.UIVolume; break;
			}
		}
	}
}
