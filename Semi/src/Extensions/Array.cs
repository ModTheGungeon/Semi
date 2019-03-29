using System;
namespace Semi {
    public static class ArrayExt {
        public static bool Contains(this Array ary, object obj) {
            var idx = Array.IndexOf(ary, obj);
            return idx > -1;
        }
    }
}
