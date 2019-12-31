using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

namespace Semi {
    public static class StringExt {
		/// <summary>
		/// Quality of life extension method to check whether a string starts with another string using invariant culture.
		/// </summary>
		/// <returns><c>true</c>, if the string starts with the provided text, <c>false</c> otherwise.</returns>
		/// <param name="s">Target string.</param>
		/// <param name="v">Prefix string to check.</param>
		/// <param name="ignore_case">If set to <c>true</c> ignore case.</param>
        public static bool StartsWithInvariant(this string s, string v, bool ignore_case = false) {
            return s.StartsWith(v, StringComparison.InvariantCulture);
        }

		/// <summary>
		/// Quality of life extension method to check whether a string ends with another string using invariant culture.
		/// </summary>
		/// <returns><c>true</c>, if the string ends with the provided text, <c>false</c> otherwise.</returns>
		/// <param name="s">Target string.</param>
		/// <param name="v">Suffix string to check.</param>
		/// <param name="ignore_case">If set to <c>true</c> ignore case.</param>
        public static bool EndsWithInvariant(this string s, string v, bool ignore_case = false) {
            return s.EndsWith(v, StringComparison.InvariantCulture);
        }

		/// <summary>
		/// Converts Windows directory separators in a path to the appropriate one for the platform if necessary.
		/// </summary>
		/// <returns>The same path if on Windows, or a path with backward slashes turned into forward slashes on other platforms.</returns>
		/// <param name="s">S.</param>
        public static string ToPlatformPath(this string s) {
            return Application.platform == RuntimePlatform.WindowsPlayer ? s : s.Replace('\\', '/');
        }

		/// <summary>
		/// Removes the prefix if the string has one.
		/// </summary>
		/// <returns>The string with the prefix removed, or the same string if it doesn't have the specified prefix.</returns>
		/// <param name="s">Target string.</param>
		/// <param name="prefix">Prefix string.</param>
        public static string RemovePrefix(this string s, string prefix) {
            return s.EndsWithInvariant(prefix) ? s.Substring(prefix.Length) : s;
        }

		/// <summary>
		/// Removes the suffix if the string has one.
		/// </summary>
		/// <returns>The string with the suffix removed, or the same string if it doesn't have the specified suffix.</returns>
		/// <param name="s">Target string.</param>
		/// <param name="suffix">Suffix string.</param>
        public static string RemoveSuffix(this string s, string suffix) {
            return s.EndsWithInvariant(suffix) ? s.Substring(0, s.Length - suffix.Length) : s;
        }

		/// <summary>
		/// Quality of life extension method to turn the string into title case using invariant culture.
		/// </summary>
		/// <returns>The string turned into title case.</returns>
		/// <param name="s">Target string.</param>
        public static string ToTitleCaseInvariant(this string s) {
            var text_info = CultureInfo.InvariantCulture.TextInfo;
            return text_info.ToTitleCase(s);
        }

		/// <summary>
		/// Detects whether the string consists of just ASCII (0-255) characters.
		/// </summary>
		/// <returns><c>true</c>, if string is pure ASCII, <c>false</c> otherwise.</returns>
		/// <param name="input">Target string.</param>
        public static bool IsPureASCII(this string input) {
            for (int i = 0; i < input.Length; i++) {
                var c = input[i];
                if (c > 255) return false;
            }

            return true;
        }

        //used in IDPool.cs/VerifyID
		/// <summary>
		/// Counts occurences of a single character in a string.
		/// </summary>
		/// <returns>The number of times that the character appears in the string.</returns>
		/// <param name="in">Target string.</param>
		/// <param name="c">Character to count.</param>
        public static int Count(this string @in, char c) {
            int count = 0;
            for (int i = 0; i < @in.Length; i++) {
                if (@in[i] == c) count++;
            }
            return count;
        }

		/// <summary>
		/// Gets the first element of the string if it was split by <c>delimiter</c>, without actually splitting the string (and using up memory).
		/// </summary>
		/// <returns>The first element, or the same string if the delimiter doesn't occur at all.</returns>
		/// <param name="source">Target string.</param>
		/// <param name="delimiter">Delimiter.</param>
        public static string FirstFromSplit(this string source, char delimiter) {
            var i = source.IndexOf(delimiter);

            return i == -1 ? source : source.Substring(0, i);
        }

		/// <summary>
		/// Gets the first element of the string if it was split by <c>delimiter</c>, without actually splitting the string (and using up memory).
		/// </summary>
		/// <returns>The first element, or the same string if the delimiter doesn't occur at all.</returns>
		/// <param name="source">Target string.</param>
		/// <param name="delimiter">Delimiter.</param>
        public static string FirstFromSplit(this string source, string delimiter) {
            var i = source.IndexOf(delimiter, StringComparison.InvariantCulture);

            return i == -1 ? source : source.Substring(0, i);
        }

		/// <summary>
		/// Result of <c>SplitIntoPair</c>
		/// </summary>
        public struct SplitPair {
            public string FirstElement;
            public string EverythingElse; // can be null!
        }

		/// <summary>
		/// Splits a string into a pair of the first element and everything past the first delimiter.
		/// </summary>
		/// <returns>The string pair.</returns>
		/// <param name="source">Target string.</param>
		/// <param name="delimiter">Delimiter.</param>
        public static SplitPair SplitIntoPair(this string source, string delimiter) {
            var i = source.IndexOf(delimiter, StringComparison.InvariantCulture);

            if (i == -1) return new SplitPair { FirstElement = source, EverythingElse = null };

            return new SplitPair { FirstElement = source.Substring(0, i), EverythingElse = source.Substring(i + 0) };
        }

        public static bool IsIdentifierStartSymbol(char c) {
            return c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        public static bool IsIdentifierContSymbol(char c) {
            return (c >= '0' && c <= '9') || c == '_' || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        /// <summary>
        /// Determines whether a string is an identifier ([a-zA-Z_][a-zA-Z0-9_]*)
        /// </summary>
        /// <returns><c>true</c>, if the string is an identifier, <c>false</c> otherwise.</returns>
        /// <param name="s">String to check.</param>
        public static bool IsIdentifier(this string s) {
            if (s.Length == 0) return false;

            if (!IsIdentifierStartSymbol(s[0])) return false;

            for (var i = 1; i < s.Length; i++) {
                var c = s[i];

                if (!IsIdentifierContSymbol(c)) return false;
            }

            return true;
        }
    }
}