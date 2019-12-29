using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Semi {
	public abstract class AudioEvent {
		/// <summary>
		/// A dictionary of audio event overrides, for replacing certain audio events with other audio events.
		/// </summary>
		internal static Dictionary<ID, ID> AudioEventOverrides = new Dictionary<ID, ID>();

		public List<AudioEvent> Consequences;

		public void AddConsequence(AudioEvent consequence) {
			if (Consequences == null) Consequences = new List<AudioEvent>();
			Consequences.Add(consequence);
		}

		public abstract void Fire(GameObject source = null);
		internal virtual string FirePostEvent(GameObject source) {
			if (Consequences != null) {
				for (var i = 0; i < Consequences.Count; i++) {
					Consequences[i].Fire(source);
				}
			}

			return null;
		}

		public class WWise : AudioEvent {
			internal static Dictionary<string, string> ReverseIDMap = new Dictionary<string, string>();
			public string WWiseEventName { get; private set; }

			internal WWise(string event_name) {
				WWiseEventName = event_name;
			}

			internal override string FirePostEvent(GameObject source) {
				base.FirePostEvent(source);

				return WWiseEventName;
			}

			public override void Fire(GameObject source = null) {
				AkSoundEngine.PostEvent(ReverseIDMap[WWiseEventName], source);
			}
		}

		public class SoundPlay : AudioEvent {
			public string ID { get; private set; }
			internal Sound CachedSound;

			public SoundPlay(ID sound_id) {
				sound_id = Registry.ModAudioTracks.ValidateExisting(sound_id);
				CachedSound = Registry.ModAudioTracks[sound_id] as Sound;
				if (CachedSound == null) throw new InvalidOperationException("Cannot use SemiSoundPlayEvent with Music - use SemiAudioPlayEvent");
			}

			internal override string FirePostEvent(GameObject source) {
				base.FirePostEvent(source);
				CachedSound.FireAndForget();

				return null;
			}

			public override void Fire(GameObject source = null) {
				FirePostEvent(source);
			}
		}

		public class Stub : AudioEvent {
			public override void Fire(GameObject source = null) { FirePostEvent(source); }
		}

		public abstract class AudioState : AudioEvent {
			public string ID { get; private set; }
			internal Audio CachedAudio;

			public AudioState(ID audio_id) {
				audio_id = Registry.ModAudioTracks.ValidateExisting(audio_id);
				CachedAudio = Registry.ModAudioTracks[audio_id];
			}

			internal override string FirePostEvent(GameObject source) {
				base.FirePostEvent(source);
				DoStateChange();

				return null;
			}

			public override void Fire(GameObject source = null) {
				FirePostEvent(source);
			}

			internal abstract void DoStateChange();
		}

		public class AudioPlay : AudioState {
			public AudioPlay(ID audio_id) : base(audio_id) { }

			internal override void DoStateChange() {
				CachedAudio.Play();
			}
		}

		public class AudioStop : AudioState {
			public AudioStop(ID audio_id) : base(audio_id) { }

			internal override void DoStateChange() {
				CachedAudio.Stop();
			}
		}

		public class AudioPause : AudioState {
			public AudioPause(ID audio_id) : base(audio_id) { }

			internal override void DoStateChange() {
				CachedAudio.Pause();
			}
		}

		public class AudioResume : AudioState {
			public AudioResume(ID audio_id) : base(audio_id) { }

			internal override void DoStateChange() {
				CachedAudio.Resume();
			}
		}

		public class AudioCrossfade : AudioEvent {
			public string ID { get; private set; }
			public int FadeDuration;
			public float Step;
			public float TargetVolume;
			internal Audio CachedFromAudio;
			internal Audio CachedToAudio;

			internal IEnumerator CrossfadeTracks() {
				var from_vol = CachedFromAudio.Volume;
				var to_vol = CachedToAudio.Volume;

				CachedToAudio.Volume = 0f;

				CachedToAudio.Play();

				for (var v = 0f; v <= TargetVolume; v += Step) {
					CachedFromAudio.Volume = Math.Max(from_vol - v, 0f);
					CachedToAudio.Volume = v;

					yield return null;
				}

				CachedFromAudio.Stop();
				CachedFromAudio.Volume = from_vol;
				yield return null;
			}

			public AudioCrossfade(ID from_audio_id, ID to_audio_id) {
				CachedFromAudio = Registry.ModAudioTracks[Registry.ModAudioTracks.ValidateExisting(from_audio_id)];
				CachedToAudio = Registry.ModAudioTracks[Registry.ModAudioTracks.ValidateExisting(to_audio_id)];
			}

			internal override string FirePostEvent(GameObject source) {
				base.FirePostEvent(source);

				StreamBufferUpdateBehaviour.Instance.StartCoroutine(CrossfadeTracks());

				return null;
			}

			public override void Fire(GameObject source = null) {
				FirePostEvent(source);
			}
		}
	}
}
