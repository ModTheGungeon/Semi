using System;
namespace Semi {
	public static class Tk2dExt {
		public static Sprite Wrap(this tk2dSprite coll) {
			return new Sprite(coll);
		}

		public static SpriteCollection Wrap(this tk2dSpriteCollectionData coll) {
			return new SpriteCollection(coll);
		}

		public static SpriteDefinition Wrap(this tk2dSpriteDefinition coll) {
			return new SpriteDefinition(coll);
		}
	}
}
