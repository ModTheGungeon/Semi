using System;
using System.IO;
using UnityEngine;

namespace Semi {
	public static class Texture2DLoader {
		/// <summary>
		/// Load a 2D texture from a byte array.
		/// </summary>
		/// <returns>A new texture.</returns>
		/// <param name="content">Array of the bytes that make up the image.</param>
		/// <param name="name">Name of the texture.</param>
		public static Texture2D LoadTexture2D(byte[] content, string name) {
			var tex = new Texture2D(0, 0);
			tex.name = name;
			tex.LoadImage(content);
			tex.filterMode = FilterMode.Point;
			return tex;
		}

		/// <summary>
		/// Load a 2D texture from a file.
		/// </summary>
		/// <returns>A new texture.</returns>
		/// <param name="path">Path to the file.</param>
		public static Texture2D LoadTexture2D(string path) {
			using (var file = File.OpenRead(path)) {
				return LoadTexture2D(new BinaryReader(file).ReadAllBytes(), path);
			}
		}

		private static bool _IsPoT(int n) {
			if (n == 0) return false;

			return (int)Math.Ceiling(Math.Log(n, 2)) == (int)Math.Floor(Math.Log(n, 2));
		}

		/// <summary>
		/// Checks if both of the texture's dimensions are powers of two.
		/// </summary>
		/// <returns><c>true</c>, if both the width and height are powers of two, <c>false</c> otherwise.</returns>
		/// <param name="tex">Target texture.</param>
		public static bool IsPowerOfTwo(Texture2D tex) {
			if (!_IsPoT(tex.width) || !_IsPoT(tex.height)) return false;
			return true;
		}

		/// <summary>
		/// Checks if both of the texture's dimensions are powers of two and equal.
		/// </summary>
		/// <returns><c>true</c>, if both the width and height are equal and powers of two, <c>false</c> otherwise.</returns>
		/// <param name="tex">Target texture.</param>
		public static bool IsEqualPowerOfTwo(Texture2D tex) {
			if (tex.width != tex.height) return false;
			return IsPowerOfTwo(tex);
		}
	}
}
