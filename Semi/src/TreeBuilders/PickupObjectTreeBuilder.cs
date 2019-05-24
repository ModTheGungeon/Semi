using System;
using UnityEngine;

namespace Semi {
	public class PickupObjectTreeBuilder {
		internal static GameObject CleanBaseObject;
		internal static GameObject StoredBarrel;

		/// <summary>
		/// Creates a new <see cref="T:Semi.FakePrefab"/> already containing certain components that need to be cloned from another item prefab.
		/// </summary>
		/// <returns>The new game object.</returns>
		/// <param name="name">Name of the new object.</param>
		public static GameObject GetNewInactiveObject(string name) {
			var obj = Semi.FakePrefab.Clone(CleanBaseObject);
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.name = name;
			return obj;
		}

		/// <summary>
		/// Creates a new <see cref="T:Semi.FakePrefab"/> of a barrel offset object.
		/// </summary>
		/// <returns>The new barrel offset object.</returns>
		public static GameObject GetNewBarrel() {
			var obj = Semi.FakePrefab.Clone(StoredBarrel);
			UnityEngine.Object.DontDestroyOnLoad(obj);
			return obj;
		}

		/// <summary>
		/// Creates a new <c>PickupObject</c>.
		/// </summary>
		/// <returns>The new pickup object.</returns>
		/// <param name="go">Target game object.</param>
		/// <typeparam name="T">Item type (descendant from <c>PickupObject</c>).</typeparam>
		public static T AddPickupObject<T>(GameObject go) where T : PickupObject {
			var pickup = go.AddComponent<T>() as PickupObject;
			var num_id = PickupObjectDatabase.Instance.Objects.Count;
			pickup.PickupObjectId = num_id;

			return pickup as T;
		}

		/// <summary>
		/// Creates a new <c>JournalEntry</c>.
		/// </summary>
		/// <returns>The new journal entry.</returns>
		/// <param name="name_key">Global ID of the localization key for the Ammmonomicon name.</param>
		/// <param name="long_desc_key">Global ID of the localization key for the Ammmonomicon long description.</param>
		/// <param name="short_desc_key">Global ID of the localization key for the Ammmonomicon short description.</param>
		/// <param name="enc_icon_name">Global ID of the encounter icon for this journal entry.</param>
		public static JournalEntry CreateJournalEntry(string name_key, string long_desc_key, string short_desc_key, string enc_icon_name) {
			var journal_entry = new JournalEntry();

			journal_entry.PrimaryDisplayName = name_key;
			journal_entry.AmmonomiconFullEntry = long_desc_key;
			journal_entry.NotificationPanelDescription = short_desc_key;
			journal_entry.AmmonomiconSprite = enc_icon_name;
			journal_entry.SuppressInAmmonomicon = false;

			return journal_entry;
		}

		/// <summary>
		/// Creates a new <c>EncounterTrackable</c>.
		/// </summary>
		/// <returns>The new encounter trackable.</returns>
		/// <param name="go">Target game object.</param>
		/// <param name="journal_entry">Journal entry object.</param>
		/// <param name="enc_guid">Unique encounter ID.</param>
		public static EncounterTrackable AddEncounterTrackable(GameObject go, JournalEntry journal_entry, string enc_guid) {
			var enc_trackable = go.AddComponent<EncounterTrackable>();
			enc_trackable.journalData = journal_entry;
			enc_trackable.EncounterGuid = enc_guid;
			enc_trackable.TrueEncounterGuid = enc_trackable.EncounterGuid;
			return enc_trackable;
		}

		/// <summary>
		/// Creates a new <c>EncounterDatabaseEntry</c>.
		/// </summary>
		/// <returns>The new encounter database entry.</returns>
		/// <param name="enc_track">Encounter trackable object.</param>
		/// <param name="path">String to set as the asset path.</param>
		public static EncounterDatabaseEntry CreateEncounterDatabaseEntry(EncounterTrackable enc_track, string path) {
			var enc_entry = new EncounterDatabaseEntry(enc_track);

			enc_entry.path = path;
			enc_entry.unityGuid = enc_entry.myGuid = enc_track.EncounterGuid;

			return enc_entry;
		}

		/// <summary>
		/// Adds a copy of a sprite to a game object.
		/// </summary>
		/// <returns>The new sprite.</returns>
		/// <param name="go">Target game object.</param>
		/// <param name="base_sprite">Sprite to copy.</param>
		public static Sprite AddSprite(GameObject go, Sprite base_sprite) {
			var sprite = go.AddComponent<tk2dSprite>().Wrap();
			base_sprite.CopyTo(sprite);
			return sprite;
		}
	}
}
