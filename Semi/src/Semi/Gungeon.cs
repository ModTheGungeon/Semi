using System;
namespace Semi {
    public static class Gungeon {
        [Flags]
        public enum ItemTag {
            Unknown = 1,
            Item = 2,
            Consumable = 4,
            Syn = 8,

            Unused = 16,
            Internal = 32
        }

        [Flags]
        public enum EnemyTag {
            Unknown = 1,
            Enemy = 2,
            Friendly = 4,

            Unused = 16,
            Internal = 32
        }

        public static IDPool<PickupObject, ItemTag> Items { get; internal set; }
        public static IDPool<AIActor, EnemyTag> Enemies { get; internal set; }
    }
}
