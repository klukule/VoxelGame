using Ionic.Zip;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using VoxelGame.Assets;
using VoxelGame.Worlds;
using VoxelGame.Entities;
using VoxelGame.Noise;
using VoxelGame.Rendering;
using VoxelGame.Rendering.Buffers;
using VoxelGame.UI;

namespace VoxelGame.Worlds
{
    /// <summary>
    /// Game world
    /// </summary>
    public partial class World : IDisposable
    {
        private Skybox _skybox;                         // Skybox model
        private Player _player;                         // Player instance
        private Thread _chunkUpdateThread;              // Chunk update thread
        private bool _chunkUpdateThreadExit = false;    // Exit flag (mostly because .NET Core doesn't support Thread.Abort)

        // Entity & chunk cache
        private List<Entity> _loadedEntities = new List<Entity>();              // List of active entities
        private List<Chunk> _loadedChunks = new List<Chunk>();                  // List of active chunks
        private List<Entity> _entitiesToDestroy = new List<Entity>();           // List of entities to destroy
        private LinkedList<Chunk> _chunksToUpdate = new LinkedList<Chunk>();    // List of chunks to update

        // Loading screen
        private int _worldSize = 6;                 // Initial distance of chunks to load
        private int _requiredChunksLoadedNum = 0;   // Number of chunks required for initial load
        private int _currentChunksLoadedNum = 0;    // Number of chunks loaded
        private Texture _loadingScreenTexture;      // Loading screen background
        private GUIStyle _loadingScreenStyle;       // Loading screen text style

        private static World _instance;                             // World instance
        private LightingUniformBuffer _lightBufferData;             // Skybox light info
        private float _lightAngle;                                  // Sun/moon angle
        private Vector2 _lastPlayerPos = Vector2.One;               // Last known player position
        private List<Vector2> _chunksToKeep = new List<Vector2>();  // Chunks that are to be kept loaded
        private List<Vector2> _newChunks = new List<Vector2>();     // Chunks that are to be loaded

        /// <summary>
        /// World seed
        /// </summary>
        public string Seed { get; }

        /// <summary>
        /// Whether the world has loaded spawn chunks
        /// </summary>
        public bool HasFinishedInitialLoading { get; private set; }

        /// <summary>
        /// Camera in this world
        /// </summary>
        public Camera WorldCamera { get; private set; }


        /// <summary>
        /// Currently loaded texture pack
        /// </summary>
        public TexturePack TexturePack { get; private set; }

        /// <summary>
        /// Global randomizer
        /// </summary>
        public Random Randomizer { get; private set; }

        /// <summary>
        /// Seeded noise
        /// </summary>
        public OpenSimplex TerrainNoise { get; private set; }

        /// <summary>
        /// Seeded biome noise
        /// </summary>
        public CellNoise BiomeNoise { get; private set; }

        /// <summary>
        /// Default water height
        /// </summary>
        public float WaterHeight { get; }

        /// <summary>
        /// Render distance
        /// </summary>
        public int RenderDistance => _worldSize;

        /// <summary>
        /// World Instance
        /// </summary>
        public static World Instance => _instance;

        /// <summary>
        /// List of currently loaded chunks
        /// </summary>
        public Chunk[] LoadedChunks => _loadedChunks.ToArray();

        /// <summary>
        /// List of currently loaded entities
        /// </summary>
        public IReadOnlyCollection<Entity> LoadedEntities => _loadedEntities;

        /// <summary>
        /// Player
        /// </summary>
        public Player Player => _player;

        public int UpdateQueueLength => _chunksToUpdate.Count;

        /// <summary>
        /// Create new empty world - used for deserialization
        /// </summary>
        public World()
        {
            //if (_instance != null)
            //    Dispose();
            //Begin();
        }

        /// <summary>
        /// Create new seeded world
        /// </summary>
        /// <param name="name">World name</param>
        /// <param name="seed">World seed</param>
        public World(string name, string seed)
        {
            if (_instance != null) Dispose();   // Dispose of old instance if any

            _instance = this;

            Name = name;
            Seed = seed;
            WaterHeight = 30;

            // Spawn player
            _player = new Player();
            Player.SetControlsActive(false);

            _loadedEntities.Add(_player);

            // Load data
            Begin();
        }

        /// <summary>
        /// Determines whether the chunk is in queue for update or not
        /// </summary>
        /// <param name="chunk">The chunk</param>
        /// <returns>True if chunk is in queue; otherwise false</returns>
        public bool IsChunkQueuedForRegeneration(Chunk chunk) => _chunksToUpdate.Contains(chunk);

        /// <summary>
        /// Loads assets, initializes structures and starts async update thread
        /// </summary>
        public void Begin()
        {
            TexturePack = AssetDatabase.GetAsset<TexturePack>("");  // Load default texture pack

            OpenStorage();

            // Setup seeded objects
            TerrainNoise = new OpenSimplex(Seed.GetSeed());
            BiomeNoise = new CellNoise(Seed.GetSeed());
            Randomizer = new Random(Seed.GetSeed());
            WorldCamera = new Camera();

            // Load skybox and loading screen UI data
            _skybox = new Skybox(AssetDatabase.GetAsset<Material>("Materials/World/Sky.mat"));
            _loadingScreenTexture = AssetDatabase.GetAsset<Texture>("Textures/GUI/menu_bg.png");
            _loadingScreenStyle = new GUIStyle()
            {
                Normal = new GUIStyleOption() { TextColor = Color4.White },
                HorizontalAlignment = HorizontalAlignment.Middle,
                VerticalAlignment = VerticalAlignment.Middle,
                FontSize = 48,
                Font = GUI.DefaultLabelStyle.Font
            };

            // Setup light

            var storedAngle = LoadPropertyFromStorage("LightAngle");
            if (string.IsNullOrEmpty(storedAngle))
                _lightAngle = 5;
            else
                _lightAngle = float.Parse(storedAngle);

            _lightBufferData = new LightingUniformBuffer();

            HasFinishedInitialLoading = false;

            // TODO: Use constant radius to allow for larger render distances
            _requiredChunksLoadedNum = (_worldSize + _worldSize + 1) * (_worldSize + _worldSize + 1);

            // TODO: Move to player class
            var dto = GetPlayerFromStorage(1);
            if (dto != null)
            {
                _player.Inventory.ItemsList.AddRange(dto.Inventory);
                _player.SetInitialPosition(dto.Position.Position, dto.Position.Rotation);
            }
            // Initialize all entities
            foreach (var entity in _loadedEntities)
                entity.Begin();

            // Start chunk update thread
            _chunkUpdateThread = new Thread(ChunkThread) { Name = "Chunk Generation Thread" };
            _chunkUpdateThread.Start();
        }

        /// <summary>
        /// Chunk thread processor
        /// </summary>
        void ChunkThread()
        {
            while (Program.IsRunning && _instance != null && !_chunkUpdateThreadExit)
            {
                try
                {
                    if (_chunksToUpdate.First != null)
                    {
                        Chunk chunk = _chunksToUpdate.First.Value;
                        lock (chunk)
                        {
                            chunk.GenerateLight();
                            chunk.GenerateMesh();
                            _chunksToUpdate.Remove(chunk);
                            if (!HasFinishedInitialLoading)
                                _currentChunksLoadedNum++;
                        }
                    }
                }
                catch (SynchronizationLockException ex)
                {
                    Debug.Log(ex.Message + ": " + ex.Source + " - " + ex.StackTrace, DebugLevel.Error);
                }
            }
        }

        /// <summary>
        /// Requests chunk to be updated
        /// </summary>
        /// <param name="chunk">The chunk to be updated</param>
        /// <param name="isPriority">Has high priority</param>
        /// <param name="modXPos">Modified X coordinate</param>
        /// <param name="modZPos">Modified Z coordinate</param>
        /// <param name="threaded">Update asynchronously</param>
        public void RequestChunkUpdate(Chunk chunk, bool isPriority, int modXPos, int modZPos, bool threaded = true)
        {
            if (threaded) // Async
            {
                lock (_chunksToUpdate)
                {
                    if (!_chunksToUpdate.Contains(chunk)) // Not yet queued
                    {
                        if (chunk.AreAllNeighborsSet) // If has neighbors
                        {
                            // Modifying around the border causes the neighboring chunk to have high prio. update
                            bool f = modZPos == Chunk.WIDTH - 1;    // Front neighbor high-prio.
                            bool b = modZPos == 0;                  // Back neighbor high-prio.
                            bool l = modXPos == Chunk.WIDTH - 1;    // Left neighbor high-prio.
                            bool r = modXPos == 0;                  // Right neighbor high-prio.

                            // Enqueue front neighbor
                            if (f)
                                _chunksToUpdate.AddFirst(chunk.FrontNeighbor);
                            else
                                _chunksToUpdate.AddLast(chunk.FrontNeighbor);

                            // Enqueue back neighbor
                            if (b)
                                _chunksToUpdate.AddFirst(chunk.BackNeighbor);
                            else
                                _chunksToUpdate.AddLast(chunk.BackNeighbor);

                            // Enqueue left neighbor
                            if (l)
                            {
                                if (chunk.LeftNeighbor.AreAllNeighborsSet)
                                {
                                    _chunksToUpdate.AddFirst(chunk.LeftNeighbor.FrontNeighbor);
                                    _chunksToUpdate.AddFirst(chunk.LeftNeighbor.BackNeighbor);
                                }
                                _chunksToUpdate.AddFirst(chunk.LeftNeighbor);
                            }
                            else
                            {
                                if (chunk.LeftNeighbor.AreAllNeighborsSet)
                                {
                                    _chunksToUpdate.AddLast(chunk.LeftNeighbor.FrontNeighbor);
                                    _chunksToUpdate.AddLast(chunk.LeftNeighbor.BackNeighbor);
                                }
                                _chunksToUpdate.AddLast(chunk.LeftNeighbor);
                            }

                            // Enqueue right neighbor
                            if (r)
                            {
                                if (chunk.RightNeighbor.AreAllNeighborsSet)
                                {
                                    _chunksToUpdate.AddFirst(chunk.RightNeighbor.FrontNeighbor);
                                    _chunksToUpdate.AddFirst(chunk.RightNeighbor.BackNeighbor);
                                }
                                _chunksToUpdate.AddFirst(chunk.RightNeighbor);
                            }
                            else
                            {
                                if (chunk.RightNeighbor.AreAllNeighborsSet)
                                {
                                    _chunksToUpdate.AddLast(chunk.RightNeighbor.FrontNeighbor);
                                    _chunksToUpdate.AddLast(chunk.RightNeighbor.BackNeighbor);
                                }
                                _chunksToUpdate.AddLast(chunk.RightNeighbor);
                            }
                        }
                        if (isPriority) // Enqueu self
                            _chunksToUpdate.AddFirst(chunk);
                        else
                            _chunksToUpdate.AddLast(chunk);
                    }
                }
            }
            else // Sync
            {
                chunk.GenerateLight();
                chunk.GenerateMesh();
            }
        }

        /// <summary>
        /// Tries to get chunk with given XZ location
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Z coordinate</param>
        /// <param name="chunk">Chunk</param>
        /// <returns>Returns true if chunk was found, otherwise false</returns>
        public bool TryGetChunkAtPosition(int x, int y, out Chunk chunk)
        {
            lock (_loadedChunks)
                chunk = _loadedChunks.FirstOrDefault(v => v.Position.X == x && v.Position.Y == y);
            return chunk != null;
        }

        /// <summary>
        /// World update
        /// </summary>
        public void Update()
        {
            if (TexturePack == null) return; // Wait for texturepack

            // Update entities
            for (var index = 0; index < _loadedEntities.Count; index++)
                _loadedEntities[index].Update();

            // Update dynamic lighting
            if (HasFinishedInitialLoading)
            {
                float dayInMins = 20f;

                _lightAngle += Time.DeltaTime * (6f / dayInMins);

                float colTime = (float)Math.Abs(Math.Cos(MathHelper.DegreesToRadians(_lightAngle)));
                colTime = (float)Math.Pow(colTime, 10);

                _lightBufferData.SunColor = Vector4.Lerp(Color4.LightYellow.ToVector4(), Color4.OrangeRed.ToVector4(), colTime);
                _lightBufferData.SunDirection = new Vector4(Mathf.GetForwardFromRotation(new Vector3(_lightAngle, 0, 0)), 1);

                float t = Vector3.Dot(_lightBufferData.SunDirection.Xyz, new Vector3(0, -1, 0));
                t = Math.Max(.000000001f, t);
                t = (float)Math.Pow(t, .25f) * 1.75f;

                _lightBufferData.SunStrength = t;

                Vector4 col = Vector4.Lerp(Color4.DarkSlateGray.ToVector4() / 5f, Color4.DarkSlateGray.ToVector4(), t) / 5f;
                _lightBufferData.AmbientColor = col;
            }

            UpdateView();
            ClearUpEntities();
        }

        /// <summary>
        /// Cleanup entities
        /// </summary>
        void ClearUpEntities()
        {
            for (var i = 0; i < _entitiesToDestroy.ToArray().Length; i++)
            {
                if (_loadedEntities.Contains(_entitiesToDestroy[i]))
                {
                    int index = _loadedEntities.IndexOf(_entitiesToDestroy[i]);
                    _loadedEntities[index].Destroyed();
                    _loadedEntities[index] = null;
                    _loadedEntities.RemoveAt(index);
                }
            }

            _entitiesToDestroy.Clear();
        }

        /// <summary>
        /// Render GUI
        /// </summary>
        public void RenderGUI()
        {
            if (HasFinishedInitialLoading) // Render any entity UI
            {
                foreach (var entity in _loadedEntities)
                    entity.RenderGUI();
            }
            else // Render loading screen
            {
                int winWidth = Window.WindowWidth;
                int winHeight = Window.WindowHeight;
                int perc = (int)((_currentChunksLoadedNum / (float)_requiredChunksLoadedNum / 2f) * 100f);
                Vector2 scale = new Vector2(winWidth / _loadingScreenTexture.Width, winHeight / _loadingScreenTexture.Height) / 4f;
                GUI.Image(_loadingScreenTexture, new Rect(0, 0, winWidth, winHeight), Vector2.Zero, scale);
                GUI.Label($"LOADING...", new Rect(0, 0, winWidth, winHeight), _loadingScreenStyle);
                GUI.Label($"{perc}%", new Rect(0, 48, winWidth, winHeight), _loadingScreenStyle);

                GUI.Label($"Required chunks: {_requiredChunksLoadedNum}\nLoaded chunks: {_currentChunksLoadedNum}\nChunks: {_loadedChunks.Count}\nLoad queue: {_chunksToUpdate.Count}", new Rect(10, 10, 400, 200));
            }
        }

        /// <summary>
        /// Update chunks
        /// </summary>
        void UpdateView()
        {
            // TODO: Move hightmap generation to async too

            int roundedX = (int)Math.Floor(WorldCamera.Position.X / 16);//Math.Ceiling(WorldCamera.Position.X / 16) * 16;
            int roundedZ = (int)Math.Floor(WorldCamera.Position.Z / 16);//Math.Ceiling(WorldCamera.Position.Z / 16) * 16;

            // If player moved
            if (roundedX != (int)_lastPlayerPos.X || roundedZ != (int)_lastPlayerPos.Y)
            {
                // Check whole world
                for (int x = -_worldSize - 1; x < _worldSize; x++)
                {
                    for (int z = -_worldSize - 1; z < _worldSize; z++)
                    {
                        int wantedX = x + (roundedX);
                        int wantedZ = z + (roundedZ);

                        // If chunk is loaded, keep it
                        if (TryGetChunkAtPosition(wantedX, wantedZ, out Chunk oChunk))
                        {
                            _chunksToKeep.Add(new Vector2(wantedX, wantedZ));
                            continue;
                        }

                        // Otherwise create new chunk
                        Chunk c = new Chunk(new Vector2(wantedX, wantedZ));
                        _newChunks.Add(c.Position);
                        if (ChunkExists(wantedX, wantedZ))
                        {
                            LoadChunkFromFile(c);
                        }
                        else
                        {
                            c.GenerateHeightMap();
                            c.FillBlocks();
                            lock (_loadedChunks)
                                _loadedChunks.Add(c);
                        }
                        // Link with left neighbor
                        if (TryGetChunkAtPosition(wantedX - 1, wantedZ, out oChunk))
                        {
                            c.LeftNeighbor = oChunk;
                            oChunk.RightNeighbor = c;

                            if (oChunk.AreAllNeighborsSet)
                            {
                                //7 as x & z as to not force update surrounding chunks too
                                RequestChunkUpdate(oChunk, false, 7, 7, true);
                            }
                        }
                        // Link with right neighbor
                        if (TryGetChunkAtPosition(wantedX + 1, wantedZ, out oChunk))
                        {
                            c.RightNeighbor = oChunk;
                            oChunk.LeftNeighbor = c;

                            if (oChunk.AreAllNeighborsSet)
                            {
                                //7 as x & z as to not force update surrounding chunks too
                                RequestChunkUpdate(oChunk, false, 7, 7, true);
                            }
                        }
                        // Link with back neighbor
                        if (TryGetChunkAtPosition(wantedX, wantedZ - 1, out oChunk))
                        {
                            c.BackNeighbor = oChunk;
                            oChunk.FrontNeighbor = c;

                            if (oChunk.AreAllNeighborsSet)
                            {
                                //7 as x & z as to not force update surrounding chunks too
                                RequestChunkUpdate(oChunk, false, 7, 7, true);
                            }
                        }
                        // Link with front neighbor
                        if (TryGetChunkAtPosition(wantedX, wantedZ + 1, out oChunk))
                        {
                            c.FrontNeighbor = oChunk;
                            oChunk.BackNeighbor = c;

                            if (oChunk.AreAllNeighborsSet)
                            {
                                //7 as x & z as to not force update surrounding chunks too
                                RequestChunkUpdate(oChunk, false, 7, 7, true);
                            }
                        }
                    }
                }

                // Go through all the chunks
                for (int i = 0; i < _loadedChunks.Count; i++)
                {
                    // If chunk should be kept... keep it
                    if (_chunksToKeep.Any(v => (int)v.X == (int)_loadedChunks[i].Position.X && (int)v.Y == (int)_loadedChunks[i].Position.Y) ||
                        _newChunks.Any(v => (int)v.X == (int)_loadedChunks[i].Position.X && (int)v.Y == (int)_loadedChunks[i].Position.Y))
                        continue;

                    // Otherwise remove from queue
                    if (_chunksToUpdate.Contains(_loadedChunks[i]))
                        _chunksToUpdate.Remove(_loadedChunks[i]);

                    // And destroy
                    var chunk = _loadedChunks[i];
                    _loadedChunks.Remove(chunk);
                    SaveChunkToFile(chunk, false);
                    chunk.Dispose();
                }

                //
                _chunksToKeep.Clear();
                _newChunks.Clear();
                _lastPlayerPos = new Vector2(roundedX, roundedZ);
            }

            if (!HasFinishedInitialLoading)
            {
                if (_currentChunksLoadedNum >= _requiredChunksLoadedNum)
                {
                    HasFinishedInitialLoading = true;
                    Player.SetControlsActive(true);
                }
            }
        }


        /// <summary>
        /// Render the world
        /// </summary>
        public void Render()
        {
            UniformBuffers.DirectionalLightBuffer.Update(_lightBufferData);
            WorldCamera.Update();
            for (var index = 0; index < _loadedEntities.Count; index++)
                _loadedEntities[index].Render();

            for (var index = 0; index < _loadedChunks.Count; index++)
                _loadedChunks[index].Render();

            _skybox.Render();
        }


        /// <summary>
        /// Add entity
        /// </summary>
        /// <param name="entity">The entity</param>
        public void AddEntity(Entity entity)
        {
            _loadedEntities.Add(entity);
            entity.Begin();
        }

        /// <summary>
        /// Remvoe the entity
        /// </summary>
        /// <param name="entity">entity</param>
        public void DestroyEntity(Entity entity)
        {
            _entitiesToDestroy.Add(entity);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            // Stop the chunk processor
            _chunkUpdateThreadExit = true;
            _chunkUpdateThread.Join();
            _chunkUpdateThread = null;


            // Save player
            var dto = new PlayerDTO()
            {
                ID = 1,
                Name = "Default",
                Inventory = Player.Inventory.ItemsList,
                // TODO: Health & hunger
            };
            dto.Position.Position = Player.Position;
            dto.Position.Rotation = Player.Rotation;

            StorePlayerInStorage(dto);

            // Clear entities
            foreach (var entity in _loadedEntities)
                DestroyEntity(entity);

            ClearUpEntities();
            _loadedEntities.Clear();
            _loadedEntities = null;
            _player = null;

            // Clear chunks
            lock (_loadedChunks)
                foreach (var loadedChunk in _loadedChunks)
                {
                    SaveChunkToFile(loadedChunk, true);
                    loadedChunk.Dispose();
                }

            _loadedChunks.Clear();
            _loadedChunks = null;

            _chunksToUpdate.Clear();
            _chunksToUpdate = null;

            _chunksToKeep.Clear();
            _chunksToKeep = null;

            _newChunks.Clear();
            _newChunks = null;

            // Save time
            SavePropertyToStorage("LightAngle", _lightAngle.ToString());


            // Close storage
            CloseStorage();

            // Clear skybox
            _skybox.Dispose();
            _instance = null;
            GC.Collect();
        }
    }
}
