using System;
using ModTheGungeon;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.Linq;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

namespace Semi {
	/// <summary>
	/// Thrown when Semi encounters an issue with the setup of order.txt and blacklist.txt files.
	/// </summary>
	public class ModPreloadException : Exception {
		public ModPreloadException(string message) : base($"Failed loading mods: {message}") { }
	}

	/// <summary>
	/// Thrown when a mod fails to load for reasons other than misconfigured metadata.
	/// </summary>
	public class ModLoadException : Exception {
		public ModLoadException(string mod_id, string message) : base($"Failed loading mod '{mod_id}': {message}") { }
	}

	/// <summary>
	/// Thrown when a mod fails the checksum validation.
	/// </summary>
	public class ChecksumMismatchException : ModLoadException {
		public ChecksumMismatchException(string mod_id) : base(mod_id, "Checksum mismatch. The mod has been edited, likely by someone other than its author. For your safety, it's been disabled.") { }
	}

	/// <summary>
	/// Thrown when a mod metadata is invalid or points to files that don't exist.
	/// </summary>
	public class InvalidConfigException : ModLoadException {
		public InvalidConfigException(string mod_id, string message) : base(mod_id, $"Invalid config: {message}") { }
	}

	/// <summary>
	/// Thrown when a mod is outdated (targets an old api_version).
	/// </summary>
	public class OutdatedModException : ModLoadException {
		public OutdatedModException(string mod_id, int target_api_version) : base(mod_id, $"Mod is outdated - targets API version {target_api_version}, but you are running API version {SemiLoader.API_VERSION}") { }
	}

	/// <summary>
	/// Main class of the Semi mod loader.
	/// </summary>
	public static class SemiLoader {
		/// <summary>
		/// Representation of a loaded mod.
		/// </summary>
		public class ModInfo {
			/// <summary>
			/// Instance of the loaded mod component.
			/// </summary>
			/// <value>The instance.</value>
			public Mod Instance { get; internal set; }
			/// <summary>
			/// Loaded mod metadata.
			/// </summary>
			/// <value>The metadata.</value>
			public ModConfig Config { get; internal set; }
			/// <summary>
			/// Absolute path of the mod's unpacked directory.
			/// </summary>
			/// <value>The path to the mod.</value>
			public string Path { get; internal set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="T:Semi.SemiLoader.ModInfo"/> class.
			/// </summary>
			/// <param name="instance">Mod instance.</param>
			/// <param name="config">Metadata.</param>
			/// <param name="path">Path.</param>
			public ModInfo(Mod instance, ModConfig config, string path) {
				Instance = instance;
				Config = config;
				Path = path;
			}
		}

		/// <summary>
		/// Specifies whether debug mode is enabled.
		/// Debug mode enables some features of Semi that are intended to be disabled in release builds, for example the console.
		/// </summary>
#if DEBUG
		public const bool DEBUG_MODE = true;
#else
		public const bool DEBUG_MODE = false;
#endif

		/// <summary>
		/// String version of the mod loader.
		/// </summary>
#if DEBUG
		public const string VERSION = "0.1-dev";
#else
		public const string VERSION = "0.1";
#endif

		/// <summary>
		/// Version of the API (increased when breaking changes are made)
		/// </summary>
		public const int API_VERSION = 1;

		/// <summary>
		/// Set to true once Semi is loaded.
		/// </summary>
		internal static bool Loaded = false;

		/// <summary>
		/// Determines whether mods should undergo checksum validation.
		/// </summary>
		internal static bool ValidateMods = true;

		/// <summary>
		/// Determines whether .sum files should be written containing checksums of mods.
		/// </summary>
		internal static bool SaveModChecksums = false;

		/// <summary>
		/// Dictionary containing loaded mods.
		/// </summary>
        internal static Dictionary<string, ModInfo> Mods;

		/// <summary>
		/// GameObject containing MonoBehaviours of loaded mods.
		/// </summary>
        internal static UnityEngine.GameObject ModsStorageObject;

		/// <summary>
		/// GameObject containing StreamBufferUpdateBehaviour to update buffers on music streams (to let them play correctly).
		/// </summary>
		internal static UnityEngine.GameObject MusicStreamBufferUpdateObject;

		/// <summary>
		/// GameObject containing mod-registered sprite collections.
		/// </summary>
		internal static UnityEngine.GameObject SpriteCollectionStorageObject;

		/// <summary>
		/// GameObject containing mod-registered sprite templates.
		/// </summary>
		internal static UnityEngine.GameObject SpriteTemplateStorageObject;

		/// <summary>
		/// GameObject containing mod-registered animation templates.
		/// </summary>
		internal static UnityEngine.GameObject AnimationTemplateStorageObject;

		/// <summary>
		/// Semi Logger.
		/// </summary>
        internal static Logger Logger = new Logger("Semi");

		/// <summary>
		/// Debug console.
		/// </summary>
		internal static DebugConsole.Console Console;

		/// <summary>
		/// MonoBehaviour for handling the debug console.
		/// </summary>
		internal static DebugConsole.ConsoleController ConsoleController;

		/// <summary>
		/// SGUI GUI root reference, used for the debug console.
		/// </summary>
		internal static SGUI.SGUIRoot GUIRoot;

		/// <summary>
		/// Name of the mod currently being loaded (null if not loading any).
		/// </summary>
		internal static string CurrentLoadingModName;

		/// <summary>
		/// ID of the mod currently being loaded (null if not loading any).
		/// </summary>
		internal static string CurrentLoadingModID;

		/// <summary>
		/// A list of errors thrown by mods while loading them to show on the error screen page on startup.
		/// </summary>
		internal static List<ModError> ModLoadErrors;

		/// <summary>
		/// Reference to the sprite collection holding Ammonomicon icons.
		/// </summary>
		internal static SpriteCollection EncounterIconCollection;

		/// <summary>
		/// Set of currently active synergies.
		/// </summary>
		internal static HashSet<string> ActiveSynergyIDs = new HashSet<string>();

		/// <summary>
		/// Dictionary of delegates registered to run on synergy activation.
		/// </summary>
		internal static Dictionary<string, Gungeon.SynergyStateChangeAction> SynergyActivatedActions = new Dictionary<string, Gungeon.SynergyStateChangeAction>();

		/// <summary>
		/// Dictionary of delegates registered to run on synergy deactivation.
		/// </summary>
		internal static Dictionary<string, Gungeon.SynergyStateChangeAction> SynergyDeactivatedActions = new Dictionary<string, Gungeon.SynergyStateChangeAction>();

		/// <summary>
		/// Executed right before GameManager.Awake. Primary entry point.
		/// </summary>
		/// <param name="mgr">The GameManager instance.</param>
		internal static void OnGameManagerAlive(GameManager mgr) {
            Logger.Debug("ENTRYPOINT: GameManager alive");

			ModLoadErrors = new List<ModError>();

			Logger.Debug($"Initializing Semi storage GameObjects");
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
				Logger.Debug($"Initializing debug mode");

				var args = Environment.GetCommandLineArgs();
				for (int i = 0; i < args.Length; i++) {
					var arg = args[i];
					if (arg == "--disable-mod-validation") {
						ValidateMods = false;
					}
				}

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

			Gungeon.Languages = new IDPool<I18N.Language>();
			Gungeon.Localizations = new IDPool<I18N.LocalizationSource>();

			FileHierarchy.Verify();

			Logger.Debug($"Loading mods");
			LoadMods();
        }

		/// <summary>
		/// Executed right after GameManager.Awake. Secondary entry point.
		/// (Coroutine)
		/// </summary>
		/// <param name="mgr">The GameManager instance.</param>
		internal static IEnumerator OnGameManagerReady(GameManager mgr) {
			Logger.Debug("ENTRYPOINT: GameManager ready");

			InitializeAudio();

			InitializeTreeBuilders();
			Logger.Debug($"Waiting frame to delete tree builder base objects");
			yield return null;

			InitializeBuiltinLanguages();
			yield return null;

			InitializeBuiltinLocalizations();
			yield return null;

			I18N.ChangeLanguage(GameManager.Options.CurrentLanguage);
			// call this once at the start to initialize the dictionaries
			// TODO @serialization Save language as ID in save file

			Gungeon.SpriteCollections = new IDPool<SpriteCollection>();
			Gungeon.SpriteTemplates = new IDPool<Sprite>();
			Gungeon.AnimationTemplates = new IDPool<SpriteAnimation>();
			yield return mgr.StartCoroutine(InitializeIDMaps());

			EncounterIconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection.Wrap();

			BeginRegisteringContent();
			RegisterContentInMods();
			CommitContent();

			InitializeContentInMods();
		}

		/// <summary>
		/// Sets registries and other objects to expect rapid modification.
		/// This is used for performance reasons so that certain objects can create fast-access caches but also
		/// allow mods to use the various available abstractions.
		/// </summary>
		internal static void BeginRegisteringContent() {
			EncounterIconCollection.BeginModifyingDefinitionList();
		}

		/// <summary>
		/// Sets registries and other objects to stop expecting rapid modification.
		/// This allows them to create the necessary internal caches.
		/// </summary>
		internal static void CommitContent() {
			EncounterIconCollection.CommitDefinitionList();
			I18N.ReloadLocalizations();
			InitializeStreamBufferUpdateBehaviourCache();
		}

		/// <summary>
		/// Runs RegisterContent() in every loaded mod, allowing it to call the Register* set of methods while inside.
		/// </summary>
		internal static void RegisterContentInMods() {
			Logger.Info("Running RegisterContent()");

			foreach (var mod in Mods) {
				Logger.Debug($"Registering content in '{mod.Key}'");
				mod.Value.Instance.RegisteringMode = true;
				mod.Value.Instance.RegisterContent();
				mod.Value.Instance.RegisteringMode = false;
			}
		}

		/// <summary>
		/// Runs InitializeContent() in every loaded mod.
		/// </summary>
		internal static void InitializeContentInMods() {
			Logger.Info("Running InitializeContent()");

			foreach (var mod in Mods) {
				Logger.Debug($"Initializing content in '{mod.Key}'");
				mod.Value.Instance.InitializeContent();
			}
		}

		/// <summary>
		/// Loads mods from the SemiMods folder, respecting order.txt, blacklist.txt and MySemiMods.txt.
		/// </summary>
        internal static void LoadMods() {
            Logger.Info("Loading mods");

            var mods_dir = FileHierarchy.ModsFolder;
            var mod_files = Directory.GetFileSystemEntries(mods_dir);

            var order_ary = SimpleListFileParser.ParseFile(FileHierarchy.ModsOrderFile, trim: true);
            var ignore_ary = SimpleListFileParser.ParseFile(FileHierarchy.ModsBlacklistFile, trim: true);
			var my_mods_ary = SimpleListFileParser.ParseFile(FileHierarchy.MyModsFile, trim: true);

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
					if (!Directory.Exists(mod_file)) throw new ModPreloadException($"The load order file specifies a '{filename}' mod but neither a file nor a directory exists with that name in the mods folder.");

                    loaded_mods_list.Add(filename);

					try {
						LoadModDir(filename, mod_file, my_mods_ary);
					} catch (Exception e) {
						ModLoadErrors.Add(new ModError {
							Name = CurrentLoadingModName,
							ID = CurrentLoadingModID ?? filename,
							Exception = e
						});
						Logger.Error($"Failed loading mod: [{e.GetType().Name}] {e.Message}");
						Logger.ErrorPretty(e.StackTrace);
					}
                }
            }

            for (int i = 0; i < mod_files.Length; i++) {
                var mod_file = mod_files[i];
                Logger.Debug($"Entry: {mod_file}");

                var filename = Path.GetFileName(mod_file);

                if (filename == FileHierarchy.MODS_ORDER_FILE_NAME) continue;
                if (filename == FileHierarchy.MODS_BLACKLIST_FILE_NAME) continue;
                if (filename == FileHierarchy.MODS_CACHE_FOLDER_NAME) continue;
				if (filename.EndsWithInvariant(".sum")) continue;

                if (ignore_ary != null && ignore_ary.Contains(filename)) continue;
                if (loaded_mods_list != null && loaded_mods_list.Contains(filename)) continue;

                if (File.Exists(mod_file)) throw new Exception("mod archives not supported yet");
                try {
					LoadModDir(filename, mod_file, my_mods_ary);
				} catch (Exception e) {
					ModLoadErrors.Add(new ModError {
						Name = CurrentLoadingModName,
						ID = CurrentLoadingModID ?? filename,
						Exception = e
					});
					Logger.Error($"Failed loading mod: [{e.GetType().Name}] {e.Message}");
					Logger.ErrorPretty(e.StackTrace);
				}
            }
        }

		/// <summary>
		/// Makes sure that a mod's ID is alphanumeric and contains no spaces. Throws if this is violated.
		/// </summary>
		/// <param name="mod_file">Path to the mod.</param>
		/// <param name="id">The mod's ID.</param>
        internal static void ValidateModID(string mod_file, string id) {
            if (id.ToLowerInvariant() == id && !id.Contains(" ") && id.IsPureASCII()) return;
            throw new InvalidConfigException(id, $"Tried loading mod '{mod_file}', but its ID specified in the config file is invalid. Make sure that it's all lowercase, with no spaces and only ASCII characters.");
        }

		/// <summary>
		/// Generates an assembly resolver event based on the mod's path to allow for loading DLLs inside the mod's folder.
		/// </summary>
		/// <returns>The mod assembly resolver.</returns>
		/// <param name="dir_path">Path to the mod's directory.</param>
        internal static ResolveEventHandler GenerateModAssemblyResolver(string dir_path) {
            return delegate (object sender, ResolveEventArgs args) {
                string asm_path = Path.Combine(dir_path, new AssemblyName(args.Name).Name + ".dll");
                if (!File.Exists(asm_path)) {
                    return null;
                }
                return Assembly.LoadFrom(asm_path);
            };
        }

		/// <summary>
		/// Loads a mod from a directory.
		/// </summary>
		/// <param name="dir_name">Name of the directory.</param>
		/// <param name="dir_path">Path to the directory.</param>
		/// <param name="trusted_mod_ids">An array of mod IDs that are trusted and don't need to have their checksum verified.</param>
        internal static void LoadModDir(string dir_name, string dir_path, string[] trusted_mod_ids = null) {
			CurrentLoadingModID = null;
			CurrentLoadingModName = null;

            var config_path = Path.Combine(dir_path, FileHierarchy.MOD_INFO_FILE_NAME);
            if (!File.Exists(config_path)) throw new InvalidConfigException("???", $"Tried loading mod '{dir_name}' but it has no {FileHierarchy.MOD_INFO_FILE_NAME} file.");

            var mod_config = SerializationHelper.DeserializeFile<ModConfig>(config_path);
            if (mod_config.ID == null) throw new InvalidConfigException("???", $"Tried loading mod '{dir_name}', but the config file does not specify an ID");
			if (mod_config.APIVersion == null) throw new InvalidConfigException(mod_config.ID, $"Mod does not specify an api_version, and therefore cannot be loaded");

			if (mod_config.APIVersion.Value != API_VERSION) throw new OutdatedModException(mod_config.ID, mod_config.APIVersion.Value);

			CurrentLoadingModID = mod_config.ID;
			CurrentLoadingModName = mod_config.Name;

            ValidateModID(dir_name, mod_config.ID);

			if (trusted_mod_ids != null && trusted_mod_ids.Contains(mod_config.ID)) {
				Logger.Debug($"Is trusted mod - not verifying checksum");
			} else if (ValidateMods) {
				Logger.Debug($"Verifying mod checksum");
				var hash = ModVerification.GetModHash(dir_path, mod_config);
				if (SaveModChecksums) {
					File.WriteAllBytes(Path.Combine(FileHierarchy.ModsFolder, $"{dir_name}.sum"), hash);
				}
				if (ModVerification.ValidateHashOnline(mod_config.ID, hash)) {
					Logger.Info($"Valid mod checksum");
				} else {
					Logger.Error($"Invalid mod checksum");
					throw new ChecksumMismatchException(mod_config.ID);
				}
			}

            var dll_name = mod_config.DLL ?? "mod.dll";
            var dll_path = Path.Combine(dir_path, dll_name);
            if (!File.Exists(config_path)) throw new InvalidConfigException(mod_config.ID, $"Tried loading mod '{dir_name}' but it doesn't have the specified DLL file {dll_name}.");

            AppDomain.CurrentDomain.AssemblyResolve += GenerateModAssemblyResolver(dir_path);
                
            Assembly asm;
            using (FileStream f = File.OpenRead(dll_path)) {
                asm = AssemblyRelinker.GetRelinkedAssembly(dll_name, dll_path, f);
            }

            if (asm == null) throw new ModLoadException(mod_config.ID, "Failed loading/relinking assembly");

            Logger.Debug($"Searching for Mod subclasses");
			Type[] types = null;
			try {
				types = asm.GetTypes();
			} catch (ReflectionTypeLoadException e) {
				Logger.Error($"Failed loading types from mod:");
				if (e.LoaderExceptions == null) {
					Logger.ErrorIndent("(No loader exceptions)");
				} else {
					for (int i = 0; i < e.LoaderExceptions.Length; i++) {
						var ex = e.LoaderExceptions[i];
						Logger.ErrorIndent($"- [{ex.GetType().Name}] {ex.Message}");
					}
				}
				return;
			}
            Logger.Debug($"{types.Length} type(s)");
            for (int i = 0; i < types.Length; i++) {
                var type = types[i];
                if (!typeof(Mod).IsAssignableFrom(type) || type.IsAbstract) continue;

                Mod mod_instance = (Mod)ModsStorageObject.AddComponent(type);
				mod_instance.Logger = new Logger(mod_config.Name ?? mod_config.ID);
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

		/// <summary>
		/// Opens the load error screen if ModLoadErrors has at least one entry.
		/// </summary>
		internal static void OpenLoadErrorScreenIfNecessary() {
			Logger.Debug($"Checking if error screen needs to be opened: {ModLoadErrors.Count} error(s)");
			if (ModLoadErrors.Count > 0) {
				Logger.Debug($"Opening error screen");
				UI.OpenLoadErrorScreen(ModLoadErrors);
			}
		}

		/// <summary>
		/// Converts any list or array of integer item IDs into an array of pooled string IDs.
		/// </summary>
		/// <returns>The string ID list.</returns>
		/// <param name="ids">The numeric ID list.</param>
		internal static string[] ConvertItemIDList(IList<int> ids) {
			var ary = new string[ids.Count];
			for (int i = 0; i < ids.Count; i++) {
				var id = ids[i];
				var string_id = ((Patches.PickupObject)PickupObjectDatabase.Instance.Objects[id])?.UniqueItemID;
				ary[i] = string_id;
			}
			return ary;
		}

		/// <summary>
		/// Loads ID maps (mappings of the game's internal numeric IDs to the string ID system) from the assembly's resources.
		/// (Coroutine)
		/// </summary>
        internal static IEnumerator InitializeIDMaps() {
			Logger.Debug($"INITIALIZING: ID MAPS");
            var asm = Assembly.GetExecutingAssembly();
            
			Logger.Debug("IDMap: items.txt");
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

			Logger.Debug("IDMap: enemies.txt");
            using (StreamReader stream = new StreamReader(asm.GetManifestResourceStream("idmaps:enemies.txt"))) {
                Logger.Debug($"IDMap enemies.txt: stream = {stream}");
                Gungeon.Enemies = IDMapParser<AIActor, Gungeon.EnemyTag>.Parse(
                    stream,
                    "gungeon",
					(id) => EnemyDatabase.Instance.Entries[int.Parse(id)].GetPrefab<AIActor>()
                );
            }

			Logger.Debug("IDMap: synergies.txt");
			using (StreamReader stream = new StreamReader(asm.GetManifestResourceStream("idmaps:synergies.txt"))) {
				Logger.Debug($"IDMap synergies.txt: stream = {stream}");
				Gungeon.Synergies = IDMapParser<AdvancedSynergyEntry, SynergyEntry.SynergyActivation>.Parse(
					stream,
					"gungeon",
					(id) => GameManager.Instance.SynergyManager.synergies[int.Parse(id)],
					do_after: (id, gungeon_syn) => {
						var syn = (Semi.Patches.AdvancedSynergyEntry)gungeon_syn;

						syn.OptionalGuns = ConvertItemIDList(syn.OptionalGunIDs);
						syn.MandatoryGuns = ConvertItemIDList(syn.MandatoryGunIDs);
						syn.OptionalItems = ConvertItemIDList(syn.OptionalItemIDs);
						syn.MandatoryItems = ConvertItemIDList(syn.MandatoryItemIDs);
						syn.UniqueID = id;

						// null these out so that accessing these fields will error
						syn.OptionalGunIDs = syn.MandatoryGunIDs = syn.OptionalItemIDs = syn.MandatoryItemIDs = null;
					}
				);
			}
			yield return null;
        }

		/// <summary>
		/// Initializes Gungeon.Languages with IDs corresponding to the languages that Gungeon provides out of the box.
		/// </summary>
		internal static void InitializeBuiltinLanguages() {
			Logger.Debug($"INITIALIZING: BUILTIN LANGUAGES");
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

		/// <summary>
		/// Initializes Gungeon.Localizations with IDs corresponding to the languages that Gungeon provides out of the box.
		/// </summary>
		internal static void InitializeBuiltinLocalizations() {
			Logger.Debug($"INITIALIZING: BUILTIN LOCALIZATIONS");
			for (int i = 0; i <= (int)StringTableManager.GungeonSupportedLanguages.CHINESE; i++) {
				var lang = (StringTableManager.GungeonSupportedLanguages)i;

				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_core"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Core);
				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_enemies"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Enemies);
				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_intro"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Intro);
				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_items"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Items);
				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_synergies"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Synergies);
				Gungeon.Localizations[$"{I18N.GungeonLanguage.LanguageToID(lang)}_ui"] = new I18N.PrefabLocalization(lang, I18N.StringTable.UI, format: I18N.LocalizationSource.FormatType.DF);

				var lang_id = I18N.GungeonLanguage.LanguageToID(lang);
				var lang_name = IDPool<bool>.Split(lang_id).Name;
				var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"loc:{lang_name}.txt");
				if (stream != null) {
					var text = new StreamReader(stream).ReadToEnd();
					Logger.Debug($"Found Semi localization stream for {lang_id}");
					Gungeon.Localizations[$"semi:{lang_name}_core"] = new I18N.RuntimeLocalization("semi", text, lang_id, I18N.StringTable.Core);
					stream.Close();
				}
			}
		}

		/// <summary>
		/// Invokes the delegates registered to run when this synergy is activated, passing the player to them.
		/// </summary>
		/// <param name="id">ID of the synergy.</param>
		/// <param name="p">PlayerController to pass on.</param>
		internal static void InvokeSynergyActivated(string id, PlayerController p) {
			Logger.Debug($"Synergy activated: '{id}'");
			id = IDPool<AdvancedSynergyEntry>.Resolve(id);
			Gungeon.SynergyStateChangeAction action = null;
			if (SynergyActivatedActions.TryGetValue(id, out action)) {
				action.Invoke(p);
			}
		}

		/// <summary>
		/// Invokes the delegates registered to run when this synergy is deactivated, passing the player to them.
		/// </summary>
		/// <param name="id">ID of the synergy.</param>
		/// <param name="p">PlayerController to pass on.</param>
		internal static void InvokeSynergyDeactivated(string id, PlayerController p) {
			Logger.Debug($"Synergy deactivated: '{id}'");
			id = IDPool<AdvancedSynergyEntry>.Resolve(id);
			Gungeon.SynergyStateChangeAction action = null;
			if (SynergyDeactivatedActions.TryGetValue(id, out action)) {
				action.Invoke(p);
			}
		}

		/// <summary>
		/// Initializes the modded audio subsystem (RayAudio).
		/// </summary>
		internal static void InitializeAudio() {
			Logger.Debug($"INITIALIZING: AUDIO");
			Gungeon.ModAudioTracks = new IDPool<Audio>();

			Logger.Debug($"Starting audio device");
			RayAudio.AudioDevice.Initialize();

			InitializeStreamBufferUpdateBehaviour();
		}

		/// <summary>
		/// Initializes the stream buffer update object, which makes sure to refresh music stream buffers every frame.
		/// </summary>
		internal static void InitializeStreamBufferUpdateBehaviour() {
			Logger.Debug($"INITIALIZING: AUDIO STREAM BUFFER UPDATE");
			MusicStreamBufferUpdateObject = new GameObject("SEMI: Music Stream Buffer Update Behaviour");
			MusicStreamBufferUpdateObject.AddComponent<StreamBufferUpdateBehaviour>();
		}

		/// <summary>
		/// Initializes the stream buffer update object's cache of Audio tracks.
		/// </summary>
		internal static void InitializeStreamBufferUpdateBehaviourCache() {
			Logger.Debug($"INITIALIZING: AUDIO STREAM BUFFER UPDATE CACHE");

			var len = Gungeon.ModAudioTracks.Count;

			StreamBufferUpdateBehaviour.Paused = true;
			StreamBufferUpdateBehaviour.Tracks = new List<Audio>();

			foreach (var tr in Gungeon.ModAudioTracks.Entries) {
				StreamBufferUpdateBehaviour.Tracks.Add(tr);
			}

			StreamBufferUpdateBehaviour.Paused = false;
		}

		/// <summary>
		/// Initializes the pickup object tree builder static class to facilitate internal creation of items.
		/// </summary>
		internal static void InitializePickupObjectTreeBuilder() {
			Logger.Debug($"INITIALIZING: PICKUP OBJECT TREE BUILDER");

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

		/// <summary>
		/// Initializes the entity tree builder static class to facilitate internal creation of entities (enemies).
		/// </summary>
		internal static void InitializeEntityTreeBuilder() {
			Logger.Debug($"INITIALIZING: ENTITY TREE BUILDER");

			var bullet_kin = (UnityEngine.GameObject)EnemyDatabase.AssetBundle.LoadAsset("BulletMan");
			bullet_kin.SetActive(false);
			var new_go = UnityEngine.Object.Instantiate(bullet_kin);
			bullet_kin.SetActive(true);

			new_go.DestroyComponentIfExists<tk2dSprite>();
			new_go.DestroyComponentIfExists<tk2dSpriteAnimator>();
			new_go.DestroyComponentIfExists<SpeculativeRigidbody>();
			new_go.DestroyComponentIfExists<AIActor>();
			new_go.DestroyComponentIfExists<AIShooter>();
			new_go.DestroyComponentIfExists<AIBulletBank>();
			new_go.DestroyComponentIfExists<HitEffectHandler>();
			new_go.DestroyComponentIfExists<HealthHaver>();
			new_go.DestroyComponentIfExists<KnockbackDoer>();
			new_go.DestroyComponentIfExists<AIAnimator>();
			new_go.DestroyComponentIfExists<ObjectVisibilityManager>();
			new_go.DestroyComponentIfExists<BehaviorSpeculator>();
			new_go.DestroyComponentIfExists<EncounterTrackable>();

			UnityEngine.Object.DontDestroyOnLoad(new_go);

			EntityTreeBuilder.CleanBaseObject = new_go;

			var new_corpse_go = UnityEngine.Object.Instantiate(bullet_kin.GetComponent<AIActor>().CorpseObject);
			new_corpse_go.DestroyComponentIfExists<DebrisObject>();
			new_corpse_go.DestroyComponentIfExists<tk2dSprite>();
			new_corpse_go.DestroyComponentIfExists<tk2dSpriteAnimator>();

			UnityEngine.Object.DontDestroyOnLoad(new_corpse_go);

			EntityTreeBuilder.CleanBaseCorpseObject = new_corpse_go;

			var new_bullet_go = UnityEngine.Object.Instantiate(bullet_kin.GetComponent<AIBulletBank>().Bullets[0].BulletObject);
			new_bullet_go.DestroyComponentIfExists<BulletScriptBehavior>();
			new_bullet_go.DestroyComponentIfExists<Projectile>();

			UnityEngine.Object.DontDestroyOnLoad(new_bullet_go);

			EntityTreeBuilder.CleanBaseBulletObject = new_bullet_go;

		}

		/// <summary>
		/// Initializes static tree builder classes, that facilitate the internal creation of various ingame elements like items.
		/// </summary>
		internal static void InitializeTreeBuilders() {
			Logger.Debug($"INITIALIZING: TREE BUILDERS");
			InitializePickupObjectTreeBuilder();
		}

		/// <summary>
		/// Initializes fields and objects depending on the presence of the main menu, such as acquiring the current font or the UI manager responsible for the main menu.
		/// </summary>
		internal static void InitializeMainMenuUIHelpers() {
			Logger.Debug($"INITIALIZING: MAIN MENU UI HELPERS");
			UI.MainMenuGUIManager = dfGUIManager.ActiveManagers.ElementAt(2);

			UI.GungeonFont = Patches.MainMenuFoyerController.Instance.VersionLabel.Font;

			InitializeLoadErrorScreen();
		}

		/// <summary>
		/// Initializes the load error screen that displays exceptions thrown while loading mods.
		/// </summary>
		internal static void InitializeLoadErrorScreen() {
			Logger.Debug($"INITIALIZING: LOAD ERROR SCREEN");
			var title = GameUIRoot.Instance.Manager.AddControl<dfLabel>();
			title.zindex = 3;
			title.AutoSize = true;
			title.Text = "Error";
			title.BackgroundColor = Color.black;
			title.Color = Color.red;
			title.Font = UI.GungeonFont;
			title.TextScale = 3;
			title.IsVisible = false;
			title.Position = new Vector3(-title.Size.x / 2, Screen.height / 2, 0);
			UI.LoadErrorTitle = title;

			var subtitle = GameUIRoot.Instance.Manager.AddControl<dfLabel>();
			subtitle.zindex = 3;
			subtitle.AutoSize = true;
			subtitle.Text = "Semi failed to load certain installed mods.";
			subtitle.BackgroundColor = Color.black;
			subtitle.Color = Color.gray;
			subtitle.Font = UI.GungeonFont;
			subtitle.TextScale = 3;
			subtitle.IsVisible = false;
			subtitle.Position = new Vector3(-subtitle.Size.x / 2, title.Position.y - title.Size.y / 2 - 15);
			UI.LoadErrorSubtitle = subtitle;

			var ok_button = GameUIRoot.Instance.Manager.AddControl<dfButton>();
			ok_button.zindex = 3;
			ok_button.AutoSize = true;
			ok_button.Text = "OK";
			ok_button.Color = Color.white;
			ok_button.NormalBackgroundColor = new Color(255 / 66f, 255 / 66f, 255 / 66f);
			ok_button.FocusBackgroundColor = new Color(255 / 66f, 255 / 66f, 255 / 66f);
			ok_button.HoverBackgroundColor = new Color(255 / 96f, 255 / 96f, 255 / 96f);
			ok_button.TextScale = 3;
			ok_button.IsVisible = false;
			ok_button.Position = new Vector3(-ok_button.Size.x / 2, -Screen.height / 2);
			ok_button.Click += (control, mouseEvent) => {
				mouseEvent.Use();
				UI.CloseLoadErrorScreen();
			};
			UI.LoadErrorOKButton = ok_button;
		}
    }
}
