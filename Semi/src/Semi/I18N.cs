using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

namespace Semi {
	public static class I18N {
		internal static Logger Logger = new Logger("I18N");

		internal static Dictionary<string, string> CurrentUIDict = null;

		/// <summary>
		/// Type of localization string table.
		/// Different elements of the game will request localizations from a different string table.
		/// For example, the GUI notification when you receive a synergy will request strings from the synergy table, but the one when you pick up an item will request strings from the item table.
		/// </summary>
		public enum StringTable {
			Core,
			Enemies,
			Intro,
			Items,
			Synergies,
			UI
		}

		/// <summary>
		/// Abstract localization source. Children of this class have to define how to load localization data.
		/// </summary>
		public abstract class LocalizationSource {
			public string TargetLanguageID { get; internal set; }
			public StringTable TargetStringTable { get; internal set; }
			public FormatType Format { get; internal set; }

			public abstract void LoadInto(Dictionary<string, StringTableManager.StringCollection> dict);
			public abstract void LoadInto(Dictionary<string, string> dict);
			public abstract void LoadIntoDF(Dictionary<string, string> dict);

			public enum FormatType {
				Gungeon,
				DF
			}
		}

		/// <summary>
		/// Prefab localization source. This is used for localizations included with the game, to load them from the game's assets.
		/// </summary>
		public class PrefabLocalization : LocalizationSource {
			/// <summary>
			/// Path to the <c>TextAsset</c> prefab.
			/// </summary>
			public string Path;

			internal static string GetPrefabMainName(StringTableManager.GungeonSupportedLanguages lang) {
				string dir_lang = null;

				switch (lang) {
				case StringTableManager.GungeonSupportedLanguages.ENGLISH: dir_lang = "english"; break;
				case StringTableManager.GungeonSupportedLanguages.RUBEL_TEST: dir_lang = "english"; break;
				case StringTableManager.GungeonSupportedLanguages.FRENCH: dir_lang = "french"; break;
				case StringTableManager.GungeonSupportedLanguages.SPANISH: dir_lang = "spanish"; break;
				case StringTableManager.GungeonSupportedLanguages.ITALIAN: dir_lang = "italian"; break;
				case StringTableManager.GungeonSupportedLanguages.GERMAN: dir_lang = "german"; break;
				case StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE: dir_lang = "portuguese"; break;
				case StringTableManager.GungeonSupportedLanguages.JAPANESE: dir_lang = "japanese"; break;
				case StringTableManager.GungeonSupportedLanguages.KOREAN: dir_lang = "korean"; break;
				case StringTableManager.GungeonSupportedLanguages.RUSSIAN: dir_lang = "russian"; break;
				case StringTableManager.GungeonSupportedLanguages.POLISH: dir_lang = "polish"; break;
				case StringTableManager.GungeonSupportedLanguages.CHINESE: dir_lang = "chinese"; break;
				default: throw new InvalidOperationException($"Cannot use GungeonLanguage on custom languages");
				}

				return dir_lang;
			}

			internal static string GetPrefabPath(StringTableManager.GungeonSupportedLanguages lang, StringTable table) {
				var main_name = GetPrefabMainName(lang);
				switch (table) {
					case StringTable.Core: return main_name;
					case StringTable.Enemies: return $"{main_name}_items/enemies";
					case StringTable.Intro: return $"{main_name}_items/intro";
					case StringTable.Items: return $"{main_name}_items/items";
					case StringTable.Synergies: return $"{main_name}_items/synergies";
					case StringTable.UI: return $"{main_name}_items/ui";
					default: throw new InvalidOperationException($"Unknown string table");
				}
			}

			internal static TextAsset LoadTextAsset(string path) {
				Logger.Debug($"Loading builtin localization asset 'strings/{path}'");
				return (TextAsset)BraveResources.Load("strings/" + path, typeof(TextAsset), ".txt");
			}

			internal PrefabLocalization(StringTableManager.GungeonSupportedLanguages lang, StringTable table, FormatType format = FormatType.Gungeon) {
				TargetLanguageID = GungeonLanguage.LanguageToID(lang);
				TargetStringTable = table;
				Path = GetPrefabPath(lang, table);
				Format = format;
			}

			/// <summary>
			/// Loads localization data from the asset specified in this class.
			/// </summary>
			/// <param name="dict">Target dictionary of strings.</param>
			public override void LoadInto(Dictionary<string, StringTableManager.StringCollection> dict) {
				var asset = LoadTextAsset(Path);
				using (var reader = new StringReader(asset.text)) LoadLocalizationText(reader, dict);
			}

			public override void LoadInto(Dictionary<string, string> dict) {
				var asset = LoadTextAsset(Path);
				using (var reader = new StringReader(asset.text)) LoadLocalizationText(reader, dict);
			}

			public override void LoadIntoDF(Dictionary<string, string> dict) {
				var asset = LoadTextAsset(Path);
				using (var reader = new StringReader(asset.text)) LoadDFLocalizationText(reader, dict);
			}
		}

		public class RuntimeLocalization : LocalizationSource {
			/// <summary>
			/// Specifies whether to allow localizations loaded from this source to override sources loaded before it.
			/// </summary>
			public bool OverwriteMode = false;
			/// <summary>
			/// Default namespace for the entries in this localization file.
			/// </summary>
			public string DefaultNamespace;
			/// <summary>
			/// The localization file.
			/// </summary>
			public string Data;

			public RuntimeLocalization(string default_namespace, string data, string target_lang, StringTable target_table, bool allow_overwrite = false, FormatType format = FormatType.Gungeon) {
				TargetLanguageID = Gungeon.Languages.ValidateEntry(target_lang);
				TargetStringTable = target_table;
				Data = data;
				OverwriteMode = allow_overwrite;
				DefaultNamespace = default_namespace;
				Format = format;
			}

			/// <summary>
			/// Loads localization data from the provided string.
			/// </summary>
			/// <param name="dict">Target dictionary of strings.</param>
			public override void LoadInto(Dictionary<string, StringTableManager.StringCollection> dict) {
				LoadLocalizationText(new StringReader(Data), dict, overwrite: OverwriteMode, default_namespace: DefaultNamespace);
			}

			public override void LoadInto(Dictionary<string, string> dict) {
				LoadLocalizationText(new StringReader(Data), dict, overwrite: OverwriteMode, default_namespace: DefaultNamespace);
			}

			public override void LoadIntoDF(Dictionary<string, string> dict) {
				LoadDFLocalizationText(new StringReader(Data), dict, overwrite: OverwriteMode, default_namespace: DefaultNamespace);
			}
		}

		public class ModLocalization : LocalizationSource {
			/// <summary>
			/// Absolute path to the text file containing the localization data.
			/// </summary>
			public string Path;
			/// <summary>
			/// <see cref="Semi.ModInfo"/> of the mod that added this localization.
			/// </summary>
			public SemiLoader.ModInfo Owner;
			/// <summary>
			/// Specifies whether to allow localizations loaded from this source to override sources loaded before it.
			/// </summary>
			public bool OverwriteMode = false;

			internal ModLocalization(SemiLoader.ModInfo owner, string path, string target_lang, StringTable target_table, bool allow_overwrite = false, FormatType format = FormatType.Gungeon) {
				TargetLanguageID = Gungeon.Languages.ValidateEntry(target_lang);
				TargetStringTable = target_table;
				Path = path;
				Owner = owner;
				OverwriteMode = allow_overwrite;
				Format = format;
			}

			/// <summary>
			/// Loads localization data from the file specified in this class.
			/// </summary>
			/// <param name="dict">Target dictionary of strings.</param>
			public override void LoadInto(Dictionary<string, StringTableManager.StringCollection> dict) {
				using (var reader = new StreamReader(File.OpenRead(Path))) LoadLocalizationText(reader, dict, overwrite: OverwriteMode, default_namespace: Owner.Config.ID);
			}

			public override void LoadInto(Dictionary<string, string> dict) {
				using (var reader = new StreamReader(File.OpenRead(Path))) LoadLocalizationText(reader, dict, overwrite: OverwriteMode, default_namespace: Owner.Config.ID);
			}

			public override void LoadIntoDF(Dictionary<string, string> dict) {
				using (var reader = new StreamReader(File.OpenRead(Path))) LoadDFLocalizationText(reader, dict, overwrite: OverwriteMode, default_namespace: Owner.Config.ID);
			}
		}

		/// <summary>
		/// Abstract representation of a language.
		/// </summary>
		public abstract class Language {
			public string ID { get; internal set; }
			public Patches.GungeonSupportedLanguages MappedLanguage { get; internal set; }
		}

		/// <summary>
		/// Representation of a builtin (vanilla) language.
		/// </summary>
		public class GungeonLanguage : Language {
			/// <summary>
			/// Converts a <c>GungeonSupportedLanguages</c> value to a named ID.
			/// </summary>
			/// <returns>Global ID of this language.</returns>
			/// <param name="lang">Builtin enum value of this language.</param>
			public static string LanguageToID(StringTableManager.GungeonSupportedLanguages lang) {
				switch (lang) {
					case StringTableManager.GungeonSupportedLanguages.ENGLISH: return "gungeon:english";
					case StringTableManager.GungeonSupportedLanguages.RUBEL_TEST: return "gungeon:rubel_test";
					case StringTableManager.GungeonSupportedLanguages.FRENCH: return "gungeon:french";
					case StringTableManager.GungeonSupportedLanguages.SPANISH: return "gungeon:spanish";
					case StringTableManager.GungeonSupportedLanguages.ITALIAN: return "gungeon:italian";
					case StringTableManager.GungeonSupportedLanguages.GERMAN: return "gungeon:german";
					case StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE: return "gungeon:portuguese";
					case StringTableManager.GungeonSupportedLanguages.JAPANESE: return "gungeon:japanese";
					case StringTableManager.GungeonSupportedLanguages.KOREAN: return "gungeon:korean";
					case StringTableManager.GungeonSupportedLanguages.RUSSIAN: return "gungeon:russian";
					case StringTableManager.GungeonSupportedLanguages.POLISH: return "gungeon:polish";
					case StringTableManager.GungeonSupportedLanguages.CHINESE: return "gungeon:chinese";
					default: throw new InvalidOperationException($"Cannot use GungeonLanguage on custom languages");
				}
			}

			internal GungeonLanguage(StringTableManager.GungeonSupportedLanguages lang) {
				MappedLanguage = (Patches.GungeonSupportedLanguages)lang;
				ID = LanguageToID(lang);
			}
		}

		/// <summary>
		/// Representation of a custom (modded) language.
		/// </summary>
		public class CustomLanguage : Language {
			public CustomLanguage(string id) {
				ID = id;
				MappedLanguage = Patches.GungeonSupportedLanguages.SEMI_CUSTOM;
			}
		}

		internal static void LoadLocalizationTextGeneral(TextReader reader, Func<string, bool> dupe_check, Action<string, StringTableManager.StringCollection> assign_action, bool overwrite = false, string default_namespace = null) {
			// mostly based on copied StringTableManager code
			// all the files have duplicate loading code but it all seems to be doing
			// the same thing /shrug
			StringTableManager.StringCollection coll = null;

			string text;
			while ((text = reader.ReadLine()) != null) {
				if (!text.StartsWithInvariant("//")) {
					if (text.StartsWithInvariant("#")) {
						coll = new StringTableManager.ComplexStringCollection();

						if (default_namespace != null) {
							var id = text.Substring(1);
							if (id.Count(':') > 1) {
								Logger.Error($"Failed to add invalid key to table: {id}");
								continue;
							}

							if (!id.Contains(":")) {
								id = $"{default_namespace}:{id}";
							}

							if (id.StartsWithInvariant("gungeon:")) {
								text = $"#{id.Substring("gungeon:".Length)}";
							} else {
								text = $"#{id}";
							}
						}

						if (dupe_check.Invoke(text) && !overwrite) {
							Logger.Error($"Failed to add duplicate key to table: {text}");
						} else {
							assign_action.Invoke(text, coll);
						}
					} else {
						if (coll == null) continue;
						// ignore the KEY,EN stuff on ui.txt
						// we don't need it anyway because we have our own ID system

						string[] array = text.Split(new char[] {
							'|'
						});
						if (array.Length == 1) {
							coll.AddString(array[0], 1f);
						} else {
							coll.AddString(array[1], float.Parse(array[0]));
						}
					}
				}
			}
		}

		internal static Dictionary<string, StringTableManager.StringCollection> LoadLocalizationText(TextReader reader, Dictionary<string, StringTableManager.StringCollection> dict, bool overwrite = false, string default_namespace = null) {
			LoadLocalizationTextGeneral(reader, (id) => dict.ContainsKey(id), (id, coll) => dict[id] = coll, overwrite, default_namespace);
			return dict;
		}

		internal static Dictionary<string, string> LoadLocalizationText(TextReader reader, Dictionary<string, string> dict, bool overwrite = false, string default_namespace = null) {
			StringTableManager.StringCollection last_coll = null;
			string last_id = null;

			LoadLocalizationTextGeneral(reader, (id) => dict.ContainsKey(id), (id, coll) => {
				if (last_coll != null) {
					var str = last_coll.GetExactString(0);
					if (last_coll.Count() > 1) {
						Logger.Warn($"Randomized localizations are not supported by DF localization. Only the first option ('{str}') will be used.");
					}
					dict[last_id] = str;
				}
				last_id = id;
				last_coll = coll;
			}, overwrite, default_namespace);

			if (last_id != null) {
				dict[last_id] = last_coll.GetExactString(0);
				if (last_coll.Count() > 1) {
					Logger.Warn($"Randomized localizations are not supported by DF localization. Only the first option ('{dict[last_id]}') will be used.");
				}
			}

			return dict;
		}
			

		internal static Dictionary<string, string> LoadDFLocalizationText(TextReader reader, Dictionary<string, string> dict, bool overwrite = false, string default_namespace = null) {
			string text;
			while ((text = reader.ReadLine()) != null) {
				if (text.Trim() == "") continue;

				if (!text.StartsWithInvariant("//")) {
					if (text.StartsWithInvariant("#")) {
						var elements = text.Split(new char[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
						// we split on " instead of ,
						// because from my research it appears that " is never used escaped,
						// and commas appear in the localized text

						string value = null;

						if (elements.Length < 2) {
							Logger.Warn($"DF loc line has <2 elements! Assuming empty value string.");
							value = "";
						} else value = elements[1];

						var key = elements[0].Substring(1, elements[0].Length - 2);

						if (default_namespace != null) {
							if (key.Count(':') > 1) {
								Logger.Error($"Failed to add invalid key to table: {key}");
								continue;
							}

							if (!key.Contains(":")) {
								key = $"{default_namespace}:{key}";
							}

							if (key.StartsWithInvariant("gungeon:")) {
								var pair = key.SplitIntoPair(":");
								key = $"{pair.EverythingElse}";
							}
						}

						key = $"#{key}";

						if (elements.Length > 2) {
							Logger.Warn($"Ignoring unknown elements in DF loc line: #{text}");
						}

						if (dict.ContainsKey(key) && !overwrite) {
							Logger.Error($"Failed to add duplicate key to table: {text}");
						} else {
							Logger.Debug($"DF INSERT KEY: {key} VALUE: {value}");
							dict[key] = value;
						}
					}
				}
			}
			return dict;
		}

		private static string _CurrentLanguageID;
		/// <summary>
		/// Global ID of the currently selected language.
		/// </summary>
		/// <value>The current language ID.</value>
		public static string CurrentLanguageID {
			get {
				if (_CurrentLanguageID != null) return _CurrentLanguageID;
				return _CurrentLanguageID = GungeonLanguage.LanguageToID(GameManager.Options.CurrentLanguage);
			}
			internal set {
				var lang = Gungeon.Languages[value];
				GameManager.Options.CurrentLanguage = (StringTableManager.GungeonSupportedLanguages)lang.MappedLanguage;
				_CurrentLanguageID = value;
			}
		}

		internal static void LoadDFLocalizationsForLanguage(string lang_id, bool prefabs = false) {
			lang_id = Gungeon.Languages.ValidateEntry(lang_id);
			Logger.Debug($"Scanning localizations for {lang_id}");

			foreach (var pair in Gungeon.Localizations.Pairs) {
				var loc = pair.Value;
				var target_lang_id = IDPool<LocalizationSource>.Resolve(loc.TargetLanguageID);
				if (target_lang_id == lang_id && loc.TargetStringTable == StringTable.UI) {
					Logger.Debug($"Found DFGUI localization '{pair.Key}' ({loc.GetType().Name})");

					if ((loc is PrefabLocalization) != prefabs) continue;
					// only prefabs if prefabs == true, no prefabs if prefabs == false

					if (loc.Format == LocalizationSource.FormatType.DF) loc.LoadIntoDF(CurrentUIDict);
					else loc.LoadInto(CurrentUIDict);
				}
			}

			CurrentLanguageID = lang_id;
		}

		internal static void LoadLocalizationsForLanguage(string lang_id, bool prefabs = false) {
			lang_id = Gungeon.Languages.ValidateEntry(lang_id);
			Logger.Debug($"Scanning localizations for {lang_id}");

			foreach (var pair in Gungeon.Localizations.Pairs) {
				var loc = pair.Value;
				var target_lang_id = IDPool<LocalizationSource>.Resolve(loc.TargetLanguageID);
				if (target_lang_id == lang_id) {
					Logger.Debug($"Found localization '{pair.Key}' for table {loc.TargetStringTable} ({loc.GetType().Name})");

					if (loc.Format == LocalizationSource.FormatType.DF && loc.TargetStringTable != StringTable.UI) {
						Logger.Error("Cannot load a DF format localization for anything other than the UI string table.");
						continue;
					}

					if ((loc is PrefabLocalization) != prefabs) continue;
					// only prefabs if prefabs == true, no prefabs if prefabs == false

					switch (loc.TargetStringTable) {
						case StringTable.Core: loc.LoadInto(Patches.StringTableManager.m_coreTable); break;
						case StringTable.Enemies: loc.LoadInto(Patches.StringTableManager.m_enemiesTable); break;
						case StringTable.Intro: loc.LoadInto(Patches.StringTableManager.m_introTable); break;
						case StringTable.UI: break; // this is used/handled by DF stuff
						case StringTable.Synergies: loc.LoadInto(Patches.StringTableManager.m_synergyTable); break;
						case StringTable.Items: loc.LoadInto(Patches.StringTableManager.m_itemsTable); break;
						default: throw new InvalidOperationException($"Unknown string table {loc.TargetStringTable}");
					}
				}
			}

			CurrentLanguageID = lang_id;
		}

		/// <summary>
		/// Changes the current language to another one.
		/// </summary>
		/// <param name="id">Global ID of the language.</param>
		public static void ChangeLanguage(string id) {
			id = Gungeon.Languages.ValidateEntry(id);
			Logger.Debug($"Changing language to '{id}'");
			CurrentLanguageID = id;
			dfLanguageManager.ChangeGungeonLanguage();
			ReloadLocalizations();
		}

		internal static void ChangeLanguage(StringTableManager.GungeonSupportedLanguages lang) {
			var id = Gungeon.Languages.ValidateEntry(GungeonLanguage.LanguageToID(lang));
			ChangeLanguage(id);
		}

		/// <summary>
		/// Reloads loaded localization strings.
		/// </summary>
		public static void ReloadLocalizations() {
			Logger.Debug($"Reloading localizations");
			JournalEntry.ReloadDataSemaphore += 1;
			StringTableManager.ReloadAllTables();
			CurrentUIDict = new Dictionary<string, string>();
			Patches.StringTableManager.m_coreTable = new Dictionary<string, StringTableManager.StringCollection>();
			Patches.StringTableManager.m_enemiesTable = new Dictionary<string, StringTableManager.StringCollection>();
			Patches.StringTableManager.m_introTable = new Dictionary<string, StringTableManager.StringCollection>();
			Patches.StringTableManager.m_itemsTable = new Dictionary<string, StringTableManager.StringCollection>();
			Patches.StringTableManager.m_synergyTable = new Dictionary<string, StringTableManager.StringCollection>();
			Logger.Debug($"Loading prefab (builtin) localizations first");
			LoadLocalizationsForLanguage(CurrentLanguageID, prefabs: true);
			LoadDFLocalizationsForLanguage(CurrentLanguageID, prefabs: true);
			Logger.Debug($"Loading modded and runtime localizations");
			LoadLocalizationsForLanguage(CurrentLanguageID, prefabs: false);
			LoadDFLocalizationsForLanguage(CurrentLanguageID, prefabs: false);
			dfLanguageManager.ChangeGungeonLanguage(); // just to trigger the UI locales update
		}

		public static string GetString(StringTable table, string key) {
			if (key.StartsWithInvariant("#")) key = key.Substring(1);

			switch (table) {
			case StringTable.Core: return StringTableManager.GetString(key);
			case StringTable.Enemies: return StringTableManager.GetEnemiesString(key);
			case StringTable.Intro: return StringTableManager.GetIntroString(key);
			case StringTable.Items: return StringTableManager.GetItemsString(key);
			case StringTable.Synergies: return StringTableManager.GetSynergyString(key);
			case StringTable.UI: return Patches.dfLanguageManager.Instance.GetValue("#" + key);
			}

			return key;
		}

	}
}

