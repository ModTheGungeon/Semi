using System;
using System.IO;

namespace Semi {
    public static class IDMapParser<T> {
        public class IDMapFormatException : Exception {
            public IDMapFormatException(string msg) : base($"Mismatched ID map format: {msg}") { }
        }

		/// <summary>
		/// Defines how to acquire the item based on the ID in string format.
		/// </summary>
        public delegate T AcquireItem(string id);
		/// <summary>
		/// Defines what to do with the item once it's been acquired, before registering it to the <c>IDPool</c>.
		/// </summary>
		public delegate void DoAfterAcquiredItem(ID strid, T item);

		/// <summary>
		/// Parses a file in Semi IDMap format.
		/// </summary>
		/// <returns>A new <c>IDPool</c> containing all the specified objects.</returns>
		/// <param name="file">IDMap file.</param>
		/// <param name="nspace">Namespace to assign to all the entries.</param>
		/// <param name="callback"><see cref="AcquireItem"/></param>
		/// <param name="percent_space_escape">If set to <c>true</c>, triple percent signs will be replaced with single spaces in source IDs.</param>
		/// <param name="do_after"><see cref="DoAfterAcquiredItem" /></param>
		public static IDPool<T> Parse(StreamReader file, string nspace, AcquireItem callback, bool percent_space_escape = true, DoAfterAcquiredItem do_after = null) {
            var pool = new IDPool<T>();

            while (!file.EndOfStream) {
                var line = file.ReadLine().Trim();
                if (line.StartsWithInvariant("#") || line.Length == 0) continue;

                var elements = line.Split(' ');
                if (elements.Length != 3) throw new IDMapFormatException("Each line must have three elements separated by spaces");

                var strtaglist = elements[0];
                var strnid = elements[1];
                var strid = elements[2];

                var strtags = strtaglist.Split(',');

                if (percent_space_escape) strnid = strnid.Replace("%%%", " ");

                var item = callback.Invoke(strnid);
                var id = ((ID)strid).WithNamespace(nspace);

                pool.Add(id, item);

                // ignore deprecated strtags

				if (do_after != null) {
					do_after.Invoke(id, item);
				}
            }

            return pool;
        }
    }
}
