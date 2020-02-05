#pragma warning disable 0626

using System;
using MonoMod;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

/*
 * ENTRY POINT
 */

namespace Semi.Patches {
	/// <summary>
	/// Patches Gungeon GameManager class. Serves as the entry point for Semi.
	/// </summary>
    [MonoModPatch("global::GameManager")]
    public class GameManager : BraveBehaviour {
        private extern void orig_Awake();

        private void Awake() {
            if (!SemiLoader.Loaded) {
				SemiLoader.Loaded = true;
				SemiLoader.Logger.Info($"Semi Loader {SemiLoader.VERSION} starting");

                SemiLoader.OnGameManagerAlive((global::GameManager)(object)this);
			}
            orig_Awake();
			StartCoroutine(SemiLoader.OnGameManagerReady((global::GameManager)(object)this));
        }
    }
}
