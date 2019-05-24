using System;
using System.Collections.Generic;
using UnityEngine;

namespace Semi {
	public static class EntityTreeBuilder {
		internal static GameObject CleanBaseObject;
		internal static GameObject CleanBaseCorpseObject;
		internal static GameObject CleanBaseBulletObject;

		public static GameObject GetNewInactiveObject(string name) {
			var obj = Semi.FakePrefab.Clone(CleanBaseObject);
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.name = name;
			return obj;
		}

		public static GameObject GetNewCorpse(string name) {
			var obj = Semi.FakePrefab.Clone(CleanBaseCorpseObject);
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.name = name;
			return obj;
		}

		public static GameObject GetNewBullet(string name) {
			var obj = Semi.FakePrefab.Clone(CleanBaseBulletObject);
			UnityEngine.Object.DontDestroyOnLoad(obj);
			obj.name = name;
			return obj;
		}

		public static AIActor AddAIActor(GameObject go, int enemy_id, string enemy_guid, GameObject corpse, bool is_normal = true, bool is_harmless = false, bool is_signature = false) {
			var actor = go.AddComponent<AIActor>();

			actor.EnemyId = enemy_id;
			actor.EnemyGuid = enemy_guid;

			actor.IsNormalEnemy = is_normal;
			actor.IsSignatureEnemy = is_signature;
			actor.IsHarmlessEnemy = is_harmless;

			actor.CollisionVFX = new VFXPool();
			actor.NonActorCollisionVFX = new VFXPool();

			actor.AvoidRadius = 4;
			actor.OverrideDodgeRollDeath = "";
			actor.HasOverrideDodgeRollDeath = false;

			actor.CanDropCurrency = true;
			actor.CanDropItems = true; // bullet kin have this? might be related to CustomLootTable which is null for them

			actor.CorpseObject = corpse;
			actor.CorpseShadow = true;
			actor.OnCorpseVFX = new VFXPool();
			actor.reinforceType = AIActor.ReinforceType.FullVfx;
			actor.EnemySwitchState = "Metal_Bullet_Man"; // this is for audio
			actor.OverrideSpawnReticleAudio = "";
			actor.OverrideSpawnAppearAudio = "";

			actor.StartMovingEvent = "";
			actor.StopMovingEvent = "";

			// TODO @audio
			actor.animationAudioEvents = new List<ActorAudioEvent>() {
				new ActorAudioEvent { eventTag = "footstep", eventName = "Play_CHR_metalBullet_step_01" }
			};

			actor.IdentifierForEffects = AIActor.EnemyTypeIdentifier.UNIDENTIFIED;
			actor.LocalTimeScale = 1;

			// Settings for when jammed
			// Default values for the fields are exact same as in the bullet kin
			// So we just use the defaults
			actor.BlackPhantomProperties = new BlackPhantomProperties();

			actor.ActorName = go.name;
			actor.OverrideDisplayName = "";
			actor.HasShadow = true;
			actor.FreezeDispelFactor = 20f;
			actor.placeableWidth = 1;
			actor.placeableHeight = 1;
			actor.difficulty = DungeonPlaceableBehaviour.PlaceableDifficulty.BASE;
			actor.isPassable = true;

			return actor;
		}

		public static DebrisObject AddCorpseDebrisObject(GameObject go) {
			var debris = go.AddComponent<DebrisObject>();

			debris.IsCorpse = false; // BulletManCorpse has this set to false
			debris.audioEventName = "";
			debris.directionalAnimationData = new DebrisDirectionalAnimationInfo {
				fallDown = "",
				fallLeft = "",
				fallRight = "",
				fallUp = ""
			};
			debris.breaksOnFall = true;
			debris.breakOnFallChance = 1;
			debris.groundedCollisionLayer = CollisionLayer.LowObstacle;
			debris.animatePitFall = true;
			debris.pitFallSplash = true;
			debris.inertialMass = 1;
			debris.motionMultiplier = 1;
			debris.bounceCount = 1;
			debris.decayOnBounce = 0.5f;
			debris.GoopRadius = 1;
			debris.Priority = EphemeralObject.EphemeralPriority.Middling;

			return debris;
		}

		public static EncounterTrackable AddEncounterTrackable(GameObject go, JournalEntry journal_entry, string enc_guid) {
			return PickupObjectTreeBuilder.AddEncounterTrackable(go, journal_entry, enc_guid);
		}

		public static Sprite AddSprite(GameObject go, Sprite base_sprite) {
			return PickupObjectTreeBuilder.AddSprite(go, base_sprite);
		}

		public static SpriteAnimator AddSpriteAnimator(GameObject go, SpriteAnimation anim, string initial_clip = null) {
			return SpriteAnimator.Construct(
				go, anim, initial_clip
			);
		}

		public static ObjectVisibilityManager AddObjectVisibilityManager(GameObject go) {
			// bullet kin has an OVM with all nulls, so we should be fine with it too
			return go.AddComponent<ObjectVisibilityManager>();
		}

		public static HitEffectHandler AddHitEffectHandler(GameObject go) {
			var handler = go.AddComponent<HitEffectHandler>();

			// we're good here with almost all defaults as well

			handler.overrideHitEffect = new VFXComplex();
			handler.overrideHitEffectPool = new VFXPool();

			return handler;
		}

		// TODO:
		//   - AIBulletBank::Bullets
		//       (list of Entries with gameobjects with Projectile and BulletScriptBehavior)
		//   - Rest of enemy components
		public static Projectile AddProjectile(GameObject go) {
			var proj = go.AddComponent<Projectile>();

		//	proj.

			return proj;
		}

		//public static BulletScriptBehavior AddBulletScriptBehavior(GameObject go) {
		//}

		public static AIBulletBank AddAIBulletBank(GameObject go, List<AIBulletBank.Entry> bullets) {
			var bank = go.AddComponent<AIBulletBank>();

			bank.Bullets = bullets;


			return bank;
		}
	}
}
