using System;
using UnityEngine;

namespace Semi.DebugConsole {
	public class ConsoleController : MonoBehaviour {
		public void Awake() {
			Console.Logger.Debug($"Console controller Awake");
		}

		public void Update() {
			if (!SemiLoader.DEBUG_MODE) return;

			if (Input.GetKey(KeyCode.F2)) {
				Console.Logger.Debug($"F2 press");
				if (SemiLoader.Console.Window.Visible) {
					Console.Logger.Debug($"VISIBLE - HIDING");
					SemiLoader.Console.Hide();
				} else {
					Console.Logger.Debug($"INVISIBLE - SHOWING");
					SemiLoader.Console.Show();
				}
			}
		}
	}
}
