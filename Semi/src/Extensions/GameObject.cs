using System;
namespace Semi {
	public static class GameObjectExt {
		public static void DestroyComponentIfExists<T>(this UnityEngine.GameObject go) where T : UnityEngine.Object {
			var obj = go.GetComponent<T>();
			if (obj == null) return;
			UnityEngine.Object.Destroy(obj);
		}
	}
}
