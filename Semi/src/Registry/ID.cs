using System;
using System.Collections.Generic;

namespace Semi {
    /// <summary>
    /// Representation of a Semi ID, including namespace-defaulting (`key` ->
    /// `gungeon:key`) and context ID (`@:key`) expansion.
    /// </summary>
    public struct ID : IEquatable<ID>, IEquatable<string> {
        public class InvalidIDException : Exception {
            public InvalidIDException(string id, string msg) : base($"ID '{id}' is invalid - {msg}") {

            }
        }

        internal ID(StringView nspace, StringView name, bool contextual, bool default_namespace) {
            Namespace = nspace;
            Name = name;
            Contextual = contextual;
            DefaultNamespace = default_namespace;
        }

        private static string GUNGEON_NAMESPACE = "gungeon";
        private static string CONTEXT_NAMESPACE = "@";
        private static StringView GUNGEON_NAMESPACE_VIEW = new StringView(GUNGEON_NAMESPACE, 0);
        private static StringView CONTEXT_NAMESPACE_VIEW = new StringView(CONTEXT_NAMESPACE, 0);

        /// <summary>
        /// Whether this ID was created from a default-namespaced style string
        /// (`key`). Note that in both the case of `gungeon:key` and `key`,
        /// the `Namespace` <see cref="StringView"/> will be equal to `gungeon`,
        /// but in the first case this field will be `false`. This is a field
        /// used sparsely in internal Semi code and should not be used by anything
        /// else. If you wish to autofill namespaces based on context, use the
        /// contextual ID system.
        /// </summary>
        public readonly bool DefaultNamespace;
        /// <summary>
        /// Whether this ID is a contextual style ID (`@:key`). In all cases,
        /// if this field is `true`, then the `Namespace` field is equal to `@`
        /// and vice versa. Context IDs are accepted by a select few methods
        /// (mostly in <see cref="Mod"/>), and they need to be expanded using
        /// <see cref="WithContextNamespace"/> before being passed to storages
        /// such as <see cref="IDPool{T}"/> or any other hash-based data structure.
        /// </summary>
        public readonly bool Contextual;
        /// <summary>
        /// Namespace part of this ID. Implemented as a <see cref="StringView"/>
        /// to avoid unnecessary heap allocations. Please note that in the case
        /// of the `gungeon` namespace (`key` or `gungeon:key`) and contextual
        /// IDs (`@:key`), this will be equal to a StringView of the expected
        /// values (`gungeon` and `@` respectively). See the description of
        /// <see cref="Contextual"/> to understand how to properly work with
        /// contextual IDs.
        /// </summary>
        public readonly StringView Namespace;
        /// <summary>
        /// Name (key) part of this ID. Implemented as a <see cref="StringView"/>
        /// to avoid unnecessary heap allocations.
        /// </summary>
        public readonly StringView Name;

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Semi.ID"/> object.
        /// It is very important to note that `DefaultNamespace` does not
        /// participate in the hashing process (i.e. whatever value it has,
        /// `ID`s with otherwise equal fields will yield the same hash).
        /// Unresolved contextual IDs have no way to retrieve context in this
        /// method and therefore two instances of `@:abc` will hash to the
        /// same value, even if the intention was for them to resolve to two
        /// different namespaces. Expand the context with
        /// <see cref="WithContextNamespace"/> before calling this method.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode() {
            return (int)(Namespace.GetHashCodeUint() ^ Name.GetHashCodeUint());
        }

        /// <summary>
        /// Returns a string representation of this ID, expanding `key` style IDs
        /// to `gungeon:key` style IDs if necessary.
        /// </summary>
        /// <returns>The string representation of this ID.</returns>
        public override string ToString() {
            return $"{Namespace}:{Name}";
        }

        /// <summary>
        /// Returns a string representation of this ID as a localization key
        /// to be used in localized text by prepending it with a `#`.
        /// Note that in the current version of Semi this is also a valid
        /// `ID` representation and will correctly be read by <see cref="FromString"/>.
        /// </summary>
        /// <returns>The localization key.</returns>
        public string ToLocalizationKey() {
            return $"#{Namespace}:{Name}";
        }

        /// <summary>
        /// Returns a new `ID` with the same `Name` as this one, but with
        /// a different `Namespace`. If `nspace` is `@`, then the result is
        /// a contextual ID.
        /// </summary>
        /// <returns>The new ID.</returns>
        /// <param name="nspace">The target namespace.</param>
        public ID WithNamespace(string nspace) {
            return new ID(
                nspace: new StringView(nspace, 0),
                name: Name,
                contextual: nspace == "@",
                default_namespace: false
            );
        }

        /// <summary>
        /// If this `ID` is a contextual ID, then the result of this is
        /// equivalent to <see cref="WithNamespace"/>. Otherwise, the same `ID`
        /// that this is being called on is returned.
        /// </summary>
        /// <returns>The contextless ID.</returns>
        /// <param name="nspace">The target namespace.</param>
        public ID WithContextNamespace(string nspace) {
            if (Contextual) return WithNamespace(nspace);
            return this;
        }

        /// <summary>
        /// Creates a new `ID` from a string representation (`space:key`, `key`,
        /// `@:key`). Will also create an `ID` from a localization key (`#space:key`,
        /// `#key`, `#@:key`). As opposed to using this method directly, it is
        /// recommended to use the more idiomatic explicit cast (`(ID)"foo:bar"`).
        /// </summary>
        /// <returns>The new ID.</returns>
        /// <param name="id">The string representation of the ID.</param>
        public static ID FromString(string id) {
            if (id.Count(':') > 1) throw new InvalidIDException(id, "too many colon characters");

            var loc = false;
            if (id.StartsWithInvariant("#")) {
                loc = true;
            }

            var colon_idx = id.IndexOf(':');
            if (colon_idx == -1) {
                if (!IsLaxIdentifier(new StringView(id, 0))) throw new InvalidIDException(id, "keys must be alphanumeric identifiers");
                return new ID (nspace: GUNGEON_NAMESPACE_VIEW, name: new StringView(id, loc ? 1 : 0), contextual: false, default_namespace: true);
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

                return new ID(nspace: nspace, name: name, contextual: contextual, default_namespace: false);
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

        public bool Equals(ID other) {
            return EqualsID(other);
        }

        public bool Equals(string other) {
            return EqualsID((ID)other);
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
