using System;
using System.IO;
using UnityEngine;

namespace Semi {
	public static class Texture2DLoader {
		public static Texture2D LoadTexture2D(byte[] content, string name) {
			var tex = new Texture2D(0, 0);
			tex.name = name;
			tex.LoadImage(content);
			tex.filterMode = FilterMode.Point;
			return tex;
		}

		public static Texture2D LoadTexture2D(string path) {
			using (var file = File.OpenRead(path)) {
				return LoadTexture2D(new BinaryReader(file).ReadAllBytes(), path);
			}
		}

		private static bool _IsPoT(int n) {
			if (n == 0) return false;

			return (int)Math.Ceiling(Math.Log(n, 2)) == (int)Math.Floor(Math.Log(n, 2));
		}

		public static bool IsEqualPowerOfTwo(Texture2D tex) {
			if (tex.width != tex.height) return false;
			if (!_IsPoT(tex.width) || !_IsPoT(tex.height)) return false;
			return true;
		}
	}
}
