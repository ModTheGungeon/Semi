#pragma warning disable 0626

using System;
using MonoMod;

/*
 * ENTRY POINT
 */

namespace Semi.Patches {
    [MonoModPatch("global::GameManager")]
    public class GameManager : BraveBehaviour {
        private extern void orig_Awake();

        private void Awake() {
            orig_Awake();
			if (!SemiLoader.Loaded) {
				SemiLoader.Loaded = true;
				SemiLoader.Logger.Info($"Semi Loader {SemiLoader.VERSION} starting");

				StartCoroutine(SemiLoader.OnGameManagerAlive());
			}
        }
    }
}
