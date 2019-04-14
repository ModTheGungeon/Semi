using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

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
			target.Wrap.attachParent = Wrap.attachParent;
			target.Wrap.autodetectFootprint = Wrap.autodetectFootprint;
			target.Wrap.automaticallyManagesDepth = Wrap.automaticallyManagesDepth;
			target.Wrap.depthUsesTrimmedBounds = Wrap.depthUsesTrimmedBounds;
			target.Wrap.SortingOrder = Wrap.SortingOrder;
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
		internal static Logger Logger = new Logger("SpriteCollection");
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

		internal SpriteDefinition? GetDefinitionByIndex(int id) {
			if (id < 0) return null;
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

		public int GetIndex(string id) {
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

		public SpriteDefinition? GetDefinition(string id) {
			var idx = GetIndex(id);
			if (idx < 0) return null;
			return new SpriteDefinition(GetDefinitionByIndex(idx));
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

		public static SpriteCollection Construct(GameObject parent, string name, string unique_id, Material material, params SpriteDefinition[] defs) {
			var coll = parent.AddComponent<tk2dSpriteCollectionData>().Wrap();

			coll.Wrap.assetName = name;
			coll.Wrap.buildKey = 0x0ade;
			coll.Wrap.dataGuid = unique_id;
			coll.Wrap.spriteCollectionGUID = unique_id;
			coll.Wrap.spriteCollectionName = name;
			coll.Wrap.spriteDefinitions = new tk2dSpriteDefinition[0];

			coll.Wrap.material = material;
			coll.Wrap.materials = new Material[] { material };
			coll.Wrap.materialInsts = new Material[] { material };

			coll.Wrap.textures = new Texture[] { material?.mainTexture };
			coll.Wrap.textureInsts = new Texture2D[] { (Texture2D)material?.mainTexture };

			if (defs != null && defs.Length > 0) {
				// this call won't break in Mod.RegisterContent
				// because this class handles stacked
				// calls to begin modifying definitions
				coll.AddDefinitions(defs);
			}

			return new SpriteCollection(coll);
		}

		public static SpriteCollection Load(Tk0dConfigParser.ParsedCollection parsed, string base_dir, string coll_namespace) {
			if (parsed.SpritesheetPath == null || parsed.SpritesheetPath.Trim() == "") throw new Exception("Missing spritesheet path!");
			var tex = Texture2DLoader.LoadTexture2D(Path.Combine(base_dir, parsed.SpritesheetPath));
			var mat = new Material(ShaderCache.Acquire(SpriteDefinition.DEFAULT_SHADER));
			mat.mainTexture = tex;

			var unit_w = parsed.UnitW < 1 ? 1 : parsed.UnitW;
			var unit_h = parsed.UnitH < 1 ? 1 : parsed.UnitH;

			var id = $"{coll_namespace}:{parsed.ID}";

			Logger.Debug($"Loading {id}");
			var coll = Construct(SemiLoader.SpriteCollectionStorageObject, parsed.Name, id, mat);
			Logger.Debug($"Collection object: {coll}");
			coll.BeginModifyingDefinitionList();
			try {
				foreach (var def in parsed.Definitions) {
					var w = def.Value.W < 1 ? parsed.SizeW : def.Value.W;
					var h = def.Value.H < 1 ? parsed.SizeH : def.Value.H;
					var def_id = $"{coll_namespace}:{def.Value.ID}";

					var tk0d_def = SpriteDefinition.Construct(
						mat,
						def_id,
						def.Value.X * unit_w,
						def.Value.Y * unit_h,
						w * unit_w,
						h * unit_h
					);

					var def_index = coll.Register(tk0d_def);

					Logger.Debug($"Registered definition {def_id} as {def_index}");

					if (parsed.AttachPoints.ContainsKey(def.Value.ID)) {
						var attach_data_list = parsed.AttachPoints[def.Value.ID];
						Logger.Debug($"Definition contains {attach_data_list.Count} attach point(s)");
						var tk2d_attach_list = new tk2dSpriteDefinition.AttachPoint[attach_data_list.Count];

						for (int i = 0; i < attach_data_list.Count; i++) {
							var attach_data = attach_data_list[i];

							Logger.Debug($"- {attach_data.AttachPoint} @ ({attach_data.X}, {attach_data.Y}, {attach_data.Z}) angle {attach_data.Angle}");

							var tk2d_attach_data = new tk2dSpriteDefinition.AttachPoint {
								name = attach_data.AttachPoint,
								position = new Vector3(attach_data.X, attach_data.Y, attach_data.Z),
								angle = attach_data.Angle
							};

							tk2d_attach_list[i] = tk2d_attach_data;
						}

						coll.Wrap.SpriteDefinedAttachPoints.Add(new AttachPointData(tk2d_attach_list));
						coll.Wrap.SpriteIDsWithAttachPoints.Add(def_index);
					} else if (parsed.AttachPoints != null && parsed.AttachPoints.Count > 0) {
						Logger.Debug($"Definition doesn't contain attach points, but other definitions in this collection do - inserting null attach point data entry");
						coll.Wrap.SpriteDefinedAttachPoints.Add(null);
					}
				}
			} finally {
				coll.CommitDefinitionList();
			}

			Gungeon.SpriteCollections.Add(id, coll);

			return coll;
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

		public static readonly Vector2[] UV_MAP_FULL_TOPLEFT = {
					new Vector2(0, 1),
					new Vector2(1, 1),
					new Vector2(0, 0),
					new Vector2(1, 0)
		};

		public static readonly Vector2 DEFAULT_TEXEL_SIZE = new Vector2(1 / 16f, 1 / 16f);
		public static readonly string DEFAULT_SHADER = "Sprites/Default";

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

		public static void GenerateGeometry(IntVector2 xy, IntVector2 wh, IntVector2 tex_size, Vector2 scale, out Vector2[] uvs, out Vector3 position0, out Vector3 position1, out Vector3 position2, out Vector3 position3) {
			var xo = (float)xy.x / tex_size.x;
			var yo = (float)xy.y / tex_size.y;
			var xl = (float)wh.x / tex_size.x;
			var yl = (float)wh.y / tex_size.y;
			var xe = xl + xo;
			var ye = yl + yo;

			uvs = new Vector2[] {
				new Vector2(xo, yo),
				new Vector2(xe, yo),
				new Vector2(xo, ye),
				new Vector2(xe, ye)
			};

			var w = xl * scale.x;
			var h = yl * scale.y;

			position0 = new Vector3(0, 0);
			position1 = new Vector3(w, 0);
			position2 = new Vector3(0, h);
			position3 = new Vector3(w, h);
		}

		public static SpriteDefinition Construct(Material mat, string override_name = null, int? region_x = null, int? region_y = null, int? region_w = null, int? region_h = null) {
			//if (!Texture2DLoader.IsEqualPowerOfTwo(texture)) throw new Exception("Texture size must be equal powers of two!");

			var texture = mat.mainTexture;

			var tex_width = texture.width;
			var tex_height = texture.height;

			if (region_x == null) region_x = 0;
			if (region_y == null) region_y = 0;
			if (region_w == null) region_w = tex_width;
			if (region_h == null) region_h = tex_height;

			Vector2[] uvs;
			Vector3 pos0;
			Vector3 pos1;
			Vector3 pos2;
			Vector3 pos3;

			GenerateGeometry(
				new IntVector2(region_x.Value, tex_height - region_y.Value - region_h.Value), // convert topleft to bottomleft
				new IntVector2(region_w.Value, region_h.Value),
				new IntVector2(tex_width, tex_height),
				new Vector2(tex_width / 16f, tex_height / 16f),
				out uvs, out pos0, out pos1, out pos2, out pos3
			);

			var def = new tk2dSpriteDefinition {
				normals = DEFAULT_NORMALS,
				tangents = DEFAULT_TANGENTS,
				texelSize = new Vector2(
					1.0f / tex_width,
					1.0f / tex_height
				),
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
				position0 = pos0,
				position1 = pos1,
				position2 = pos2,
				position3 = pos3,
				material = mat,
				materialInst = mat,
				materialId = 0,
				uvs = uvs,
				boundsDataCenter = pos3 / 2f,
				boundsDataExtents = pos3,
				untrimmedBoundsDataCenter = pos3 / 2f,
				untrimmedBoundsDataExtents = pos3,
			};

			if (override_name != null) def.name = override_name;
			else def.name = texture.name;

			return new SpriteDefinition(def);
		}
	}

	public struct SpriteAnimationFrame {
		public tk2dSpriteAnimationFrame Wrap;

		internal SpriteAnimationFrame(tk2dSpriteAnimationFrame frame) {
			Wrap = frame;
		}

		public static implicit operator tk2dSpriteAnimationFrame(SpriteAnimationFrame s) => s.Wrap;

		internal static SpriteAnimationFrame Construct(SpriteCollection coll, int definition) {
			var new_frame = new SpriteAnimationFrame(new tk2dSpriteAnimationFrame());
			new_frame.Wrap.spriteId = definition;
			new_frame.Wrap.spriteCollection = coll;
			return new_frame;
		}

		public static SpriteAnimationFrame Construct(SpriteCollection coll, string definition) {
			var id = coll.GetIndex(definition);
			if (id < 0) throw new Exception($"Sprite definition {definition} doesn't exist - did you forget a namespace?");
			return Construct(coll, id);
		}
	}

	public struct SpriteAnimationClip {
		public tk2dSpriteAnimationClip Wrap;

		internal List<SpriteAnimationFrame> WorkingFrameList;
		internal int WorkingDepth;

		internal SpriteAnimationClip(tk2dSpriteAnimationClip clip) {
			Wrap = clip;
			WorkingFrameList = null;
			WorkingDepth = 0;
		}

		public static implicit operator tk2dSpriteAnimationClip(SpriteAnimationClip s) => s.Wrap;

		internal void BeginModifyingFrames() {
			if (WorkingDepth <= 0) {
				WorkingFrameList = new List<SpriteAnimationFrame>();
				for (int i = 0; i < Frames.Count; i++) {
					WorkingFrameList.Add(Frames[i]);
				}
			}
			WorkingDepth += 1;
		}

		internal void CommitFrames() {
			WorkingDepth -= 1;
			if (WorkingDepth <= 0) {
				Frames = Frames.ConvertCompatibleList(WorkingFrameList);
				WorkingFrameList = null;
			}
		}

		internal int Add(SpriteAnimationFrame frame) {
			if (WorkingFrameList == null) throw new Exception("Cannot add new frames while not working on frame list");

			var frames_idx = WorkingFrameList.Count;
			WorkingFrameList.Add(frame);

			return frames_idx;
		}

		public ProxyList<SpriteAnimationFrame, tk2dSpriteAnimationFrame> Frames {
			get {
				return new ProxyList<SpriteAnimationFrame, tk2dSpriteAnimationFrame>(
					Wrap.frames,
					(from) => new SpriteAnimationFrame(from),
					(to) => to.Wrap
				);
			}
			set {
				Wrap.frames = value.ToTargetArray();
			}
		}

		public static SpriteAnimationClip Construct(string name, int fps, tk2dSpriteAnimationClip.WrapMode wrap_mode, params SpriteAnimationFrame[] frames) {
			var new_clip = new SpriteAnimationClip(new tk2dSpriteAnimationClip());
			new_clip.Wrap.name = name;
			new_clip.Wrap.fps = fps;
			new_clip.Wrap.wrapMode = wrap_mode;
			new_clip.Wrap.frames = ListConverter.ToArray(frames, (f) => f.Wrap);
			return new_clip;
		}
	}

	public struct SpriteAnimation {
		public tk2dSpriteAnimation Wrap;

		internal List<SpriteAnimationClip> WorkingClipList;
		internal int WorkingDepth;

		internal SpriteAnimation(tk2dSpriteAnimation animation) {
			Wrap = animation;
			WorkingClipList = null;
			WorkingDepth = 0;
		}

		public static implicit operator tk2dSpriteAnimation(SpriteAnimation s) => s.Wrap;

		internal void BeginModifyingClips() {
			if (WorkingDepth <= 0) {
				WorkingClipList = new List<SpriteAnimationClip>();
				for (int i = 0; i < Clips.Count; i++) {
					WorkingClipList.Add(Clips[i]);
				}
			}
			WorkingDepth += 1;
		}

		internal void CommitClips() {
			WorkingDepth -= 1;
			if (WorkingDepth <= 0) {
				Clips = Clips.ConvertCompatibleList(WorkingClipList);
				WorkingClipList = null;
			}
		}

		internal int Add(SpriteAnimationClip clip) {
			if (WorkingClipList == null) throw new Exception("Cannot add new frames while not working on frame list");

			var clips_idx = WorkingClipList.Count;
			WorkingClipList.Add(clip);

			return clips_idx;
		}

		public ProxyList<SpriteAnimationClip, tk2dSpriteAnimationClip> Clips {
			get {
				return new ProxyList<SpriteAnimationClip, tk2dSpriteAnimationClip>(
					Wrap.clips,
					(from) => new SpriteAnimationClip(from),
					(to) => to.Wrap
				);
			}
			set {
				Wrap.clips = value.ToTargetArray();
			}
		}

		public static SpriteAnimation Construct(GameObject parent, params SpriteAnimationClip[] clips) {
			var new_anim = new SpriteAnimation(parent.AddComponent<tk2dSpriteAnimation>());
			new_anim.Wrap.clips = ListConverter.ToArray(clips, (f) => f.Wrap);
			return new_anim;
		}

		public static SpriteAnimation Load(Tk0dConfigParser.ParsedAnimation parsed, string anim_namespace) {
			// TODO make use of the name
			var anim = Construct(SemiLoader.AnimationTemplateStorageObject);
			var id = $"{anim_namespace}:{parsed.ID}";

			var fps = parsed.DefaultFPS;

			var coll_id = parsed.Collection;
			if (!Gungeon.SpriteCollections.ContainsID(coll_id)) throw new Exception($"Semi collection '{coll_id}' doesn't exist. Did you forget to load it before loading the animation?");
			var coll = Gungeon.SpriteCollections[coll_id];

			try {
				anim.BeginModifyingClips();
				foreach (var clip in parsed.Clips) {
					var tk0d_clip = SpriteAnimationClip.Construct(
						clip.Value.Name,
						clip.Value.FPS == 0 ? fps : clip.Value.FPS,
						clip.Value.WrapMode
					);

					tk0d_clip.BeginModifyingFrames();
					try {
						var prefix = clip.Value.Prefix ?? "gungeon";
						for (int i = 0; i < clip.Value.Frames.Count; i++) {
							var frame = clip.Value.Frames[i];
							string def = frame.Definition;
							if (!def.Contains(":")) {
								def = $"{prefix}:{frame.Definition}";
							}

							var tk0d_def_id = coll.GetIndex(def);
							if (tk0d_def_id < 0) throw new Exception($"There is no sprite definition '{def}' in collection '{coll_id}'");

							var xdef = coll.GetDefinition(def);

							Console.WriteLine($"Adding frame {tk0d_def_id} {xdef?.Name}");

							tk0d_clip.Add(SpriteAnimationFrame.Construct(
								coll,
								tk0d_def_id
							));

						}
					} finally {
						tk0d_clip.CommitFrames();
					}

					anim.Add(tk0d_clip);
				}
			} finally {
				anim.CommitClips();
			}

			Gungeon.AnimationTemplates.Add(id, anim);
			return anim;
		}
	}

	public struct SpriteAnimator {
		public tk2dSpriteAnimator Wrap;

		internal SpriteAnimator(tk2dSpriteAnimator animator) {
			Wrap = animator;
		}

		public static implicit operator tk2dSpriteAnimator(SpriteAnimator s) => s.Wrap;

		public static SpriteAnimator Construct(GameObject parent, SpriteAnimation animation, string default_clip = null) {
			var new_anim = new SpriteAnimator(parent.AddComponent<tk2dSpriteAnimator>());
			new_anim.Wrap.Library = animation;
			new_anim.Wrap.DefaultClipId = 0;
			if (default_clip != null) {
				int clip_id = -1;
				for (int i = 0; i < animation.Wrap.clips.Length; i++) {
					if (animation.Wrap.clips[i].name == default_clip) {
						clip_id = i;
						break;
					}
				}
				if (clip_id == -1) throw new Exception($"Default clip '{default_clip}' doesn't exist.");
			}
			return new_anim;
		}
	}
}
