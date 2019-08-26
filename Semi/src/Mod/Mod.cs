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
		/// Called after all <c>RegisterContent</c> methods are ran.
		/// You can't register any new content in this method, but you can use pre-existing references and modify their fields.
		/// You can do things like iterating on items in this method because no new ID pool entries will be added.
		/// </summary>
		public abstract void InitializeContent();

		/// <summary>
		/// Called when Semi loads the mod.
		/// You cannot register any content in this method. Use it to setup fields and such.
		/// </summary>
        public abstract void Loaded();

		/// <summary>
		/// Called when the save file is updated.
		/// This method should return a string that you will later load in Deserialize().
		/// Use this to implement persistence in your mod (save file interaction).
		/// </summary>
		public virtual string Serialize() { return null; }

		/// <summary>
		/// Called when the save file is loaded.
		/// This method is fed the last recorded result of Serialize().
		/// Use this to implement persistence in your mod (save file interaction).
		/// </summary>
		public virtual void Deserialize(string s) { }
    }

	public struct ModError {
		public Exception Exception;
		public string Name;
		public string ID;

		public string DisplayName {
			get {
				if (Name == null) return ID;
				return $"{Name} ({ID})";
			}
		}

		public string ExceptionInfo {
			get {
				return $"[{Exception.GetType().FullName}] {Exception.Message}";
			}
		}
	}
}
