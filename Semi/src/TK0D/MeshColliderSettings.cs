using System;
using UnityEngine;

namespace Semi {
	/// <summary>
	/// Helper struct that combines all three mesh collider related <c>tk2dSprite</c> fields.
	/// </summary>
	public struct MeshColliderSettings {
		public MeshCollider Collider;
		public Mesh Mesh;
		public Vector3[] Positions;

		/// <summary>
		/// Initializes a new instance of the <see cref="Semi.MeshColliderSettings"/> struct.
		/// </summary>
		/// <param name="collider">Mesh collider.</param>
		/// <param name="mesh">Mesh collider mesh.</param>
		/// <param name="pos">Mesh collider position.</param>
		public MeshColliderSettings(MeshCollider collider, Mesh mesh, Vector3[] pos) {
			Collider = collider;
			Mesh = mesh;
			Positions = pos;
		}
	}
}
