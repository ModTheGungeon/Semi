using System;
using System.Collections.Generic;
using MonoMod;

namespace Semi.Patches {
	[MonoModPatch("global::tk2dSpriteCollectionData")]
	public class tk2dSpriteCollectionData {
		[MonoModIgnore]
		public Dictionary<string, int> spriteNameLookupDict;

		public IDPool<int> SpritePool;
	}
}
