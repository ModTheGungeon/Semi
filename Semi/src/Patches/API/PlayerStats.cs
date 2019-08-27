using System;
using System.Collections.Generic;
using MonoMod;

namespace Semi.Patches {
	/// <summary>
	/// Patches PlayerStats to create a hashset of active synergies and call synergy-related events.
	/// </summary>
	[MonoModPatch("global::PlayerStats")]
	public class PlayerStats : global::PlayerStats {
		public extern void orig_RecalculateStatsInternal(PlayerController owner);

		public void RecalculateStatsInternal(PlayerController owner) {
			orig_RecalculateStatsInternal(owner);
			var new_active_ids = new HashSet<string>();
			//TODO @performance can you avoid these expensive loops here?
			for (int k = 0; k < owner.ActiveExtraSynergies.Count; k++) {
				var entry = (AdvancedSynergyEntry)global::GameManager.Instance.SynergyManager.synergies[owner.ActiveExtraSynergies[k]];
				if (entry.SynergyIsActive(global::GameManager.Instance.PrimaryPlayer, global::GameManager.Instance.SecondaryPlayer)) {
					new_active_ids.Add(entry.UniqueID);
				}
			}
			foreach (var id in SemiLoader.ActiveSynergyIDs) {
				if (!new_active_ids.Contains(id)) {
					SemiLoader.InvokeSynergyDeactivated(id, owner);
				}
			}
			foreach (var id in new_active_ids) {
				if (!SemiLoader.ActiveSynergyIDs.Contains(id)) {
					SemiLoader.InvokeSynergyActivated(id, owner);
				}
			}
		}
	}
}
