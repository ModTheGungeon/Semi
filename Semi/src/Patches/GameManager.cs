#pragma warning disable 0626

using System;
using MonoMod;

/*
 * ENTRY POINT
 */

namespace Semi.Patches {
    [MonoModPatch("global::GameManager")]
    public class GameManager {
        private extern void orig_Awake();

        private void Awake() {
            orig_Awake();
            SemiLoader.Logger.Info($"Semi Loader {SemiLoader.VERSION} starting");

            SemiLoader.OnGameManagerAlive();
        }
    }
}
