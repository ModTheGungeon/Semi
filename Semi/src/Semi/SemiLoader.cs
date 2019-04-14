using System;
using ModTheGungeon;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace Semi {
	public class InvalidConfigException : Exception {
		public InvalidConfigException(string message) : base($"Invalid config: {message}") { }
	}

	public class ModLoadException : Exception {
		public ModLoadException(string mod_id, string message) : base($"Failed loading mod '{mod_id}': {message}") { }
	}

	public static class SemiLoader {
		public class ModInfo {
			public Mod Instance { get; internal set; }
			public ModConfig Config { get; internal set; }
			public string Path { get; internal set; }

			public ModInfo(Mod instance, ModConfig config, string path) {
				Instance = instance;
				Config = config;
				Path = path;
			}
		}

#if DEBUG
		public const bool DEBUG_MODE = true;
#else
		public const bool DEBUG_MODE = false;
#endif

		public const string VERSION = "cont-dev";
		internal static bool Loaded = false;

        internal static Dictionary<string, ModInfo> Mods;
        internal static UnityEngine.GameObject ModsStorageObject;

		internal static UnityEngine.GameObject SpriteCollectionStorageObject;
		internal static UnityEngine.GameObject SpriteTemplateStorageObject;
		internal static UnityEngine.GameObject AnimationTemplateStorageObject;
        internal static Logger Logger = new Logger("Semi");

		internal static DebugConsole.Console Console;
		internal static DebugConsole.ConsoleController ConsoleController;

		internal static SGUI.SGUIRoot GUIRoot;

		internal static SpriteCollection EncounterIconCollection;

		//internal static GlobalSpriteCollectionManager AmmonomiconCollectionManager;
		//internal static GlobalSpriteCollectionManager ItemCollectionManager;


		internal static IEnumerator OnGameManagerAlive(GameManager mgr) {
			// INVESTIGATING:
			// native exception a couple seconds after entering a room
			// (varies in time, mostly consistent but not 100%, didn't happen once (at least
			//  not for a very long time) with same conditions where it happened)
			//
			//doesnt happen if paused in entered room
			// NOT fakeprefab related
			// no sprites added
			//no items
			//(no mod content)
			//only semi +the mod itself was loaded
			// investigate recent stuff:
			//   - the mgr argument on this method
			//   - the PickupObjectTreeBuilder base object thing
			//     (null exception is in a destructor :thinking:)
			//   - don't bother testing fakeprefab (tried commenting out the patch)
			// unsure if this was happening before but highly doubt it
			// doesn't happen in classic etgmod so...
			// try uncommenting one-by-one feature until it goes away?
			// also try not loading any mods at all

            Logger.Debug("GameManager alive");
			
            ModsStorageObject = new UnityEngine.GameObject("Semi Mod Loader");
			SpriteCollectionStorageObject = new UnityEngine.GameObject("Semi Mod Sprite Collections");
			SpriteTemplateStorageObject = new UnityEngine.GameObject("Semi Mod Sprite Templates");
			AnimationTemplateStorageObject = new UnityEngine.GameObject("Semi Mod Animation Templates");

			SpriteCollectionStorageObject.SetActive(false);
			SpriteTemplateStorageObject.SetActive(false);
			AnimationTemplateStorageObject.SetActive(false);

			UnityEngine.Object.DontDestroyOnLoad(ModsStorageObject);
			UnityEngine.Object.DontDestroyOnLoad(SpriteCollectionStorageObject);
			UnityEngine.Object.DontDestroyOnLoad(SpriteTemplateStorageObject);
			UnityEngine.Object.DontDestroyOnLoad(AnimationTemplateStorageObject);

            Mods = new Dictionary<string, ModInfo>();

			if (DEBUG_MODE) {
				Logger.Debug($"Debug mode active");
				GUIRoot = SGUI.SGUIRoot.Setup();

				SGUI.SGUIIMBackend.GetFont = (SGUI.SGUIIMBackend backend) => {
					if (Patches.MainMenuFoyerController.Instance?.VersionLabel == null) return null;
					return DebugConsole.FontCache.GungeonFont ?? (DebugConsole.FontCache.GungeonFont = DebugConsole.FontConverter.DFFontToUnityFont((dfFont)Patches.MainMenuFoyerController.Instance.VersionLabel.Font, 2));
				};

				Console = new DebugConsole.Console();
				ConsoleController = ModsStorageObject.AddComponent<DebugConsole.ConsoleController>();
				UnityEngine.Object.DontDestroyOnLoad(ConsoleController);
			}

			InitializeTreeBuilders();
			Logger.Debug($"Waiting frame to delete tree builder base objects");
			yield return null;

			Gungeon.Languages = new IDPool<I18N.Language>();
			Gungeon.Localizations = new IDPool<I18N.LocalizationSource>();

			LoadBuiltinLanguages();
			yield return null;

			LoadBuiltinLocalizations();
			yield return null;

			I18N.ChangeLanguage(GameManager.Options.CurrentLanguage);
			// call this once at the start to initialize the dictionaries
			// TODO @serialization Save language as ID in save file

			Gungeon.SpriteCollections = new IDPool<SpriteCollection>();
			Gungeon.SpriteTemplates = new IDPool<Sprite>();
			Gungeon.AnimationTemplates = new IDPool<SpriteAnimation>();
			yield return mgr.StartCoroutine(LoadIDMaps());

			EncounterIconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection.Wrap();
			//SimpleSpriteLoader.BaseSprite = Gungeon.Items["gungeon:singularity"].GetComponent<tk2dSprite>();
			//ItemCollectionManager = new GlobalSpriteCollectionManager(SimpleSpriteLoader.BaseSprite.Collection);

            FileHierarchy.Verify();
            LoadMods();

			BeginRegisteringContent();
			RunContentMods();
			CommitContent();
        }

		public static void BeginRegisteringContent() {
			EncounterIconCollection.BeginModifyingDefinitionList();
		}

		public static void CommitContent() {
			EncounterIconCollection.CommitDefinitionList();
			I18N.ReloadLocalizations();
		}

		internal static void RunContentMods() {
			Logger.Info("Running content mods");

			foreach (var mod in Mods) {
				Logger.Debug($"Registering content in '{mod.Key}'");
				mod.Value.Instance.RegisteringMode = true;
				mod.Value.Instance.RegisterContent();
				mod.Value.Instance.RegisteringMode = false;
			}
		}

        internal static void LoadMods() {
            Logger.Info("Loading mods");

            var mods_dir = FileHierarchy.ModsFolder;
            var mod_files = Directory.GetFileSystemEntries(mods_dir);

            var order_ary = SimpleListFileParser.ParseFile(FileHierarchy.ModsOrderFile, trim: true);
            var ignore_ary = SimpleListFileParser.ParseFile(FileHierarchy.ModsBlacklistFile, trim: true);

            List<string> loaded_mods_list = null;
            // we only need this list if there are mods with forced order

            if (order_ary != null) {
                Logger.Debug("Load order config exists");
                loaded_mods_list = new List<string>();

                for (int i = 0; i < order_ary.Length; i++) {
                    var filename = order_ary[i];
                    var mod_file = Path.Combine(FileHierarchy.ModsFolder, filename);
                    Logger.Debug($"Entry: '{mod_file}'");

                    if (ignore_ary != null && ignore_ary.Contains(filename)) continue;

                    if (File.Exists(mod_file)) throw new Exception("mod archives not supported yet");
                    if (!Directory.Exists(mod_file)) throw new InvalidConfigException($"The load order file specifies a '{filename}' mod but neither a file nor a directory exists with that name in the mods folder.");

                    loaded_mods_list.Add(filename);

                    LoadModDir(filename, mod_file);
                }
            }

            for (int i = 0; i < mod_files.Length; i++) {
                var mod_file = mod_files[i];
                Logger.Debug($"Entry: {mod_file}");

                var filename = Path.GetFileName(mod_file);

                if (filename == FileHierarchy.MODS_ORDER_FILE_NAME) continue;
                if (filename == FileHierarchy.MODS_BLACKLIST_FILE_NAME) continue;
                if (filename == FileHierarchy.MODS_CACHE_FOLDER_NAME) continue;

                if (ignore_ary != null && ignore_ary.Contains(filename)) continue;
                if (loaded_mods_list != null && loaded_mods_list.Contains(filename)) continue;

                if (File.Exists(mod_file)) throw new Exception("mod archives not supported yet");
                LoadModDir(filename, mod_file);
            }
        }

        internal static void ValidateModID(string mod_file, string id) {
            if (id.ToLowerInvariant() == id && !id.Contains(" ") && id.IsPureASCII()) return;
            throw new InvalidConfigException($"Tried loading mod '{mod_file}', but its ID specified in the config file is invalid. Make sure that it's all lowercase, with no spaces and only ASCII characters.");
        }

        internal static ResolveEventHandler GenerateModAssemblyResolver(string dir_path) {
            return delegate (object sender, ResolveEventArgs args) {
                string asm_path = Path.Combine(dir_path, new AssemblyName(args.Name).Name + ".dll");
                if (!File.Exists(asm_path)) {
                    return null;
                }
                return Assembly.LoadFrom(asm_path);
            };
        }

        internal static void LoadModDir(string dir_name, string dir_path) {
            var config_path = Path.Combine(dir_path, FileHierarchy.MOD_INFO_FILE_NAME);
            if (!File.Exists(config_path)) throw new InvalidConfigException($"Tried loading mod '{dir_name}' but it has no {FileHierarchy.MOD_INFO_FILE_NAME} file.");

            var mod_config = SerializationHelper.DeserializeFile<ModConfig>(config_path);
            if (mod_config.ID == null) throw new InvalidConfigException($"Tried loading mod '{dir_name}', but the config file does not specify an ID");
            ValidateModID(dir_name, mod_config.ID);

            var dll_name = mod_config.DLL ?? "mod.dll";
            var dll_path = Path.Combine(dir_path, dll_name);
            if (!File.Exists(config_path)) throw new InvalidConfigException($"Tried loading mod '{dir_name}' but it doesn't have the specified DLL file {dll_name}.");

            AppDomain.CurrentDomain.AssemblyResolve += GenerateModAssemblyResolver(dir_path);
                
            Assembly asm;
            using (FileStream f = File.OpenRead(dll_path)) {
                asm = AssemblyRelinker.GetRelinkedAssembly(dll_name, dll_path, f);
            }

            if (asm == null) throw new ModLoadException(mod_config.ID, "Failed loading/relinking assembly");

            Logger.Debug($"Searching for Mod subclasses");
            var types = asm.GetTypes();
            Logger.Debug($"{types.Length} type(s)");
            for (int i = 0; i < types.Length; i++) {
                var type = types[i];
                if (!typeof(Mod).IsAssignableFrom(type) || type.IsAbstract) continue;

                Mod mod_instance = (Mod)ModsStorageObject.AddComponent(type);
                mod_config.Instance = mod_instance;

				var mod_info = Mods[mod_config.ID] = new ModInfo(
								mod_instance,
								mod_config,
								dir_path
				);

				mod_instance.Info = mod_info;

                mod_instance.Loaded();
            }
        }

        internal static IEnumerator LoadIDMaps() {
            var asm = Assembly.GetExecutingAssembly();
            Logger.Debug("Loading IDMaps");

            using (StreamReader stream = new StreamReader(asm.GetManifestResourceStream("idmaps:items.txt"))) {
                Logger.Debug($"IDMap items.txt: stream = {stream}");
                Gungeon.Items = IDMapParser<PickupObject, Gungeon.ItemTag>.Parse(
                    stream,
                    "gungeon",
                    (id) => PickupObjectDatabase.GetById(int.Parse(id)),
					do_after: (id, item) => {
						((Semi.Patches.PickupObject)item).UniqueItemID = id;
					}
                );
            }

			yield return null;

            using (StreamReader stream = new StreamReader(asm.GetManifestResourceStream("idmaps:enemies.txt"))) {
                Logger.Debug($"IDMap enemies.txt: stream = {stream}");
                Gungeon.Enemies = IDMapParser<AIActor, Gungeon.EnemyTag>.Parse(
                    stream,
                    "gungeon",
                    (id) => EnemyDatabase.AssetBundle.LoadAsset<UnityEngine.GameObject>(id).GetComponent<AIActor>()
                );
            }
        }

		internal static void LoadBuiltinLanguages() {
			Gungeon.Languages["gungeon:english"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.ENGLISH);
			Gungeon.Languages["gungeon:rubel_test"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.RUBEL_TEST);
			Gungeon.Languages["gungeon:french"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.FRENCH);
			Gungeon.Languages["gungeon:spanish"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.SPANISH);
			Gungeon.Languages["gungeon:italian"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.ITALIAN);
			Gungeon.Languages["gungeon:german"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.GERMAN);
			Gungeon.Languages["gungeon:portuguese"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE);
			Gungeon.Languages["gungeon:japanese"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.JAPANESE);
			Gungeon.Languages["gungeon:korean"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.KOREAN);
			Gungeon.Languages["gungeon:russian"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.RUSSIAN);
			Gungeon.Languages["gungeon:polish"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.POLISH);
			Gungeon.Languages["gungeon:chinese"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.CHINESE);
		}

		internal static void LoadBuiltinLocalizations() {
			for (int i = 0; i <= (int)StringTableManager.GungeonSupportedLanguages.CHINESE; i++) {
				var lang = (StringTableManager.GungeonSupportedLanguages)i;

				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_core"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Core);
				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_enemies"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Enemies);
				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_intro"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Intro);
				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_items"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Items);
				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_synergies"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Synergies);
				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_ui"] = new I18N.PrefabLocalization(lang, I18N.StringTable.UI);
			}
		}

		internal static void InitializePickupObjectTreeBuilder() {
			var magic_lamp = PickupObjectDatabase.GetById(0);

			magic_lamp.gameObject.SetActive(false);
			var new_go = UnityEngine.Object.Instantiate(magic_lamp);
			magic_lamp.gameObject.SetActive(true);

			UnityEngine.Object.Destroy(new_go.GetComponent<PickupObject>());
			UnityEngine.Object.Destroy(new_go.GetComponent<tk2dSprite>());
			UnityEngine.Object.Destroy(new_go.GetComponent<EncounterTrackable>());
			var animator = new_go.GetComponent<tk2dSpriteAnimator>();
			if (animator != null) UnityEngine.Object.Destroy(animator);

			UnityEngine.Object.DontDestroyOnLoad(new_go);

			PickupObjectTreeBuilder.CleanBaseObject = new_go.gameObject;
			PickupObjectTreeBuilder.StoredBarrel = ((Gun)magic_lamp).barrelOffset.gameObject;
		}

		internal static void InitializeTreeBuilders() {
			InitializePickupObjectTreeBuilder();
		}
    }
}
