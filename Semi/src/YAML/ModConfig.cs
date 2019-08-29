using System;
using YamlDotNet.Serialization;

namespace Semi {
    public class ModConfig {
		public enum Type {
			Mod,
			Library
		}

        [YamlMember(Alias = "id")]
        public string ID { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "author")]
        public string Author { get; set; } = "Unknown";

		[YamlMember(Alias = "api_version")]
		public int? APIVersion { get; set; }

		[YamlMember(Alias = "dll")]
		public string DLL { get; set; } = "mod.dll";

        [YamlMember(Alias = "version")]
        public string Version { get; set; } = "0.1";

        [YamlMember(Alias = "description")]
        public string Description { get; set; } = "Unknown";

		[YamlMember(Alias = "depends")]
		public string[] Depends { get; set; } = null;

		[YamlMember(Alias = "type")]
		public Type ModType { get; set; } = Type.Mod;

		internal bool Priority = false;

		[YamlMember(Alias = "If you see this, the MonoMod bug has been fixed.")]
		private string _MonoModBugWorkaround { get; set; } = "Unknown";

        public Mod Instance { get; internal set; }
    }
}
