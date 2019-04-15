using System;
using UnityEngine;

namespace Semi {
    public abstract partial class Mod : MonoBehaviour {
		/// <summary>
		/// Gets the ID specified in the mod metadata.
		/// </summary>
		/// <value>The mod's ID.</value>
        public string ID {
            get { return Config.ID; }
        }

		/// <summary>
		/// Gets the loaded mod metadata.
		/// </summary>
		/// <value>The mod's metadata representation.</value>
        public ModConfig Config {
			get { return Info.Config; }
        }

		/// <summary>
		/// Gets the stored representation of the loaded mod.
		/// </summary>
		/// <value>The stored mod object.</value>
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

		/// <summary>
		/// Called after all <c>Loaded</c> methods are ran.
		/// This is where you should register any content that your mod adds - items, guns etc.
		/// Do not iterate any ID lists (e.g. the item database) in this method, as there may be mods loaded after yours that add to them.
		/// If you need to register content that depends on ID lists, register only the most basic skeleton of your content in this method and initialize it in <c>InitializeContent</c>.
		/// </summary>
		public abstract void RegisterContent();

		/// <summary>
		/// Called when Semi loads the mod.
		/// You cannot register any content in this method. Use it to setup fields and such.
		/// </summary>
        public abstract void Loaded();
    }
}
