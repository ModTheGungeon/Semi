using System;
using System.Collections.Generic;
using MonoMod;

namespace Semi.Patches {
	/// <summary>
	/// Patches tk2dSpriteCollectionData to add an <c>IDPool</c> field for definitions and to make the spriteNameLookupDict field public.
	/// </summary>
	[MonoModPatch("global::tk2dSpriteCollectionData")]
	public class tk2dSpriteCollectionData {
		[MonoModIgnore]
		public Dictionary<string, int> spriteNameLookupDict;

		public IDPool<int> SpritePool;
	}
}
