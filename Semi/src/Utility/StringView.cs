using System;
namespace Semi {
    public class StringView {
        private string _SourceString;
        private StringView _SourceStringView;
        public int SourceStringPosition;
        private int? _ForcedLength = null;

        public int Length {
            get {
                return _ForcedLength ?? (_SourceStringView == null ? _SourceString.Length : _SourceStringView.Length ) - SourceStringPosition;
            }
        }

        private StringView(int source_len, int pos, int? len = null) {
            Repoint(source_len, pos, len);
        }

        public StringView(string s, int pos, int? len = null) : this(s.Length, pos, len) {
            _SourceString = s;
        }

        public StringView(StringView s, int pos, int? len = null) : this(s.Length, pos, len) {
            _SourceStringView = s;

        }

        private void Repoint(int source_len, int pos, int? len = null) {
            SourceStringPosition = pos;
            if (pos >= source_len) {
                throw new ArgumentException("String view is out of bounds", nameof(pos));
            }
            if (len == null) return;
            var real_len = pos + len;
            if (real_len > source_len) {
                throw new ArgumentException("String view is out of bounds", nameof(len));
            }
            _ForcedLength = len;
        }

        public void Repoint(string s, int pos, int? len = null) {
            _SourceString = s;
            _SourceStringView = null;
            Repoint(s.Length, pos, len);
        }

        public void Repoint(StringView s, int pos, int? len = null) {
            _SourceStringView = s;
            _SourceString = null;
            Repoint(s.Length, pos, len);
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

                return _SourceStringView == null ? _SourceString[index] : _SourceStringView[index];
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
            return _SourceStringView == null ? _SourceString.Substring(SourceStringPosition, Length) : _SourceStringView.Substring(SourceStringPosition, Length);
        }

        public override string ToString() {
            return CutRegion();
        }

        public string Substring(int pos, int? len = null) {
            if (len != null && len >= Length) {
                throw new ArgumentOutOfRangeException(nameof(len), $"Length parameter ({len}) is greater or equal to the length of this string view ({Length})");
            }
            if (_SourceStringView == null) {
                if (len == null) return _SourceString.Substring(SourceStringPosition + pos, Length - pos);
                else return _SourceString.Substring(SourceStringPosition + pos, len.Value);
            }
            return _SourceStringView.Substring(SourceStringPosition + pos, len ?? (Length - pos));
        }

        public StringView SubviewCopy(int pos, int? len = null) {
            return new StringView(CutRegion(), pos, len);
        }

        public StringView Subview(int pos, int? len = null) {
            return new StringView(this, pos, len);
        }
    }
}
