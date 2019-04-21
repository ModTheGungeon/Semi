//using System;
//using System.Collections.Generic;
//using MonoMod;

//namespace Semi.Patches {
//	/// <summary>
//	/// Patches UINotificationController to add support for named IDs in synergies.
//	/// </summary>
//	[MonoModPatch("global::UINotificationController")]
//	public class UINotificationController : global::UINotificationController {
//		[MonoModIgnore]
//		private List<NotificationParams> m_queuedNotificationParams;

//		public void AttemptSynergyAttachment(AdvancedSynergyEntry e) {
//			for (int i = this.m_queuedNotificationParams.Count - 1; i >= 0; i--) {
//				NotificationParams notificationParams = this.m_queuedNotificationParams[i];
//				if (!string.IsNullOrEmpty(notificationParams.EncounterGuid)) {
//					EncounterDatabaseEntry entry = EncounterDatabase.GetEntry(notificationParams.EncounterGuid);
//					string id = (entry == null) ? null : ((PickupObject)(object)entry).UniqueItemID;
//					if (id != null && e.ContainsPickup(id)) {
//						notificationParams.HasAttachedSynergy = true;
//						notificationParams.AttachedSynergy = e;
//						this.m_queuedNotificationParams[i] = notificationParams;
//						break;
//					}
//				}
//			}
//		}
//	}
//}
