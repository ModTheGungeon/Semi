using System;
using System.IO;
using System.Collections.Generic;

namespace Semi {
    public static class SimpleListFileParser {
        public static string[] Parse(StreamReader file, bool trim = true) {
            List<string> entries = null; 
            // slight optimization
            // will create this list on the first valid line
            // this is so that empty files or files with just
            // comments result in a null return and no memory
            // allocation

            while (!file.EndOfStream) {
                var line = file.ReadLine();
                if (trim) line = line.Trim();

                if (line.StartsWithInvariant("#")) continue;
                if (line.Length == 0 || line.Trim().Length == 0) continue;

                if (entries == null) entries = new List<string>();
                entries.Add(line);
            }

            return entries?.ToArray();
        }

        public static string[] ParseFile(string path, bool trim = true) {
            using (var f = new StreamReader(File.OpenRead(path))) return Parse(f, trim);
        }
    }
}
