using UnityEngine;

namespace Semi {
	public static class Texture2DExt {
		/// <summary>
		/// Checks whether the texture can be read.
		/// </summary>
		/// <returns><c>true</c>, if readable, <c>false</c> otherwise.</returns>
		/// <param name="texture">Target texture.</param>
		public static bool IsReadable(this Texture2D texture) {
#if DEBUG
			try {
				texture.GetPixels();
				return true;
			} catch {
				return false;
			}
#else
        return texture.GetRawTextureData().Length != 0; // spams log
#endif
		}

		/// <summary>
		/// Gets a read/write copy of the texture.
		/// </summary>
		/// <returns>A readable/writeable texture with the same data as the parameter, which will be a copy unless the passed texture is already R/W.</returns>
		/// <param name="texture">Target texture.</param>
		public static Texture2D GetRW(this Texture2D texture) {
			if (texture == null) {
				return null;
			}
			if (texture.IsReadable()) {
				return texture;
			}
			return texture.Copy();
		}

		/// <summary>
		/// Copies the specified texture with the selected format.
		/// </summary>
		/// <returns>A read/write copy of the texture.</returns>
		/// <param name="texture">Target texture.</param>
		/// <param name="format">Format of the new texture.</param>
		public static Texture2D Copy(this Texture2D texture, TextureFormat? format = TextureFormat.ARGB32) {
			if (texture == null) {
				return null;
			}
			RenderTexture copyRT = RenderTexture.GetTemporary(
				texture.width, texture.height, 0,
				RenderTextureFormat.Default, RenderTextureReadWrite.Default
			);

			Graphics.Blit(texture, copyRT);

			RenderTexture previousRT = RenderTexture.active;
			RenderTexture.active = copyRT;

			Texture2D copy = new Texture2D(texture.width, texture.height, format != null ? format.Value : texture.format, 1 < texture.mipmapCount);
			copy.name = texture.name;
			copy.ReadPixels(new Rect(0, 0, copyRT.width, copyRT.height), 0, 0);
			copy.Apply(true, false);

			RenderTexture.active = previousRT;
			RenderTexture.ReleaseTemporary(copyRT);

			return copy;
		}
	}
}