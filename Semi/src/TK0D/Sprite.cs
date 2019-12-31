using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Logger = ModTheGungeon.Logger;

namespace Semi {
	/// <summary>
	/// Wrapper struct for <c>tk2dSprite</c> that provides a cleaner interface and works transparently with anything that expects <c>tk2dSprite</c>s.
	/// </summary>
	public struct Sprite {
		/// <summary>
		/// Real tk2d object.
		/// </summary>
		public tk2dSprite Wrap;

		internal Sprite(tk2dSprite sprite) {
			Wrap = sprite;
		}

		/// <summary>
		/// Implicit cast operator that allows you to use <c>Sprite</c> anywhere where <c>t2kdSprite</c> is expected.
		/// </summary>
		public static implicit operator tk2dSprite(Sprite s) => s.Wrap;

		/// <value>The game object that this sprite is attached to.</value>
		public GameObject GameObject {
			get { try { return Wrap.gameObject; } catch { return null; } }
		}

		/// <value>The currently assigned sprite collection.</value>
		public SpriteCollection Collection {
			get { return new SpriteCollection(Wrap.Collection); }
			set { Wrap.SetSprite(value, 0); }
		}

		internal int CurrentSpriteIndex {
			get { return Wrap.spriteId; }
			set { Wrap.SetSprite(value); }
		}

		/// <value>The string ID of the currently displayed sprite definition.</value>
		public ID CurrentSpriteID {
			get { return (ID)Collection.SpriteDefinitions[Wrap.spriteId].Name; }
			set {
				var idx = Collection.GetIndex(value);
				if (idx < 0) throw new InvalidOperationException($"Collection has no '{value}' definition");
				Wrap.SetSprite(idx);
			}
		}

		/// <value>The box collider.</value>
		public BoxCollider BoxCollider {
			get { return Wrap.boxCollider; }
			set { Wrap.boxCollider = value; }
		}

		/// <value>The 2D box collider.</value>
		public BoxCollider2D BoxCollider2D {
			get { return Wrap.boxCollider2D; }
			set { Wrap.boxCollider2D = value; }
		}

		/// <value>The mesh collider settings.</value>
		public MeshColliderSettings MeshColliderSettings {
			get { return new MeshColliderSettings(Wrap.meshCollider, Wrap.meshColliderMesh, Wrap.meshColliderPositions); }
			set {
				Wrap.meshCollider = value.Collider;
				Wrap.meshColliderMesh = value.Mesh;
				Wrap.meshColliderPositions = value.Positions;
			}
		}

		/// <value>The sorting order/render layer.</value>
		public int SortingOrder {
			get { return Wrap.SortingOrder; }
			set { Wrap.SortingOrder = value; }
		}

		/// <value>Decides whether to autodetect footprint.</value>
		public bool AutodetectFootprint {
			get { return Wrap.autodetectFootprint; }
			set { Wrap.autodetectFootprint = value; }
		}

		/// <value>Decides whether Z depth/sorting order should be managed automatically.</value>
		public bool AutomaticallyManagesDepth {
			get { return Wrap.automaticallyManagesDepth; }
			set { Wrap.automaticallyManagesDepth = value; }
		}

		/// <value>Defines a custom footprint.</value>
		public IntVector2 CustomFootprint {
			get { return Wrap.customFootprint; }
			set { Wrap.customFootprint = value; }
		}

		/// <value>Decides whether trimmed (whitespace-excluded) bounds should be used for automatically deciding depth.</value>
		public bool UseTrimmedBoundsForDepth {
			get { return Wrap.depthUsesTrimmedBounds; }
			set { Wrap.depthUsesTrimmedBounds = value; }
		}

		/// <value>Flip horizontally.</value>
		public bool FlipHorizontally {
			get { return Wrap.FlipX; }
			set { Wrap.FlipX = value; }
		}

		/// <value>Flip vertically.</value>
		public bool FlipVertically {
			get { return Wrap.FlipY; }
			set { Wrap.FlipY = value; }
		}

		/// <summary>
		/// Construct a new sprite on the specified GameObject using the collection and the sprite definition index.
		/// </summary>
		/// <returns>The newly constructed sprite.</returns>
		/// <param name="parent"><c>GameObject</c> to add this sprite to.</param>
		/// <param name="collection">The sprite collection to set.</param>
		/// <param name="spritedef_id">The sprite definition index to set as the current sprite.</param>
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

		/// <summary>
		/// Copies all fields from this sprite into another existing sprite instance.
		/// </summary>
		/// <param name="target">Target sprite.</param>
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

	/// <summary>
	/// Wrapper struct for <c>tk2dSpriteCollectionData</c> that provides a cleaner interface and works transparently with anything that expects <c>tk2dSpriteCollectionData</c>s.
	/// </summary>
	public struct SpriteCollection {
		internal static Logger Logger = new Logger("SpriteCollection");
		internal static Dictionary<string, IDPool<int>> SpritePoolMap = new Dictionary<string, IDPool<int>>();

		/// <summary>
		/// Real tk2d object.
		/// </summary>
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

		/// <summary>
		/// Implicit cast operator that allows you to use <c>SpriteCollection</c> anywhere where <c>t2kdSpriteCollectionData</c> is expected.
		/// </summary>
		public static implicit operator tk2dSpriteCollectionData(SpriteCollection s) => s.Wrap;

		/// <summary>
		/// Refreshes the lookup dictionary for definitions. Done automatically by <c>AddDefinition</c>/<c>AddDefinitions</c>.
		/// </summary>
		public void RefreshLookupDictionary() {
			((Semi.Patches.tk2dSpriteCollectionData)(object)Wrap).spriteNameLookupDict = null;
			Wrap.InitDictionary();
		}

		internal int GetSpriteIdByName(StringView name) => Wrap.GetSpriteIdByName(name.ToString());

		/// <summary>
		/// Adds a single sprite definition to this collection.
		/// </summary>
		/// <param name="def">The definition.</param>
		public void AddDefinition(SpriteDefinition def) => AddDefinitions(def);

		/// <summary>
		/// Adds multiple sprite definitions at once to this collection.
		/// </summary>
		/// <param name="defs">The array of definitions.</param>
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

			var id = (ID)sprite_def.Name;
			if (id.DefaultNamespace) {
				sprite_def.Name = id;

				// for gungeon compat - collection entries with gungeon: namespace must be saved with their name
				// missing the namespace
			}

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

		/// <summary>
		/// Gets the index of a sprite definition based on its ID.
		/// </summary>
		/// <returns>The index of the sprite definition or a value less than 0 if not found.</returns>
		/// <param name="id">The ID of the sprite definition.</param>
		public int GetIndex(ID id) {
			// we can't do ValidateEntry here because that'd check
			// for gungeon: IDs too, and those aren't actually there
			// and are just faked a'la old ETGMod Resources.Load
			if (id.DefaultNamespace) {
				// this is a special case
				// we want to avoid having to change all the old names for sprite defs
				// so we just make it look like they're in the gungeon: namespace
				// but they aren't *actually*
				return GetSpriteIdByName(id.Name);
			} else {
				var pool = SpritePool;
				if (pool == null || !pool.Contains(id)) return -1;
				var idx = pool.Get(id);
				if (idx >= GetDefinitionsLength() || idx < 0) {
					throw new Exception($"Invalid spriteDefinitions index mapped in SpritePool: {idx} for '{id}'");
				}
				return idx;
			}
		}

		/// <summary>
		/// Gets a sprite definition based on its ID.
		/// </summary>
		/// <returns>The sprite definition with this ID or <c>null</c> if it doesn't exist.</returns>
		/// <param name="id">The ID of the definition.</param>
		public SpriteDefinition? GetDefinition(ID id) {
			var idx = GetIndex(id);
			if (idx < 0) return null;
			return new SpriteDefinition(GetDefinitionByIndex(idx));
		}

		/// <value>The sprite definitions contained in this collection.</value>
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

		/// <value>The game object this collection is a component of.</value>
		public GameObject GameObject {
			get { try { return Wrap.gameObject; } catch { return null; } }
		}

		/// <value>Name of the sprite collection.</value>
		public string Name {
			get { return Wrap.spriteCollectionName; }
			set { Wrap.spriteCollectionName = value; }
		}

		/// <value>Globally Unique ID of the sprite collection.</value>
		public string GUID {
			get { return Wrap.spriteCollectionGUID; }
			set { Wrap.spriteCollectionGUID = value; }
		}

		/// <value>Array of materials.</value>
		public Material[] Materials {
			get { return Wrap.materials; }
			set { Wrap.materials = value; }
		}

		/// <value>Array of instances of materials.</value>
		public Material[] MaterialInstances {
			get { return Wrap.materialInsts; }
			set { Wrap.materialInsts = value; }
		}

		/// <summary>
		/// Patches the specified collection with this collection's data.
		/// </summary>
		/// <param name="coll">Target collection.</param>
		public void Patch(tk2dSpriteCollectionData coll) {
			Logger.Debug($"Patching collection '{coll.spriteCollectionName}' with collection '{Name}'");
			Logger.Debug($"Target has {coll.materials?.Length.ToString() ?? "no"} materials, {coll.materialInsts?.Length.ToString() ?? "no"} material insts, {coll.textures?.Length.ToString() ?? "no"} textures, {coll.textureInsts?.Length.ToString() ?? "no"} texture insts");
			Logger.Debug($"Source has {Wrap.materials?.Length.ToString() ?? "no"} materials, {Wrap.materialInsts?.Length.ToString() ?? "no"} material insts, {Wrap.textures?.Length.ToString() ?? "no"} textures, {Wrap.textureInsts?.Length.ToString() ?? "no"} texture insts");

			var tk0d_coll = new SpriteCollection(coll);

			// The code below merges the 'materials' and 'materialInsts' fields of the two collections,
			// later making sure that the new definitions coming in get the right material ID.
			var last_material_idx = 0;
			var material_idx_diff = 0;
			var materials = coll.materialInsts;
			if (materials == null) materials = coll.materials;
			if (materials != null) {
				last_material_idx = materials.Length - 1;
				material_idx_diff = materials.Length;
			}

			if (coll.materials != null && Wrap.materials != null) {
				var new_materials_ary = new Material[coll.materials.Length + Wrap.materials.Length];

				for (var i = 0; i < coll.materials.Length; i++) {
					new_materials_ary[i] = coll.materials[i];
				}

				for (var i = 0; i < Wrap.materials.Length; i++) {
					var dest_idx = i + coll.materials.Length;
					new_materials_ary[dest_idx] = Wrap.materials[i];
				}

				coll.materials = new_materials_ary;
			}
			if (coll.materialInsts != null && Wrap.materialInsts != null) {
				var new_materialinsts_ary = new Material[coll.materialInsts.Length + Wrap.materialInsts.Length];

				for (var i = 0; i < coll.materialInsts.Length; i++) {
					new_materialinsts_ary[i] = coll.materialInsts[i];
				}

				for (var i = 0; i < Wrap.materialInsts.Length; i++) {
					var dest_idx = i + coll.materialInsts.Length;
					new_materialinsts_ary[dest_idx] = Wrap.materialInsts[i];
				}

				coll.materialInsts = new_materialinsts_ary;
			}
			if (coll.textures != null && Wrap.textures != null) {
				var new_textures_ary = new Texture[coll.textures.Length + Wrap.textures.Length];

				for (var i = 0; i < coll.textures.Length; i++) {
					new_textures_ary[i] = coll.textures[i];
				}

				for (var i = 0; i < Wrap.textures.Length; i++) {
					var dest_idx = i + coll.textures.Length;
					new_textures_ary[dest_idx] = Wrap.textures[i];
				}

				coll.textures = new_textures_ary;
			}
			if (coll.textureInsts != null && Wrap.textureInsts != null) {
				var new_textureinsts_ary = new Texture2D[coll.textureInsts.Length + Wrap.textureInsts.Length];

				for (var i = 0; i < coll.textureInsts.Length; i++) {
					new_textureinsts_ary[i] = coll.textureInsts[i];
				}

				for (var i = 0; i < Wrap.textureInsts.Length; i++) {
					var dest_idx = i + coll.textureInsts.Length;
					new_textureinsts_ary[dest_idx] = Wrap.textureInsts[i];
				}

				coll.textureInsts = new_textureinsts_ary;
			}

			var ids_replaced_inplace = new HashSet<string>();
			for (var i = 0; i < coll.spriteDefinitions.Length; i++) {
				var def = new SpriteDefinition(coll.spriteDefinitions[i]);
				var id = def.ID;

				if (SpritePool.Contains(id)) {
					var this_def = GetDefinition(id).Value; // can't be null
					var new_material_id = this_def.Wrap.materialId + material_idx_diff;
					Logger.Debug($"Replacing in-place: def ID '{id}', source ID '{this_def.ID}', prev material ID '{this_def.Wrap.materialId}', new material ID '{new_material_id}'");
					this_def.Patch(def);
					def.Wrap.materialId = new_material_id;
					ids_replaced_inplace.Add(id);
				}
			}

			tk0d_coll.BeginModifyingDefinitionList();
			for (var i = 0; i < SpriteDefinitions.Count; i++) {
				var def = SpriteDefinitions[i];
				if (ids_replaced_inplace.Contains(def.ID)) continue;

				def = def.Copy();

				var new_material_id = def.Wrap.materialId + material_idx_diff;
				Logger.Debug($"Adding: def ID '{def.ID}', prev material ID '{def.Wrap.materialId}', new material ID '{new_material_id}'");
				def.Wrap.materialId = new_material_id;

				tk0d_coll.AddDefinition(def);
			}
			tk0d_coll.CommitDefinitionList();

			coll.allowMultipleAtlases = true;
			coll.assetName = Wrap.assetName;
			coll.buildKey = Wrap.buildKey;
			coll.dataGuid = Wrap.dataGuid;
			coll.halfTargetHeight = Wrap.halfTargetHeight;
			coll.hasPlatformData = Wrap.hasPlatformData;
			coll.invOrthoSize = Wrap.invOrthoSize;
			coll.loadable = Wrap.loadable;
			coll.managedSpriteCollection = Wrap.managedSpriteCollection;
			//coll.material = Wrap.material;
			coll.materialIdsValid = Wrap.materialIdsValid;
			coll.materialPngTextureId = Wrap.materialPngTextureId;
			coll.needMaterialInstance = Wrap.needMaterialInstance;
			coll.pngTextures = Wrap.pngTextures;
			coll.premultipliedAlpha = Wrap.premultipliedAlpha;
			coll.shouldGenerateTilemapReflectionData = Wrap.shouldGenerateTilemapReflectionData;
			coll.spriteCollectionGUID = Wrap.spriteCollectionGUID;
			coll.spriteCollectionName = Wrap.spriteCollectionName;
			coll.spriteCollectionPlatforms = Wrap.spriteCollectionPlatforms;
			coll.spriteCollectionPlatformGUIDs = Wrap.spriteCollectionPlatformGUIDs;
			coll.SpriteDefinedAnimationSequences = Wrap.SpriteDefinedAnimationSequences;
			coll.SpriteDefinedAttachPoints = Wrap.SpriteDefinedAttachPoints;
			coll.SpriteDefinedBagelColliders = Wrap.SpriteDefinedBagelColliders;
			coll.SpriteDefinedIndexNeighborDependencies = Wrap.SpriteDefinedIndexNeighborDependencies;
			coll.SpriteIDsWithAnimationSequences = Wrap.SpriteIDsWithAnimationSequences;
			coll.SpriteIDsWithAttachPoints = Wrap.SpriteIDsWithAttachPoints;
			coll.SpriteIDsWithBagelColliders = Wrap.SpriteIDsWithBagelColliders;
			coll.SpriteIDsWithNeighborDependencies = Wrap.SpriteIDsWithNeighborDependencies;
			coll.textureFilterMode = Wrap.textureFilterMode;
			coll.textureMipMaps = Wrap.textureMipMaps;
			coll.version = Wrap.version;
		}

		/// <summary>
		/// Exports this collection to the SemiCollection format.
		/// </summary>
		public void Export(string id, bool as_patch, string output_dir_path) {
			var coll_path = Path.Combine(output_dir_path, $"{id}.semi.coll");
			var spritesheet_path = Path.Combine(output_dir_path, $"{id}.png");

			using (var writer = new StreamWriter(File.OpenWrite(coll_path))) {

				writer.WriteLine($"@id {id}");
				writer.WriteLine($"@name {Name}");
				writer.WriteLine($"@unit 1x1");
				writer.WriteLine($"@size 1x1");
				writer.WriteLine($"@spritesheet {id}.png");
				if (as_patch) writer.WriteLine($"@patch");

				if (Wrap.textures.Length > 1) throw new NotSupportedException("Can't export collections with more than one texture yet");

				File.WriteAllBytes(spritesheet_path, ((Texture2D)Wrap.textures[0]).GetRW().EncodeToPNG());

				var attach_points = new Dictionary<string, List<Tk0dConfigParser.ParsedCollection.AttachPointData>>();
				var attach_point_names_encountered = new HashSet<string>();
				var reverse_alias_map = new Dictionary<string, string>();

				if (Wrap.SpriteDefinedAttachPoints != null) {
					for (var i = 0; i < Wrap.SpriteDefinedAttachPoints.Count; i++) {
						var ats = Wrap.SpriteDefinedAttachPoints[i];
						for (var j = 0; j < ats.attachPoints.Length; j++) {
							var at = ats.attachPoints[j];
							if (!attach_point_names_encountered.Contains(at.name)) {
								var alias = at.name.ToLowerInvariant().Replace(" ", "_");
								reverse_alias_map[at.name] = alias;
								attach_point_names_encountered.Add(at.name);

								writer.WriteLine($"@attachpoint {alias} {at.name}");
							}
						}
					}
				}

				writer.WriteLine();

				for (var i = 0; i < SpriteDefinitions.Count; i++) {
					var def = SpriteDefinitions[i];

					writer.Write($"{def.ID} {def.X},{def.Y} {def.Width}x{def.Height}");
					if (Wrap.SpriteDefinedAttachPoints != null && Wrap.SpriteDefinedAttachPoints.Count > i && Wrap.SpriteDefinedAttachPoints[i] != null) {
						var ats = Wrap.SpriteDefinedAttachPoints[i];
						for (var j = 0; i < ats.attachPoints.Length; i++) {
							var at = ats.attachPoints[j];
							writer.Write($" at {reverse_alias_map[at.name]} {at.position.x},{at.position.y} angle {at.angle}");
						}
					}
					if (def.FlipH) writer.Write(" fliph");
					if (def.FlipV) writer.Write(" flipv");
					writer.WriteLine();
				}
			}
		}

		/// <summary>
		/// Constructs a new sprite collection.
		/// </summary>
		/// <returns>The new sprite collection.</returns>
		/// <param name="parent">GameObject to add this collection to.</param>
		/// <param name="name">Name.</param>
		/// <param name="unique_id">Unique identifier.</param>
		/// <param name="material">Material.</param>
		/// <param name="defs">Optional array of preset definitions.</param>
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

		/// <summary>
		/// Load a Semi Collection format file from its parsed representation.
		/// The collection will be attached to the global modded sprite collection game object.
		/// </summary>
		/// <returns>The new sprite collection.</returns>
		/// <param name="parsed">Parsed representation of the Semi Collection.</param>
		/// <param name="base_dir">Base directory to load referenced assets from.</param>
		/// <param name="coll_namespace">Namespace to use for the collection and the definitions.</param>
		public static SpriteCollection Load(Tk0dConfigParser.ParsedCollection parsed, string base_dir, string coll_namespace, Texture2D override_spritesheet = null) {
			if (override_spritesheet == null && (parsed.SpritesheetPath == null || parsed.SpritesheetPath.Trim() == "")) throw new Exception("Missing spritesheet path!");
			var tex = override_spritesheet;
			if (tex == null) tex = Texture2DLoader.LoadTexture2D(Path.Combine(base_dir, parsed.SpritesheetPath));
			var mat = new Material(ShaderCache.Acquire(SpriteDefinition.DEFAULT_SHADER));
			mat.mainTexture = tex;

			var unit_w = parsed.UnitW < 1 ? 1 : parsed.UnitW;
			var unit_h = parsed.UnitH < 1 ? 1 : parsed.UnitH;

            var id = parsed.ID.WithContextNamespace(coll_namespace);

			Logger.Debug($"Loading {id}");
			var coll = Construct(SemiLoader.SpriteCollectionStorageObject, parsed.Name, id, mat);
			Logger.Debug($"Collection object: {coll}");
			coll.BeginModifyingDefinitionList();
			try {
				foreach (var def in parsed.Definitions) {
					var w = def.Value.W < 1 ? parsed.SizeW : def.Value.W;
					var h = def.Value.H < 1 ? parsed.SizeH : def.Value.H;

                    var def_id = def.Value.ID.WithContextNamespace(coll_namespace);

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

					if (parsed.AttachPoints != null && parsed.AttachPoints.ContainsKey(def.Value.ID)) {
						var attach_data_list = parsed.AttachPoints[def.Value.ID];
						Logger.Debug($"Definition contains {attach_data_list.Count} attach point(s)");
						var tk2d_attach_list = new tk2dSpriteDefinition.AttachPoint[attach_data_list.Count];

						for (int i = 0; i < attach_data_list.Count; i++) {
							var attach_data = attach_data_list[i];

							Logger.Debug($"- {attach_data.AttachPoint} @ ({attach_data.X}, {attach_data.Y}, {attach_data.Z}) angle {attach_data.Angle}");

							var tk2d_attach_data = new tk2dSpriteDefinition.AttachPoint {
								name = attach_data.AttachPoint,
								position = new Vector3(
									(float)attach_data.X / tk0d_def.Texture.width,
									(float)attach_data.Y / tk0d_def.Texture.height,
									attach_data.Z
								),
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

			Registry.SpriteCollections.Add(id, coll);

			return coll;
		}
	}

	/// <summary>
	/// Wrapper struct for <c>tk2dSpriteDefinition</c> that provides a cleaner interface and works transparently with anything that expects <c>tk2dSpriteDefinition</c>s.
	/// </summary>
	public struct SpriteDefinition {
		internal static Logger Logger = new Logger("SpriteDefinition");

		/// <summary>
		/// Real tk2d object.
		/// </summary>
		public tk2dSpriteDefinition Wrap;

		/// <summary>
		/// Default normals.
		/// </summary>
		public static readonly Vector3[] DEFAULT_NORMALS = {
					new Vector3(0.0f, 0.0f, -1.0f),
					new Vector3(0.0f, 0.0f, -1.0f),
					new Vector3(0.0f, 0.0f, -1.0f),
					new Vector3(0.0f, 0.0f, -1.0f),
		};

		/// <summary>
		/// Default tangents.
		/// </summary>
		public static readonly Vector4[] DEFAULT_TANGENTS = {
					new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
					new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
					new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
					new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
		};

		/// <summary>
		/// Default sprite shader.
		/// </summary>
		public static readonly string DEFAULT_SHADER = "Sprites/Default";

		internal SpriteDefinition(tk2dSpriteDefinition def) {
			Wrap = def;
		}

		/// <summary>
		/// Implicit cast operator that allows you to use <c>SpriteDefinition</c> anywhere where <c>t2kdSpriteDefinition</c> is expected.
		/// </summary>
		public static implicit operator tk2dSpriteDefinition(SpriteDefinition s) => s.Wrap;

		/// <value>The material.</value>
		public Material Material {
			get { return Wrap.materialInst; }
			set { Wrap.materialInst = value; }
		}

		/// <value>The texture (modifies the <c>Material</c>).</value>
		public Texture2D Texture {
			get { return Wrap.materialInst.mainTexture as Texture2D; }
			set { Wrap.materialInst.mainTexture = value; }
		}

		/// <value>The sprite definition's name.</value>
		public string Name {
			get { return Wrap.name; }
			set { Wrap.name = value; }
		}

		/// <value>The sprite definition's ID.</value>
		public ID ID {
			get {
				if (Wrap.name.Contains(":")) return (ID)Wrap.name;
				else return (ID)$"gungeon:{Wrap.name}";
			}
			set {
                Wrap.name = value.DefaultNamespace ? value.Name.ToString() : value.ToString();
			}
		}

		/// <value>First position (sprite geometry).</value>
		public Vector2 Position0 {
			get { return Wrap.position0; }
			set { Wrap.position0 = value; }
		}

		/// <value>Second position (sprite geometry).</value>
		public Vector2 Position1 {
			get { return Wrap.position1; }
			set { Wrap.position1 = value; }
		}

		/// <value>Third position (sprite geometry).</value>
		public Vector2 Position2 {
			get { return Wrap.position2; }
			set { Wrap.position2 = value; }
		}

		/// <value>Fourth position (sprite geometry).</value>
		public Vector2 Position3 {
			get { return Wrap.position3; }
			set { Wrap.position3 = value; }
		}

		/// <value>Center of trimmed bounds.</value>
		public Vector2 TrimmedBoundsCenter {
			get { return Wrap.boundsDataCenter; }
			set { Wrap.boundsDataCenter = value; }
		}

		/// <value>Extents of trimmed bounds.</value>
		public Vector2 TrimmedBoundsExtents {
			get { return Wrap.boundsDataExtents; }
			set { Wrap.boundsDataExtents = value; }
		}

		/// <value>Center of untrimmed bounds.</value>
		public Vector2 UntrimmedBoundsCenter {
			get { return Wrap.untrimmedBoundsDataCenter; }
			set { Wrap.untrimmedBoundsDataCenter = value; }
		}

		/// <value>Extents of untrimmed bounds.</value>
		public Vector2 UntrimmedBoundsExtents {
			get { return Wrap.untrimmedBoundsDataExtents; }
			set { Wrap.untrimmedBoundsDataExtents = value; }
		}

		public int X {
			get { return (int)Math.Round(Wrap.uvs[0].x * Texture.width); }
		}

		public int Y {
			get { return (int)Math.Round(Texture.height - (Wrap.uvs[0].y * Texture.height) - Height); }
		}

		public int Width {
			get { return (int)Math.Round((Wrap.uvs[3].x * Texture.width) - X); }
		}

		public int Height {
			get { return (int)Math.Round((Wrap.uvs[3].y * Texture.height) - (Wrap.uvs[0].y * Texture.height)); }
		}

		public bool FlipH {
			get { return IsPlaneFlippedHorizontally(Wrap.uvs); }
		}

		public bool FlipV {
			get { return IsPlaneFlippedVertically(Wrap.uvs); }
		}

		/// <summary>
		/// Patch the specified sprite definition with this definition.
		/// </summary>
		/// <param name="target">Target sprite definition.</param>
		/// <param name="use_target_collision_data">If set to <c>true</c>, collision data on the target will remain unchanged. If <c>false</c>, it will be copied over as well.</param>
		public void Patch(tk2dSpriteDefinition target, bool use_target_collision_data = true, bool suppress_debug = false) {
			if (!suppress_debug) Logger.Debug($"Patching definition '{target.name}' with definition '{Name}'");

			target.name = Wrap.name;
			target.boundsDataCenter = Wrap.boundsDataCenter;
			target.boundsDataExtents = Wrap.boundsDataExtents;
			target.untrimmedBoundsDataCenter = Wrap.untrimmedBoundsDataCenter;
			target.untrimmedBoundsDataExtents = Wrap.untrimmedBoundsDataExtents;

			if (!use_target_collision_data) {
				target.colliderConvex = Wrap.colliderConvex;
				target.colliderSmoothSphereCollisions = Wrap.colliderSmoothSphereCollisions;
				target.colliderType = Wrap.colliderType;
				target.colliderVertices = Wrap.colliderVertices;
				target.collisionLayer = Wrap.collisionLayer;
				target.complexGeometry = Wrap.complexGeometry;
				target.physicsEngine = Wrap.physicsEngine;
			}

			target.extractRegion = Wrap.extractRegion;
			target.flipped = Wrap.flipped;
			target.indices = Wrap.indices;
			target.material = Wrap.material;
			target.materialId = Wrap.materialId;
			target.materialInst = Wrap.materialInst;
			target.metadata = Wrap.metadata;
			target.normals = Wrap.normals;
			target.position0 = Wrap.position0;
			target.position1 = Wrap.position1;
			target.position2 = Wrap.position2;
			target.position3 = Wrap.position3;
			target.regionH = Wrap.regionH;
			target.regionW = Wrap.regionW;
			target.regionX = Wrap.regionX;
			target.regionY = Wrap.regionY;
			target.tangents = Wrap.tangents;
			target.texelSize = Wrap.texelSize;
			target.uvs = Wrap.uvs;
		}

		/// <summary>
		/// Creates a copy of the sprite definition.
		/// </summary>
		/// <returns>The new copy.</returns>
		public SpriteDefinition Copy() {
			var new_def = new SpriteDefinition(new tk2dSpriteDefinition());
			Patch(new_def, false, true);
			return new_def;
		}

		/// <summary>
		/// Exports this definition into a ParsedCollection.Definition.
		/// </summary>
		public Tk0dConfigParser.ParsedCollection.Definition Export(string spritesheet_path) {
			Logger.Debug($"ID: {ID} X: {X} Y: {Y} W: {Width} H: {Height}");
			Logger.Debug($"UVS: {Wrap.uvs[0].x},{Wrap.uvs[0].y} {Wrap.uvs[1].x},{Wrap.uvs[1].y} {Wrap.uvs[2].x},{Wrap.uvs[2].y} {Wrap.uvs[3].x},{Wrap.uvs[3].y}");
			var normalized_uvs = new Vector2[4];
			float lx = 999;
			float ly = 999;
			for (var i = 0; i < Wrap.uvs.Length; i++) {
				if (Wrap.uvs[i].x < lx) lx = Wrap.uvs[i].x;
				if (Wrap.uvs[i].y < ly) ly = Wrap.uvs[i].y;
			}

			for (var i = 0; i < Wrap.uvs.Length; i++) {
				float x = 0;
				float y = 0;

				if (Wrap.uvs[i].x > lx) x = 1;
				else x = 0;

				if (Wrap.uvs[i].y > ly) y = 1;
				else y = 0;

				normalized_uvs[i] = new Vector2(x, y);
			}

			Logger.Debug($"NUVS: {normalized_uvs[0].x},{normalized_uvs[0].y} {normalized_uvs[1].x},{normalized_uvs[1].y} {normalized_uvs[2].x},{normalized_uvs[2].y} {normalized_uvs[3].x},{normalized_uvs[3].y}");

			var def = new Tk0dConfigParser.ParsedCollection.Definition {
				ID = ID,
				FlipH = FlipH,
				FlipV = FlipV,
				SpritesheetOverride = spritesheet_path,
				X = X,
				Y = Y,
				W = Width,
				H = Height
			};

			return def;
		}

		public static bool IsPlaneFlippedVertically(Vector2[] uvs) {
			return uvs[3].y < uvs[0].y;
		}

		public static bool IsPlaneFlippedHorizontally(Vector2[] uvs) {
			return uvs[0].x > uvs[1].x;
		}

		/// <summary>
		/// Generates sprite geometry.
		/// </summary>
		/// <param name="xy">X and Y coordinates (top left origin).</param>
		/// <param name="wh">Width and height.</param>
		/// <param name="tex_size">Texel size.</param>
		/// <param name="scale">Scale.</param>
		/// <param name="uvs">UV array.</param>
		/// <param name="position0">First quad coordinate.</param>
		/// <param name="position1">Second quad coordinate.</param>
		/// <param name="position2">Third quad coordinate.</param>
		/// <param name="position3">Fourth quad coordinate.</param>
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

		/// <summary>
		/// Constructs a new sprite definition
		/// </summary>
		/// <returns>The new sprite definition.</returns>
		/// <param name="mat">Material with assigned texture.</param>
		/// <param name="override_name">Optional name override.</param>
		/// <param name="region_x">Optional region X coordinate (top left, only used if all 4 region arguments are provided).</param>
		/// <param name="region_y">Optional region Y coordinate (top left, only used if all 4 region arguments are provided).</param>
		/// <param name="region_w">Optional region width (only used if all 4 region arguments are provided).</param>
		/// <param name="region_h">Optional region height (only used if all 4 region arguments are provided).</param>
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

	/// <summary>
	/// Wrapper struct for <c>tk2dSpriteAnimationFrame</c> that provides a cleaner interface and works transparently with anything that expects <c>tk2dSpriteAnimationFrame</c>s.
	/// </summary>
	public struct SpriteAnimationFrame {
		/// <summary>
		/// Real tk2d object.
		/// </summary>
		public tk2dSpriteAnimationFrame Wrap;

		internal SpriteAnimationFrame(tk2dSpriteAnimationFrame frame) {
			Wrap = frame;
		}

		/// <summary>
		/// Implicit cast operator that allows you to use <c>SpriteAnimationFrame</c> anywhere where <c>tk2dSpriteAnimationFrame</c> is expected.
		/// </summary>
		public static implicit operator tk2dSpriteAnimationFrame(SpriteAnimationFrame s) => s.Wrap;

		internal static SpriteAnimationFrame Construct(SpriteCollection coll, int definition) {
			var new_frame = new SpriteAnimationFrame(new tk2dSpriteAnimationFrame());
			new_frame.Wrap.spriteId = definition;
			new_frame.Wrap.spriteCollection = coll;
			return new_frame;
		}

		/// <summary>
		/// Constructs a new sprite animation frame.
		/// </summary>
		/// <returns>The new sprite animation frame.</returns>
		/// <param name="coll">Sprite collection that this frame takes the sprite from.</param>
		/// <param name="definition">ID of the definition within the collection.</param>
		public static SpriteAnimationFrame Construct(SpriteCollection coll, ID definition) {
			var id = coll.GetIndex(definition);
			if (id < 0) throw new Exception($"Sprite definition {definition} doesn't exist - did you forget a namespace?");
			return Construct(coll, id);
		}
	}

	/// <summary>
	/// Wrapper struct for <c>tk2dSpriteAnimationClip</c> that provides a cleaner interface and works transparently with anything that expects <c>tk2dSpriteAnimationClip</c>s.
	/// </summary>
	public struct SpriteAnimationClip {
		/// <summary>
		/// Real tk2d object.
		/// </summary>
		public tk2dSpriteAnimationClip Wrap;

		internal List<SpriteAnimationFrame> WorkingFrameList;
		internal int WorkingDepth;

		internal SpriteAnimationClip(tk2dSpriteAnimationClip clip) {
			Wrap = clip;
			WorkingFrameList = null;
			WorkingDepth = 0;
		}

		/// <summary>
		/// Implicit cast operator that allows you to use <c>SpriteAnimationClip</c> anywhere where <c>tk2dSpriteAnimationClip</c> is expected.
		/// </summary>
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

		/// <value>Frames in the animation clip.</value>
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

		/// <summary>
		/// Constructs a new sprite animation clip.
		/// </summary>
		/// <returns>The new animation clip.</returns>
		/// <param name="name">Name.</param>
		/// <param name="fps">FPS.</param>
		/// <param name="wrap_mode">Wrap mode.</param>
		/// <param name="frames">Optional array of preset frames.</param>
		public static SpriteAnimationClip Construct(string name, int fps, tk2dSpriteAnimationClip.WrapMode wrap_mode, params SpriteAnimationFrame[] frames) {
			var new_clip = new SpriteAnimationClip(new tk2dSpriteAnimationClip());
			new_clip.Wrap.name = name;
			new_clip.Wrap.fps = fps;
			new_clip.Wrap.wrapMode = wrap_mode;
			new_clip.Wrap.frames = ListConverter.ToArray(frames, (f) => f.Wrap);
			return new_clip;
		}
	}

	/// <summary>
	/// Wrapper struct for <c>tk2dSpriteAnimation</c> that provides a cleaner interface and works transparently with anything that expects <c>tk2dSpriteAnimation</c>s.
	/// </summary>
	public struct SpriteAnimation {
		/// <summary>
		/// Real tk2d object.
		/// </summary>
		public tk2dSpriteAnimation Wrap;

		internal List<SpriteAnimationClip> WorkingClipList;
		internal int WorkingDepth;

		internal SpriteAnimation(tk2dSpriteAnimation animation) {
			Wrap = animation;
			WorkingClipList = null;
			WorkingDepth = 0;
		}

		/// <summary>
		/// Implicit cast operator that allows you to use <c>SpriteAnimation</c> anywhere where <c>tk2dSpriteAnimation</c> is expected.
		/// </summary>
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

		/// <value>All of the clips in this sprite animation.</value>
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

		/// <summary>
		/// Constructs a new sprite animation.
		/// </summary>
		/// <returns>The new sprite animation.</returns>
		/// <param name="parent">The game object to add this animation to.</param>
		/// <param name="clips">Optional array of preset clips.</param>
		public static SpriteAnimation Construct(GameObject parent, params SpriteAnimationClip[] clips) {
			var new_anim = new SpriteAnimation(parent.AddComponent<tk2dSpriteAnimation>());
			new_anim.Wrap.clips = ListConverter.ToArray(clips, (f) => f.Wrap);
			return new_anim;
		}

		/// <summary>
		/// Load a Semi Animation format file from its parsed representation.
		/// The animation will be attached to the global modded animation game object.
		/// </summary>
		/// <returns>The new sprite collection.</returns>
		/// <param name="parsed">Parsed representation of the Semi Animation.</param>
		/// <param name="anim_namespace">Namespace to use for registering the animation.</param>
		public static SpriteAnimation Load(Tk0dConfigParser.ParsedAnimation parsed, string anim_namespace) {
			// TODO make use of the name
			var anim = Construct(SemiLoader.AnimationTemplateStorageObject);
            var id = parsed.ID.WithContextNamespace(anim_namespace);

			var fps = parsed.DefaultFPS;

			var coll_id = parsed.Collection;
			if (!Registry.SpriteCollections.Contains(coll_id)) throw new Exception($"Semi collection '{coll_id}' doesn't exist. Did you forget to load it before loading the animation?");
			var coll = Registry.SpriteCollections[coll_id];

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
							var def = frame.Definition;
							if (def.DefaultNamespace) {
                                def = frame.Definition.WithNamespace(prefix);
							}

							var tk0d_def_id = coll.GetIndex(def);
							if (tk0d_def_id < 0) throw new Exception($"There is no sprite definition '{def}' in collection '{coll_id}'");

							var xdef = coll.GetDefinition(def);

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

			Registry.AnimationTemplates.Add(id, anim);
			return anim;
		}
	}

	/// <summary>
	/// Wrapper struct for <c>tk2dSpriteAnimator</c> that provides a cleaner interface and works transparently with anything that expects <c>tk2dSpriteAnimator</c>s.
	/// </summary>
	public struct SpriteAnimator {
		/// <summary>
		/// Real tk2d object.
		/// </summary>
		public tk2dSpriteAnimator Wrap;

		internal SpriteAnimator(tk2dSpriteAnimator animator) {
			Wrap = animator;
		}

		/// <summary>
		/// Implicit cast operator that allows you to use <c>SpriteAnimator</c> anywhere where <c>tk2dSpriteAnimator</c> is expected.
		/// </summary>
		public static implicit operator tk2dSpriteAnimator(SpriteAnimator s) => s.Wrap;

		/// <summary>
		/// Constructs a new sprite animator.
		/// </summary>
		/// <returns>The new sprite animator.</returns>
		/// <param name="parent">The game object to add this animator to.</param>
		/// <param name="animation">The <c>SpriteAnimation</c> object to use for the animator.</param>
		/// <param name="default_clip">Optional name of the default clip. If not set, the first clip will be used.</param>
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
