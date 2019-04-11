using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Semi {
	public abstract partial class Mod : MonoBehaviour {
		internal static GameObject CleanPickupObjectBase;

		public const string RESOURCES_DIR_NAME = "resources";
		
		internal bool RegisteringMode;
		internal string ResourcePath {
			get { return Path.Combine(Info.Path, RESOURCES_DIR_NAME); }
		}

		private static char[] _SeparatorSplitArray = { '\\', '/' };
		public static string NormalizePath(string path) {
			var split = path.ToLowerInvariant().Split(_SeparatorSplitArray, StringSplitOptions.RemoveEmptyEntries);
			var list = new List<string>();

			for (int i = 0; i < split.Length; i++) {
				var el = split[i];

				if (el == "..") {
					if (list.Count > 0) list.RemoveAt(list.Count - 1);
				} else if (el != ".") {
					list.Add(el);
				}
			}

			return string.Join("/", list.ToArray());
		}

		public string GetFullResourcePath(string res_path) {
			if (Path.PathSeparator == '\\') res_path = res_path.Replace('/', '\\');
			return Path.Combine(ResourcePath, res_path);
		}

		public string GetFullID(string id) {
			if (id.Contains(":")) throw new Exception($"'id' must not contain a namespace");
			return $"{Config.ID}:{id}";
		}

		internal void CheckMode() {
			if (!RegisteringMode) throw new InvalidOperationException($"Content can only be registered in the RegisterContent method.");
		}

		public SpriteDefinition CreateSpriteDefinition(string id, string sprite_path) {
			CheckMode();
			id = GetFullID(id);
			var tex = Texture2DLoader.LoadTexture2D(GetFullResourcePath(sprite_path));
			var sprite_def = SpriteDefinition.Construct(tex, id);

			return sprite_def;
		}

		public SpriteDefinition RegisterEncounterIcon(string id, string sprite_path) {
			CheckMode();
			id = GetFullID(id);

			var tex = Texture2DLoader.LoadTexture2D(GetFullResourcePath(sprite_path));
			var sprite_def = SpriteDefinition.Construct(tex, id);

			SemiLoader.EncounterIconCollection.Register(sprite_def);

			return sprite_def;
		}

		public SpriteCollection RegisterSpriteCollection(string id, params SpriteDefinition[] defs) {
			CheckMode();
			id = GetFullID(id);

			var coll = SpriteCollection.Construct(
				SemiLoader.SpriteCollectionStorageObject,
				id,
				id, // IDs are unique
				defs
			);

			Gungeon.SpriteCollections.Add(id, coll);

			return coll;
		}

		public Sprite RegisterSpriteTemplate(string id, string coll_id, string start_def_id = null) {
			CheckMode();
			id = GetFullID(id);

			var coll = Gungeon.SpriteCollections[coll_id];

			if (start_def_id != null) {
				start_def_id = coll.SpritePool.ValidateEntry(start_def_id);
			}
			var starting_idx = 0;
			if (coll.SpriteDefinitions.Count == 0) throw new ArgumentException($"The collection must have at least one sprite definition");

			if (start_def_id != null) starting_idx = coll.GetIndex(start_def_id);
			if (starting_idx < 0) throw new ArgumentException($"Collection doesn't have a '{start_def_id}' definition.");

			var sprite = Sprite.Construct(
				SemiLoader.SpriteTemplateStorageObject,
				coll,
				starting_idx
			);

			Gungeon.SpriteTemplates.Add(id, sprite);

			return sprite;
		}

		public Sprite AttachSpriteInstance(GameObject target, string id) {
			id = GetFullID(id);

			if (!Gungeon.SpriteTemplates.ContainsID(id)) throw new ArgumentException($"Invalid (nonexistant) ID: {id}");
			var template = Gungeon.SpriteTemplates[id];
			var new_sprite = target.AddComponent<tk2dSprite>().Wrap();

			template.CopyTo(new_sprite);
			return new_sprite;
		}

		public Sprite ReplaceSpriteInstance(Sprite target, string id) {
			var go = target.GameObject;
			UnityEngine.Object.Destroy(target.Wrap);
			return AttachSpriteInstance(go, id);
		}

		public I18N.ModLocalization RegisterLocalization(string id, string path, string lang_id, I18N.StringTable table, bool allow_overwrite = false) {
			CheckMode();
			id = GetFullID(id);

			lang_id = Gungeon.Languages.ValidateEntry(lang_id);

			var mod_loc = new I18N.ModLocalization(
				Info,
				GetFullResourcePath(path),
				lang_id,
				table,
				allow_overwrite
			);

			Gungeon.Localizations.Add(id, mod_loc);

			return mod_loc;
		}

		public PassiveItem RegisterPassiveItem<T>(string id, string enc_icon_id, string sprite_template_id, string name_key = "", string short_desc_key = "", string long_desc_key = "") where T : PassiveItem {
			id = GetFullID(id);
			Console.WriteLine($"STAGE: id");
			var sprite_def = SemiLoader.EncounterIconCollection.GetDefinition(enc_icon_id);
			Console.WriteLine($"STAGE: sprite_def");
			var sprite_template = Gungeon.SpriteTemplates[sprite_template_id];
			Console.WriteLine($"STAGE: sprite_template");

			var new_inst = UnityEngine.Object.Instantiate(CleanPickupObjectBase);
			Console.WriteLine($"STAGE: new_inst");
			new_inst.name = id;
			Console.WriteLine($"STAGE: new_inst.name");

			var pickup = new_inst.AddComponent<T>() as PickupObject;
			Console.WriteLine($"STAGE: pickup");
			var enc_trackable = new_inst.AddComponent<EncounterTrackable>();
			Console.WriteLine($"STAGE: enc_trackable");
			var journal_entry = new JournalEntry();
			Console.WriteLine($"STAGE: journal_entry");
			enc_trackable.journalData = journal_entry;
			Console.WriteLine($"STAGE: enc_trackable.journalData");

			journal_entry.PrimaryDisplayName = name_key;
			Console.WriteLine($"STAGE: journ.PrimaryDisplayName");
			journal_entry.AmmonomiconFullEntry = long_desc_key;
			Console.WriteLine($"STAGE: journ.AmmonomiconFullEntry");
			journal_entry.NotificationPanelDescription = short_desc_key;
			Console.WriteLine($"STAGE: journ.NotificationPanelDescription");
			journal_entry.AmmonomiconSprite = sprite_def.Name;
			Console.WriteLine($"STAGE: journ.AmmonomiconSprite");
			journal_entry.SuppressInAmmonomicon = false;
			Console.WriteLine($"STAGE: journ.SuppressInAmmonomicon");

			enc_trackable.EncounterGuid = $"SEMI+{id}";
			Console.WriteLine($"STAGE: EncounterGuid");
			enc_trackable.TrueEncounterGuid = enc_trackable.EncounterGuid;
			Console.WriteLine($"STAGE: TrueEncounterGuid");

			var num_id = PickupObjectDatabase.Instance.Objects.Count;
			Console.WriteLine($"STAGE: num_id");
			pickup.PickupObjectId = num_id;
			Console.WriteLine($"STAGE: pickup.PickupObjectId");

			var sprite = new_inst.AddComponent<tk2dSprite>().Wrap();
			Console.WriteLine($"STAGE: sprite");
			sprite_template.CopyTo(sprite);
			Console.WriteLine($"STAGE: sprite CopyTo");

			PickupObjectDatabase.Instance.Objects.Add(pickup);
			Console.WriteLine($"STAGE: pickup object database add");
			Gungeon.Items.Add(id, pickup);
			Console.WriteLine($"STAGE: gungeon items add");



			var enc_entry = new EncounterDatabaseEntry(enc_trackable);
			Console.WriteLine($"STAGE: enc_entry");
			//enc_entry.journalData = journal_entry;
			//Console.WriteLine($"STAGE: enc_entry.journalData");

			EncounterDatabase.Instance.Entries.Add(enc_entry);
			enc_entry.path = $"SEMI/Items/{id}";
			enc_entry.unityGuid = enc_entry.myGuid = enc_trackable.EncounterGuid;
			Console.WriteLine($"STAGE: add enc entry to db");

			return pickup as PassiveItem;
		}
	}
}
