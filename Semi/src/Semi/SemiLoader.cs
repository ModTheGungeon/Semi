using System;
using ModTheGungeon;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Semi {
    public class InvalidConfigException : Exception {
        public InvalidConfigException(string message) : base($"Invalid config: {message}") { }
    }

    public class ModLoadException : Exception {
        public ModLoadException(string mod_id, string message) : base($"Failed loading mod '{mod_id}': {message}") { }
    }

    public static class SemiLoader {
        public const string VERSION = "cont-dev";
        public static Dictionary<string, Mod> Mods;

        internal static UnityEngine.GameObject ModsStorageObject;
        internal static Logger Logger = new Logger("Semi");

        internal static void OnGameManagerAlive() {
            Logger.Debug("GameManager alive");

            ModsStorageObject = new UnityEngine.GameObject("Semi Mod Loader");
            Mods = new Dictionary<string, Mod>();

            LoadIDMaps();

            FileHierarchy.Verify();
            LoadMods();
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
                mod_instance.Config = mod_config;
                mod_config.Instance = mod_instance;
                Mods[mod_config.ID] = mod_instance;

                mod_instance.Loaded();
            }
        }

        internal static void LoadIDMaps() {
            var asm = Assembly.GetExecutingAssembly();
            Logger.Debug("Loading IDMaps");

            using (StreamReader stream = new StreamReader(asm.GetManifestResourceStream("idmaps:items.txt"))) {
                Logger.Debug($"IDMap items.txt: stream = {stream}");
                Gungeon.Items = IDMapParser<PickupObject, Gungeon.ItemTag>.Parse(
                    stream,
                    "gungeon",
                    (id) => PickupObjectDatabase.GetById(int.Parse(id))
                );
            }

            using (StreamReader stream = new StreamReader(asm.GetManifestResourceStream("idmaps:enemies.txt"))) {
                Logger.Debug($"IDMap enemies.txt: stream = {stream}");
                Gungeon.Enemies = IDMapParser<AIActor, Gungeon.EnemyTag>.Parse(
                    stream,
                    "gungeon",
                    (id) => EnemyDatabase.AssetBundle.LoadAsset<UnityEngine.GameObject>(id).GetComponent<AIActor>()
                );
            }
        }
    }
}
