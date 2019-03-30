using System;
using System.IO;

namespace Semi {
    public static class IDMapParser<T, TTag> {
        public class IDMapFormatException : Exception {
            public IDMapFormatException(string msg) : base($"Mismatched ID map format: {msg}") { }
        }

        public delegate T AcquireItem(string id);

        public static IDPool<T, TTag> Parse(StreamReader file, string nspace, AcquireItem callback, bool percent_space_escape = true) {
            var pool = new IDPool<T, TTag>();

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

                if (strtags.Length > 0) {
                    var tag = (TTag)(object)0;
                    for (int i = 0; i < strtags.Length; i++) {
                        tag = (TTag)(object)((int)(object)tag | (int)Enum.Parse(typeof(TTag), strtags[i], true));
                    }
                    pool.Add($"{nspace}:{strid}", item, tag);
                } else {
                    pool.Add($"{nspace}:{strid}", item);
                }
            }

            return pool;
        }
    }
}
