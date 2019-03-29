using System;
using UnityEngine;

namespace Semi {
    public abstract class Mod : MonoBehaviour {
        public string ID {
            get { return Config.ID; }
        }

        private ModConfig _Config;
        public ModConfig Config {
            get { return _Config; }
            set {
                if (_Config == null) {
                    _Config = value;
                    return;
                }
                throw new InvalidOperationException("Cannot change mod config after the mod is loaded");
            }
        }

        public abstract void Loaded();
    }
}
