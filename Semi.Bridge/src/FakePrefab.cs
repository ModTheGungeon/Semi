using System;
using UnityEngine;
using System.Collections.Generic;
using Logger = ModTheGungeon.Logger;

namespace Semi {
	public class FakePrefab : Component {
		internal static HashSet<GameObject> ExistingFakePrefabs = new HashSet<GameObject>();
		internal static Logger Logger = new Logger("FakePrefab");

		public static bool IsFakePrefab(UnityEngine.Object o) {
			if (o is GameObject) {
				return ExistingFakePrefabs.Contains((GameObject)o);
			} else if (o is Component) {
				return ExistingFakePrefabs.Contains(((Component)o).gameObject);
			}
			return false;
		}

		public static void MarkAsFakePrefab(GameObject obj) {
			ExistingFakePrefabs.Add(obj);
		}

		public static GameObject Clone(GameObject obj) {
			var already_fake = IsFakePrefab(obj);
			var was_active = obj.activeSelf;
			if (was_active) obj.SetActive(false);
			var fakeprefab = UnityEngine.Object.Instantiate(obj);
			if (was_active) obj.SetActive(true);
			ExistingFakePrefabs.Add(fakeprefab);
			if (already_fake) {
				Logger.Debug($"Fake prefab '{obj}' cloned as new fake prefab");
			} else {
				Logger.Debug($"Prefab/object '{obj}' cloned as new fake prefab");
			}
			return fakeprefab;
		}

		public static UnityEngine.Object Instantiate(UnityEngine.Object o, UnityEngine.Object new_o) {
			Console.WriteLine($"inst {o}");
			if (o is GameObject && ExistingFakePrefabs.Contains((GameObject)o)) {
				((GameObject)new_o).SetActive(true);
			} else if (o is Component && ExistingFakePrefabs.Contains(((Component)o).gameObject)) {
				((Component)new_o).gameObject.SetActive(true);
			}
			return new_o;
		}
	}
}
