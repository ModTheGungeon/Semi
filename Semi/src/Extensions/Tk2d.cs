using System;
namespace Semi {
	public static class Tk2dExt {
		/// <summary>
		/// Wraps the tk2d object with the appropriate Tk0d wrapper struct.
		/// </summary>
		/// <returns>The wrapped struct.</returns>
		/// <param name="coll">Target collection.</param>
		public static Sprite Wrap(this tk2dSprite coll) {
			return new Sprite(coll);
		}

		/// <summary>
		/// Wraps the tk2d object with the appropriate Tk0d wrapper struct.
		/// </summary>
		/// <returns>The wrapped struct.</returns>
		/// <param name="coll">Target collection.</param>
		public static SpriteCollection Wrap(this tk2dSpriteCollectionData coll) {
			return new SpriteCollection(coll);
		}

		/// <summary>
		/// Wraps the tk2d object with the appropriate Tk0d wrapper struct.
		/// </summary>
		/// <returns>The wrapped struct.</returns>
		/// <param name="def">Target definition.</param>
		public static SpriteDefinition Wrap(this tk2dSpriteDefinition def) {
			return new SpriteDefinition(def);
		}

		/// <summary>
		/// Wraps the tk2d object with the appropriate Tk0d wrapper struct.
		/// </summary>
		/// <returns>The wrapped struct.</returns>
		/// <param name="clip">Target animation clip.</param>
		public static SpriteAnimationClip Wrap(this tk2dSpriteAnimationClip clip) {
			return new SpriteAnimationClip(clip);
		}

		/// <summary>
		/// Wraps the tk2d object with the appropriate Tk0d wrapper struct.
		/// </summary>
		/// <returns>The wrapped struct.</returns>
		/// <param name="frame">Target animation frame.</param>
		public static SpriteAnimationFrame Wrap(this tk2dSpriteAnimationFrame frame) {
			return new SpriteAnimationFrame(frame);
		}

		/// <summary>
		/// Wraps the tk2d object with the appropriate Tk0d wrapper struct.
		/// </summary>
		/// <returns>The wrapped struct.</returns>
		/// <param name="anim">Target animation.</param>
		public static SpriteAnimation Wrap(this tk2dSpriteAnimation anim) {
			return new SpriteAnimation(anim);
		}

		/// <summary>
		/// Wraps the tk2d object with the appropriate Tk0d wrapper struct.
		/// </summary>
		/// <returns>The wrapped struct.</returns>
		/// <param name="anim">Target animator.</param>
		public static SpriteAnimator Wrap(this tk2dSpriteAnimator anim) {
			return new SpriteAnimator(anim);
		}
	}
}
