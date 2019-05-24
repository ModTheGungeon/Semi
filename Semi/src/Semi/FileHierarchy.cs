using System;
using System.IO;
using UnityEngine;

namespace Semi {
	/// <summary>
	/// Exception thrown when Semi detects a misconfiguration of the file hierarchy.
	/// </summary>
    public class FileHierarchyException : Exception {
        public FileHierarchyException(string msg) : base($"Invalid file hierarchy: {msg}") { }
    }

    public class FileHierarchy {
        private static string _GameFolder;
		/// <summary>
		/// Gets the absolute path to the root folder that all the game data resides in.
		/// </summary>
		/// <value>The game folder.</value>
        public static string GameFolder {
            get {
                if (_GameFolder != null) return _GameFolder;
                return _GameFolder = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            }
        }

        private static string _ManagedFolder;
		/// <summary>
		/// Gets the absolute path to the folder where all the .NET assemblies reside in.
		/// </summary>
		/// <value>The Managed folder.</value>
        public static string ManagedFolder {
            get {
                if (_ManagedFolder != null) return _ManagedFolder;
                return _ManagedFolder = Path.GetDirectoryName(typeof(FileHierarchy).Assembly.Location);
            }
        }

		/// <summary>
		/// The name of the file that determines mods to ignore during validation.
		/// </summary>
		public const string MY_MODS_FILE_NAME = "MySemiMods.txt";
		private static string _MyModsFile;
		/// <summary>
		/// Gets the absolute path to the file listing mods to ignore during checksum validation.
		/// </summary>
		public static string MyModsFile {
			get {
				if (_MyModsFile != null) return _MyModsFile;
				return _MyModsFile = Path.Combine(GameFolder, MY_MODS_FILE_NAME);
			}
		}

		/// <summary>
		/// The name of the folder inside the game folder from where mods are loaded.
		/// </summary>
        public const string MODS_FOLDER_NAME = "SemiMods";
        private static string _ModsFolder;
		/// <summary>
		/// Gets the absolute path to the folder where mods are loaded from.
		/// </summary>
		/// <value>The mods folder.</value>
        public static string ModsFolder {
            get {
                if (_ModsFolder != null) return _ModsFolder;
                return _ModsFolder = Path.Combine(GameFolder, MODS_FOLDER_NAME);
            }
        }

		/// <summary>
		/// The name of the text file that specifies what order mods should be loaded in first, before any unspecified mods that remain are loaded in default/filesystem order.
		/// </summary>
        public const string MODS_ORDER_FILE_NAME = "order.txt";
        private static string _ModsOrderFile;
		/// <summary>
		/// Gets the absolute path to the file that specifies priority mod loading order.
		/// </summary>
		/// <value>The mods order file.</value>
        public static string ModsOrderFile {
            get {
                if (_ModsOrderFile != null) return _ModsOrderFile;
                return _ModsOrderFile = Path.Combine(ModsFolder, MODS_ORDER_FILE_NAME);
            }
        }

		/// <summary>
		/// The name of the text file that specifies what mods should never be loaded, even if they are defined in the order list.
		/// </summary>
        public const string MODS_BLACKLIST_FILE_NAME = "blacklist.txt";
        private static string _ModsBlacklistFile;
		/// <summary>
		/// Gets the absolute path to the file that specifies which mods to never load.
		/// </summary>
		/// <value>The mods blacklist file.</value>
        public static string ModsBlacklistFile {
            get {
                if (_ModsBlacklistFile != null) return _ModsBlacklistFile;
                return _ModsBlacklistFile = Path.Combine(ModsFolder, MODS_BLACKLIST_FILE_NAME);
            }
        }

		/// <summary>
		/// The name of the folder where all cached data related to mods is stored.
		/// </summary>
        public const string MODS_CACHE_FOLDER_NAME = "Cache";
        private static string _ModsCacheFolder;
		/// <summary>
		/// Gets the absolute path to the folder that houses all cached data related to mods.
		/// </summary>
		/// <value>The mods cache folder.</value>
        public static string ModsCacheFolder {
            get {
                if (_ModsCacheFolder != null) return _ModsCacheFolder;
                return _ModsCacheFolder = Path.Combine(ModsFolder, MODS_CACHE_FOLDER_NAME);
            }
        }

		/// <summary>
		/// The name of the folder inside the mod cache folder where relinked assemblies are stored.
		/// </summary>
        public const string MODS_CACHE_RELINK_FOLDER_NAME = "RelinkedAssemblies";
        private static string _ModsCacheRelinkFolder;
		/// <summary>
		/// Gets the absolute path to the folder that houses mod assemblies after they've been relinked to reroute references to MonoMod patch assemblies into the main game assembly.
		/// </summary>
		/// <value>The mod relink cache folder.</value>
        public static string ModsCacheRelinkFolder {
            get {
                if (_ModsCacheRelinkFolder != null) return _ModsCacheRelinkFolder;
                return _ModsCacheRelinkFolder = Path.Combine(ModsCacheFolder, MODS_CACHE_RELINK_FOLDER_NAME);
            }
        }

        private const string ORDER_TXT_DEFAULT_TEXT = "# put names of mod folders/archives to be loaded in a specific order here\n# mods that aren't in this file will also be loaded,\n# but in unspecified order";
        private const string BLACKLIST_TXT_DEFAULT_TEXT = "# put names of mod folders/archives that you don't want loaded here\n# this file will override the order.txt file if necessary";
		private const string MY_MODS_TXT_DEFAULT_TEXT = "# put IDs of mods that you're working on\n# to allow them to load without validating the online checksum\n# do not trust anybody telling you to put in their mod here if you haven't written it,\n# as they could be trying to work around the security features to make you run a malicious mod\n\n";

		/// <summary>
		/// The name of the mod metadata file.
		/// </summary>
        public const string MOD_INFO_FILE_NAME = "mod.yml";

		/// <summary>
		/// Verifies that the file hierarchy is correct, creates missing directories and files if needed.
		/// </summary>
        public static void Verify() {
            if (File.Exists(ModsFolder)) throw new FileHierarchyException("There is no SemiMods folder, but there is a file under that same name");
            if (!Directory.Exists(ModsFolder)) {
                SemiLoader.Logger.Debug("Mods folder didn't exist, creating");
                Directory.CreateDirectory(ModsFolder);
            }
            if (!File.Exists(ModsOrderFile)) {
                SemiLoader.Logger.Debug("order.txt file didn't exist, creating");
                using (var f = File.CreateText(ModsOrderFile)) f.Write(ORDER_TXT_DEFAULT_TEXT);
            }
            if (!File.Exists(ModsBlacklistFile)) {
                SemiLoader.Logger.Debug("blacklist.txt file didn't exist, creating");
                using (var f = File.CreateText(ModsBlacklistFile)) f.Write(BLACKLIST_TXT_DEFAULT_TEXT);
            }
            if (File.Exists(ModsCacheFolder)) throw new FileHierarchyException("There is no SemiMods/Cache folder, but there is a file under that same name");
            if (!Directory.Exists(ModsCacheFolder)) {
                SemiLoader.Logger.Debug("Mod cache folder didn't exist, creating");
                Directory.CreateDirectory(ModsCacheFolder);
            }
            if (File.Exists(ModsCacheRelinkFolder)) throw new FileHierarchyException("There is no SemiMods/Cache/RelinkedAssemblies folder, but there is a file under that same name");
            if (!Directory.Exists(ModsCacheRelinkFolder)) {
                SemiLoader.Logger.Debug("Mod assembly relink cache folder didn't exist, creating");
                Directory.CreateDirectory(ModsCacheRelinkFolder);
            }
			if (!File.Exists(MyModsFile)) {
				SemiLoader.Logger.Debug("MySemiMods.txt file didn't exist, creating");
				using (var f = File.CreateText(MyModsFile)) f.Write(MY_MODS_TXT_DEFAULT_TEXT);
			}
            SemiLoader.Logger.Debug("File hierarchy verified");
        }
    }
}
