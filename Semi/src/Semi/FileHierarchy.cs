using System;
using System.IO;
using UnityEngine;

namespace Semi {
    public class FileHierarchyException : Exception {
        public FileHierarchyException(string msg) : base($"Invalid file hierarchy: {msg}") { }
    }

    public class FileHierarchy {
        private static string _GameFolder;
        public static string GameFolder {
            get {
                if (_GameFolder != null) return _GameFolder;
                return _GameFolder = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            }
        }

        private static string _ManagedFolder;
        public static string ManagedFolder {
            get {
                if (_ManagedFolder != null) return _ManagedFolder;
                return _ManagedFolder = Path.GetDirectoryName(typeof(FileHierarchy).Assembly.Location);
            }
        }

        public const string MODS_FOLDER_NAME = "SemiMods";
        private static string _ModsFolder;
        public static string ModsFolder {
            get {
                if (_ModsFolder != null) return _ModsFolder;
                return _ModsFolder = Path.Combine(GameFolder, MODS_FOLDER_NAME);
            }
        }

        public const string MODS_ORDER_FILE_NAME = "order.txt";
        private static string _ModsOrderFile;
        public static string ModsOrderFile {
            get {
                if (_ModsOrderFile != null) return _ModsOrderFile;
                return _ModsOrderFile = Path.Combine(ModsFolder, MODS_ORDER_FILE_NAME);
            }
        }

        public const string MODS_BLACKLIST_FILE_NAME = "blacklist.txt";
        private static string _ModsBlacklistFile;
        public static string ModsBlacklistFile {
            get {
                if (_ModsBlacklistFile != null) return _ModsBlacklistFile;
                return _ModsBlacklistFile = Path.Combine(ModsFolder, MODS_BLACKLIST_FILE_NAME);
            }
        }

        public const string MODS_CACHE_FOLDER_NAME = "Cache";
        private static string _ModsCacheFolder;
        public static string ModsCacheFolder {
            get {
                if (_ModsCacheFolder != null) return _ModsCacheFolder;
                return _ModsCacheFolder = Path.Combine(ModsFolder, MODS_CACHE_FOLDER_NAME);
            }
        }

        public const string MODS_CACHE_RELINK_FOLDER_NAME = "RelinkedAssemblies";
        private static string _ModsCacheRelinkFolder;
        public static string ModsCacheRelinkFolder {
            get {
                if (_ModsCacheRelinkFolder != null) return _ModsCacheRelinkFolder;
                return _ModsCacheRelinkFolder = Path.Combine(ModsCacheFolder, MODS_CACHE_RELINK_FOLDER_NAME);
            }
        }

        private const string ORDER_TXT_DEFAULT_TEXT = "# put names of mod folders/archives to be loaded in a specific order here\n# mods that aren't in this file will also be loaded,\n# but in alphabetical order";
        private const string BLACKLIST_TXT_DEFAULT_TEXT = "# put names of mod folders/archives that you don't want loaded here\n# this file will override the order.txt file if necessary";

        public const string MOD_INFO_FILE_NAME = "mod.yml";

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
            SemiLoader.Logger.Debug("File hierarchy verified");
        }
    }
}
