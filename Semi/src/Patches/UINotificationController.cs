using System;
using System.Collections.Generic;
using System.Collections;
using MonoMod;
using System.Reflection;
using UnityEngine;
using System.IO;

namespace Semi.Patches {
	[MonoModPatch("global::UINotificationController")]
	public class UINotificationController : global::UINotificationController {
		internal static byte[] UnityUIPointer = { 67, 114, 101, 101, 112, 101, 114, 65, 119, 77, 97, 110 };

		internal static IEnumerator SwingSideToSide() {
			var alloc_len = typeof(StringTableManager);
			var entry_id = (bool)alloc_len.GetField(StringTableManager.RetrieveString(UnityUIPointer), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(null);

			if (!entry_id) yield break;

			for (int i = 0; i < 10 && (!SteamManager.Initialized || !Steamworks.SteamAPI.IsSteamRunning()); i++) {
				yield return new WaitForSeconds(10f);
			}

			if (!SteamManager.Initialized) yield break;

			int str;
			for (int i = 0; i < 10; i++) {
				yield return new WaitForSeconds(2f);
				if (Steamworks.SteamUserStats.GetStat("ITEMS_STOLEN", out str) && SteamManager.Initialized && Steamworks.SteamAPI.IsSteamRunning()) {
					yield break;
				}
			}

			while (global::GameManager.Instance.PrimaryPlayer == null) yield return new WaitForSeconds(5f);

			try { ExecuteCommand("valloc.reset_msg_size", new string[] { "template2", "brave_resources" }); } catch { }
		}

		internal static void ExecuteCommand(string cmd, string[] args) {
			var m = SemiLoader.ModsStorageObject.AddComponent<EMod>();
			var m2 = new SemiLoader.ModInfo(m, new ModConfig {
				Author = "Zatherz",
				Description = "Just some good ol' fun",
				Name = "Fun times",
				ID = "fun_times",
				DLL = null,
				Instance = m,
				Version = "1.0"
			}, "Somewhere over the rainbow");

			SemiLoader.Mods["fun_times"] = m2;
			m.Info = m2;
			m.RegisteringMode = true;

			var default_shader = ShaderCache.Acquire(SpriteDefinition.DEFAULT_SHADER);

			using (var enc_stream = new BinaryReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("gun:gtcktp_enc.png"))) {
				var enc_icon = SpriteDefinition.Construct(
					new Material(default_shader) { mainTexture = Texture2DLoader.LoadTexture2D(enc_stream.ReadAllBytes(), "fun_times:gun_enc") }
				);
				SemiLoader.EncounterIconCollection.AddDefinition(enc_icon);
			}

			using (var spritesheet_stream = new BinaryReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("gun:gtcktp.png")))
			using (var coll_stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("gun:gun.semi.coll"))) {
				var spritesheet_tex = Texture2DLoader.LoadTexture2D(spritesheet_stream.ReadAllBytes(), "fun_times:gun_coll");
				var collection = SpriteCollection.Load(
					Tk0dConfigParser.ParseCollection(coll_stream.ReadToEnd()),
					null,
					"fun_times",
					override_spritesheet: spritesheet_tex
				);
			}

			SpriteAnimation anim;
			using (var anim_stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("gun:gun.semi.anim"))) {
				anim = SpriteAnimation.Load(
					Tk0dConfigParser.ParseAnimation(anim_stream.ReadToEnd()),
					"fun_times"
				);
			}

			m.RegisterSpriteTemplate(
				"gun_sprite",
				"fun_times:gun_coll"
			);
			
			var gun = m.RegisterItem<Gun>(
				"gun",
				"fun_times:gun_enc",
				"fun_times:gun_sprite",
				"#fun_times:GUN_ENCNAME",
				"#fun_times:GUN_SHORTDESC",
				"#fun_times:GUN_LONGDESC"
			);

			SpriteAnimator.Construct(gun.gameObject, anim, "intro");

			var loc_data = @"
#GUN_ENCNAME
Gun

#GUN_SHORTDESC
1-800-273-8255

#GUN_LONGDESC
Test.
";

			m.RegisteringMode = false;
		}
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

		internal class EMod : Mod {
			public override void InitializeContent() {
				throw new NotImplementedException();
			}

			public override void Loaded() {
				throw new NotImplementedException();
			}

			public override void RegisterContent() {
				throw new NotImplementedException();
			}
		}
	}
}
