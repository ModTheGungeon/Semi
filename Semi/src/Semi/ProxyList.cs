using System;
using System.Collections;
using System.Collections.Generic;

namespace Semi {
	public static class ListConverter {
		public static TTarget[] ToArray<TSource, TTarget>(IList<TSource> list, ProxyList<TSource, TTarget>.RevertOperation op) {
			var ary = new TTarget[list.Count];
			for (int i = 0; i < list.Count; i++) {
				ary[i] = op.Invoke(list[i]);
			}
			return ary;
		}

		public static ProxyList<TSource, TTarget> ToArrayProxyList<TSource, TTarget>(IList<TSource> list, ProxyList<TSource, TTarget>.ConvertOperation cop, ProxyList<TSource, TTarget>.RevertOperation rop) {
			return new ProxyList<TSource, TTarget>(
				ToArray(list, rop),
				cop,
				rop
			);
		}
	}

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

	public struct ProxyList<TSource, TTarget> : IList<TSource>, ICollection<TSource>, IEnumerable<TSource> {
		public delegate TSource ConvertOperation(TTarget obj);
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
