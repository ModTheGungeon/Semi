using System;
using YamlDotNet.Serialization;

namespace Semi {
    public class ModConfig {
        [YamlMember(Alias = "id")]
        public string ID { get; set; }

        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "author")]
        public string Author { get; set; } = "Unknown";

		[YamlMember(Alias = "dll")]
		public string DLL { get; set; } = "mod.dll";

        [YamlMember(Alias = "version")]
        public string Version { get; set; } = "0.1";

        [YamlMember(Alias = "description")]
        public string Description { get; set; } = "Unknown";

		[YamlMember(Alias = "If you see this, the MonoMod bug has been fixed.")]
		private string _MonoModBugWorkaround { get; set; } = "Unknown";

        public Mod Instance { get; internal set; }
    }
}
