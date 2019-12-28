using System;
using System.Collections;
using System.Collections.Generic;

namespace Semi {
	public static class ListConverter {
		/// <summary>
		/// Converts a list of one type into an array of another type based on delegates.
		/// </summary>
		/// <returns>The converted array.</returns>
		/// <param name="list">The source list to convert.</param>
		/// <param name="op">The operation to perform on each element in order to convert it into the target type..</param>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The target type.</typeparam>
		public static TTarget[] ToArray<TSource, TTarget>(IList<TSource> list, ProxyList<TSource, TTarget>.RevertOperation op) {
			var ary = new TTarget[list.Count];
			for (int i = 0; i < list.Count; i++) {
				ary[i] = op.Invoke(list[i]);
			}
			return ary;
		}

		/// <summary>
		/// Converts a list of one type into a <see cref="Semi.ProxyList`2"/> providing a proxy of the same type into an array of a target type.
		/// </summary>
		/// <returns>The <c>ProxyList</c> backed by the converted array.</returns>
		/// <param name="list">The source list to convert.</param>
		/// <param name="cop">Convert (source to target) operation for the <c>ProxyList</c>.</param>
		/// <param name="rop">Revert (target to source) operation for the <c>ProxyList</c>.</param>
		/// <typeparam name="TSource">The source type.</typeparam>
		/// <typeparam name="TTarget">The target type.</typeparam>
		public static ProxyList<TSource, TTarget> ToArrayProxyList<TSource, TTarget>(IList<TSource> list, ProxyList<TSource, TTarget>.ConvertOperation cop, ProxyList<TSource, TTarget>.RevertOperation rop) {
			return new ProxyList<TSource, TTarget>(
				ToArray(list, rop),
				cop,
				rop
			);
		}
	}

	/// <summary>
	/// Provides a generic and a non-generic <c>IEnumerator</c> implementation for <see cref="Semi.ProxyList`2"/>.
	/// </summary>
	public class ProxyListEnumerator<TSource, TTarget> : IEnumerator, IEnumerator<TSource> {
		internal ProxyList<TSource, TTarget> ProxyList;
		internal int Pos = -1;

		public ProxyListEnumerator(ProxyList<TSource, TTarget> list) {
			ProxyList = list;
		}

		public object Current {
			get {
				try {
					return ProxyList[Pos];
				} catch (IndexOutOfRangeException) {
					throw new InvalidOperationException();
				}
			}
		}

		TSource IEnumerator<TSource>.Current {
			get {
				try {
					return ProxyList[Pos];
				} catch (IndexOutOfRangeException) {
					throw new InvalidOperationException();
				}
			}
		}

		public bool MoveNext() {
			Pos += 1;
			return Pos < ProxyList.Count;
		}

		public void Reset() {
			Pos = -1;
		}

		public void Dispose() {
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Struct that behaves like a list but does conversion operators internally to provide an <c>IList</c> of a different type without allocating any memory.
	/// </summary>
	/// <typeparam name="TSource">Source type (that is, the type that you want this <c>ProxyList</c> to act as a list of)</typeparam>
	/// <typeparam name="TTarget">Target type (that is, the element type of the real list/array underneath)</typeparam>
	public struct ProxyList<TSource, TTarget> : IList<TSource>, ICollection<TSource>, IEnumerable<TSource> {
		/// <summary>
		/// Convert operation. Receives a target type, outputs a source type.
		/// </summary>
		public delegate TSource ConvertOperation(TTarget obj);
		/// <summary>
		/// Convert operation. Receives a target type, outputs a source type.
		/// </summary>
		public delegate TTarget RevertOperation(TSource obj);

		internal IList<TTarget> Target;
		internal ConvertOperation Convert;
		internal RevertOperation Revert;

		public int Count {
			get {
				return Target.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return Target.IsReadOnly;
			}
		}

		public TSource this[int index] {
			get {
				return Convert.Invoke(Target[index]);
			}

			set {
				Target[index] = Revert.Invoke(value);
			}
		}

		public ProxyList(IList<TTarget> target, ConvertOperation cop, RevertOperation rop) {
			Target = target;
			Convert = cop;
			Revert = rop;
		}

		public int IndexOf(TSource item) {
			return Target.IndexOf(Revert.Invoke(item));
		}

		public void Insert(int index, TSource item) {
			Target.Insert(index, Revert.Invoke(item));
		}

		public void RemoveAt(int index) {
			Target.RemoveAt(index);
		}

		public void Add(TSource item) {
			Target.Add(Revert.Invoke(item));
		}

		public void Clear() {
			Target.Clear();
		}

		public bool Contains(TSource item) {
			return Target.Contains(Revert.Invoke(item));
		}

		public void CopyTo(TSource[] array, int arrayIndex) {
			throw new NotImplementedException();
		}

		public bool Remove(TSource item) {
			return Target.Remove(Revert.Invoke(item));
		}

		public IEnumerator<TSource> GetEnumerator() {
			return new ProxyListEnumerator<TSource, TTarget>(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return new ProxyListEnumerator<TSource, TTarget>(this);
		}

		public IList<TTarget> ToTarget() {
			return Target;
		}

		public TTarget[] ToTargetArray() {
			if (Target is TTarget[]) return (TTarget[])Target;
			throw new InvalidOperationException();
		}



		public ProxyList<TSource, TTarget> ConvertCompatibleList(IList<TSource> list) {
			return ListConverter.ToArrayProxyList(list, Convert, Revert);
		}
	}
}
