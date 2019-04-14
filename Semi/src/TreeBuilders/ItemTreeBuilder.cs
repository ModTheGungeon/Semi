using System;
using UnityEngine;

namespace Semi {
	public class PickupObjectTreeBuilder {
		internal static GameObject CleanBaseObject;
		internal static GameObject StoredBarrel;

		public static GameObject GetNewInactiveObject(string name) {
			var obj = Semi.FakePrefab.Clone(CleanBaseObject);
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.name = name;
			return obj;
		}

		public static GameObject GetNewBarrel() {
			var obj = Semi.FakePrefab.Clone(StoredBarrel);
			UnityEngine.Object.DontDestroyOnLoad(obj);
			return obj;
		}

		public static T AddPickupObject<T>(GameObject go) where T : PickupObject {
			var pickup = go.AddComponent<T>() as PickupObject;
			var num_id = PickupObjectDatabase.Instance.Objects.Count;
			pickup.PickupObjectId = num_id;

			return pickup as T;
		}

		public static JournalEntry CreateJournalEntry(string name_key, string long_desc_key, string short_desc_key, string enc_icon_name) {
			var journal_entry = new JournalEntry();

			journal_entry.PrimaryDisplayName = name_key;
			journal_entry.AmmonomiconFullEntry = long_desc_key;
			journal_entry.NotificationPanelDescription = short_desc_key;
			journal_entry.AmmonomiconSprite = enc_icon_name;
			journal_entry.SuppressInAmmonomicon = false;

			return journal_entry;
		}

		public static EncounterTrackable AddEncounterTrackable(GameObject go, JournalEntry journal_entry, string enc_guid) {
			var enc_trackable = go.AddComponent<EncounterTrackable>();
			enc_trackable.journalData = journal_entry;
			enc_trackable.EncounterGuid = enc_guid;
			enc_trackable.TrueEncounterGuid = enc_trackable.EncounterGuid;
			return enc_trackable;
		}

		public static EncounterDatabaseEntry CreateEncounterDatabaseEntry(EncounterTrackable enc_track, string path) {
			var enc_entry = new EncounterDatabaseEntry(enc_track);

			enc_entry.path = path;
			enc_entry.unityGuid = enc_entry.myGuid = enc_track.EncounterGuid;

			return enc_entry;
		}

		public static Sprite AddSprite(GameObject go, Sprite base_sprite) {
			var sprite = go.AddComponent<tk2dSprite>().Wrap();
			base_sprite.CopyTo(sprite);
			return sprite;
		}
	}
}
