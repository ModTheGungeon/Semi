using System;
using System.Collections.Generic;
using UnityEngine;

namespace Semi {
	public struct Sprite {
		public tk2dSprite Wrap;

		internal Sprite(tk2dSprite sprite) {
			Wrap = sprite;
		}

		public static implicit operator tk2dSprite(Sprite s) => s.Wrap;

		public GameObject GameObject {
			get { try { return Wrap.gameObject; } catch { return null; } }
		}

		public SpriteCollection Collection {
			get { return new SpriteCollection(Wrap.Collection); }
			set { Wrap.SetSprite(value, 0); }
		}

		internal int CurrentSpriteIndex {
			get { return Wrap.spriteId; }
			set { Wrap.SetSprite(value); }
		}

		public string CurrentSpriteID {
			get { return Collection.SpriteDefinitions[Wrap.spriteId].Name; }
			set {
				var idx = Collection.GetIndex(value);
				if (idx < 0) throw new InvalidOperationException($"Collection has no '{value}' definition");
				Wrap.SetSprite(idx);
			}
		}

		public BoxCollider BoxCollider {
			get { return Wrap.boxCollider; }
			set { Wrap.boxCollider = value; }
		}

		public BoxCollider2D BoxCollider2D {
			get { return Wrap.boxCollider2D; }
			set { Wrap.boxCollider2D = value; }
		}

		public MeshColliderSettings MeshColliderSettings {
			get { return new MeshColliderSettings(Wrap.meshCollider, Wrap.meshColliderMesh, Wrap.meshColliderPositions); }
			set {
				Wrap.meshCollider = value.Collider;
				Wrap.meshColliderMesh = value.Mesh;
				Wrap.meshColliderPositions = value.Positions;
			}
		}

		public int SortingOrder {
			get { return Wrap.SortingOrder; }
			set { Wrap.SortingOrder = value; }
		}

		public bool AutodetectFootprint {
			get { return Wrap.autodetectFootprint; }
			set { Wrap.autodetectFootprint = value; }
		}

		public bool AutomaticallyManagesDepth {
			get { return Wrap.automaticallyManagesDepth; }
			set { Wrap.automaticallyManagesDepth = value; }
		}

		public IntVector2 CustomFootprint {
			get { return Wrap.customFootprint; }
			set { Wrap.customFootprint = value; }
		}

		public bool UseTrimmedBoundsForDepth {
			get { return Wrap.depthUsesTrimmedBounds; }
			set { Wrap.depthUsesTrimmedBounds = value; }
		}

		public bool FlipHorizontally {
			get { return Wrap.FlipX; }
			set { Wrap.FlipX = value; }
		}

		public bool FlipVertically {
			get { return Wrap.FlipY; }
			set { Wrap.FlipY = value; }
		}

		public static Sprite Construct(GameObject parent, SpriteCollection collection, int spritedef_id) {
			tk2dSprite sprite = parent.AddComponent<tk2dSprite>();

			sprite.attachParent = null;
			sprite.autodetectFootprint = true;
			sprite.automaticallyManagesDepth = true;
			sprite.depthUsesTrimmedBounds = false;
			sprite.SortingOrder = 2;


			sprite.SetSprite(collection, spritedef_id);
			sprite.ForceBuild();

			parent.GetComponent<BraveBehaviour>().sprite = sprite;

			return new Sprite(sprite);
		}

		public void CopyTo(Sprite target) {
			// generated through regex for all public properties
			target.Wrap.Collection = Wrap.Collection;
			target.Wrap.spriteId = Wrap.spriteId;
			target.Wrap.LockUV2OnFrameOne = Wrap.LockUV2OnFrameOne;
			target.Wrap.GenerateUV2 = Wrap.GenerateUV2;
			target.Wrap.ApplyEmissivePropertyBlock = Wrap.ApplyEmissivePropertyBlock;
			target.Wrap.StaticPositions = Wrap.StaticPositions;
			target.Wrap.boxCollider2D = Wrap.boxCollider2D;
			target.Wrap.boxCollider = Wrap.boxCollider;
			target.Wrap.meshCollider = Wrap.meshCollider;
			target.Wrap.meshColliderPositions = Wrap.meshColliderPositions;
			target.Wrap.meshColliderMesh = Wrap.meshColliderMesh;
			target.Wrap.AdditionalFlatForwardPercentage = Wrap.AdditionalFlatForwardPercentage;
			target.Wrap.AdditionalPerpForwardPercentage = Wrap.AdditionalPerpForwardPercentage;
			target.Wrap.CachedPerpState = Wrap.CachedPerpState;
			target.Wrap.IsOutlineSprite = Wrap.IsOutlineSprite;
			target.Wrap.IsBraveOutlineSprite = Wrap.IsBraveOutlineSprite;
			target.Wrap.offScreenCachedCollection = Wrap.offScreenCachedCollection;
			target.Wrap.automaticallyManagesDepth = Wrap.automaticallyManagesDepth;
			target.Wrap.ignoresTiltworldDepth = Wrap.ignoresTiltworldDepth;
			target.Wrap.depthUsesTrimmedBounds = Wrap.depthUsesTrimmedBounds;
			target.Wrap.allowDefaultLayer = Wrap.allowDefaultLayer;
			target.Wrap.attachParent = Wrap.attachParent;
			target.Wrap.OverrideMaterialMode = Wrap.OverrideMaterialMode;
			target.Wrap.independentOrientation = Wrap.independentOrientation;
			target.Wrap.offScreenCachedID = Wrap.offScreenCachedID;
			target.Wrap.autodetectFootprint = Wrap.autodetectFootprint;
			target.Wrap.customFootprintOrigin = Wrap.customFootprintOrigin;
			target.Wrap.customFootprint = Wrap.customFootprint;
			target.Wrap.hasOffScreenCachedUpdate = Wrap.hasOffScreenCachedUpdate;
			target.Wrap.IsZDepthDirty = Wrap.IsZDepthDirty;
		}
	}

	public struct SpriteCollection {
		internal static Dictionary<string, IDPool<int>> SpritePoolMap = new Dictionary<string, IDPool<int>>();

		public tk2dSpriteCollectionData Wrap;

		internal List<SpriteDefinition> WorkingDefinitionList;
		internal int WorkingDepth;

		internal IDPool<int> SpritePool {
			get {
				return ((Semi.Patches.tk2dSpriteCollectionData)(object)Wrap).SpritePool;
			}
			set {
				((Semi.Patches.tk2dSpriteCollectionData)(object)Wrap).SpritePool = value;
			}
		}

		internal SpriteCollection(tk2dSpriteCollectionData coll) {
			Wrap = coll;
			WorkingDefinitionList = null;
			WorkingDepth = 0;
		}

		public static implicit operator tk2dSpriteCollectionData(SpriteCollection s) => s.Wrap;

		public void RefreshLookupDictionary() {
			((Semi.Patches.tk2dSpriteCollectionData)(object)Wrap).spriteNameLookupDict = null;
			Wrap.InitDictionary();
		}

		public int GetSpriteIdByName(string name) => Wrap.GetSpriteIdByName(name);

		public void AddDefinition(SpriteDefinition def) => AddDefinitions(def);

		public void AddDefinitions(params SpriteDefinition[] defs) {
			BeginModifyingDefinitionList();

			for (int i = 0; i < defs.Length; i++) {
				Register(defs[i]);
			}

			CommitDefinitionList();
		}

		internal void BeginModifyingDefinitionList() {
			if (WorkingDepth <= 0) {
				SpritePool = new IDPool<int>();
				WorkingDefinitionList = new List<SpriteDefinition>();
				for (int i = 0; i < SpriteDefinitions.Count; i++) {
					WorkingDefinitionList.Add(SpriteDefinitions[i]);
				}
			}
			WorkingDepth += 1;
		}

		internal void CommitDefinitionList() {
			WorkingDepth -= 1;
			if (WorkingDepth <= 0) {
				SpriteDefinitions = SpriteDefinitions.ConvertCompatibleList(WorkingDefinitionList);
				WorkingDefinitionList = null;

				// no need to refresh dictionary manually - SpriteCollection does it for us
				// when we set SpriteDefinitions
			}
		}

		internal int Register(SpriteDefinition sprite_def) {
			if (WorkingDefinitionList == null) throw new Exception("Cannot register new definitions while not working on definition list");

			var id = SpritePool.ValidateNewEntry(sprite_def.Name);

			var def_idx = WorkingDefinitionList.Count;
			WorkingDefinitionList.Add(sprite_def);
			SpritePool[id] = def_idx;

			return def_idx;
		}

		internal SpriteDefinition GetDefinitionByID(int id) {
			if (WorkingDefinitionList == null) {
				return SpriteDefinitions[id];
			} else {
				return WorkingDefinitionList[id];
			}
		}

		internal int GetDefinitionsLength() {
			if (WorkingDefinitionList == null) {
				return SpriteDefinitions.Count;
			} else {
				return WorkingDefinitionList.Count;
			}
		}

		internal int GetIndex(string id) {
			// we can't do ValidateEntry here because that'd check
			// for gungeon: IDs too, and those aren't actually there
			// and are just faked a'la old ETGMod Resources.Load
			IDPool<int>.VerifyID(id);
			id = IDPool<int>.Resolve(id);
			var id_split = IDPool<int>.Split(id);
			if (id_split.Namespace == "gungeon") {
				// this is a special case
				// we want to avoid having to change all the old names for sprite defs
				// so we just make it look like they're in the gungeon: namespace
				// but they aren't *actually*
				return GetSpriteIdByName(id_split.Name);
			} else {
				var pool = SpritePool;
				if (pool == null || !pool.ContainsID(id)) return -1;
				var idx = pool.Get(id);
				if (idx >= GetDefinitionsLength() || idx < 0) {
					throw new Exception($"Invalid spriteDefinitions index mapped in SpritePool: {idx} for '{id}'");
				}
				return idx;
			}
		}

		public SpriteDefinition GetDefinition(string id) {
			var idx = GetIndex(id);
			return new SpriteDefinition(GetDefinitionByID(idx));
		}

		public ProxyList<SpriteDefinition, tk2dSpriteDefinition> SpriteDefinitions {
			get {
				return new ProxyList<SpriteDefinition, tk2dSpriteDefinition>(
					Wrap.spriteDefinitions,
					(from) => new SpriteDefinition(from),
					(to) => to.Wrap
				);
			}
			set {
				Wrap.spriteDefinitions = value.ToTargetArray();
				RefreshLookupDictionary();
			}
		}

		public GameObject GameObject {
			get { try { return Wrap.gameObject; } catch { return null; } }
		}

		public string Name {
			get { return Wrap.spriteCollectionName; }
			set { Wrap.spriteCollectionName = value; }
		}

		public string GUID {
			get { return Wrap.spriteCollectionGUID; } 
			set { Wrap.spriteCollectionGUID = value; }
		}

		public Material[] Materials {
			get { return Wrap.materials; }
			set { Wrap.materials = value; }
		}

		public Material[] MaterialInstances {
			get { return Wrap.materialInsts; }
			set { Wrap.materialInsts = value; }
		}

		public static SpriteCollection Construct(GameObject parent, string name, string unique_id, params SpriteDefinition[] defs) {
			var coll = parent.AddComponent<tk2dSpriteCollectionData>().Wrap();

			coll.Wrap.assetName = name;
			coll.Wrap.buildKey = 0x0ade;
			coll.Wrap.dataGuid = unique_id;
			coll.Wrap.spriteCollectionGUID = unique_id;
			coll.Wrap.spriteCollectionName = name;
			coll.Wrap.spriteDefinitions = new tk2dSpriteDefinition[0];
			if (defs != null && defs.Length > 0) {
				// this call won't break in Mod.RegisterContent
				// because this class handles stacked
				// calls to begin modifying definitions
				coll.AddDefinitions(defs);
			}

			return new SpriteCollection(coll);
		}
	}

	public struct SpriteDefinition {
		public tk2dSpriteDefinition Wrap;
		public static readonly Vector3[] DEFAULT_NORMALS = {
					new Vector3(0.0f, 0.0f, -1.0f),
					new Vector3(0.0f, 0.0f, -1.0f),
					new Vector3(0.0f, 0.0f, -1.0f),
					new Vector3(0.0f, 0.0f, -1.0f),
		};

		public static readonly Vector4[] DEFAULT_TANGENTS = {
					new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
					new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
					new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
					new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
		};

		public static readonly Vector2[] UV_MAP_FULL = {
					new Vector2(0, 0),
					new Vector2(1, 0),
					new Vector2(0, 1),
					new Vector2(1, 1)
		};

		public static readonly Vector2 DEFAULT_TEXEL_SIZE = new Vector2(1 / 16f, 1 / 16f);
		public static readonly string DEFAULT_SHADER = "tk2d/BlendVertexColor";

		internal SpriteDefinition(tk2dSpriteDefinition def) {
			Wrap = def;
		}

		public static implicit operator tk2dSpriteDefinition(SpriteDefinition s) => s.Wrap;

		public Material Material {
			get { return Wrap.materialInst; }
			set { Wrap.materialInst = value; }
		}

		public Texture2D Texture {
			get { return Wrap.materialInst.mainTexture as Texture2D; }
			set { Wrap.materialInst.mainTexture = value; }
		}

		public string Name {
			get { return Wrap.name; }
			set { Wrap.name = value; }
		}

		// TODO @disinfo Verify whether this is actually the top left corner
		public Vector2 CornerTopLeft {
			get { return Wrap.position0; }
			set { Wrap.position0 = value; }
		}

		public Vector2 CornerTopRight {
			get { return Wrap.position1; }
			set { Wrap.position1 = value; }
		}

		public Vector2 CornerBottomRight {
			get { return Wrap.position2; }
			set { Wrap.position2 = value; }
		}

		public Vector2 CornerBottomLeft {
			get { return Wrap.position3; }
			set { Wrap.position3 = value; }
		}

		public Vector2 TrimmedBoundsCenter {
			get { return Wrap.boundsDataCenter; }
			set { Wrap.boundsDataCenter = value; }
		}

		public Vector2 TrimmedBoundsExtents {
			get { return Wrap.boundsDataExtents; }
			set { Wrap.boundsDataExtents = value; }
		}

		public Vector2 UntrimmedBoundsCenter {
			get { return Wrap.untrimmedBoundsDataCenter; }
			set { Wrap.untrimmedBoundsDataCenter = value; }
		}

		public Vector2 UntrimmedBoundsExtents {
			get { return Wrap.untrimmedBoundsDataExtents; }
			set { Wrap.untrimmedBoundsDataExtents = value; }
		}

		public static SpriteDefinition Construct(Texture2D texture, string override_name = null) {
			if (!Texture2DLoader.IsEqualPowerOfTwo(texture)) throw new Exception("Texture size must be equal powers of two!");

			Material material = new Material(Shader.Find(DEFAULT_SHADER));
			material.mainTexture = texture;

			var width = texture.width;
			var height = texture.height;

			var x = 0f;
			var y = 0f;

			var w = width / 16f;
			var h = height / 16f;

			var def = new tk2dSpriteDefinition {
				normals = DEFAULT_NORMALS,
				tangents = DEFAULT_TANGENTS,
				texelSize = DEFAULT_TEXEL_SIZE,
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
				uvs = UV_MAP_FULL,
				boundsDataCenter = new Vector3(w / 2f, h / 2f, 0f),
				boundsDataExtents = new Vector3(w, h, 0f),
				untrimmedBoundsDataCenter = new Vector3(w / 2f, h / 2f, 0f),
				untrimmedBoundsDataExtents = new Vector3(w, h, 0f),
			};

			if (override_name != null) def.name = override_name;
			else def.name = texture.name;

			return new SpriteDefinition(def);
		}
	}
}
