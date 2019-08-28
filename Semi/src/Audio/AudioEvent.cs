using System;
using System.Collections.Generic;
using UnityEngine;

namespace Semi {
	public abstract class AudioEvent {
		/// <summary>
		/// A dictionary of audio event overrides, for replacing certain audio events with other audio events.
		/// </summary>
		internal static Dictionary<string, string> AudioEventOverrides = new Dictionary<string, string>();

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
	}

	public class WWiseAudioEvent : AudioEvent {
		internal static Dictionary<string, string> ReverseIDMap = new Dictionary<string, string>();
		public string WWiseEventName { get; private set; }

		internal WWiseAudioEvent(string event_name) {
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

	public class SemiSoundPlayEvent : AudioEvent {
		public string ID { get; private set; }
		internal Sound CachedSound;

		public SemiSoundPlayEvent(string sound_id) {
			sound_id = Gungeon.ModAudioTracks.ValidateEntry(sound_id);
			CachedSound = Gungeon.ModAudioTracks[sound_id] as Sound;
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

	public class StubAudioEvent : AudioEvent {
		public override void Fire(GameObject source = null) { FirePostEvent(source); }
	}

	public abstract class SemiAudioStateEvent : AudioEvent {
		public string ID { get; private set; }
		internal Audio CachedAudio;

		public SemiAudioStateEvent(string audio_id) {
			audio_id = Gungeon.ModAudioTracks.ValidateEntry(audio_id);
			CachedAudio = Gungeon.ModAudioTracks[audio_id];
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

	public class SemiAudioPlayEvent : SemiAudioStateEvent {
		public SemiAudioPlayEvent(string audio_id) : base(audio_id) { }

		internal override void DoStateChange() {
			CachedAudio.Play();
		}
	}

	public class SemiAudioStopEvent : SemiAudioStateEvent {
		public SemiAudioStopEvent(string audio_id) : base(audio_id) { }

		internal override void DoStateChange() {
			CachedAudio.Stop();
		}
	}

	public class SemiAudioPauseEvent : SemiAudioStateEvent {
		public SemiAudioPauseEvent(string audio_id) : base(audio_id) { }

		internal override void DoStateChange() {
			CachedAudio.Pause();
		}
	}

	public class SemiAudioResumeEvent : SemiAudioStateEvent {
		public SemiAudioResumeEvent(string audio_id) : base(audio_id) { }

		internal override void DoStateChange() {
			CachedAudio.Resume();
		}
	}
}
