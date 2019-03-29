using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

namespace Semi {
    public static class StringExt {
        public static bool StartsWithInvariant(this string s, string v, bool ignore_case = false) {
            return s.StartsWith(v, StringComparison.InvariantCulture);
        }

        public static bool EndsWithInvariant(this string s, string v, bool ignore_case = false) {
            return s.EndsWith(v, StringComparison.InvariantCulture);
        }

        public static string ToPlatformPath(this string s) {
            return Application.platform == RuntimePlatform.WindowsPlayer ? s : s.Replace('\\', '/');
        }

        public static string RemovePrefix(this string s, string prefix) {
            return s.EndsWithInvariant(prefix) ? s.Substring(prefix.Length) : s;
        }

        public static string RemoveSuffix(this string s, string suffix) {
            return s.EndsWithInvariant(suffix) ? s.Substring(0, s.Length - suffix.Length) : s;
        }

        public static string ToTitleCaseInvariant(this string s) {
            var text_info = CultureInfo.InvariantCulture.TextInfo;
            return text_info.ToTitleCase(s);
        }

        public static bool IsPureASCII(this string input) {
            for (int i = 0; i < input.Length; i++) {
                var c = input[i];
                if (c > 255) return false;
            }

            return true;
        }
    }
}