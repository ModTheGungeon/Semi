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
	/// Thrown when a mod depends on a mod that isn't installed.
	/// </summary>
	public class MissingDependencyException : ModLoadException {
		public MissingDependencyException(string mod_id, string dep_id) : base(mod_id, $"Missing dependency: mod '{dep_id}' is not installed") { }
	}

	/// <summary>
	/// Thrown when two mods depend on eachother.
	/// </summary>
	public class CyclicDependencyException : ModLoadException {
		public CyclicDependencyException(string mod_id, string dep_id) : base(mod_id, $"Cyclic dependency detected: '{dep_id}' caused '{mod_id}' to load, but now '{mod_id}' wants to cause '{dep_id}' to load") { }
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
			/// Initializes a new instance of the <see cref="Semi.SemiLoader.ModInfo"/> class.
			/// </summary>
			/// <param name="instance">Mod instance.</param>
			/// <param name="config">Metadata.</param>
			/// <param name="path">Path.</param>
			public ModInfo(ModConfig config, string path, Mod instance = null) {
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
		/// The mod approval system is deprecated.
		/// </summary>
		internal static bool ValidateMods = false;

		/// <summary>
		/// Determines whether .sum files should be written containing checksums of mods.
		/// </summary>
		internal static bool SaveModChecksums = false;

		/// <summary>
		/// List determining the final order of loading mods.
		/// </summary>
		internal static List<ModConfig> ModLoadOrder;

		/// <summary>
		/// List containing all the mod infos for all the mods present in SemiMods.
		/// </summary>
		internal static Dictionary<string, ModInfo> FlatModInfos;

		/// <summary>
		/// Dictionary containing loaded mods.
		/// </summary>
        internal static Dictionary<string, ModInfo> Mods;

		/// <summary>
		/// GameObjects containing MonoBehaviours of loaded mods.
		/// </summary>
        internal static List<UnityEngine.GameObject> ModsStorageObjects;

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
		public static DebugConsole.Console Console;

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
		internal static HashSet<ID> ActiveSynergyIDs = new HashSet<ID>();

		/// <summary>
		/// Dictionary of delegates registered to run on synergy activation.
		/// </summary>
		internal static Dictionary<ID, Registry.SynergyStateChangeAction> SynergyActivatedActions = new Dictionary<ID, Registry.SynergyStateChangeAction>();

		/// <summary>
		/// Dictionary of delegates registered to run on synergy deactivation.
		/// </summary>
		internal static Dictionary<ID, Registry.SynergyStateChangeAction> SynergyDeactivatedActions = new Dictionary<ID, Registry.SynergyStateChangeAction>();

		/// <summary>
		/// Executed right before GameManager.Awake. Primary entry point.
		/// </summary>
		/// <param name="mgr">The GameManager instance.</param>
		internal static void OnGameManagerAlive(GameManager mgr) {
            Logger.Debug("ENTRYPOINT: GameManager alive");

			ModLoadErrors = new List<ModError>();

			Logger.Debug($"Initializing Semi storage GameObjects");
			ModsStorageObjects = new List<GameObject>();
			SpriteCollectionStorageObject = new UnityEngine.GameObject("Semi Mod Sprite Collections");
			SpriteTemplateStorageObject = new UnityEngine.GameObject("Semi Mod Sprite Templates");
			AnimationTemplateStorageObject = new UnityEngine.GameObject("Semi Mod Animation Templates");

			SpriteCollectionStorageObject.SetActive(false);
			SpriteTemplateStorageObject.SetActive(false);
			AnimationTemplateStorageObject.SetActive(false);

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
				var console_go = new GameObject("Debug Console");
				UnityEngine.Object.DontDestroyOnLoad(console_go);
				ModsStorageObjects.Add(console_go);
				ConsoleController = console_go.AddComponent<DebugConsole.ConsoleController>();
				UnityEngine.Object.DontDestroyOnLoad(ConsoleController);
			}

			Registry.Languages = new IDPool<I18N.Language>();
			Registry.Localizations = new IDPool<I18N.LocalizationSource>();

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

			Registry.SpriteCollections = new IDPool<SpriteCollection>();
			Registry.SpriteTemplates = new IDPool<Sprite>();
			Registry.AnimationTemplates = new IDPool<SpriteAnimation>();
			yield return mgr.StartCoroutine(InitializeIDMaps());

			EncounterIconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection.Wrap();

			InitializeModOptions();

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

			foreach (var mod in Mods) {
				var page = UI.CreateModOptionsPage(mod.Value);
				mod.Value.Instance.OptionsPage = page;
				for (var i = 0; i < mod.Value.Instance.MenuOptions.Count; i++) {
					var opt = mod.Value.Instance.MenuOptions[i];
					opt.Insert(page);
				}
			}
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
			FlatModInfos = new Dictionary<string, ModInfo>();

			var mods_dir = FileHierarchy.ModsFolder;
			var mod_files = Directory.GetFileSystemEntries(mods_dir);
			var ignore_ary = SimpleListFileParser.ParseFile(FileHierarchy.ModsBlacklistFile, trim: true);
			var order_ary = SimpleListFileParser.ParseFile(FileHierarchy.ModsOrderFile, trim: true);
			List<ModInfo> ordered_infos = null;

			Logger.Debug($"Preloading mods.");

			for (int i = 0; i < mod_files.Length; i++) {
				var mod_file = mod_files[i];
				Logger.Debug($"Entry: {mod_file}");

				var filename = Path.GetFileName(mod_file);

				if (filename == FileHierarchy.MODS_ORDER_FILE_NAME) continue;
				if (filename == FileHierarchy.MODS_BLACKLIST_FILE_NAME) continue;
				if (filename == FileHierarchy.MODS_CACHE_FOLDER_NAME) continue;
				if (filename.EndsWithInvariant(".sum")) continue;

				if (ignore_ary != null && ignore_ary.Contains(filename)) continue;

				if (File.Exists(mod_file)) throw new Exception("mod archives not supported yet");
				try {
					var is_ordered = order_ary != null && order_ary.Contains(filename);
					
					var info = PreloadModDir(filename, mod_file, order_ary != null && order_ary.Contains(filename));

					if (is_ordered) {
						if (ordered_infos == null) ordered_infos = new List<ModInfo>();
						ordered_infos.Add(info);
					}
				} catch (Exception e) {
					ModLoadErrors.Add(new ModError {
						Name = CurrentLoadingModName,
						ID = CurrentLoadingModID ?? filename,
						Exception = e
					});
					Logger.Error($"Failed preloading mod: [{e.GetType().Name}] {e.Message}");
					Logger.ErrorPretty(e.StackTrace);
				}
			}

			Logger.Debug($"{FlatModInfos.Count} mods found.");

			ModLoadOrder = new List<ModConfig>();

			if (ordered_infos != null) {
				Logger.Debug($"Loading ordered mods first");

				for (var i = 0; i < ordered_infos.Count; i++) {
					var mod_info = ordered_infos[i];
					Logger.Debug($"Forced order mod #{i + 1}: '{mod_info.Config.ID}'");

					try {
						LoadMod(mod_info);
					} catch (Exception e) {
						ModLoadErrors.Add(new ModError {
							Name = CurrentLoadingModName,
							ID = CurrentLoadingModID ?? "???",
							Exception = e
						});
						Logger.Error($"Failed loading mod: [{e.GetType().Name}] {e.Message}");
						Logger.ErrorPretty(e.StackTrace);
					}
                }
            }

            Logger.Debug($"Loading mods");

			foreach (var mod in FlatModInfos) {
				Logger.Debug($"Mod: '{mod.Key}'");

				try {
					LoadMod(mod.Value);
				} catch (Exception e) {
					ModLoadErrors.Add(new ModError {
						Name = CurrentLoadingModName,
						ID = CurrentLoadingModID ?? "???",
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
		/// Preloads a mod into a flat dictionary to enable dependency resolution later.
		/// </summary>
		/// <param name="dir_name">Name of the extracted mod dir.</param>
		/// <param name="dir_path">Path to the extracted mod dir.</param>
		/// <param name="priority">If set to <c>true</c>, Priority will be set on the new ModInfo.</param>
		internal static ModInfo PreloadModDir(string dir_name, string dir_path, bool priority = false) {
			var config_path = Path.Combine(dir_path, FileHierarchy.MOD_INFO_FILE_NAME);
			if (!File.Exists(config_path)) throw new InvalidConfigException("???", $"Tried loading mod '{dir_name}' but it has no {FileHierarchy.MOD_INFO_FILE_NAME} file.");

			var mod_config = SerializationHelper.DeserializeFile<ModConfig>(config_path);
			if (mod_config.ID == null) throw new InvalidConfigException("???", $"Tried loading mod '{dir_name}', but the config file does not specify an ID");
			ValidateModID(dir_path, mod_config.ID);
			if (mod_config.APIVersion == null) throw new InvalidConfigException(mod_config.ID, $"Mod does not specify an api_version, and therefore cannot be loaded");

			if (mod_config.APIVersion.Value != API_VERSION) throw new OutdatedModException(mod_config.ID, mod_config.APIVersion.Value);

			mod_config.Priority = priority;

			var mod_info = new ModInfo(mod_config, dir_path, null);

			Logger.Debug($"Adding config for '{mod_config.ID}'");
			FlatModInfos[mod_config.ID] = mod_info;

			return mod_info;
		}

		/// <summary>
		/// Loads a mod.
		/// </summary>
		/// <param name="info">Preloaded mod info.</param>
		/// <param name="tree_history">Hash set of mod IDs in the dependency tree. This is used to detect cyclic dependencies.</param>
		internal static void LoadMod(ModInfo info, HashSet<string> tree_history = null) {
			Logger.Debug($"Loading: {info.Config.ID}");
			CurrentLoadingModID = null;
			CurrentLoadingModName = null;

			var config = info.Config;

			if (Mods.ContainsKey(config.ID)) return; // already loaded!

			CurrentLoadingModID = config.ID;
			CurrentLoadingModName = config.Name;

            var dll_name = config.DLL ?? "mod.dll";
			var dll_path = Path.Combine(info.Path, dll_name);
            if (!File.Exists(dll_path)) throw new InvalidConfigException(config.ID, $"Tried loading mod '{config.ID}' but it doesn't have the specified DLL file {dll_name}.");

			if (config.Depends != null) {
				if (tree_history == null) tree_history = new HashSet<string>();
				tree_history.Add(config.ID);
				            
				for (var i = 0; i < config.Depends.Length; i++) {
					var dep = config.Depends[i].Trim();
					Logger.Debug($"'{config.ID}' dependency: '{dep}'");
					ModInfo dep_info = null;
					if (!FlatModInfos.TryGetValue(dep, out dep_info)) {
						throw new MissingDependencyException(config.ID, dep);
					}

					if (tree_history != null && tree_history.Contains(dep)) {
						throw new CyclicDependencyException(config.ID, dep);
					}

					LoadMod(dep_info, tree_history);
				}
			}

			AppDomain.CurrentDomain.AssemblyResolve += GenerateModAssemblyResolver(info.Path);
                
            Assembly asm;
            using (FileStream f = File.OpenRead(dll_path)) {
                asm = AssemblyRelinker.GetRelinkedAssembly(config.ID, dll_name, dll_path, f);
            }

            if (asm == null) throw new ModLoadException(config.ID, "Failed loading/relinking assembly");

			if (info.Config.ModType == ModConfig.Type.Library) {
				Logger.Debug($"Mod is a library; not creating any Mod instances");
				return;
			}

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

				var mod_go = new GameObject($"Semi Mod '{config.ID}' GameObject");
				UnityEngine.Object.DontDestroyOnLoad(mod_go);
				ModsStorageObjects.Add(mod_go);
                Mod mod_instance = (Mod)mod_go.AddComponent(type);
				Logger.Debug($"Type name: {mod_instance.GetType().FullName}");
				mod_instance.Logger = new Logger(config.Name ?? config.ID);

				info.Instance = mod_instance;
				config.Instance = mod_instance;
				Mods[config.ID] = info;

				mod_instance.Info = info;

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
		/// Converts any list or array of integer item IDs into an array of pooled string IDs based on the pickup object database.
		/// </summary>
		/// <returns>The string ID list.</returns>
		/// <param name="ids">The numeric ID list.</param>
		internal static ID[] ConvertItemIDList(IList<int> ids) {
			var ary = new ID[ids.Count];
			for (int i = 0; i < ids.Count; i++) {
				var id = ids[i];
				var unique_id = ((Patches.PickupObject)PickupObjectDatabase.Instance.Objects[id])?.UniqueItemID;
                if (unique_id == null) continue;
				ary[i] = unique_id.Value;
			}
			return ary;
		}

		internal static void InitializeModOptions() {
			Logger.Debug($"INITIALIZING: MOD OPTIONS");
			Registry.ModMenuOptions = new IDPool<UI.MenuOption>();
		}

		/// <summary>
		/// Loads ID maps (mappings of the game's internal numeric IDs to the string ID system) from the assembly's resources.
		/// (Coroutine)
		/// </summary>
        internal static IEnumerator InitializeIDMaps() {
			Logger.Debug($"INITIALIZING: ID MAPS");

            Registry.Items = new IDPool<PickupObject>();
            Registry.Enemies = new IDPool<AIActor>();
            Registry.Synergies = new IDPool<AdvancedSynergyEntry>();
            Semi.Generated.GeneratedIDMaps.Apply();

            var asm = Assembly.GetExecutingAssembly();
            
			//Logger.Debug("IDMap: items.txt");
   //         using (StreamReader stream = new StreamReader(asm.GetManifestResourceStream("idmaps:items.txt"))) {
   //             Logger.Debug($"IDMap items.txt: stream = {stream}");
   //             Registry.Items = IDMapParser<PickupObject>.Parse(
   //                 stream,
   //                 "gungeon",
   //                 (id) => PickupObjectDatabase.GetById(int.Parse(id)),
			//		do_after: (id, item) => {
			//			((Semi.Patches.PickupObject)item).UniqueItemID = id;
			//		}
   //             );
   //         }

			//Logger.Debug("IDMap: enemies.txt");
   //         using (StreamReader stream = new StreamReader(asm.GetManifestResourceStream("idmaps:enemies.txt"))) {
   //             Logger.Debug($"IDMap enemies.txt: stream = {stream}");
   //             Registry.Enemies = IDMapParser<AIActor>.Parse(
   //                 stream,
   //                 "gungeon",
			//		(id) => EnemyDatabase.Instance.Entries[int.Parse(id)].GetPrefab<AIActor>()
   //             );
   //         }

			//Logger.Debug("IDMap: synergies.txt");
			//using (StreamReader stream = new StreamReader(asm.GetManifestResourceStream("idmaps:synergies.txt"))) {
			//	Logger.Debug($"IDMap synergies.txt: stream = {stream}");
			//	Registry.Synergies = IDMapParser<AdvancedSynergyEntry>.Parse(
			//		stream,
			//		"gungeon",
			//		(id) => GameManager.Instance.SynergyManager.synergies[int.Parse(id)],
			//		do_after: (id, gungeon_syn) => {
			//			var syn = (Semi.Patches.AdvancedSynergyEntry)gungeon_syn;

			//			syn.OptionalGuns = ConvertItemIDList(syn.OptionalGunIDs);
			//			syn.MandatoryGuns = ConvertItemIDList(syn.MandatoryGunIDs);
			//			syn.OptionalItems = ConvertItemIDList(syn.OptionalItemIDs);
			//			syn.MandatoryItems = ConvertItemIDList(syn.MandatoryItemIDs);
			//			syn.UniqueID = id;

			//			// null these out so that accessing these fields will error
			//			syn.OptionalGunIDs = syn.MandatoryGunIDs = syn.OptionalItemIDs = syn.MandatoryItemIDs = null;
			//		}
			//	);
			//}

			Logger.Debug($"IDMap: wwise_events.txt");
			using (StreamReader stream = new StreamReader(asm.GetManifestResourceStream("idmaps:wwise_events.txt"))) {
				Logger.Debug($"IDMap wwise_events.txt: stream = {stream}");
				Registry.AudioEvents = IDMapParser<AudioEvent>.Parse(
					stream,
					"gungeon",
					(id) => new AudioEvent.WWise(id),
					do_after: (id, ev) => {
						AudioEvent.WWise.ReverseIDMap[((AudioEvent.WWise)ev).WWiseEventName] = id;
					}
				);
			}
			yield return null;
        }

		/// <summary>
		/// Initializes Registry.Languages with IDs corresponding to the languages that Gungeon provides out of the box.
		/// </summary>
		internal static void InitializeBuiltinLanguages() {
			Logger.Debug($"INITIALIZING: BUILTIN LANGUAGES");
			Registry.Languages[(ID)(ID)"gungeon:english"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.ENGLISH);
			Registry.Languages[(ID)"gungeon:rubel_test"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.RUBEL_TEST);
			Registry.Languages[(ID)"gungeon:french"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.FRENCH);
			Registry.Languages[(ID)"gungeon:spanish"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.SPANISH);
			Registry.Languages[(ID)"gungeon:italian"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.ITALIAN);
			Registry.Languages[(ID)"gungeon:german"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.GERMAN);
			Registry.Languages[(ID)"gungeon:portuguese"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE);
			Registry.Languages[(ID)"gungeon:japanese"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.JAPANESE);
			Registry.Languages[(ID)"gungeon:korean"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.KOREAN);
			Registry.Languages[(ID)"gungeon:russian"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.RUSSIAN);
			Registry.Languages[(ID)"gungeon:polish"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.POLISH);
			Registry.Languages[(ID)"gungeon:chinese"] = new I18N.GungeonLanguage(StringTableManager.GungeonSupportedLanguages.CHINESE);
		}

		/// <summary>
		/// Initializes Registry.Localizations with IDs corresponding to the languages that Gungeon provides out of the box.
		/// </summary>
		internal static void InitializeBuiltinLocalizations() {
			Logger.Debug($"INITIALIZING: BUILTIN LOCALIZATIONS");
			for (int i = 0; i <= (int)StringTableManager.GungeonSupportedLanguages.CHINESE; i++) {
				var lang = (StringTableManager.GungeonSupportedLanguages)i;

				Registry.Localizations[(ID)$"{I18N.GungeonLanguage.LanguageToID(lang)}_core"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Core);
				Registry.Localizations[(ID)$"{I18N.GungeonLanguage.LanguageToID(lang)}_enemies"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Enemies);
				Registry.Localizations[(ID)$"{I18N.GungeonLanguage.LanguageToID(lang)}_intro"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Intro);
				Registry.Localizations[(ID)$"{I18N.GungeonLanguage.LanguageToID(lang)}_items"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Items);
				Registry.Localizations[(ID)$"{I18N.GungeonLanguage.LanguageToID(lang)}_synergies"] = new I18N.PrefabLocalization(lang, I18N.StringTable.Synergies);
				Registry.Localizations[(ID)$"{I18N.GungeonLanguage.LanguageToID(lang)}_ui"] = new I18N.PrefabLocalization(lang, I18N.StringTable.UI, format: I18N.LocalizationSource.FormatType.DF);

				var lang_id = I18N.GungeonLanguage.LanguageToID(lang);
                var lang_name = lang_id.Name;
				var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"loc:{lang_name}.txt");
				if (stream != null) {
					var text = new StreamReader(stream).ReadToEnd();
					Logger.Debug($"Found Semi localization stream for {lang_id}");
					Registry.Localizations[(ID)$"semi:{lang_name}_core"] = new I18N.RuntimeLocalization("semi", text, lang_id, I18N.StringTable.UI);
					stream.Close();
				}
			}
		}

		/// <summary>
		/// Invokes the delegates registered to run when this synergy is activated, passing the player to them.
		/// </summary>
		/// <param name="id">ID of the synergy.</param>
		/// <param name="p">PlayerController to pass on.</param>
		internal static void InvokeSynergyActivated(ID id, PlayerController p) {
			Logger.Debug($"Synergy activated: '{id}'");
			Registry.SynergyStateChangeAction action = null;
			if (SynergyActivatedActions.TryGetValue(id, out action)) {
				action.Invoke(p);
			}
		}

		/// <summary>
		/// Invokes the delegates registered to run when this synergy is deactivated, passing the player to them.
		/// </summary>
		/// <param name="id">ID of the synergy.</param>
		/// <param name="p">PlayerController to pass on.</param>
		internal static void InvokeSynergyDeactivated(ID id, PlayerController p) {
			Logger.Debug($"Synergy deactivated: '{id}'");
			Registry.SynergyStateChangeAction action = null;
			if (SynergyDeactivatedActions.TryGetValue(id, out action)) {
				action.Invoke(p);
			}
		}

		/// <summary>
		/// Initializes the modded audio subsystem (RayAudio).
		/// </summary>
		internal static void InitializeAudio() {
			Logger.Debug($"INITIALIZING: AUDIO");
			Registry.ModAudioTracks = new IDPool<Audio>();

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
			UnityEngine.Object.DontDestroyOnLoad(MusicStreamBufferUpdateObject);
		}

		/// <summary>
		/// Initializes the stream buffer update object's cache of Audio tracks.
		/// </summary>
		internal static void InitializeStreamBufferUpdateBehaviourCache() {
			Logger.Debug($"INITIALIZING: AUDIO STREAM BUFFER UPDATE CACHE (DEPRECATED)");

			//var len = Registry.ModAudioTracks.Count;

			//StreamBufferUpdateBehaviour.Paused = true;
			//StreamBufferUpdateBehaviour.Tracks = new List<Audio>();

			//foreach (var tr in Registry.ModAudioTracks.Entries) {
			//	StreamBufferUpdateBehaviour.Tracks.Add(tr);
			//}

			//StreamBufferUpdateBehaviour.Paused = false;
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

			// after i18n is done
			var tab_mods_selector = Patches.PreOptionsMenuController.Instance.TabModsSelector;
			tab_mods_selector.Text = "#semi:TAB_MODS";
			tab_mods_selector.RelativePosition = new Vector2(Patches.PreOptionsMenuController.Instance.MainPanel.Width / 2 - tab_mods_selector.Width / 2, tab_mods_selector.RelativePosition.y);
			tab_mods_selector.Localize();

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
