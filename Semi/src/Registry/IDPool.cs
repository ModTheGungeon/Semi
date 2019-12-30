using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Semi {
    /// <summary>
    /// This class manages a mapping of <see cref="ID"/>s to objects.
    /// </summary>
    public class IDPool<T> {
        /// <summary>
        /// Main class for IDPool exceptions.
        /// </summary>
        public class IDPoolException : Exception {
            public IDPoolException(string msg) : base(msg) { }
        }

        /// <summary>
        /// Thrown if an attempt is made to map an object to an ID with a
        /// namespace that has been previously locked with `LockNamespace`.
        /// </summary>
        public class LockedNamespaceException : IDPoolException {
            public LockedNamespaceException(StringView namesp) : base($"The ID namespace {namesp} is locked") { }
        }

        /// <summary>
        /// Thrown if an attempt is made to acquire an object through `Get` or
        /// the index operator, but the ID provided to them isn't mapped to
        /// any object.
        /// </summary>
        public class MissingIDException : IDPoolException {
            public MissingIDException(ID id) : base($"The ID {id} does not exist") { }
        }

        /// <summary>
        /// Thrown if an attempt is made to map an object to an ID through `Add`
        /// (note: not `Set`), but that ID is already mapped to an object.
        /// </summary>
        public class IDAlreadyExistsException : IDPoolException {
            public IDAlreadyExistsException(ID id) : base($"The ID {id} already exists in this ID pool") { }
        }

        /// <summary>
        /// Thrown if a contextual ID is passed. Users of `IDPool` must
        /// expand contextual namespaces themselves using appropriate methods on
        /// <see cref="ID"/> before using any methods such as `Get`, `TryGet`,
        /// `Set` or `Add`.
        /// </summary>
        public class UnresolvedContextException : IDPoolException {
            public UnresolvedContextException(ID id) : base($"The contextual ID {id} was passed to the ID pool without expanding its context") { }
        }

        private Dictionary<ID, T> _Storage = new Dictionary<ID, T>();
        private HashSet<StringView> _LockedNamespaces = new HashSet<StringView>();

        /// <summary>
        /// Gets or sets the object mapped to the provided ID. Will throw while
        /// accessing if the object doesn't exist. Will not throw while setting
        /// if the object already exists (functionally equivalent to the `Set`
        /// method). If this behavior is preferred, use the `Add` method.
        /// </summary>
        /// <param name="id">The contextless ID.</param>
        public T this[ID id] {
            get {
                return Get(id);
            }
            set {
                Set(id, value);
            }
        }

        private void CheckNamespace(ID id) {
            if (_LockedNamespaces.Contains(id.Namespace)) throw new LockedNamespaceException(id.Namespace);
        }

        private void CheckContext(ID id) {
            if (id.Contextual) throw new UnresolvedContextException(id);
        }

        /// <summary>
        /// Retrieves an object with the specified ID.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="id">The contextless ID.</param>
        public T Get(ID id) {
            CheckContext(id);
            if (!_Storage.TryGetValue(id, out var entry)) {
                throw new MissingIDException(id);
            }
            return entry;
        }

        /// <summary>
        /// Retrieves an object with the specified ID. Sets `val` if the object
        /// exists.
        /// </summary>
        /// <returns>`true` if the object exists and `val` has been set,
        /// `false` otherwise.</returns>
        /// <param name="id">The contextless ID.</param>
        /// /// <param name="val">Output parameter for the object.</param>
        public bool TryGet(ID id, out T val) {
            CheckContext(id);
            return _Storage.TryGetValue(id, out val);
        }

        /// <summary>
        /// Retrieves an object with the specified ID, or the default
        /// value for that type if no object is mapped to that ID
        /// (`null` for objects).
        /// </summary>
        /// <returns>The object or `null`.</returns>
        /// <param name="id">The contextless ID.</param>
        public T TryGet(ID id) {
            CheckContext(id);
            if (!_Storage.TryGetValue(id, out var entry)) return default(T);
            return entry;
        }

        /// <summary>
        /// Maps the specified ID to the specified object. Will throw an
        /// exception if the namespace is locked.
        /// </summary>
        /// <param name="id">The contextless ID.</param>
        public void Set(ID id, T val) {
            CheckContext(id);
            CheckNamespace(id);
            _Storage[id] = val;
        }

        /// <summary>
        /// Maps the specified ID to the specified object. Will throw an
        /// exception if the namespace is locked or if an object is already
        /// mapped to this ID.
        /// </summary>
        /// <param name="id">The contextless ID.</param>
        public void Add(ID id, T val) {
            CheckContext(id);
            CheckNamespace(id);
            if (Contains(id)) throw new IDAlreadyExistsException(id);
            Set(id, val);
        }

        /// <summary>
        /// Locks the specified namespace, causing all future calls to
        /// `Set` and `Add` to fail if the ID provided to them has
        /// this namespace.
        /// </summary>
        /// <param name="nspace">Namespace to lock.</param>
        public void LockNamespace(string nspace) {
            _LockedNamespaces.Add(new StringView(nspace, 0));
        }

        /// <summary>
        /// Checks if this ID pool contains the specified ID.
        /// </summary>
        /// <returns>`true` if the ID is mapped, `false` if not.</returns>
        /// <param name="id">The contextless ID.</param>
        public bool Contains(ID id) {
            CheckContext(id);
            return _Storage.ContainsKey(id);
        }

        /// <summary>
        /// Ensures that the provided ID is mapped to an object. Will
        /// throw appropriate exceptions if not.
        /// </summary>
        /// <returns>The `id` parameter.</returns>
        /// <param name="id">The contextless ID.</param>
        public ID ValidateExisting(ID id) {
            CheckContext(id);
            Get(id);
            return id;
        }

        /// <summary>
        /// Provides an interface to enumerate entries in this ID pool.
        /// </summary>
        /// <value>Iterator over key-value pairs of IDs and objects.</value>
        public IEnumerable<KeyValuePair<ID, T>> Pairs {
            get {
                foreach (var kv in _Storage) {
                    yield return new KeyValuePair<ID, T>(kv.Key, kv.Value);   
                }
            }
        }
    }
}