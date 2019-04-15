using System;
using UnityEngine;
using MonoMod;

namespace Semi {
	/// <summary>
	/// Debug patch, will be removed.
	/// </summary>
	[MonoModPatch("global::Gun")]
	public class GunDebugPatch : global::Gun {
		[MonoModIgnore]
		private GameActor m_owner;
		[MonoModIgnore]
		private int m_defaultSpriteID;
		[MonoModIgnore]
		private Vector3 m_originalMuzzleOffsetPosition;
		[MonoModIgnore]
		private Transform m_transform;
		[MonoModIgnore]
		private bool m_isThrown;
		[MonoModIgnore]
		private Transform m_attachTransform;
		[MonoModIgnore]
		private float prevGunAngleUnmodified;
		[MonoModIgnore]
		private float gunAngle;
		[MonoModIgnore]
		private Vector2 m_localAimPoint;
		[MonoModIgnore]
		private bool m_isPreppedForThrow;
		[MonoModIgnore]
		private float LockedHorizontalCachedAngle = -1f;
		[MonoModIgnore]
		private Vector3 m_unroundedBarrelPosition;
		[MonoModIgnore]
		private VFXPool m_currentlyPlayingChargeVFX;
		[MonoModIgnore]
		private tk2dBaseSprite m_sprite;
		[MonoModIgnore]
		private extern bool ShouldDoLaserSight();
		[MonoModIgnore]
		private tk2dTiledSprite m_extantLaserSight;
		[MonoModIgnore]
		private GameObject m_extantLockOnSprite;
		[MonoModIgnore]
		private float m_prepThrowTime = -0.3f;
		[MonoModIgnore]
		private MeshRenderer m_meshRenderer;

		public float HandleAimRotation(Vector3 ownerAimPoint, bool limitAimSpeed = false, float aimTimeScale = 1f) {
			Console.WriteLine("0");
			if (this.m_isThrown) {
				Console.WriteLine("1");
				return this.prevGunAngleUnmodified;
				Console.WriteLine("2");
			}
			Console.WriteLine("3");
			Vector2 b;
			Console.WriteLine("4");
			if (this.usesDirectionalIdleAnimations) {
				Console.WriteLine("5");
				b = (this.m_transform.position + Quaternion.Euler(0f, 0f, -this.m_attachTransform.localRotation.z) * this.barrelOffset.localPosition).XY();
				Console.WriteLine("6");
				b = this.m_owner.specRigidbody.HitboxPixelCollider.UnitCenter;
				Console.WriteLine("7");
			} else if (this.LockedHorizontalOnCharge) {
				Console.WriteLine("8");
				b = this.m_owner.specRigidbody.HitboxPixelCollider.UnitCenter;
				Console.WriteLine("9");
			} else {
				Console.WriteLine($"10 transform {m_transform} barrel offset {barrelOffset} attach transform {m_attachTransform}");
				b = (this.m_transform.position + Quaternion.Euler(0f, 0f, this.gunAngle) * Quaternion.Euler(0f, 0f, -this.m_attachTransform.localRotation.z) * this.barrelOffset.localPosition).XY();
				Console.WriteLine("11");
			}
			Console.WriteLine("12");
			float num = Vector2.Distance(ownerAimPoint.XY(), b);
			Console.WriteLine("13");
			float t = Mathf.Clamp01((num - 2f) / 3f);
			Console.WriteLine("14");
			b = Vector2.Lerp(this.m_owner.specRigidbody.HitboxPixelCollider.UnitCenter, b, t);
			Console.WriteLine("15");
			this.m_localAimPoint = ownerAimPoint.XY();
			Console.WriteLine("16");
			Vector2 vector = this.m_localAimPoint - b;
			Console.WriteLine("17");
			float num2 = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			Console.WriteLine("18");
			if (this.OverrideAngleSnap.HasValue) {
				Console.WriteLine("19");
				num2 = BraveMathCollege.QuantizeFloat(num2, this.OverrideAngleSnap.Value);
				Console.WriteLine("20");
			}
			Console.WriteLine("21");
			if ((limitAimSpeed && aimTimeScale != 1f) || this.m_activeBeams.Count > 0) {
				Console.WriteLine("22");
				float num3 = 3.40282347E+38f * BraveTime.DeltaTime * aimTimeScale;
				Console.WriteLine("23");
				if (this.m_activeBeams.Count > 0 && this.Volley && this.Volley.UsesBeamRotationLimiter) {
					Console.WriteLine("24");
					num3 = this.Volley.BeamRotationDegreesPerSecond * BraveTime.DeltaTime * aimTimeScale;
					Console.WriteLine("25");
				}
				Console.WriteLine("26");
				float value = BraveMathCollege.ClampAngle180(num2 - this.prevGunAngleUnmodified);
				Console.WriteLine("27");
				num2 = BraveMathCollege.ClampAngle180(this.prevGunAngleUnmodified + Mathf.Clamp(value, -num3, num3));
				Console.WriteLine("28");
				this.m_localAimPoint = (base.transform.position + (Quaternion.Euler(0f, 0f, num2) * Vector3.right).normalized * vector.magnitude).XY();
				Console.WriteLine("29");
			}
			Console.WriteLine("30");
			this.prevGunAngleUnmodified = num2;
			Console.WriteLine("31");
			this.gunAngle = num2;
			Console.WriteLine("32");
			this.m_attachTransform.localRotation = Quaternion.Euler(this.m_attachTransform.localRotation.x, this.m_attachTransform.localRotation.y, num2);
			Console.WriteLine("33");
			this.m_unroundedBarrelPosition = this.barrelOffset.position;
			Console.WriteLine("34");
			float num4 = (float)((!this.forceFlat) ? (Mathf.RoundToInt(num2 / 10f) * 10) : (Mathf.RoundToInt(num2 / 3f) * 3));
			Console.WriteLine("35");
			if (this.IgnoresAngleQuantization) {
				Console.WriteLine("36");
				num4 = num2;
				Console.WriteLine("37");
			}
			Console.WriteLine("38");
			bool flag = base.sprite.FlipY;
			Console.WriteLine("39");
			float num5 = 75f;
			Console.WriteLine("40");
			float num6 = 105f;
			Console.WriteLine("41");
			if (num4 <= 155f && num4 >= 25f) {
				Console.WriteLine("42");
				num5 = 75f;
				Console.WriteLine("43");
				num6 = 105f;
				Console.WriteLine("44");
			}
			Console.WriteLine("45");
			if (!base.sprite.FlipY && Mathf.Abs(num4) > num6) {
				Console.WriteLine("46");
				flag = true;
				Console.WriteLine("47");
			} else if (base.sprite.FlipY && Mathf.Abs(num4) < num5) {
				Console.WriteLine("48");
				flag = false;
				Console.WriteLine("49");
			}
			Console.WriteLine("50");
			if (this.LockedHorizontalOnCharge) {
				Console.WriteLine("51");
				float chargeFraction = this.GetChargeFraction();
				Console.WriteLine("52");
				this.LockedHorizontalCachedAngle = num2;
				Console.WriteLine("53");
				num4 = Mathf.LerpAngle(num4, (float)((!flag) ? 0 : 180), chargeFraction);
				Console.WriteLine("54");
			}
			Console.WriteLine("55");
			if (this.LockedHorizontalOnReload && this.IsReloading) {
				Console.WriteLine("56");
				num4 = (float)((!flag) ? 0 : 180);
				Console.WriteLine("57");
			}
			Console.WriteLine("58");
			if (this.m_isPreppedForThrow) {
				Console.WriteLine("59");
				if (this.m_prepThrowTime < 1.2f) {
					Console.WriteLine("60");
					num4 = (float)Mathf.FloorToInt(Mathf.LerpAngle(num4, -90f, Mathf.Clamp01(this.m_prepThrowTime / 1.2f)));
					Console.WriteLine("61");
				} else {
					Console.WriteLine("62");
					num4 = (float)Mathf.FloorToInt(Mathf.PingPong(this.m_prepThrowTime * 15f, 10f) + -95f);
					Console.WriteLine("63");
				}
				Console.WriteLine("64");
			}
			Console.WriteLine("65");
			if (this.preventRotation) {
				Console.WriteLine("66");
				num4 = 0f;
				Console.WriteLine("67");
			}
			Console.WriteLine("68");
			if (this.usesDirectionalIdleAnimations) {
				Console.WriteLine("69");
				int num7 = BraveMathCollege.AngleToOctant(num4 + 90f);
				Console.WriteLine("70");
				float num8 = (float)(num7 * -45);
				Console.WriteLine("71");
				Debug.Log(num8);
				Console.WriteLine("72");
				float z = (num4 + 360f) % 360f - num8;
				Console.WriteLine("73");
				this.m_attachTransform.localRotation = Quaternion.Euler(this.m_attachTransform.localRotation.x, this.m_attachTransform.localRotation.y, z);
				Console.WriteLine("74");
			} else {
				Console.WriteLine("75");
				this.m_attachTransform.localRotation = Quaternion.Euler(this.m_attachTransform.localRotation.x, this.m_attachTransform.localRotation.y, num4);
				Console.WriteLine("76");
			}
			Console.WriteLine("77");
			if (this.m_currentlyPlayingChargeVFX != null) {
				Console.WriteLine("78");
				this.UpdateChargeEffectZDepth(vector);
				Console.WriteLine("79");
			}
			Console.WriteLine("80");
			if (this.m_sprite != null) {
				Console.WriteLine("81");
				this.m_sprite.ForceRotationRebuild();
				Console.WriteLine("82");
			}
			Console.WriteLine("83");
			if (this.ShouldDoLaserSight()) {
				Console.WriteLine("84");
				if (this.m_extantLaserSight == null) {
					Console.WriteLine("85");
					string path = "Global VFX/VFX_LaserSight";
					Console.WriteLine("86");
					if (!(this.m_owner is PlayerController)) {
						Console.WriteLine("87");
						path = ((!this.LaserSightIsGreen) ? "Global VFX/VFX_LaserSight_Enemy" : "Global VFX/VFX_LaserSight_Enemy_Green");
						Console.WriteLine("88");
					}
					Console.WriteLine("89");
					this.m_extantLaserSight = SpawnManager.SpawnVFX((GameObject)BraveResources.Load(path, ".prefab"), false).GetComponent<tk2dTiledSprite>();
					Console.WriteLine("90");
					this.m_extantLaserSight.IsPerpendicular = false;
					Console.WriteLine("91");
					this.m_extantLaserSight.HeightOffGround = this.CustomLaserSightHeight;
					Console.WriteLine("92");
					this.m_extantLaserSight.renderer.enabled = this.m_meshRenderer.enabled;
					Console.WriteLine("93");
					this.m_extantLaserSight.transform.parent = this.barrelOffset;
					Console.WriteLine("94");
					if (this.m_owner is AIActor) {
						Console.WriteLine("95");
						this.m_extantLaserSight.renderer.enabled = false;
						Console.WriteLine("96");
					}
					Console.WriteLine("97");
				}
				Console.WriteLine("98");
				this.m_extantLaserSight.transform.localPosition = Vector3.zero;
				Console.WriteLine("99");
				this.m_extantLaserSight.transform.rotation = Quaternion.Euler(0f, 0f, num2);
				Console.WriteLine("100");
				if (this.m_extantLaserSight.renderer.enabled) {
					Console.WriteLine("101");
					Func<SpeculativeRigidbody, bool> rigidbodyExcluder = (SpeculativeRigidbody otherRigidbody) => otherRigidbody.minorBreakable && !otherRigidbody.minorBreakable.stopsBullets;
					Console.WriteLine("102");
					bool flag2 = false;
					Console.WriteLine("103");
					float num9 = 3.40282347E+38f;
					Console.WriteLine("104");
					if (this.DoubleWideLaserSight) {
						Console.WriteLine("105");
						CollisionLayer layer = (!(this.m_owner is PlayerController)) ? CollisionLayer.PlayerHitBox : CollisionLayer.EnemyHitBox;
						Console.WriteLine("106");
						int rayMask = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, layer, CollisionLayer.BulletBreakable);
						Console.WriteLine("107");
						Vector2 b2 = BraveMathCollege.DegreesToVector(vector.ToAngle() + 90f, 0.0625f);
						Console.WriteLine("108");
						RaycastResult raycastResult;
						Console.WriteLine("109");
						if (PhysicsEngine.Instance.Raycast(this.barrelOffset.position.XY() + b2, vector, this.CustomLaserSightDistance, out raycastResult, true, true, rayMask, null, false, rigidbodyExcluder, null)) {
							Console.WriteLine("110");
							flag2 = true;
							Console.WriteLine("111");
							num9 = Mathf.Min(num9, raycastResult.Distance);
							Console.WriteLine("112");
						}
						Console.WriteLine("113");
						RaycastResult.Pool.Free(ref raycastResult);
						Console.WriteLine("114");
						if (PhysicsEngine.Instance.Raycast(this.barrelOffset.position.XY() - b2, vector, this.CustomLaserSightDistance, out raycastResult, true, true, rayMask, null, false, rigidbodyExcluder, null)) {
							Console.WriteLine("115");
							flag2 = true;
							Console.WriteLine("116");
							num9 = Mathf.Min(num9, raycastResult.Distance);
							Console.WriteLine("117");
						}
						Console.WriteLine("118");
						RaycastResult.Pool.Free(ref raycastResult);
						Console.WriteLine("119");
					} else {
						Console.WriteLine("120");
						CollisionLayer layer2 = (!(this.m_owner is PlayerController)) ? CollisionLayer.PlayerHitBox : CollisionLayer.EnemyHitBox;
						Console.WriteLine("121");
						int rayMask2 = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, layer2, CollisionLayer.BulletBreakable);
						Console.WriteLine("122");
						RaycastResult raycastResult2;
						Console.WriteLine("123");
						if (PhysicsEngine.Instance.Raycast(this.barrelOffset.position.XY(), vector, this.CustomLaserSightDistance, out raycastResult2, true, true, rayMask2, null, false, rigidbodyExcluder, null)) {
							Console.WriteLine("124");
							flag2 = true;
							Console.WriteLine("125");
							num9 = raycastResult2.Distance;
							Console.WriteLine("126");
							if (raycastResult2.SpeculativeRigidbody && raycastResult2.SpeculativeRigidbody.aiActor) {
								Console.WriteLine("127");
								this.HandleEnemyHitByLaserSight(raycastResult2.SpeculativeRigidbody.aiActor);
								Console.WriteLine("128");
							}
							Console.WriteLine("129");
						}
						Console.WriteLine("130");
						RaycastResult.Pool.Free(ref raycastResult2);
						Console.WriteLine("131");
					}
					Console.WriteLine("132");
					this.m_extantLaserSight.dimensions = new Vector2((!flag2) ? 480f : (num9 / 0.0625f), 1f);
					Console.WriteLine("133");
					this.m_extantLaserSight.ForceRotationRebuild();
					Console.WriteLine("134");
					this.m_extantLaserSight.UpdateZDepth();
					Console.WriteLine("135");
				}
				Console.WriteLine("136");
			} else if (this.m_extantLaserSight != null) {
				Console.WriteLine("137");
				SpawnManager.Despawn(this.m_extantLaserSight.gameObject);
				Console.WriteLine("138");
				this.m_extantLaserSight = null;
				Console.WriteLine("139");
			}
			Console.WriteLine("140");
			if (!this.OwnerHasSynergy(CustomSynergyType.PLASMA_LASER) && this.m_extantLockOnSprite) {
				Console.WriteLine("141");
				SpawnManager.Despawn(this.m_extantLockOnSprite);
				Console.WriteLine("142");
			}
			Console.WriteLine("143");
			if (this.usesDirectionalAnimator) {
				Console.WriteLine("144");
				base.aiAnimator.LockFacingDirection = true;
				Console.WriteLine("145");
				base.aiAnimator.FacingDirection = num2;
				Console.WriteLine("146");
			}
			Console.WriteLine("147");
			return num2;
			Console.WriteLine("148");
		}
	}
}
