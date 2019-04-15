using System;
using System.IO;
using System.Text;

namespace Semi {
	public static class BinaryReaderExt {
		// http://stackoverflow.com/a/8613300
		/// <summary>
		/// Reads all bytes from the stream into a byte array.
		/// </summary>
		/// <returns>Array of all the remaining bytes in this stream.</returns>
		/// <param name="reader">Target reader.</param>
		public static byte[] ReadAllBytes(this BinaryReader reader) {
			const int bufferSize = 4096;
			using (var ms = new MemoryStream()) {
				byte[] buffer = new byte[bufferSize];
				int count;
				while ((count = reader.Read(buffer, 0, buffer.Length)) != 0) ms.Write(buffer, 0, count);
				return ms.ToArray();
			}
		}
	}
}
