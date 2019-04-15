using System;
namespace Semi {
    public static class ArrayExt {
		/// <summary>
		/// Extension method to check whether the array contains an element (in O(n) time).
		/// </summary>
		/// <returns><c>true</c>, if the array contains the object, <c>false</c> otherwise.</returns>
		/// <param name="ary">Target array.</param>
		/// <param name="obj">Object to search for.</param>
        public static bool Contains(this Array ary, object obj) {
            var idx = Array.IndexOf(ary, obj);
            return idx > -1;
        }
    }
}
