using System;
using System.Collections.Generic;

namespace Semi {
    public struct ID {
        public class InvalidIDException : Exception {
            public InvalidIDException(string id, string msg) : base($"ID '{id}' is invalid - {msg}") {

            }
        }

        private static string GUNGEON_NAMESPACE = "gungeon";
        private static string CONTEXT_NAMESPACE = "@";
        private static StringView GUNGEON_NAMESPACE_VIEW = new StringView(GUNGEON_NAMESPACE, 0);
        private static StringView CONTEXT_NAMESPACE_VIEW = new StringView(CONTEXT_NAMESPACE, 0);

        public bool DefaultNamespace;
        public bool Contextual;
        public StringView Namespace;
        public StringView Name;

        public override int GetHashCode() {
            // Note that we don't make use of DefaultNamespace in hashes or comparison
            // because all IDs with DefaultNamespace = true will have Namespace as
            // GUNGEON_NAMESPACE_VIEW, but there can be entries with that namespace
            // that *don't* have DefaultNamespace set to true - in that case, we would
            // end up in different buckets and therefore be treated as two different
            // objects (in other words, the DefaultNamespace field is a legacy
            // technicality that isn't related in any way to what the ID struct *is*,
            // just what it *was* before "parsing"). Ideally it will become useless
            // once explicit context IDs (@:something) become enforced everywhere 
            // as opposed to implicit context IDs (e.g. something like #ABC in a 
            // localization expanding to #my_mod:ABC when parsing).
            // (We also don't use Contextual because it is equivalent to
            // Namespace == CONTEXT_NAMESPACE_VIEW).
            return (int)(Namespace.GetHashCodeUint() ^ Name.GetHashCodeUint());
        }

        public override string ToString() {
            return $"{Namespace}:{Name}";
        }

        public string ToLocalizationKey() {
            return $"#{Namespace}:{Name}";
        }

        public ID WithNamespace(string nspace) {
            return new ID {
                Namespace = new StringView(nspace, 0),
                Name = Name,
            };
        }

        public ID WithContextNamespace(string nspace) {
            if (Contextual) return WithNamespace(nspace);
            return this;
        }

        public static ID FromString(string id) {
            if (id.Count(':') > 1) throw new InvalidIDException(id, "too many colon characters");

            var loc = false;
            if (id.StartsWithInvariant("#")) {
                loc = true;
            }

            var colon_idx = id.IndexOf(':');
            if (colon_idx == -1) {
                if (!IsLaxIdentifier(new StringView(id, 0))) throw new InvalidIDException(id, "keys must be alphanumeric identifiers");
                return new ID { DefaultNamespace = true, Namespace = GUNGEON_NAMESPACE_VIEW, Name = new StringView(id, loc ? 1 : 0) };
            } else {
                StringView nspace;
                var contextual = false;
                if (id.StartsWithInvariant("gungeon:")) {
                    nspace = GUNGEON_NAMESPACE_VIEW;
                } else if (id.StartsWithInvariant("@:")) {
                    nspace = CONTEXT_NAMESPACE_VIEW;
                    contextual = true;
                } else {
                    nspace = new StringView(id, loc ? 1 : 0, colon_idx);
                }
                var name = new StringView(id, colon_idx + (loc ? 2 : 1));
                if (nspace != CONTEXT_NAMESPACE_VIEW && !IsLaxIdentifier(name)) throw new InvalidIDException(id, "namespaces must be alphanumeric identifiers");
                if (!IsLaxIdentifier(name)) throw new InvalidIDException(id, "names must be alphanumeric identifiers");

                return new ID { Contextual = contextual, Namespace = nspace, Name = name };
            }
        }

        public static explicit operator ID(string s) {
            return FromString(s);
        }

        public static implicit operator string(ID s) {
            return s.ToString();
        }

        public bool EqualsID(ID id) {
            return Name == id.Name && Namespace == id.Namespace;
        }

        public override bool Equals(object obj) {
            if (obj is ID) return EqualsID((ID)obj);
            if (obj is string) return EqualsID((ID)(string)obj);
            return false;
        }

        internal static bool IsLaxIdentifierSymbol(char c) {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'z') ||
                   (c == '_' || c == '+' || c == '-' || c == '.' || c == '/');
        }

        internal static bool IsLaxIdentifier(StringView s) {
            if (s.Length == 0) return false;

            if (!IsLaxIdentifierSymbol(s[0])) return false;

            for (var i = 1; i < s.Length; i++) {
                var c = s[i];

                if (!IsLaxIdentifierSymbol(c)) return false;
            }

            return true;
        }

    }
}
