using System;
using UnityEngine;

namespace Semi {
	public struct MeshColliderSettings {
		public MeshCollider Collider;
		public Mesh Mesh;
		public Vector3[] Positions;

		public MeshColliderSettings(MeshCollider collider, Mesh mesh, Vector3[] pos) {
			Collider = collider;
			Mesh = mesh;
			Positions = pos;
		}

		public void SetOnTk2dSprite(tk2dSprite sprite) {
			sprite.meshCollider = Collider;
			sprite.meshColliderMesh = Mesh;
			sprite.meshColliderPositions = Positions;
		}
	}
}
