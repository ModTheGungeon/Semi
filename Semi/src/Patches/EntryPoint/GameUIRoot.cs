//using System;
//using MonoMod;

//namespace Semi.Patches {
//	[MonoModPatch("global::GameUIRoot")]
//	public class GameUIRoot : global::GameUIRoot{
//		private bool _SemiUIInitialized = false;

//		protected extern void orig_InvariantUpdate(float realDeltaTime);
//		protected override void InvariantUpdate(float realDeltaTime) {
//			if (!_SemiUIInitialized) {
//				_SemiUIInitialized = true;
//				SemiLoader.InitializeUIHelpers();
//			}
//			orig_InvariantUpdate(realDeltaTime);
//		}
//	}
//}
