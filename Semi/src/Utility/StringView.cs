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
    //public class StringHashDictionary<T> {
    //    private Dictionary<uint, StringView> _KeyStrings = new Dictionary<uint, StringView>();
    //    private Dictionary<uint, T> _Dictionary = new Dictionary<uint, T>();

    //    public T this[string key] {
    //        get { return _Dictionary[FNVHash.Compute(key)]; }
    //        set {
    //            var hash = FNVHash.Compute(key);
    //            _Dictionary[hash] = value;
    //            _KeyStrings[hash] = new StringView(key, 0);
    //        }
    //    }

    //    public T this[StringView key] {
    //        get { return _Dictionary[key.GetHashCodeUint()]; }
    //        set {
    //            var hash = key.GetHashCodeUint();
    //            _Dictionary[hash] = value;
    //            _KeyStrings[hash] = key;
    //        }
    //    }

    //    public bool TryGetValue(string key, out T field) {
    //        return _Dictionary.TryGetValue(FNVHash.Compute(key), out field);
    //    }

    //    public bool TryGetValue(StringView key, out T field) {
    //        return _Dictionary.TryGetValue(key.GetHashCodeUint(), out field);
    //    }

    //    public bool ContainsKey(string key) {
    //        return _Dictionary.ContainsKey(FNVHash.Compute(key));
    //    }

    //    public bool ContainsKey(StringView key) {
    //        return _Dictionary.ContainsKey(key.GetHashCodeUint());
    //    }

    //    public IEnumerable<KeyValuePair<StringView, T>> Entries {
    //        get {
    //            foreach (var kv in _Dictionary) {
    //                yield return new KeyValuePair<StringView, T>(_KeyStrings[kv.Key], kv.Value);
    //            }
    //        }
    //    }
    //}

    //public class StringHashSet {
    //    // Note: no iteration since we only store hashes!

    //    private HashSet<uint> _HashSet = new HashSet<uint>();

    //    public void Add(string s) {
    //        _HashSet.Add(FNVHash.Compute(s));
    //    }

    //    public void Add(StringView s) {
    //        _HashSet.Add(s.GetHashCodeUint());
    //    }

    //    public bool Contains(string s) {
    //        return _HashSet.Contains(FNVHash.Compute(s));
    //    }

    //    public bool Contains(StringView s) {
    //        return _HashSet.Contains(s.GetHashCodeUint());
    //    }

    //    public void Remove(string s) {
    //        _HashSet.Remove(FNVHash.Compute(s));
    //    }

    //    public void Remove(StringView s) {
    //        _HashSet.Remove(s.GetHashCodeUint());
    //    }
    //}

    public struct StringView {
        private string _SourceString;
        public int SourceStringPosition;
        private int? _ForcedLength;
        private uint? _CachedHash;

        public int Length {
            get {
                return _ForcedLength ?? _SourceString.Length;
            }
        }

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

        public uint GetHashCodeUint() {
            if (_CachedHash == null) _CachedHash = FNVHash.Compute(this);
            return _CachedHash.Value;
        }

        public override int GetHashCode() {
            return (int)GetHashCodeUint();
        }

        public int RelativeToRealPosition(int pos) {
            return SourceStringPosition + pos;
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

        public string CutRegion() {
            return _SourceString.Substring(SourceStringPosition, Length);
        }

        public override string ToString() {
            return CutRegion();
        }

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

        public StringView SubviewCopy(int pos, int? len = null) {
            return new StringView(CutRegion(), pos, len);
        }

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
