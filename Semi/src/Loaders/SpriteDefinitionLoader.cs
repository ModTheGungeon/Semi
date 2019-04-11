using System;
using System.IO;
using UnityEngine;

namespace Semi {
	public class SpriteDefinitionLoader {
		internal static tk2dSprite BaseSprite;



		public static tk2dSpriteDefinition ConstructSpriteDefinition(Texture2D texture, string name) {
			if (!Texture2DLoader.IsEqualPowerOfTwo(texture)) throw new Exception("Sprite size must be equal powers of two!");

			Material material = new Material(Shader.Find("tk2d/BlendVertexColor"));
			material.mainTexture = texture;
			//material.mainTexture = texture;

			var width = texture.width;
			var height = texture.height;

			var x = 0f;
			var y = 0f;

			var w = width / 16f;
			var h = height / 16f;

			var def = new tk2dSpriteDefinition {
				normals = new Vector3[] {
				new Vector3(0.0f, 0.0f, -1.0f),
				new Vector3(0.0f, 0.0f, -1.0f),
				new Vector3(0.0f, 0.0f, -1.0f),
				new Vector3(0.0f, 0.0f, -1.0f),
			},
				tangents = new Vector4[] {
				new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
				new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
			},
				texelSize = new Vector2(1 / 16f, 1 / 16f),
				extractRegion = false,
				regionX = 0,
				regionY = 0,
				regionW = 0,
				regionH = 0,
				flipped = tk2dSpriteDefinition.FlipMode.None,
				complexGeometry = false,
				physicsEngine = tk2dSpriteDefinition.PhysicsEngine.Physics3D,
				colliderType = tk2dSpriteDefinition.ColliderType.None,
				collisionLayer = CollisionLayer.HighObstacle,
				position0 = new Vector3(x, y, 0f),
				position1 = new Vector3(x + w, y, 0f),
				position2 = new Vector3(x, y + h, 0f),
				position3 = new Vector3(x + w, y + h, 0f),
				material = material,
				materialInst = material,
				materialId = 0,
				//uvs = ETGMod.Assets.GenerateUVs(texture, 0, 0, width, height), //uv machine broke
				uvs = new Vector2[] {
					new Vector2(0, 0),
					new Vector2(1, 0),
					new Vector2(0, 1),
					new Vector2(1, 1)
				},
				boundsDataCenter = new Vector3(w / 2f, h / 2f, 0f),
				boundsDataExtents = new Vector3(w, h, 0f),
				untrimmedBoundsDataCenter = new Vector3(w / 2f, h / 2f, 0f),
				untrimmedBoundsDataExtents = new Vector3(w, h, 0f),
			};

			def.name = name;
			return def;
		}



		public static GameObject AssignNewSpriteFromDefinition(GameObject obj, tk2dSpriteCollectionData collection, int spritedef_id) {
			if (BaseSprite == null) throw new Exception("BaseSprite not initialized yet!");
			tk2dSprite sprite = obj.AddComponent<tk2dSprite>();

			sprite.ApplyEmissivePropertyBlock = BaseSprite.ApplyEmissivePropertyBlock;
			sprite.GenerateUV2 = BaseSprite.GenerateUV2;
			sprite.LockUV2OnFrameOne = BaseSprite.LockUV2OnFrameOne;
			sprite.StaticPositions = BaseSprite.StaticPositions;
			sprite.AdditionalFlatForwardPercentage = BaseSprite.AdditionalFlatForwardPercentage;
			sprite.AdditionalPerpForwardPercentage = BaseSprite.AdditionalPerpForwardPercentage;
			sprite.allowDefaultLayer = BaseSprite.allowDefaultLayer;
			sprite.attachParent = BaseSprite.attachParent;
			sprite.autodetectFootprint = BaseSprite.autodetectFootprint;
			sprite.automaticallyManagesDepth = BaseSprite.automaticallyManagesDepth;
			sprite.boxCollider = BaseSprite.boxCollider;
			sprite.boxCollider2D = BaseSprite.boxCollider2D;
			sprite.CachedPerpState = BaseSprite.CachedPerpState;
			// sprite.Collection
			sprite.color = BaseSprite.color;
			// sprite.CurrentSprite
			sprite.customFootprint = BaseSprite.customFootprint;
			sprite.customFootprintOrigin = BaseSprite.customFootprintOrigin;
			sprite.depthUsesTrimmedBounds = BaseSprite.depthUsesTrimmedBounds;
			// sprite.FlipX (no flips here)
			// sprite.FlipY (no flips here)
			sprite.hasOffScreenCachedUpdate = BaseSprite.hasOffScreenCachedUpdate;
			sprite.HeightOffGround = BaseSprite.HeightOffGround;
			sprite.ignoresTiltworldDepth = BaseSprite.ignoresTiltworldDepth;
			sprite.independentOrientation = BaseSprite.independentOrientation;
			sprite.IsBraveOutlineSprite = BaseSprite.IsBraveOutlineSprite;
			sprite.IsOutlineSprite = BaseSprite.IsOutlineSprite;
			sprite.IsPerpendicular = BaseSprite.IsPerpendicular;
			sprite.IsZDepthDirty = BaseSprite.IsZDepthDirty;
			sprite.meshCollider = BaseSprite.meshCollider;
			sprite.meshColliderMesh = BaseSprite.meshColliderMesh;
			sprite.meshColliderPositions = BaseSprite.meshColliderPositions;
			// sprite.offScreenCachedCollection
			// sprite.offScreenCachedID
			// sprite.OverrideMaterialMode
			sprite.scale = BaseSprite.scale;
			sprite.ShouldDoTilt = BaseSprite.ShouldDoTilt;
			// sprite.SortingOrder
			// sprite.spriteId
			// sprite.usesOverrideMaterial

			// hopefully this is enough :)


			sprite.SetSprite(collection, spritedef_id);
			sprite.spriteId = spritedef_id;
			sprite.SortingOrder = 0;
			sprite.ForceBuild();

			obj.GetComponent<BraveBehaviour>().sprite = sprite;

			return obj;
		}
	}
}
