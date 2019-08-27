using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

namespace Semi {

	public abstract class Audio : IDisposable {
		/// <summary>
		/// Volume category, one value for each bar in settings.
		/// </summary>
		public enum VolumeCategory {
			Sound,
			Music,
			UI
		}

		/// <summary>
		/// A dictionary of audio overrides, for replacing audio tracks with other audio tracks.
		/// </summary>
		internal static Dictionary<string, string> AudioOverrides = new Dictionary<string, string>();

		/// <summary>
		/// The volume category of this audio track.
		/// </summary>
		public VolumeCategory Category;

		/// <summary>
		/// Volume for Sound set in the settings.
		/// </summary>
		/// <value>0f -> 1f</value>
		public static float SoundVolume {
			get {
				return GameManager.Options.SoundVolume / 100f;
			}
			set {
				GameManager.Options.SoundVolume = value * 100f;
			}
		}

		/// <summary>
		/// Volume for Music set in the settings.
		/// </summary>
		/// <value>0f -> 1f</value>
		public static float MusicVolume {
			get {
				return GameManager.Options.MusicVolume / 100f;
			}
			set {
				GameManager.Options.MusicVolume = value * 100f;
			}
		}


		/// <summary>
		/// Volume for UI set in the settings.
		/// </summary>
		/// <value>0f -> 1f</value>
		public static float UIVolume {
			get {
				return GameManager.Options.UIVolume / 100f;
			}
			set {
				GameManager.Options.UIVolume = value * 100f;
			}
		}

		/// <summary>
		/// Plays an audio track with the specified ID.
		/// </summary>
		/// <param name="id">ID of the audio track.</param>
		/// <param name="source">Source GameObject.</param>
		public static void Play(string id, GameObject source = null) {
			global::AkSoundEngine.PostEvent(id, source);
		}

		/// <summary>
		/// Play this audio track.
		/// </summary>
		public abstract void Play();

		/// <summary>
		/// Stop this audio track.
		/// </summary>
		public abstract void Stop();

		/// <summary>
		/// Pause this audio track.
		/// </summary>
		public abstract void Pause();

		/// <summary>
		/// Resume this audio track.
		/// </summary>
		public abstract void Resume();

		/// <summary>
		/// Releases all resource used by the <see cref="Semi.Audio"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Semi.Audio"/>. The <see cref="Dispose"/>
		/// method leaves the <see cref="Semi.Audio"/> in an unusable state. After calling <see cref="Dispose"/>, you must
		/// release all references to the <see cref="Semi.Audio"/> so the garbage collector can reclaim the memory that the
		/// <see cref="Semi.Audio"/> was occupying.</remarks>
		public abstract void Dispose();

		/// <summary>
		/// Gets or sets the volume of this track.
		/// </summary>
		/// <value>The volume.</value>
		public abstract float Volume { get; set; }

		/// <summary>
		/// Gets or sets the pitch of this track.
		/// </summary>
		/// <value>The pitch.</value>
		public abstract float Pitch { get; set; }

		/// <summary>
		/// Whether this audio track is currently playing.
		/// </summary>
		/// <value><c>true</c> if it is playing; otherwise, <c>false</c>.</value>
		public abstract bool IsPlaying { get; }

		/// <summary>
		/// Used to facilitate compatibility with the category-separated ingame volume settings.
		/// </summary>
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

		/// <summary>
		/// Gets the total length of this music track.
		/// </summary>
		/// <value>The length.</value>
		public float Length { get { return RayMusic.Length; } }

		/// <summary>
		/// Gets the time that this music track has been played for.
		/// </summary>
		/// <value>The time played.</value>
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
