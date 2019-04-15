using System;
using MonoMod;
using UnityEngine;

namespace Semi.Patches {
	[MonoModPatch("UnityEngine.Object")]
	internal class UnityEngineObject {
		[MonoModOriginal]
		public extern static UnityEngine.Object orig_Instantiate_opi(UnityEngine.Object original, Transform parent, bool instantiateInWorldSpace);

		[MonoModOriginalName("orig_Instantiate_opi")]
		public static UnityEngine.Object Instantiate(UnityEngine.Object original, Transform parent, bool instantiateInWorldSpace) {
			return FakePrefab.Instantiate(original, orig_Instantiate_opi(original, parent, instantiateInWorldSpace));
		}

		[MonoModOriginal]
		public extern static UnityEngine.Object orig_Instantiate_op(UnityEngine.Object original, Transform parent);

		[MonoModOriginalName("orig_Instantiate_op")]
		public static UnityEngine.Object Instantiate(UnityEngine.Object original, Transform parent) {
			return FakePrefab.Instantiate(original, orig_Instantiate_op(original, parent));
		}

		[MonoModOriginal]
		public extern static UnityEngine.Object orig_Instantiate_o(UnityEngine.Object original);

		[MonoModOriginalName("orig_Instantiate_o")]
		public static UnityEngine.Object Instantiate(UnityEngine.Object original) {
			return FakePrefab.Instantiate(original, orig_Instantiate_o(original));
		}

		[MonoModOriginal]
		public extern static UnityEngine.Object orig_Instantiate_opr(UnityEngine.Object original, Vector3 position, Quaternion rotation);

		[MonoModOriginalName("orig_Instantiate_opr")]
		public static UnityEngine.Object Instantiate(UnityEngine.Object original, Vector3 position, Quaternion rotation) {
			return FakePrefab.Instantiate(original, orig_Instantiate_opr(original, position, rotation));
		}

		[MonoModOriginal]
		public extern static UnityEngine.Object orig_Instantiate_oprp(UnityEngine.Object original, Vector3 position, Quaternion rotation, Transform parent);

		[MonoModOriginalName("orig_Instantiate_oprp")]
		public static UnityEngine.Object Instantiate(UnityEngine.Object original, Vector3 position, Quaternion rotation, Transform parent) {
			return FakePrefab.Instantiate(original, orig_Instantiate_oprp(original, position, rotation, parent));
		}

	}
}
