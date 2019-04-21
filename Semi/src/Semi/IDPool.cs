using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Semi {
	/// <summary>
	/// Shortcut for specifying an <c>IDPool</c> without tags.
	/// </summary>
	public class IDPool<T> : IDPool<T, IDPool<T, bool>.NoTags> { }

	/// <summary>
	/// An extended dictionary featuring namespace handling, defaulting and locking as well as tags.
	/// An basic intro on how this works is:
	/// - Attempting to add an entry with the ID <c>test</c> will fail, as it has no namespace.
	/// - Attempting to add an entry with the ID <c>something:test</c> will succeed.
	/// - If the namespace <c>something</c> is locked, attempting to add an entry with the ID <c>something:test</c> will fail.
	/// - Attempting to retrieve an entry with the ID <c>something:test</c> will succeed if such an entry exists.
	/// - Attempting to retrieve an entry with the ID <c>test</c> will expand this ID with the default namespace to produce <c>gungeon:test</c>, and the operation will succeed if such an entry exists.
	/// 
	/// ID pools are used to allow mod authors not to worry about conflicts that would inevitably appear as a result of using numeric IDs.
	/// Namespaces introduce another protection against ID conflicts, as thanks to them more than one mod can add content with the same ID (and not break anything, because the namespace makes it unique).
	/// </summary>
	/// <typeparam name="T">Type of the object stored in this pool.</typeparam>
	/// <typeparam name="TTag">Type to use for the tag. Even though it's not enforced at compiletime, this type parameter must be an enum type.</typeparam>
    public class IDPool<T, TTag> {
		/// <summary>
		/// Empty enum, used to define an ID pool that doesn't use tags.
		/// </summary>
		public enum NoTags { }

        private Dictionary<string, T> _Storage = new Dictionary<string, T>();
        private HashSet<string> _LockedNamespaces = new HashSet<string>();
        private HashSet<string> _Namespaces = new HashSet<string>();
        private Dictionary<string, TTag> _Tags = new Dictionary<string, TTag>();

		/// <summary>
		/// Allows accessing content in the pool using indexer syntax.
		/// Setting an ID will replace an existing entry with the same ID unless the namespace is locked, in which case it'll throw an exception.
		/// </summary>
		/// <param name="id">Identifier.</param>
        public T this[string id] {
            set {
                Set(Resolve(id), value);
            }
            get {
                return Get(id);
            }
        }

		/// <summary>
		/// Gets the amount of entries in this ID pool.
		/// </summary>
		/// <value>The count.</value>
        public int Count {
            get {
                return _Storage.Count;
            }
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Semi.IDPool`2"/> class.
		/// </summary>
		public IDPool() {}

        //Exceptions

		/// <summary>
		/// Base class for exceptions related to ID pools.
		/// </summary>
        public class IDPoolException : Exception {
            public IDPoolException(string msg) : base(msg) { }
        }

		/// <summary>
		/// Thrown when an attempt is made to acquire an entry under an ID that doesn't exist.
		/// </summary>
        public class NonExistantIDException : IDPoolException {
            public NonExistantIDException(string id) : base($"Object with ID {id} doesn't exist") { }
        }

		/// <summary>
		/// Thrown when a fragment of the ID contains illegal characters.
		/// </summary>
        public class BadIDElementException : IDPoolException {
            public BadIDElementException(string name) : base($"The ID's {name} can not contain any colons or whitespace") { }
        }

		/// <summary>
		/// Thrown when an attempt is made to modify entries in a locked namespace.
		/// </summary>
        public class LockedNamespaceException : IDPoolException {
            public LockedNamespaceException(string namesp) : base($"The ID namespace {namesp} is locked") { }
        }

		/// <summary>
		/// Thrown when an attempt is made to explicitly add (not set) an entry, but an entry with the same ID already exists.
		/// </summary>
        public class ItemIDExistsException : IDPoolException {
            public ItemIDExistsException(string id) : base($"Item {id} already exists") { }
        }

		/// <summary>
		/// Thrown when the ID is not formatted properly.
		/// </summary>
        public class BadlyFormattedIDException : IDPoolException {
            public BadlyFormattedIDException(string id) : base($"ID was improperly formatted: {id}") { }
        }

        //Methods

		/// <summary>
		/// Locks a namespace, preventing any methods from modifying entries under it.
		/// </summary>
		/// <param name="namesp">Namespace to lock.</param>
        public void LockNamespace(string namesp) {
            _LockedNamespaces.Add(namesp);
        }

		/// <summary>
		/// Acquires the tag of a particular ID, or the default value for the type if the entry has no tags.
		/// </summary>
		/// <returns>The tag enum.</returns>
		/// <param name="id">ID of the entry.</param>
        public TTag GetTag(string id) {
            id = Resolve(id);
            VerifyID(id);
            if (id.Any(char.IsWhiteSpace)) throw new BadIDElementException("name");
            if (!_Storage.ContainsKey(id)) throw new NonExistantIDException(id);
            TTag t;
            if (_Tags.TryGetValue(id, out t)) {
                return t;
            }
            return default(TTag);
        }

		/// <summary>
		/// Sets the tag of a particular entry.
		/// </summary>
		/// <param name="id">ID of the entry.</param>
		/// <param name="tag">Tag enum value to replace the old value with.</param>
        public void SetTag(string id, TTag tag) {
            id = Resolve(id);
            VerifyID(id);
            var entry = Split(id);
            if (_LockedNamespaces.Contains(entry.Namespace)) throw new LockedNamespaceException(entry.Namespace);
            if (id.Any(char.IsWhiteSpace)) throw new BadIDElementException("name");
            if (!_Storage.ContainsKey(id)) throw new NonExistantIDException(id);
            _Tags[id] = tag;
        }

		/// <summary>
		/// Adds a bitflag to the tag of an entry with a particular ID, or creates a tag of the specified value for that entry if it doesn't have one.
		/// This assumes that the tag type is an enum marked with <c>[Flags]</c>.
		/// </summary>
		/// <param name="id">ID of the entry.</param>
		/// <param name="tag">Bitflag to add to the entry's tag.</param>
        public void AddTag(string id, TTag tag) {
            id = Resolve(id);
            VerifyID(id);
            var entry = Split(id);
            if (_LockedNamespaces.Contains(entry.Namespace)) throw new LockedNamespaceException(entry.Namespace);
            if (id.Any(char.IsWhiteSpace)) throw new BadIDElementException("name");
            if (!_Storage.ContainsKey(id)) throw new NonExistantIDException(id);
            TTag old_tag;
            if (_Tags.TryGetValue(id, out old_tag)) {
                _Tags[id] = (TTag)(object)((int)(object)old_tag | (int)(object)tag); // ugly
            } else {
                _Tags[id] = tag;
            }
        }

		/// <summary>
		/// Removes a bitflag from the tag of an entry with a particular ID. Does nothing if the entry doesn't have a tag.
		/// This assumes that the tag type is an enum marked with <c>[Flags]</c>.
		/// </summary>
		/// <param name="id">ID of the entry.</param>
		/// <param name="tag">Bitflag to remove from the entry's tag.</param>
        public void RemoveTag(string id, TTag tag) {
            id = Resolve(id);
            VerifyID(id);
            var entry = Split(id);
            if (_LockedNamespaces.Contains(entry.Namespace)) throw new LockedNamespaceException(entry.Namespace);
            if (id.Any(char.IsWhiteSpace)) throw new BadIDElementException("name");
            if (!_Storage.ContainsKey(id)) throw new NonExistantIDException(id);
            TTag old_tag;
            if (_Tags.TryGetValue(id, out old_tag)) {
                _Tags[id] = (TTag)(object)((int)(object)old_tag & ~(int)(object)tag); // ugly
            }
        }

		/// <summary>
		/// Checks for whether the tag of an entry with a particular ID contains a bitflag.
		/// This assumes that the tag type is an enum marked with <c>[Flags]</c>.
		/// </summary>
		/// <param name="id">ID of the entry.</param>
		/// <param name="tag">Bitflag to check for the existence of.</param>
        public bool HasTag(string id, TTag tag) {
            id = Resolve(id);
            VerifyID(id);
            var entry = Split(id);
            if (_LockedNamespaces.Contains(entry.Namespace)) throw new LockedNamespaceException(entry.Namespace);
            if (id.Any(char.IsWhiteSpace)) throw new BadIDElementException("name");
            if (!_Storage.ContainsKey(id)) throw new NonExistantIDException(id);
            TTag old_tag;
            if (_Tags.TryGetValue(id, out old_tag)) {
                var tagint = (int)(object)tag;
                return ((int)(object)old_tag & tagint) == tagint;
            } else {
                return false;
            }
        }

		/// <summary>
		/// Sets the ID to a particular entry, overwriting any existing entry at this ID.
		/// If the entry being overwritten is in a locked namespace, this method will throw an exception.
		/// </summary>
		/// <param name="id">ID of the new entry.</param>
		/// <param name="obj">Object to assign to the specified ID.</param>
        public void Set(string id, T obj) {
            id = Resolve(id);
            VerifyID(id);
            var entry = Split(id);
            if (_LockedNamespaces.Contains(entry.Namespace)) throw new LockedNamespaceException(entry.Namespace);
            if (id.Any(char.IsWhiteSpace)) throw new BadIDElementException("name");
            _Storage[id] = obj;
            if (!_Namespaces.Contains(entry.Namespace)) {
                _Namespaces.Add(entry.Namespace);
            }
        }

		/// <summary>
		/// Adds an entry with a particular ID.
		/// This method will throw if an entry already exists with that ID.
		/// </summary>
		/// <param name="id">ID of the new entry.</param>
		/// <param name="obj">Object to assign to the specified ID.</param>
        public void Add(string id, T obj) {
            id = Resolve(id);
            if (_Storage.ContainsKey(id)) throw new ItemIDExistsException(id);
            Set(id, obj);
        }

		/// <summary>
		/// Adds an entry with a particular ID and a preset tag.
		/// This method will throw if an entry already exists with that ID.
		/// </summary>
		/// <param name="id">ID of the new entry.</param>
		/// <param name="obj">Object to assign to the specified ID.</param>
		/// <param name="tag">Tag to set on this entry.</param>
        public void Add(string id, T obj, TTag tag) {
            id = Resolve(id);
            if (_Storage.ContainsKey(id)) throw new ItemIDExistsException(id);
            Set(id, obj);
            _Tags[id] = tag;
        }

		/// <summary>
		/// Retrieves an entry with the specified ID.
		/// This method will throw if the entry doesn't exist.
		/// </summary>
		/// <returns>The object assigned to this ID.</returns>
		/// <param name="id">ID of the entry.</param>
        public T Get(string id) {
            id = Resolve(id);
            if (!_Storage.ContainsKey(id)) throw new NonExistantIDException(id);
            return _Storage[id];
        }

		/// <summary>
		/// Removes the entry from this ID pool.
		/// This method will throw if the ID is in a locked namespace.
		/// </summary>
		/// <param name="id">ID of the entry.</param>
		/// <param name="destroy">If set to <c>true</c>, will destroy the object if it is a <c>UnityEngine.Object</c>. If it is not a descendant of that type, this argument will not do anything.</param>
        public void Remove(string id, bool destroy = true) {
            id = Resolve(id);
            var split = Split(id);
            if (_LockedNamespaces.Contains(split.Namespace)) throw new LockedNamespaceException(split.Namespace);
            if (!_Storage.ContainsKey(id)) throw new NonExistantIDException(id);
            if (_Storage[id] is UnityEngine.Object && destroy) UnityEngine.Object.Destroy(_Storage[id] as UnityEngine.Object);
            _Storage.Remove(id);
        }

		/// <summary>
		/// Renames an entry into a new ID. This supports renaming the namespace as well.
		/// This method will throw if the source entry's ID is in a locked namespace.
		/// If an entry already exists with the same ID as the target, it will be overwritten unless it is in a locked namespace, in which case the method will throw as well.
		/// </summary>
		/// <param name="source">ID of the entry to rename.</param>
		/// <param name="target">New ID to rename the entry to.</param>
        public void Rename(string source, string target) {
            source = Resolve(source);
            target = Resolve(target);
            var target_entry = Split(target);
            if (_LockedNamespaces.Contains(target_entry.Namespace)) throw new LockedNamespaceException(target_entry.Namespace);
            if (!_Storage.ContainsKey(source)) throw new NonExistantIDException(source);

            var obj = _Storage[source];
            _Storage.Remove(source);
            _Storage[target] = obj;
        }

		/// <summary>
		/// Throws an exception if the ID doesn't contain more than one colon character.
		/// Note that the ID provided to this method is not checked in any other way.
		/// </summary>
		/// <param name="id">IDPool format ID.</param>
        public static void VerifyID(string id) {
            if (id.Count(':') > 1) throw new BadlyFormattedIDException(id);
        }

		/// <summary>
		/// Throws if an entry with the specified ID wouldn't be able to be added to this pool.
		/// This method does not check if an entry of the same ID already exists. It also does not actually add the entry to the pool.
		/// </summary>
		/// <returns>The resolved ID in case the provided ID had no namespace, otherwise the same string.</returns>
		/// <param name="id">ID of the new entry.</param>
		public string ValidateNewEntry(string id) {
			VerifyID(id);
			id = Resolve(id);
			var id_split = Split(id);
			if (_LockedNamespaces.Contains(id_split.Namespace)) throw new LockedNamespaceException(id_split.Namespace);
			return id;
		}

		/// <summary>
		/// Throws if the entry with the specified ID wouldn't be able to be acquired from this pool.
		/// This method does not check if an entry of the same ID already exists. It also does not actually add the entry to the pool.
		/// </summary>
		/// <returns>The resolved ID in case the provided ID had no namespace, otherwise the same string.</returns>
		/// <param name="id">ID of the entry.</param>
		public string ValidateEntry(string id) {
			VerifyID(id);
			id = Resolve(id);
			if (!_Storage.ContainsKey(id)) throw new NonExistantIDException(id);
			return id;
		}

		/// <summary>
		/// Resolves an ID. This means adding the default namespace (<c>gungeon:</c>) if the ID doesn't have one.
		/// This method will throw if the ID has more than one colon character in it.
		/// </summary>
		/// <returns>The resolved ID.</returns>
		/// <param name="id">ID of the entry. Note that this method does not check whether an entry with this ID actually exists in this pool.</param>
        public static string Resolve(string id) {
            id = id.Trim();
            if (id.Contains(":")) {
                VerifyID(id);
                return id;
            } else {
                return $"gungeon:{id}";
            }
        }

		/// <summary>
		/// Resolves a list of IDs (<c>IList</c> interface). See <see cref="Resolve"/>.
		/// If <c>ids</c> is <c>null</c>, the result of this method will be a string array of size 0.
		/// </summary>
		/// <returns>Array of resolved IDs.</returns>
		/// <param name="ids">List of IDs.</param>
		public static string[] ResolveList(IList<string> ids) {
			if (ids == null) return new string[0];
			var ary = new string[ids.Count];
			for (int i = 0; i < ids.Count; i++) {
				ary[i] = Resolve(ids[i]);
			}
			return ary;
		}

        //Strut

		/// <summary>
		/// A struct representation of an ID, split into a namespace and a name.
		/// </summary>
        public struct Entry {
            public string Namespace;
            public string Name;

            public Entry(string namesp, string name) {
                Namespace = namesp;
                Name = name;
            }
        }

		/// <summary>
		/// Splits an ID into an <see cref="T:Semi.IDPool.Entry"/>.
		/// </summary>
		/// <returns>The split entry.</returns>
		/// <param name="id">The ID to split. Note that this method does not check for whether this ID exists in this pool.</param>
        public static Entry Split(string id) {
            VerifyID(id);
            string[] split = id.Split(':');
            if (split.Length != 2) throw new BadlyFormattedIDException(id);
            return new Entry(split[0], split[1]);
        }

        //bools

		/// <summary>
		/// Checks if this pool contains an entry with the provided ID.
		/// </summary>
		/// <returns><c>true</c>, if the pool contains an entry with this ID, <c>false</c> otherwise.</returns>
		/// <param name="id">ID to check.</param>
        public bool ContainsID(string id) {
            return _Storage.ContainsKey(Resolve(id));
        }

		/// <summary>
		/// Checks if a namespace is locked.
		/// </summary>
		/// <returns><c>true</c>, if the namespace is locked, <c>false</c> otherwise.</returns>
		/// <param name="namesp">Namespace to check.</param>
        public bool NamespaceIsLocked(string namesp) {
            return _LockedNamespaces.Contains(namesp);
        }

		/// <summary>
		/// Gets a string array of all the IDs in this pool (with namespaces included in all of them).
		/// </summary>
		/// <value>All IDs.</value>
        public string[] AllIDs {
            get {
                return _Storage.Keys.ToArray();
            }
        }

        //IEnumerables
		/// <summary>
		/// Enumerates all the entries in this pool.
		/// </summary>
		/// <value>An enumerable of all the entries.</value>
        public IEnumerable<T> Entries {
            get {
                foreach (var v in _Storage.Values) {
                    yield return v;
                }
            }
        }

		/// <summary>
		/// Enumerates all the IDs in this pool (with namespaces included).
		/// </summary>
		/// <value>An enumerable of all the IDs.</value>
        public IEnumerable<string> IDs {
            get {
                foreach (var k in _Storage.Keys) {
                    yield return k;
                }
            }
        }

		/// <summary>
		/// Enumerates all the pairs of all the entries in this pool along with their IDs. All the IDs will have namespaces included in them.
		/// </summary>
		/// <value>An enumerable of all the entries along with IDs.</value>
        public IEnumerable<KeyValuePair<string, T>> Pairs {
            get {
                foreach (var kv in _Storage) {
                    yield return new KeyValuePair<string, T>(kv.Key, kv.Value);
                }
            }
        }

		/// <summary>
		/// Gets a random object from this pool.
		/// </summary>
		/// <value>A random value.</value>
        public T RandomValue {
            get {
                var count = _Storage.Count;
                var idx = UnityEngine.Random.Range(0, count - 1);
                return _Storage.Values.ToList()[idx];
            }
        }

		/// <summary>
		/// Gets a random ID from this pool. The ID will always include the namespace.
		/// </summary>
		/// <value>A random key.</value>
        public string RandomKey {
            get {
                var count = _Storage.Count;
                var idx = UnityEngine.Random.Range(0, count - 1);
                return _Storage.Keys.ToList()[idx];
            }
        }

		/// <summary>
		/// Gets a random object and ID pair from this pool. The ID will always include the namespace.
		/// </summary>
		/// <value>An random key/value pair..</value>
        public KeyValuePair<string, T> RandomPair {
            get {
                var count = _Storage.Count;
                var idx = UnityEngine.Random.Range(0, count - 1);
                var key = _Storage.Keys.ToList()[idx];
                return new KeyValuePair<string, T>(key, _Storage[key]);
            }
        }
    }
}