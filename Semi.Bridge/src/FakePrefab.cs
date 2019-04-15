using System;
using UnityEngine;
using System.Collections.Generic;
using Logger = ModTheGungeon.Logger;

namespace Semi {
	/// <summary>
	/// This class allows the creation of "fake prefabs".
	/// In Unity, prefabs are a special kind of <c>GameObject</c>. A prefab is never active, even if its <c>activeSelf</c> field is true. However, if you instantiate such an object, your new instance will inherit the <c>activeSelf</c> field and if true, will create an alive object.
	/// On many occassions we want to be able to create our own "prefabs", in other words, objects that act as a template to make other objects from. Unfortunately, the closest way to do this in Unity normally is to have objects that are created as inactive.
	/// This creates a problem, because now our new custom "prefab" has to remain inactive, and any object that is instantiated from it will be inactive as well.
	/// Semi works around this in two parts - this class is the first part, and the other part is in a patch for the <c>UnityEngine.Object</c> class.
	/// When we create a fake prefab with this class, we're doing the same workaround mentioned before (inactive object), but we also add it to a global static list. The second part of this workaround patches Unity to check every object that is being instantiated and verify it against this list. If it is found that the original object does in fact belong in this list, the new object is immediately set as active.
	/// </summary>
	public class FakePrefab : Component {
		internal static HashSet<GameObject> ExistingFakePrefabs = new HashSet<GameObject>();
		internal static Logger Logger = new Logger("FakePrefab");

		/// <summary>
		/// Checks if an object is marked as a fake prefab.
		/// </summary>
		/// <returns><c>true</c>, if object is in the list of fake prefabs, <c>false</c> otherwise.</returns>
		/// <param name="o">Unity object to test.</param>
		public static bool IsFakePrefab(UnityEngine.Object o) {
			if (o is GameObject) {
				return ExistingFakePrefabs.Contains((GameObject)o);
			} else if (o is Component) {
				return ExistingFakePrefabs.Contains(((Component)o).gameObject);
			}
			return false;
		}

		/// <summary>
		/// Marks an object as a fake prefab.
		/// </summary>
		/// <param name="obj">GameObject to add to the list.</param>
		public static void MarkAsFakePrefab(GameObject obj) {
			ExistingFakePrefabs.Add(obj);
		}

		/// <summary>
		/// Clones a real prefab or a fake prefab into a new fake prefab.
		/// </summary>
		/// <returns>The new game object.</returns>
		/// <param name="obj">GameObject to clone.</param>
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

		/// <summary>
		/// Activates objects that have been created from a fake prefab, otherwise simply returns them.
		/// </summary>
		/// <returns>The same Unity object as the one passed in <c>new_o</c>, activated if <c>o</c> is a fake prefab..</returns>
		/// <param name="o">Original object.</param>
		/// <param name="new_o">The object instantiated from the original object.</param>
		public static UnityEngine.Object Instantiate(UnityEngine.Object o, UnityEngine.Object new_o) {
			if (o is GameObject && ExistingFakePrefabs.Contains((GameObject)o)) {
				((GameObject)new_o).SetActive(true);
			} else if (o is Component && ExistingFakePrefabs.Contains(((Component)o).gameObject)) {
				((Component)new_o).gameObject.SetActive(true);
			}
			return new_o;
		}
	}
}
