using System;
using UnityEngine;

namespace Semi {
    public abstract partial class Mod : MonoBehaviour {
        public string ID {
            get { return Config.ID; }
        }

        public ModConfig Config {
			get { return Info.Config; }
        }

		private SemiLoader.ModInfo _Info;
		public SemiLoader.ModInfo Info {
			get { return _Info; }
			set {
				if (_Info == null) {
					_Info = value;
					return;
				}
				throw new InvalidOperationException("Cannot change mod config after the mod is loaded");
			}
		}

		public abstract void RegisterContent();
        public abstract void Loaded();
    }
}
