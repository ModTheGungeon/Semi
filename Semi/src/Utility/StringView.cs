using System;
using System.Collections.Generic;

namespace Semi {
    public static class FNVHash {
        // http://isthe.com/chongo/tech/comp/fnv/
        public const uint FNV_PRIME = 16777619;
        public const uint FNV_OFFSET_BASIS = 2166136261;

        public static uint Compute(StringView s) {
            var hash = FNV_OFFSET_BASIS;
            for (var i = 0; i < s.Length; i++) {
                var octet = (byte)s[i];
                hash = hash * FNV_PRIME;
                hash = hash ^ octet;
            }
            return hash;
        }

        public static uint Compute(string s) {
            var hash = FNV_OFFSET_BASIS;
            for (var i = 0; i < s.Length; i++) {
                var octet = (byte)s[i];
                hash = hash * FNV_PRIME;
                hash = hash ^ octet;
            }
            return hash;
        }
    }

    /// <summary>
    /// Provides a view into a section of a string, avoiding heap allocations
    /// that would happen if operations such as `string.Substring` were used.
    /// </summary>
    public struct StringView {
        private string _SourceString;
        public int SourceStringPosition;
        private int? _ForcedLength;
        private uint? _CachedHash;

        /// <value>The length of this view.</value>
        public int Length {
            get {
                return _ForcedLength ?? _SourceString.Length;
            }
        }

        /// <summary>
        /// Initializes a new StringView from the specified string.
        /// </summary>
        /// <param name="s">The backing string.</param>
        /// <param name="pos">The position in the string that will be used as the origin of the string view.</param>
        /// <param name="len">The optional length of the view within the provided string.</param>
        public StringView(string s, int pos, int? len = null) {
            _SourceString = s;
            var source_len = s.Length;
            SourceStringPosition = pos;
            if (pos >= source_len) {
                throw new ArgumentException("String view is out of bounds", nameof(pos));
            }
            _CachedHash = null;
            _ForcedLength = pos == 0 ? null : (int?)(s.Length - pos); 
            if (len == null) return;
            var real_len = pos + len;
            if (real_len > source_len) {
                throw new ArgumentException("String view is out of bounds", nameof(len));
            }
            _ForcedLength = len;
        }

        /// <summary>
        /// Calculates an FNV hash of the region specified by this string view
        /// (no copying is done).
        /// </summary>
        /// <returns>The hash code.</returns>
        public uint GetHashCodeUint() {
            if (_CachedHash == null) _CachedHash = FNVHash.Compute(this);
            return _CachedHash.Value;
        }

        /// <summary>
        /// Hash function for the fragment of a string pointed to by this string
        /// view. Equivalent to casting <see cref="GetHashCodeUint"/> to `int`.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode() {
            return (int)GetHashCodeUint();
        }

        public char this[int index] {
            get {
                if (_ForcedLength.HasValue && index >= _ForcedLength.Value) {
                    throw new IndexOutOfRangeException($"Length is {_ForcedLength.Value}, attempted to index {index}");
                }
                index += SourceStringPosition;

                return _SourceString[index];
            }
        }

        public bool StartsWith(string value, bool case_sensitive = true) {
            if (Length < value.Length) return false;
            for (var i = 0; i < value.Length; i++) {
                var matches_caseinsensitive = (case_sensitive && char.ToLower(value[i]) == char.ToLower(this[i]));
                if (value[i] != this[i] && !matches_caseinsensitive) return false; 
            }
            return true;
        }

        public bool EndsWith(string value, bool case_sensitive = true) {
            if (Length < value.Length) return false;
            for (var i = value.Length - 1; i >= 0; i++) {
                var real_idx = i + SourceStringPosition;
                var matches_caseinsensitive = (case_sensitive && char.ToLower(value[i]) == char.ToLower(this[i]));
                if (value[i] != this[i] && !matches_caseinsensitive) return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the data pointed to by this string view as a new `string`.
        /// This method will allocate the new string.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Semi.StringView"/>.</returns>
        public override string ToString() {
            return _SourceString.Substring(SourceStringPosition, Length);
        }

        /// <summary>
        /// Returns the substring of the data pointed to by this string view
        /// within the backing string. This method allocates memory for the new
        /// string. See <see cref="Subview"/> for a `StringView`-based alternative.
        /// </summary>
        /// <returns>The substring.</returns>
        /// <param name="pos">Position.</param>
        /// <param name="len">Length.</param>
        public string Substring(int pos, int? len = null) {
            if (len != null && len > Length) {
                throw new ArgumentOutOfRangeException(nameof(len), $"Length parameter ({len}) is greater than the length of this string view ({Length})");
            }
            if (len == null) return _SourceString.Substring(SourceStringPosition + pos, Length - pos);
            else return _SourceString.Substring(SourceStringPosition + pos, len.Value);
           
        }

        public bool EqualsString(string s) {
            if (Length != s.Length) return false;
            if (_SourceString == s && SourceStringPosition == 0) return true;
            if (_SourceString == s && SourceStringPosition != 0) return false;

            for (var i = 0; i < Length; i++) {
                if (this[i] != s[i]) return false;
            }

            return true;
        }
        public bool EqualsStringView(StringView v) {
            return GetHashCodeUint() == v.GetHashCodeUint();
        }

        /// <summary>
        /// Creates a new StringView that operates on a newly-allocated `string`
        /// region of the backing data of this string view.
        /// </summary>
        /// <returns>The new StringView.</returns>
        /// <param name="pos">Position.</param>
        /// <param name="len">Length.</param>
        public StringView SubviewCopy(int pos, int? len = null) {
            return new StringView(ToString(), pos, len);
        }

        /// <summary>
        /// Creates a new StringView that operates on the same backing string
        /// as this view, but a sub-fragment of it. As opposed to <see cref="SubviewCopy"/>,
        /// this method does not allocate any memory.
        /// </summary>
        /// <returns>The subview.</returns>
        /// <param name="pos">Position.</param>
        /// <param name="len">Length.</param>
        public StringView Subview(int pos, int? len = null) {
            if (len != null && len > Length) {
                throw new ArgumentOutOfRangeException(nameof(len), $"Length parameter ({len}) is greater than the length of this string view ({Length})");
            }
            return new StringView(_SourceString, SourceStringPosition + pos, len);
        }

        public override bool Equals(object obj) {
            if (obj is string) return EqualsString(obj as string);
            if (obj is StringView) return EqualsStringView((StringView)obj);
            return false;
        }

        public static explicit operator string(StringView s) {
            return s.ToString();
        }

        public static explicit operator StringView(string s) {
            return new StringView(s, 0);
        }

        public static bool operator ==(StringView v, string s) {
            if (object.ReferenceEquals(s, null)) return false;
            return v.EqualsString(s);
        }

        public static bool operator !=(StringView v, string s) {
            if (object.ReferenceEquals(s, null)) return true;
            return !v.EqualsString(s);
        }

        public static bool operator ==(string s, StringView v) {
            if (object.ReferenceEquals(s, null)) return false;
            return v.EqualsString(s);
        }

        public static bool operator !=(string s, StringView v) {
            if (object.ReferenceEquals(s, null)) return true;
            return !v.EqualsString(s);
        }

        public static bool operator ==(StringView v, StringView s) {
            return v.EqualsStringView(s);
        }

        public static bool operator !=(StringView v, StringView s) {
            return !v.EqualsStringView(s);
        }
    }
}
